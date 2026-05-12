using FundoInvestimento.Domain.DTOs.Requests.Ordem;
using FundoInvestimento.Domain.DTOs.Response.Ordem;
using FundoInvestimento.Domain.Interfaces.Repositories;
using FundoInvestimento.Domain.Interfaces.UseCases;
using FundoInvestimento.Libs.Utils;

namespace FundoInvestimento.Application.UseCases;

/// <summary>
/// Caso de uso para recuperação e filtragem do histórico de ordens de investimento.
/// </summary>
public class ObterOrdensUseCase : IObterOrdensUseCase
{
    private readonly IOrdemRepository _ordemRepository;

    public ObterOrdensUseCase(IOrdemRepository ordemRepository)
    {
        _ordemRepository = ordemRepository;
    }

    /// <inheritdoc />
    public async Task<Result<IEnumerable<OrdemResponse>>> ExecuteAsync(ListarOrdensRequest request, CancellationToken cancellationToken = default)
    {
        var ordens = await _ordemRepository.ObterHistoricoAsync(
            request.IdCliente,
            request.IdFundo,
            request.DataInicio,
            request.DataFim,
            cancellationToken);

        var response = ordens.Select(o => new OrdemResponse
        {
            Id = o.Id,
            IdCliente = o.IdCliente,
            IdFundo = o.IdFundo,
            TipoOperacao = o.TipoOperacao,
            QuantidadeCotas = o.QuantidadeCotas,
            DataAgendamento = o.DataAgendamento,
            Status = o.Status,
            CriadoEm = o.CriadoEm
        });

        return Result<IEnumerable<OrdemResponse>>.Success(response);
    }
}