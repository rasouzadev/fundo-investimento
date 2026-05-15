using Serilog;
using Serilog.Formatting.Compact;
using System.Diagnostics.CodeAnalysis;

namespace FundoInvestimento.Api.Extensions;

[ExcludeFromCodeCoverage]
public static class ObservabilityExtensions
{
    /// <summary>
    /// Configura os provedores de log e métricas (Observabilidade) da aplicação.
    /// </summary>
    public static IHostBuilder AddObservability(this IHostBuilder hostBuilder)
    {
        return hostBuilder.UseSerilog((context, services, configuration) =>
        {
            configuration
                .ReadFrom.Configuration(context.Configuration)
                .Enrich.FromLogContext()
                .Enrich.WithMachineName()
                .Enrich.WithEnvironmentName()
                .WriteTo.Console(new RenderedCompactJsonFormatter())
                .WriteTo.Seq(context.Configuration["Seq:ServerUrl"] ?? throw new Exception("A variável de ambiente 'Seq:ServerUrl' não foi encontrada nas configurações."));
        });
    }
}