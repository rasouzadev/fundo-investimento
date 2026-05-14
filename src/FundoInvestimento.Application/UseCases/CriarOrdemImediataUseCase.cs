using FundoInvestimento.Domain.DTOs.Requests.Ordem;
using FundoInvestimento.Domain.DTOs.Response.Ordem;
using FundoInvestimento.Domain.Interfaces.Data;
using FundoInvestimento.Domain.Interfaces.Repositories;
using FundoInvestimento.Domain.Interfaces.Strategies;
using FundoInvestimento.Domain.Interfaces.UseCases;
using FundoInvestimento.Libs.Utils;
using Microsoft.Extensions.Logging;

namespace FundoInvestimento.Application.UseCases;

/// <summary>
/// Caso de uso responsável por orquestrar a recepção, validação de cut-off e efetivação de ordens imediatas (Aporte ou Resgate).
/// </summary>
public class CriarOrdemImediataUseCase : ICriarOrdemImediataUseCase
{
    private readonly IClienteRepository _clienteRepository;
    private readonly IFundoRepository _fundoRepository;
    private readonly IPosicaoClienteRepository _posicaoRepository;
    private readonly IOrdemRepository _ordemRepository;
    private readonly IEnumerable<IProcessadorOperacaoStrategy> _processadores;
    private readonly IUnitOfWork _unitOfWork;
    private readonly TimeProvider _timeProvider;
    private readonly ILogger<CriarOrdemImediataUseCase> _logger;

    public CriarOrdemImediataUseCase(
        IClienteRepository clienteRepository,
        IFundoRepository fundoRepository,
        IPosicaoClienteRepository posicaoRepository,
        IOrdemRepository ordemRepository,
        IEnumerable<IProcessadorOperacaoStrategy> processadores,
        IUnitOfWork unitOfWork,
        TimeProvider timeProvider,
        ILogger<CriarOrdemImediataUseCase> logger)
    {
        _clienteRepository = clienteRepository;
        _fundoRepository = fundoRepository;
        _posicaoRepository = posicaoRepository;
        _ordemRepository = ordemRepository;
        _processadores = processadores;
        _unitOfWork = unitOfWork;
        _timeProvider = timeProvider;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<OrdemResponse>> ExecuteAsync(OrdemRequest request, CancellationToken cancellationToken = default)
    {
        var fundo = await _fundoRepository.ObterPorIdAsync(request.IdFundo, cancellationToken);
        if (fundo is null)
            return Result<OrdemResponse>.Failure(new CustomError("FUNDO_NAO_ENCONTRADO", "Fundo de investimento não localizado.", 404));

        var horaAtual = TimeOnly.FromTimeSpan(_timeProvider.GetLocalNow().TimeOfDay);
        var cutOffResult = fundo.DentroDoHorarioDeCorte(horaAtual);
        if (cutOffResult.IsFailure)
            return Result<OrdemResponse>.Failure(cutOffResult.GetError());

        var processador = _processadores.FirstOrDefault(p => p.TipoOperacao == request.TipoOperacao);
        if (processador is null)
            return Result<OrdemResponse>.Failure(new CustomError("OPERACAO_INVALIDA", "Tipo de operação não suportado.", 400));

        _unitOfWork.BeginTransaction();

        try
        {
            var cliente = await _clienteRepository.ObterPorIdAsync(request.IdCliente, cancellationToken);
            if (cliente is null)
                return RollbackAndFail(new CustomError("CLIENTE_NAO_ENCONTRADO", "Cliente não localizado.", 404));

            var posicao = await _posicaoRepository.ObterPorIdAsync(request.IdCliente, request.IdFundo, cancellationToken);
            bool ehNovaPosicao = posicao == null;
            var dataAtual = DateOnly.FromDateTime(_timeProvider.GetLocalNow().Date);

            var criacaoResult = processador.CriarImediata(cliente, fundo, posicao, request.QuantidadeCotas, dataAtual);
            if (criacaoResult.IsFailure)
                return RollbackAndFail(criacaoResult.GetError());

            var (ordem, posicaoAtualizada) = criacaoResult.GetSuccess();

            await _clienteRepository.AtualizarAsync(cliente, cancellationToken);

            if (ehNovaPosicao)
                await _posicaoRepository.AdicionarAsync(posicaoAtualizada, cancellationToken);
            else
                await _posicaoRepository.AtualizarAsync(posicaoAtualizada, cancellationToken);

            await _ordemRepository.AdicionarAsync(ordem, cancellationToken);

            _unitOfWork.Commit();

            return Result<OrdemResponse>.Success(new OrdemResponse
            {
                Id = ordem.Id,
                IdCliente = ordem.IdCliente,
                IdFundo = ordem.IdFundo,
                TipoOperacao = ordem.TipoOperacao,
                QuantidadeCotas = ordem.QuantidadeCotas,
                DataAgendamento = ordem.DataAgendamento,
                Status = ordem.Status,
                CriadoEm = ordem.CriadoEm
            });
        }
        catch (Exception ex)
        {
            _unitOfWork.Rollback();
            _logger.LogError(ex, "Erro crítico ao processar ordem imediata. ClienteId: {ClienteId}, FundoId: {FundoId}", request.IdCliente, request.IdFundo);
            throw;
        }
    }

    private Result<OrdemResponse> RollbackAndFail(CustomError error)
    {
        _unitOfWork.Rollback();
        return Result<OrdemResponse>.Failure(error);
    }
}