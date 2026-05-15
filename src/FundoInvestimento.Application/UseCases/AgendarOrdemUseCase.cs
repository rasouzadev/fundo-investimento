using FundoInvestimento.Application.Policies;
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
/// Caso de uso responsável por registrar intenções de investimento para datas futuras.
/// Delega a validação de regras do agendamento para a Strategy e apenas persiste a ordem PENDENTE.
/// </summary>
public class AgendarOrdemUseCase : IAgendarOrdemUseCase
{
    private readonly IClienteRepository _clienteRepository;
    private readonly IFundoRepository _fundoRepository;
    private readonly IPosicaoClienteRepository _posicaoRepository;
    private readonly IOrdemRepository _ordemRepository;
    private readonly IEnumerable<IProcessadorOperacaoStrategy> _processadores;
    private readonly IUnitOfWork _unitOfWork;
    private readonly TimeProvider _timeProvider;
    private readonly ILogger<AgendarOrdemUseCase> _logger;

    public AgendarOrdemUseCase(
        IClienteRepository clienteRepository,
        IFundoRepository fundoRepository,
        IPosicaoClienteRepository posicaoRepository,
        IOrdemRepository ordemRepository,
        IEnumerable<IProcessadorOperacaoStrategy> processadores,
        IUnitOfWork unitOfWork,
        TimeProvider timeProvider,
        ILogger<AgendarOrdemUseCase> logger)
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
    public async Task<Result<OrdemResponse>> ExecuteAsync(AgendarOrdemRequest request, CancellationToken cancellationToken = default)
    {
        return await ResiliencePolicies.DbRetryPolicy.ExecuteAsync(async (ct) =>
        {
            var fundo = await _fundoRepository.ObterPorIdAsync(request.IdFundo, ct);
            if (fundo is null)
                return Result<OrdemResponse>.Failure(new CustomError("FUNDO_NAO_ENCONTRADO", "Fundo de investimento não localizado.", 404));

            var processador = _processadores.FirstOrDefault(p => p.TipoOperacao == request.TipoOperacao);
            if (processador is null)
                return Result<OrdemResponse>.Failure(new CustomError("OPERACAO_INVALIDA", "Tipo de operação não suportado.", 400));

            _unitOfWork.BeginTransaction();

            try
            {
                var cliente = await _clienteRepository.ObterPorIdAsync(request.IdCliente, ct);
                if (cliente is null)
                    return RollbackAndFail(new CustomError("CLIENTE_NAO_ENCONTRADO", "Cliente não localizado.", 404));

                var posicao = await _posicaoRepository.ObterPorIdAsync(request.IdCliente, request.IdFundo, ct);
                var dataAtual = DateOnly.FromDateTime(_timeProvider.GetLocalNow().Date);

                var criacaoResult = processador.CriarAgendamento(cliente, fundo, posicao, request.QuantidadeCotas, request.DataAgendamento, dataAtual);
                if (criacaoResult.IsFailure)
                    return RollbackAndFail(criacaoResult.GetError());

                var ordemAgendada = criacaoResult.GetSuccess();

                await _ordemRepository.AdicionarAsync(ordemAgendada, ct);

                _unitOfWork.Commit();

                return Result<OrdemResponse>.Success(new OrdemResponse
                {
                    Id = ordemAgendada.Id,
                    IdCliente = ordemAgendada.IdCliente,
                    IdFundo = ordemAgendada.IdFundo,
                    TipoOperacao = ordemAgendada.TipoOperacao,
                    QuantidadeCotas = ordemAgendada.QuantidadeCotas,
                    DataAgendamento = ordemAgendada.DataAgendamento,
                    Status = ordemAgendada.Status,
                    CriadoEm = ordemAgendada.CriadoEm
                });
            }
            catch (Exception ex)
            {
                _unitOfWork.Rollback();
                _logger.LogError(ex, "Erro crítico ao processar agendamento de ordem. ClienteId: {ClienteId}, FundoId: {FundoId}", request.IdCliente, request.IdFundo);
                throw;
            }
        }, cancellationToken);
    }

    private Result<OrdemResponse> RollbackAndFail(CustomError error)
    {
        _unitOfWork.Rollback();
        return Result<OrdemResponse>.Failure(error);
    }
}