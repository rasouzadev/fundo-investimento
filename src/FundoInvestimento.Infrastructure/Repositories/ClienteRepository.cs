using Dapper;
using FundoInvestimento.Domain.Entities;
using FundoInvestimento.Domain.Interfaces.Repositories;
using FundoInvestimento.Infrastructure.Data;
using System.Diagnostics.CodeAnalysis;

namespace FundoInvestimento.Infrastructure.Repositories;

/// <summary>
/// Implementação do repositório de clientes utilizando Dapper.
/// </summary>
[ExcludeFromCodeCoverage]
public class ClienteRepository : IClienteRepository
{
    private readonly DbSession _session;

    /// <summary>
    /// Inicializa o repositório com a sessão de banco de dados.
    /// </summary>
    /// <param name="session">Sessão compartilhada.</param>
    public ClienteRepository(DbSession session)
    {
        _session = session;
    }

    /// <inheritdoc/>
    public async Task<Cliente?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT id AS Id, 
                   nome AS Nome, 
                   cpf AS Cpf, 
                   saldo_disponivel AS SaldoDisponivel
            FROM cliente 
            WHERE id = @Id
            FOR UPDATE;";

        var command = new CommandDefinition(sql, new { Id = id }, _session.Transaction, cancellationToken: cancellationToken);
        return await _session.Connection.QuerySingleOrDefaultAsync<Cliente>(command);
    }

    /// <inheritdoc/>
    public async Task AtualizarAsync(Cliente cliente, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            UPDATE cliente 
            SET saldo_disponivel = @SaldoDisponivel 
            WHERE id = @Id;";

        var command = new CommandDefinition(sql, new { cliente.SaldoDisponivel, cliente.Id }, _session.Transaction, cancellationToken: cancellationToken);
        await _session.Connection.ExecuteAsync(command);
    }
}