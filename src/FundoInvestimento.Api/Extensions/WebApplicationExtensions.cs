using FundoInvestimento.Api.Middlewares;
using Scalar.AspNetCore;
using System.Diagnostics.CodeAnalysis;

namespace FundoInvestimento.Api.Extensions;

[ExcludeFromCodeCoverage]
public static class WebApplicationExtensions
{
    /// <summary>
    /// Configura middlewares da aplicação
    /// </summary>
    public static WebApplication UseMiddlewares(this WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
            app.MapScalarApiReference();
        }

        app.UseMiddleware<GlobalExceptionMiddleware>();
        return app;
    }
}