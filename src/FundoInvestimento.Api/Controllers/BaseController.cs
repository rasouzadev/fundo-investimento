using FundoInvestimento.Libs.Utils;
using Microsoft.AspNetCore.Mvc;

namespace FundoInvestimento.Api.Controllers;

[ApiController]
public abstract class BaseController : ControllerBase
{
    /// <summary>
    /// Traduz o Result do domínio para uma resposta HTTP correspondente (RFC 7807 para erros).
    /// </summary>
    /// <param name="result">O resultado da operação (Sucesso ou Falha).</param>
    /// <param name="successStatusCode">O HTTP Status Code desejado em caso de sucesso (Padrão: 200 OK).</param>
    protected IActionResult CustomResponse<T>(Result<T> result, int successStatusCode = StatusCodes.Status200OK)
    {
        if (result.IsFailure)
        {
            var error = result.GetError();
            
            var problemDetails = new ProblemDetails
            {
                Status = error.StatusCode,
                Title = error.Code,
                Detail = error.Message,
                Instance = HttpContext.Request.Path
            };

            return StatusCode(error.StatusCode, problemDetails);
        }

        return successStatusCode switch
        {
            StatusCodes.Status201Created => StatusCode(StatusCodes.Status201Created, result.GetSuccess()),
            StatusCodes.Status204NoContent => NoContent(),
            _ => Ok(result.GetSuccess())
        };
    }

    /// <summary>
    /// Overload para Results que não possuem retorno de dados.
    /// </summary>
    protected IActionResult CustomResponse(Result result, int successStatusCode = StatusCodes.Status200OK)
    {
        if (result.IsFailure)
        {
            var error = result.GetError();

            var problemDetails = new ProblemDetails
            {
                Status = error.StatusCode,
                Title = error.Code,
                Detail = error.Message,
                Instance = HttpContext.Request.Path
            };

            return StatusCode(error.StatusCode, problemDetails);
        }

        return successStatusCode switch
        {
            StatusCodes.Status201Created => StatusCode(StatusCodes.Status201Created),
            StatusCodes.Status204NoContent => NoContent(),
            _ => Ok()
        };
    }
}