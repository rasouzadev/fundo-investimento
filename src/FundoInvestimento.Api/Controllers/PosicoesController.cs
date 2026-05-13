using FundoInvestimento.Domain.DTOs.Response.Posicao;
using FundoInvestimento.Domain.Interfaces.UseCases;
using Microsoft.AspNetCore.Mvc;

namespace FundoInvestimento.Api.Controllers;

/// <summary>
/// Endpoints para consulta de carteira e posições de investimento.
/// </summary>
[Route("api/v1/posicoes")]
public class PosicoesController : BaseController
{
    private readonly IObterPosicaoConsolidadaUseCase _obterPosicaoUseCase;

    public PosicoesController(IObterPosicaoConsolidadaUseCase obterPosicaoUseCase)
    {
        _obterPosicaoUseCase = obterPosicaoUseCase;
    }

    /// <summary>
    /// Retorna a posição consolidada (carteira de investimentos) de um cliente específico.
    /// </summary>
    /// <param name="idCliente">O identificador único do cliente.</param>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    /// <returns>A lista de fundos investidos e o patrimônio total.</returns>
    [HttpGet("{idCliente}")]
    [ProducesResponseType(typeof(PosicaoConsolidadaResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> ObterPosicaoConsolidada(Guid idCliente, CancellationToken cancellationToken)
    {
        var result = await _obterPosicaoUseCase.ExecuteAsync(idCliente, cancellationToken);

        return CustomResponse(result);
    }
}