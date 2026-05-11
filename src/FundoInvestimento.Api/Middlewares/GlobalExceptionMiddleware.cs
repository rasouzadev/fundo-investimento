using FundoInvestimento.Libs.Utils;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;

namespace FundoInvestimento.Api.Middlewares;

/// <summary>
/// Middleware global para captura e tratamento de exceções não tratadas na aplicação.
/// </summary>
[ExcludeFromCodeCoverage]
public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    /// <summary>
    /// Inicializa o middleware de tratamento de exceções.
    /// </summary>
    /// <param name="next">O próximo delegate no pipeline da requisição HTTP.</param>
    /// <param name="logger">O serviço de log para registro das falhas.</param>
    public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    /// <summary>
    /// Invoca o middleware, interceptando a requisição para capturar possíveis falhas.
    /// </summary>
    /// <param name="context">O contexto HTTP da requisição atual.</param>
    /// <returns>Uma tarefa assíncrona.</returns>
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Uma exception não tratada ocorreu durante o processamento da requisição. Rota: {Path}", context.Request.Path);
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var error = new CustomError(
            code: "ERRO_INTERNO_SERVIDOR",
            message: "Ocorreu um erro inesperado ao processar a sua solicitação. Tente novamente mais tarde.",
            statusCode: StatusCodes.Status500InternalServerError
        );

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = error.StatusCode;

        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
        };
        var jsonResponse = JsonSerializer.Serialize(error, options);

        await context.Response.WriteAsync(jsonResponse);
    }
}