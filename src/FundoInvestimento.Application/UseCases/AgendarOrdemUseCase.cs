 using FundoInvestimento.Domain.DTOs.Requests.Ordem;
using FundoInvestimento.Domain.DTOs.Response.Ordem;
using FundoInvestimento.Domain.Entities;
using FundoInvestimento.Domain.Interfaces.Data;
using FundoInvestimento.Domain.Interfaces.Repositories;
using FundoInvestimento.Domain.Interfaces.UseCases;
using FundoInvestimento.Libs.Utils;

namespace FundoInvestimento.Application.UseCases;

/// <summary>
/// Caso de uso que orquestra a criação de uma ordem agendada.
/// Valida a existência das entidades relacionadas e garante que a data do agendamento seja estritamente no futuro.
/// </summary>
public class AgendarOrdemUseCase : IAgendarOrdemUseCase
{
    private readonly IClienteRepository _clienteRepository;
    private readonly IFundoRepository _fundoRepository;
    private readonly IOrdemRepository _ordemRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly TimeProvider _timeProvider;

    public AgendarOrdemUseCase(
        IClienteRepository clienteRepository,
        IFundoRepository fundoRepository,
        IOrdemRepository ordemRepository,
        IUnitOfWork unitOfWork,
        TimeProvider timeProvider)
    {
        _clienteRepository = clienteRepository;
        _fundoRepository = fundoRepository;
        _ordemRepository = ordemRepository;
        _unitOfWork = unitOfWork;
        _timeProvider = timeProvider;
    }

    /// <inheritdoc />
    public async Task<Result<OrdemResponse>> ExecuteAsync(AgendarOrdemRequest request, CancellationToken cancellationToken = default)
    {
        var cliente = await _clienteRepository.ObterPorIdAsync(request.IdCliente, cancellationToken);
        if (cliente == null)
            return Result<OrdemResponse>.Failure(new CustomError("CLIENTE_NAO_ENCONTRADO", "Cliente não encontrado.", 404));

        var fundo = await _fundoRepository.ObterPorIdAsync(request.IdFundo, cancellationToken);
        if (fundo == null)
            return Result<OrdemResponse>.Failure(new CustomError("FUNDO_NAO_ENCONTRADO", "Fundo não encontrado.", 404));

        var dataAtual = DateOnly.FromDateTime(_timeProvider.GetLocalNow().Date);

        var ordemResult = Ordem.CriarAgendada(
            request.IdCliente,
            request.IdFundo,
            request.TipoOperacao,
            request.QuantidadeCotas,
            request.DataAgendamento,
            dataAtual);

        if (ordemResult.IsFailure)
            return Result<OrdemResponse>.Failure(ordemResult.GetError());

        var ordemAgendada = ordemResult.GetSuccess();

        _unitOfWork.BeginTransaction();
        try
        {
            await _ordemRepository.AdicionarAsync(ordemAgendada, cancellationToken);
            _unitOfWork.Commit();
        }
        catch
        {
            _unitOfWork.Rollback();
            throw;
        }

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
}