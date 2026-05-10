namespace FundoInvestimento.Api.Extensions;

using Scalar.AspNetCore;
using System.Diagnostics.CodeAnalysis;

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

        return app;
    }
}