using FundoInvestimento.Domain.Interfaces.Data;
using FundoInvestimento.Infrastructure.Data;
using System.Diagnostics.CodeAnalysis;

namespace FundoInvestimento.Api.Extensions;

[ExcludeFromCodeCoverage]
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adiciona configurações e dependências da aplicação
    /// </summary>
    public static IServiceCollection AddServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddStandardOpenApi("Fundo de Investimento API", "v1");
        services.AddDatabaseConnection(configuration);

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

    /// <summary>
    /// Registra o serviço do banco de dados. O DatabaseInitializer deve ser utilizado apenas em ambiente dev/local.
    /// </summary>
    private static IServiceCollection AddDatabaseConnection(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Database");

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new ArgumentNullException(nameof(connectionString), "A connection string 'Database' não foi encontrada nas configurações.");
        }

        services.AddSingleton<IDbConnectionFactory>(new NpgsqlConnectionFactory(connectionString));
        services.AddScoped<DatabaseInitializer>();

        return services;
    }
}