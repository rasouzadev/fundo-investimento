using Dapper;
using FundoInvestimento.Domain.Entities;
using FundoInvestimento.Domain.Interfaces.Repositories;
using FundoInvestimento.Infrastructure.Data;
using System.Diagnostics.CodeAnalysis;

namespace FundoInvestimento.Infrastructure.Repositories;

/// <summary>
/// Implementação do repositório de fundos de investimento utilizando Dapper.
/// </summary>
[ExcludeFromCodeCoverage]
public class FundoRepository : IFundoRepository
{
    private readonly DbSession _session;

    /// <summary>
    /// Inicializa o repositório com a sessão de banco de dados.
    /// </summary>
    /// <param name="session">Sessão compartilhada.</param>
    public FundoRepository(DbSession session)
    {
        _session = session;
    }

    /// <inheritdoc/>
    public async Task<Fundo?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT id AS Id, 
                   nome AS Nome, 
                   horario_corte AS HorarioCorte, 
                   valor_cota AS ValorCota, 
                   valor_minimo_aporte AS ValorMinimoAporte, 
                   valor_minimo_permanencia AS ValorMinimoPermanencia, 
                   status_captacao AS StatusCaptacao
            FROM fundo 
            WHERE id = @Id;";

        var command = new CommandDefinition(sql, new { Id = id }, _session.Transaction, cancellationToken: cancellationToken);
        return await _session.Connection.QuerySingleOrDefaultAsync<Fundo>(command);
    }
}