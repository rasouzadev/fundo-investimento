using FundoInvestimento.Domain.Entities;
using FundoInvestimento.Domain.Enums;
using FundoInvestimento.Domain.Interfaces.Data;
using FundoInvestimento.Domain.Interfaces.Repositories;
using FundoInvestimento.Domain.Interfaces.Strategies;
using FundoInvestimento.Domain.Interfaces.UseCases;
using Microsoft.Extensions.Logging;

namespace FundoInvestimento.Application.UseCases;

/// <summary>
/// Implementação do motor de processamento em lote (Worker) de ordens agendadas.
/// Atua como orquestrador, delegando a validação final (saldos atuais, capacity) para as estratégias.
/// </summary>
public class ProcessarOrdensAgendadasUseCase : IProcessarOrdensAgendadasUseCase
{
    private readonly IOrdemRepository _ordemRepository;
    private readonly IFundoRepository _fundoRepository;
    private readonly IClienteRepository _clienteRepository;
    private readonly IPosicaoClienteRepository _posicaoRepository;
    private readonly IEnumerable<IProcessadorOperacaoStrategy> _processadores;
    private readonly IUnitOfWork _unitOfWork;
    private readonly TimeProvider _timeProvider;
    private readonly ILogger<ProcessarOrdensAgendadasUseCase> _logger;

    public ProcessarOrdensAgendadasUseCase(
        IOrdemRepository ordemRepository,
        IFundoRepository fundoRepository,
        IClienteRepository clienteRepository,
        IPosicaoClienteRepository posicaoRepository,
        IEnumerable<IProcessadorOperacaoStrategy> processadores,
        IUnitOfWork unitOfWork,
        TimeProvider timeProvider,
        ILogger<ProcessarOrdensAgendadasUseCase> logger)
    {
        _ordemRepository = ordemRepository;
        _fundoRepository = fundoRepository;
        _clienteRepository = clienteRepository;
        _posicaoRepository = posicaoRepository;
        _processadores = processadores;
        _unitOfWork = unitOfWork;
        _timeProvider = timeProvider;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        var dataAtual = DateOnly.FromDateTime(_timeProvider.GetLocalNow().Date);
        _logger.LogInformation("Iniciando processamento de ordens agendadas para a data {DataAtual}", dataAtual);

        var ordensPendentes = await _ordemRepository.ObterPendentesAteDataAsync(dataAtual, cancellationToken);
        var processadas = 0;

        foreach (var ordem in ordensPendentes)
        {
            _unitOfWork.BeginTransaction();

            try
            {
                var fundo = await _fundoRepository.ObterPorIdAsync(ordem.IdFundo, cancellationToken);
                var cliente = await _clienteRepository.ObterPorIdAsync(ordem.IdCliente, cancellationToken);
                var posicao = await _posicaoRepository.ObterPorIdAsync(ordem.IdCliente, ordem.IdFundo, cancellationToken);

                bool ehNovaPosicao = posicao == null;

                if (fundo == null || cliente == null)
                {
                    await RejeitarOrdemAsync(ordem, "Fundo ou Cliente não encontrados na base de dados.", cancellationToken);
                    continue;
                }

                var processador = _processadores.FirstOrDefault(p => p.TipoOperacao == ordem.TipoOperacao);
                if (processador == null)
                {
                    await RejeitarOrdemAsync(ordem, $"Nenhuma estratégia implementada para a operação {ordem.TipoOperacao}.", cancellationToken);
                    continue;
                }

                var execucaoResult = processador.ProcessarOrdemPendente(ordem, cliente, fundo, posicao);

                if (execucaoResult.IsSuccess)
                {
                    var concluirResult = ordem.Concluir();
                    if (concluirResult.IsFailure)
                    {
                        await RejeitarOrdemAsync(ordem, concluirResult.GetError().Message, cancellationToken);
                        continue;
                    }

                    var posicaoAtualizada = execucaoResult.GetSuccess();

                    await _clienteRepository.AtualizarAsync(cliente, cancellationToken);

                    if (ehNovaPosicao && ordem.TipoOperacao == TipoOperacao.APORTE)
                        await _posicaoRepository.AdicionarAsync(posicaoAtualizada, cancellationToken);
                    else
                        await _posicaoRepository.AtualizarAsync(posicaoAtualizada, cancellationToken);

                    await _ordemRepository.AtualizarAsync(ordem, cancellationToken);

                    _unitOfWork.Commit();
                    processadas++;
                }
                else
                {
                    await RejeitarOrdemAsync(ordem, execucaoResult.GetError().Message, cancellationToken);
                }
            }
            catch (Exception ex)
            {
                _unitOfWork.Rollback();
                _logger.LogError(ex, "Falha técnica ao processar a ordem {Id}", ordem.Id);
            }
        }

        _logger.LogInformation("Processamento concluído. {Total} ordens efetivadas com sucesso.", processadas);
    }

    /// <summary>
    /// Helper para encapsular a rejeição da ordem, garantindo que o status PENDENTE -> REJEITADO seja comitado no banco.
    /// </summary>
    private async Task RejeitarOrdemAsync(Ordem ordem, string motivo, CancellationToken cancellationToken)
    {
        var rejeitarResult = ordem.Rejeitar();
        if (rejeitarResult.IsSuccess)
        {
            await _ordemRepository.AtualizarAsync(ordem, cancellationToken);
            _unitOfWork.Commit();
            _logger.LogWarning("Ordem agendada {Id} foi REJEITADA. Motivo: {Motivo}", ordem.Id, motivo);
        }
        else
        {
            _unitOfWork.Rollback();
            _logger.LogError("Erro crítico ao tentar alterar o status da ordem {Id} para REJEITADO.", ordem.Id);
        }
    }
}