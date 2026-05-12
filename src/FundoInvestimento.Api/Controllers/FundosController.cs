using FundoInvestimento.Domain.DTOs.Response.Fundo;
using FundoInvestimento.Domain.Enums;
using FundoInvestimento.Domain.Interfaces.UseCases;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel;

namespace FundoInvestimento.Api.Controllers;

/// <summary>
/// Endpoints para consulta do catálogo de Fundos de Investimento.
/// </summary>
[Route("api/v1/fundos")]
public class FundosController : BaseController
{
    private readonly IObterFundosUseCase _obterFundosUseCase;

    public FundosController(IObterFundosUseCase obterFundosUseCase)
    {
        _obterFundosUseCase = obterFundosUseCase;
    }

    /// <summary>
    /// Lista os fundos de investimento disponíveis no catálogo do sistema.
    /// </summary>
    /// <param name="status">Opcional. Filtra os fundos pelo seu status de captação (ex: ABERTO ou FECHADO).</param>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    /// <returns>Uma lista detalhada com as características de cada fundo.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<FundoResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ObterFundos(
        [FromQuery, Description("Filtro opcional pelo status de captação do fundo.")] StatusCaptacao? status,
        CancellationToken cancellationToken)
    {
        var result = await _obterFundosUseCase.ExecuteAsync(status, cancellationToken);

        return CustomResponse(result);
    }
}