using Dapper;
using FundoInvestimento.Domain.Interfaces.Data;
using System.Diagnostics.CodeAnalysis;

namespace FundoInvestimento.Infrastructure.Data;

[ExcludeFromCodeCoverage]
public class DatabaseInitializer
{
    private readonly IDbConnectionFactory _connectionFactory;
    private const string ScriptsFolder = "Scripts";
    private const string CreateTablesScript = "01_CreateTables.sql";

    public DatabaseInitializer(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    /// <summary>
    /// Inicializa o banco de dados, criando as tabelas necessárias.
    /// </summary>
    public void Initialize()
    {
        using var connection = _connectionFactory.CreateConnection();
        connection.Open();

        var scriptPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ScriptsFolder, CreateTablesScript);

        if (File.Exists(scriptPath))
        {
            var script = File.ReadAllText(scriptPath);
            connection.Execute(script);
        }
        else
        {
            throw new FileNotFoundException($"Script de banco de dados não encontrado em: {scriptPath}");
        }
    }
}