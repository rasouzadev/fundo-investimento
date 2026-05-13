using FundoInvestimento.Application.UseCases;
using FundoInvestimento.Domain.DTOs.Requests.Ordem;
using FundoInvestimento.Domain.DTOs.Response.Ordem;
using FundoInvestimento.Domain.Interfaces.UseCases;
using Microsoft.AspNetCore.Mvc;

namespace FundoInvestimento.Api.Controllers;

/// <summary>
/// Endpoints para gestão de ordens de investimento (Aportes e Resgates).
/// </summary>
[Route("api/v1/ordens")]
public class OrdensController : BaseController
{
    private readonly ICriarOrdemImediataUseCase _criarOrdemImediataUseCase;
    private readonly IObterOrdensUseCase _obterOrdensUseCase;
    private readonly IAgendarOrdemUseCase _agendarOrdemUseCase;

    public OrdensController(ICriarOrdemImediataUseCase criarOrdemImediataUseCase, IObterOrdensUseCase obterOrdensUseCase, IAgendarOrdemUseCase agendarOrdemUseCase)
    {
        _criarOrdemImediataUseCase = criarOrdemImediataUseCase;
        _obterOrdensUseCase = obterOrdensUseCase;
        _agendarOrdemUseCase = agendarOrdemUseCase;
    }

    /// <summary>
    /// Solicita a execução de uma nova ordem imediata (Aporte ou Resgate) em um fundo de investimentos.
    /// </summary>
    /// <param name="request">Payload contendo os dados do cliente, fundo e operação desejada.</param>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    /// <returns>Os dados da ordem processada ou os detalhes da violação da regra de negócio.</returns>
    [HttpPost]
    [ProducesResponseType(typeof(OrdemResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CriarOrdemImediata([FromBody] OrdemRequest request, CancellationToken cancellationToken)
    {
        var result = await _criarOrdemImediataUseCase.ExecuteAsync(request, cancellationToken);

        return CustomResponse(result, StatusCodes.Status201Created);
    }

    /// <summary>
    /// Recupera o histórico de ordens com base nos filtros fornecidos.
    /// </summary>
    /// <param name="filtro">DTO contendo filtros por cliente, fundo e período.</param>
    /// <param name="cancellationToken">Token de cancelamento da requisição.</param>
    /// <returns>Uma coleção de ordens que atendem aos critérios de busca.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<OrdemResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ListarOrdens([FromQuery] ListarOrdensRequest filtro, CancellationToken cancellationToken)
    {
        var result = await _obterOrdensUseCase.ExecuteAsync(filtro, cancellationToken);
        return CustomResponse(result);
    }

    /// <summary>
    /// Registra o agendamento de uma ordem de aporte ou resgate para uma data futura.
    /// </summary>
    /// <param name="request">Os dados do agendamento, incluindo a data alvo.</param>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    /// <returns>A ordem criada com o status PENDENTE.</returns>
    [HttpPost("agendamentos")]
    [ProducesResponseType(typeof(OrdemResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> AgendarOrdem([FromBody] AgendarOrdemRequest request, CancellationToken cancellationToken)
    {
        var result = await _agendarOrdemUseCase.ExecuteAsync(request, cancellationToken);

        return CustomResponse(result, StatusCodes.Status201Created);
    }
}