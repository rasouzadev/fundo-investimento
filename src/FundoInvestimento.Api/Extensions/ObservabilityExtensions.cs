using System.Diagnostics.CodeAnalysis;

namespace FundoInvestimento.Api.Extensions;

[ExcludeFromCodeCoverage]
public static class ObservabilityExtensions
{
    /// <summary>
    /// Configura os provedores de log e métricas (Observabilidade) da aplicação.
    /// </summary>
    public static WebApplicationBuilder AddObservability(this WebApplicationBuilder builder)
    {
        builder.Logging.ClearProviders();
        builder.Logging.AddConsole();
        builder.Logging.AddDebug();

        return builder;
    }
}