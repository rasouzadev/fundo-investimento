using FundoInvestimento.Domain.DTOs.Response.Fundo;
using FundoInvestimento.Domain.Enums;
using FundoInvestimento.Libs.Utils;

namespace FundoInvestimento.Domain.Interfaces.UseCases;

/// <summary>
/// Contrato para o caso de uso responsável por listar o catálogo de fundos.
/// </summary>
public interface IObterFundosUseCase
{
    /// <summary>
    /// Executa a busca do catálogo de fundos aplicando os filtros solicitados.
    /// </summary>
    /// <param name="status">Filtro opcional de status de captação (ABERTO ou FECHADO).</param>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    /// <returns>Um <see cref="Result{T}"/> contendo a coleção de <see cref="FundoResponse"/>.</returns>
    Task<Result<IEnumerable<FundoResponse>>> ExecuteAsync(StatusCaptacao? status, CancellationToken cancellationToken = default);
}