using Dapper;
using FundoInvestimento.Application.UseCases;
using FundoInvestimento.Domain.Interfaces.Data;
using FundoInvestimento.Domain.Interfaces.Repositories;
using FundoInvestimento.Domain.Interfaces.UseCases;
using FundoInvestimento.Infrastructure.Data;
using FundoInvestimento.Infrastructure.Data.Handlers;
using FundoInvestimento.Infrastructure.Repositories;
using Microsoft.AspNetCore.Http.Json;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace FundoInvestimento.Api.Extensions;

[ExcludeFromCodeCoverage]
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adiciona configurações e dependências da aplicação
    /// </summary>
    public static IServiceCollection AddServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton(TimeProvider.System);

        services.AddJsonOptions();
        services.AddStandardOpenApi("Fundo de Investimento API", "v1");
        services.AddDatabaseConnection(configuration);
        services.AddRepositories();
        services.AddUseCases();

        return services;
    }

    /// <summary>
    /// Registra configurações referentes a políticas de JSON e endpoints da API.
    /// </summary>
    private static IServiceCollection AddJsonOptions(this IServiceCollection services)
    {
        var jsonOptionsSetup = (JsonSerializerOptions options) =>
        {
            options.PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower;
            options.DictionaryKeyPolicy = JsonNamingPolicy.SnakeCaseLower;
            options.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
            options.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.SnakeCaseUpper));
        };

        services.AddControllers()
        .AddJsonOptions(options => jsonOptionsSetup(options.JsonSerializerOptions));

        services.Configure<JsonOptions>(options => jsonOptionsSetup(options.SerializerOptions));

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

        SqlMapper.AddTypeHandler(new DateOnlyTypeHandler());
        SqlMapper.AddTypeHandler(new TimeOnlyTypeHandler());

        services.AddSingleton<IDbConnectionFactory>(new NpgsqlConnectionFactory(connectionString));
        services.AddScoped<DatabaseInitializer>();

        return services;
    }

    /// <summary>
    /// Registra as implementações dos repositórios e do UnitOfWork para injeção de dependência.
    /// </summary>
    private static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        services.AddScoped<DbSession>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IClienteRepository, ClienteRepository>();
        services.AddScoped<IFundoRepository, FundoRepository>();
        services.AddScoped<IOrdemRepository, OrdemRepository>();
        services.AddScoped<IPosicaoClienteRepository, PosicaoClienteRepository>();
        return services;
    }

    /// <summary>
    /// Registra as implementações dos UseCases para injeção de dependência.
    /// </summary>
    private static IServiceCollection AddUseCases(this IServiceCollection services)
    {
        services.AddScoped<ICriarOrdemImediataUseCase, CriarOrdemImediataUseCase>();
        services.AddScoped<IObterOrdensUseCase, ObterOrdensUseCase>();
        services.AddScoped<IObterFundosUseCase, ObterFundosUseCase>();
        services.AddScoped<IObterPosicaoConsolidadaUseCase, ObterPosicaoConsolidadaUseCase>();

        return services;
    }
}