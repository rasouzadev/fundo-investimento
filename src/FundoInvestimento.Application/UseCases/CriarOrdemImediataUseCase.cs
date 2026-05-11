using FundoInvestimento.Domain.DTOs.Requests.Ordem;
using FundoInvestimento.Domain.DTOs.Response.Ordem;
using FundoInvestimento.Domain.Entities;
using FundoInvestimento.Domain.Enums;
using FundoInvestimento.Domain.Interfaces.Data;
using FundoInvestimento.Domain.Interfaces.Repositories;
using FundoInvestimento.Domain.Interfaces.UseCases;
using FundoInvestimento.Libs.Utils;
using Microsoft.Extensions.Logging;

namespace FundoInvestimento.Application.UseCases;

/// <summary>
/// Caso de uso responsável por orquestrar a criação, validação e efetivação de uma ordem imediata (Aporte ou Resgate).
/// </summary>
public class CriarOrdemImediataUseCase : ICriarOrdemImediataUseCase
{
    private readonly IClienteRepository _clienteRepository;
    private readonly IFundoRepository _fundoRepository;
    private readonly IPosicaoClienteRepository _posicaoRepository;
    private readonly IOrdemRepository _ordemRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly TimeProvider _timeProvider;
    private readonly ILogger<CriarOrdemImediataUseCase> _logger;

