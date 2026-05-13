using FundoInvestimento.Domain.DTOs.Response.Cliente;
using FundoInvestimento.Domain.Interfaces.UseCases;
using Microsoft.AspNetCore.Mvc;

namespace FundoInvestimento.Api.Controllers;

/// <summary>
/// Endpoints relacionados à gestão da conta corrente dos clientes.
/// </summary>
[Route("api/v1/clientes")]
public class ClientesController : BaseController
{
    private readonly IObterSaldoClienteUseCase _obterSaldoUseCase;

    public ClientesController(IObterSaldoClienteUseCase obterSaldoUseCase)
    {
        _obterSaldoUseCase = obterSaldoUseCase;
    }

    /// <summary>
    /// Retorna o saldo financeiro disponível na conta corrente do cliente.
    /// </summary>
    /// <param name="id">Identificador único do cliente.</param>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    /// <returns>O saldo disponível para operações.</returns>
    [HttpGet("{id}/saldo")]
    [ProducesResponseType(typeof(SaldoResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ObterSaldo(Guid id, CancellationToken cancellationToken)
    {
        var result = await _obterSaldoUseCase.ExecuteAsync(id, cancellationToken);

        return CustomResponse(result);
    }
}