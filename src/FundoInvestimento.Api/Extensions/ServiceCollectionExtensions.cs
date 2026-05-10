using System.Diagnostics.CodeAnalysis;

namespace FundoInvestimento.Api.Extensions;

[ExcludeFromCodeCoverage]
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adiciona configurações e dependências da aplicação
    /// </summary>
    public static IServiceCollection AddServices(this IServiceCollection services)
    {
        services.AddStandardOpenApi("Fundo de Investimento API", "v1");

        return services;
    }

    /// <summary>
    /// Adiciona a configuração do OpenAPI
    /// </summary>
    private static IServiceCollection AddStandardOpenApi(this IServiceCollection services, string title, string version)
    {
        services.AddEndpointsApiExplorer();
        services.AddOpenApi(c =>
        {
            c.AddDocumentTransformer((doc, ctx, ct) =>
            {
                doc.Info.Title = title;
                doc.Info.Version = version;
                doc.Info.Description = $"API {title} - Versão {version}";

                doc.Info.Contact = new()
                {
                    Name = "API Fundo Investimento",
                    Url = new Uri("https://github.com/rasouzadev/fundo-investimento")
                };

                doc.Info.License = new()
                {
                    Name = "MIT",
                    Url = new Uri("https://opensource.org/licenses/MIT")
                };

                return Task.CompletedTask;
            });
        });

        return services;
    }
}