    public CriarOrdemImediataUseCase(
        IClienteRepository clienteRepository,
        IFundoRepository fundoRepository,
        IPosicaoClienteRepository posicaoRepository,
        IOrdemRepository ordemRepository,
        IUnitOfWork unitOfWork,
        TimeProvider timeProvider,
        ILogger<CriarOrdemImediataUseCase> logger)
    {
        _clienteRepository = clienteRepository;
        _fundoRepository = fundoRepository;
        _posicaoRepository = posicaoRepository;
        _ordemRepository = ordemRepository;
        _unitOfWork = unitOfWork;
        _timeProvider = timeProvider;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<OrdemResponse>> ExecuteAsync(OrdemRequest request, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Iniciando processamento de ordem imediata. ClienteId: {ClienteId}, FundoId: {FundoId}",
            request.IdCliente, request.IdFundo);

        var fundo = await _fundoRepository.ObterPorIdAsync(request.IdFundo, cancellationToken);
        if (fundo is null)
        {
            _logger.LogWarning("Ordem rejeitada: Fundo não encontrado. FundoId: {FundoId}", request.IdFundo);
            return Result<OrdemResponse>.Failure(new CustomError("FUNDO_NAO_ENCONTRADO", "Fundo de investimento não localizado.", 404));
        }

        var horaAtual = _timeProvider.GetLocalNow().TimeOfDay;
        var cutOffResult = fundo.DentroDoHorarioDeCorte(horaAtual);
        if (cutOffResult.IsFailure)
        {
            _logger.LogWarning("Ordem rejeitada: Fora do horário de corte. FundoId: {FundoId}", request.IdFundo);
            return Result<OrdemResponse>.Failure(cutOffResult.GetError());
        }

        _unitOfWork.BeginTransaction();

        try
        {
            var cliente = await _clienteRepository.ObterPorIdAsync(request.IdCliente, cancellationToken);
            if (cliente is null)
            {
                _unitOfWork.Rollback();
                _logger.LogWarning("Ordem rejeitada: Cliente não encontrado. ClienteId: {ClienteId}", request.IdCliente);
                return Result<OrdemResponse>.Failure(new CustomError("CLIENTE_NAO_ENCONTRADO", "Cliente não localizado.", 404));
            }

            var posicao = await _posicaoRepository.ObterPorIdAsync(request.IdCliente, request.IdFundo, cancellationToken)
                          ?? new PosicaoCliente(request.IdCliente, request.IdFundo, 0);

            var ordemResult = Ordem.CriarImediata(request.IdCliente, request.IdFundo, request.TipoOperacao, request.QuantidadeCotas);
            if (ordemResult.IsFailure)
            {
                _unitOfWork.Rollback();
                _logger.LogWarning("Ordem rejeitada: Dados da ordem inválidos.");
                return Result<OrdemResponse>.Failure(ordemResult.GetError());
            }

            var ordem = ordemResult.GetSuccess();
            var valorFinanceiroTotal = request.QuantidadeCotas * fundo.ValorCota;

            Result processamentoResult = request.TipoOperacao == TipoOperacao.APORTE
                ? ProcessarAporte(fundo, cliente, posicao, request.QuantidadeCotas, valorFinanceiroTotal, request)
                : ProcessarResgate(fundo, cliente, posicao, request.QuantidadeCotas, valorFinanceiroTotal, request);

            if (processamentoResult.IsFailure)
            {
                _unitOfWork.Rollback();
                return Result<OrdemResponse>.Failure(processamentoResult.GetError());
            }

            var concluirResult = ordem.Concluir();
            if (concluirResult.IsFailure)
            {
                _unitOfWork.Rollback();
                return Result<OrdemResponse>.Failure(concluirResult.GetError());
            }

            await _clienteRepository.AtualizarAsync(cliente, cancellationToken);

            if (posicao.QuantidadeCotas == request.QuantidadeCotas && request.TipoOperacao == TipoOperacao.APORTE)
                await _posicaoRepository.AdicionarAsync(posicao, cancellationToken);
            else
                await _posicaoRepository.AtualizarAsync(posicao, cancellationToken);

            await _ordemRepository.AdicionarAsync(ordem, cancellationToken);

            _unitOfWork.Commit();

            return Result<OrdemResponse>.Success(new OrdemResponse
            {
                Id = ordem.Id,
                TipoOperacao = ordem.TipoOperacao,
                Status = ordem.Status,
                CriadoEm = ordem.CriadoEm
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro crítico ao processar ordem imediata. Realizando rollback.");
            _unitOfWork.Rollback();
            throw;
        }
    }

    private Result ProcessarAporte(Fundo fundo, Cliente cliente, PosicaoCliente posicao, int quantidadeCotas, decimal valorFinanceiroTotal, OrdemRequest request)
    {
        var aceiteResult = fundo.AceitaAporte(valorFinanceiroTotal);
        if (aceiteResult.IsFailure) return LogAndReturnFailure(aceiteResult.GetError(), request);

        var debitoResult = cliente.DebitarSaldo(valorFinanceiroTotal);
        if (debitoResult.IsFailure) return LogAndReturnFailure(debitoResult.GetError(), request);

        var addCotasResult = posicao.AdicionarCotas(quantidadeCotas);
        if (addCotasResult.IsFailure) return LogAndReturnFailure(addCotasResult.GetError(), request);

        return Result.Success();
    }

    private Result ProcessarResgate(Fundo fundo, Cliente cliente, PosicaoCliente posicao, int quantidadeCotas, decimal valorFinanceiroTotal, OrdemRequest request)
    {
        var remCotasResult = posicao.RemoverCotas(quantidadeCotas);
        if (remCotasResult.IsFailure) return LogAndReturnFailure(remCotasResult.GetError(), request);

        var saldoCotizadoTotal = posicao.QuantidadeCotas * fundo.ValorCota;
        var permanenciaResult = fundo.ResgateDeixaSaldoValido(saldoCotizadoTotal + valorFinanceiroTotal, valorFinanceiroTotal);
        if (permanenciaResult.IsFailure) return LogAndReturnFailure(permanenciaResult.GetError(), request);

        var creditoResult = cliente.CreditarSaldo(valorFinanceiroTotal);
        if (creditoResult.IsFailure) return LogAndReturnFailure(creditoResult.GetError(), request);

        return Result.Success();
    }

    private Result LogAndReturnFailure(CustomError error, OrdemRequest request)
    {
        _logger.LogWarning("Regra de negócio violada: {ErrorCode} para o Cliente {ClienteId}", error.Code, request.IdCliente);
        return Result.Failure(error);
    }
}