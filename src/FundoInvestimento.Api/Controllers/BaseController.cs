using FundoInvestimento.Libs.Utils;
using Microsoft.AspNetCore.Mvc;

namespace FundoInvestimento.Api.Controllers;

[ApiController]
public abstract class BaseController : ControllerBase
{
    /// <summary>
    /// Converte um CustomError do domínio em uma resposta HTTP padronizada (ProblemDetails).
    /// </summary>
    protected IActionResult CustomResponse(CustomError error)
    {
        return Problem(
            statusCode: error.StatusCode,
            title: error.Code,
            detail: error.Message
        );
    }

    /// <summary>
    /// Manipula o retorno do Result Pattern. Se sucesso, devolve 200 OK. Se falha, devolve o erro formatado.
    /// </summary>
    protected IActionResult CustomResponse<T>(Result<T> result)
    {
        if (result.IsSuccess)
        {
            return Ok(result.GetSuccess());
        }

        return CustomResponse(result.GetError());
    }

    /// <summary>
    /// Manipula o retorno do Result Pattern sem payload de dados (para métodos void/Task).
    /// </summary>
    protected IActionResult CustomResponse(Result result)
    {
        if (result.IsSuccess)
        {
            return NoContent();
        }

        return CustomResponse(result.GetError());
    }
}