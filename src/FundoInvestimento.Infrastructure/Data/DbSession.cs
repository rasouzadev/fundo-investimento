using Microsoft.Extensions.Configuration;
using Npgsql;
using System.Data;
using System.Diagnostics.CodeAnalysis;

namespace FundoInvestimento.Infrastructure.Data;

/// <summary>
/// Gerencia o ciclo de vida da conexão com o banco de dados e compartilha a transação ativa entre os repositórios.
/// </summary>
[ExcludeFromCodeCoverage]
public sealed class DbSession : IDisposable
{
    /// <summary>
    /// A conexão ativa com o banco de dados.
    /// </summary>
    public IDbConnection Connection { get; }

    /// <summary>
    /// A transação atual, caso o Unit of Work tenha iniciado uma.
    /// </summary>
    public IDbTransaction? Transaction { get; set; }

    /// <summary>
    /// Inicializa a sessão abrindo a conexão com o banco de dados.
    /// </summary>
    /// <param name="configuration">Interface de configuração para obter a connection string.</param>
    public DbSession(IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Database");
        Connection = new NpgsqlConnection(connectionString);
        Connection.Open();
    }

    /// <summary>
    /// Libera os recursos de transação e conexão da memória.
    /// </summary>
    public void Dispose()
    {
        Transaction?.Dispose();
        Connection?.Dispose();
    }
}