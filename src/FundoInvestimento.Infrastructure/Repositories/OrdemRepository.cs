using Dapper;
using FundoInvestimento.Domain.Entities;
using FundoInvestimento.Domain.Interfaces.Repositories;
using FundoInvestimento.Infrastructure.Data;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace FundoInvestimento.Infrastructure.Repositories;

/// <summary>
/// Implementação do repositório de histórico de ordens utilizando Dapper.
/// </summary>
[ExcludeFromCodeCoverage]
public class OrdemRepository : IOrdemRepository
{
    private readonly DbSession _session;

    /// <summary>
    /// Inicializa o repositório com a sessão de banco de dados.
    /// </summary>
    /// <param name="session">Sessão compartilhada gerenciada pela injeção de dependência.</param>
    public OrdemRepository(DbSession session)
    {
        _session = session;
    }

    /// <inheritdoc/>
    public async Task<Ordem?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT id AS Id, 
                   id_cliente AS IdCliente, 
                   id_fundo AS IdFundo, 
                   tipo_operacao AS TipoOperacao, 
                   quantidade_cotas AS QuantidadeCotas, 
                   data_agendamento AS DataAgendamento, 
                   status AS Status, 
                   criado_em AS CriadoEm
            FROM ordem 
            WHERE id = @Id;";

        var command = new CommandDefinition(
            sql,
            new { Id = id },
            _session.Transaction,
            cancellationToken: cancellationToken);

        return await _session.Connection.QuerySingleOrDefaultAsync<Ordem>(command);
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<Ordem>> ObterHistoricoAsync(
        Guid? idCliente,
        Guid? idFundo,
        DateOnly? inicio,
        DateOnly? fim,
        CancellationToken cancellationToken = default)
    {
        var sql = new StringBuilder(@"
        SELECT 
            id AS Id, 
            id_cliente AS IdCliente, 
            id_fundo AS IdFundo, 
            tipo_operacao AS TipoOperacao, 
            quantidade_cotas AS QuantidadeCotas, 
            data_agendamento AS DataAgendamento, 
            status AS Status, 
            criado_em AS CriadoEm 
        FROM ordem 
        WHERE 1=1 ");

        var parameters = new DynamicParameters();

        if (idCliente.HasValue)
        {
            sql.Append("AND id_cliente = @IdCliente ");
            parameters.Add("IdCliente", idCliente);
        }

        if (idFundo.HasValue)
        {
            sql.Append("AND id_fundo = @IdFundo ");
            parameters.Add("IdFundo", idFundo);
        }

        if (inicio.HasValue)
        {
            sql.Append("AND criado_em >= @Inicio ");
            parameters.Add("Inicio", inicio.Value.ToDateTime(TimeOnly.MinValue));
        }

        if (fim.HasValue)
        {
            sql.Append("AND criado_em <= @Fim ");
            parameters.Add("Fim", fim.Value.ToDateTime(TimeOnly.MaxValue));
        }

        sql.Append("ORDER BY criado_em DESC");

        return await _session.Connection.QueryAsync<Ordem>(
            sql.ToString(),
            parameters,
            _session.Transaction);
    }

    /// <inheritdoc/>
    public async Task AdicionarAsync(Ordem ordem, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            INSERT INTO ordem (id, id_cliente, id_fundo, tipo_operacao, quantidade_cotas, data_agendamento, status, criado_em) 
            VALUES (@Id, @IdCliente, @IdFundo, @TipoOperacao, @QuantidadeCotas, @DataAgendamento, @Status, @CriadoEm);";

        var command = new CommandDefinition(
            sql,
            new
            {
                ordem.Id,
                ordem.IdCliente,
                ordem.IdFundo,
                TipoOperacao = ordem.TipoOperacao.ToString().ToUpper(),
                ordem.QuantidadeCotas,
                ordem.DataAgendamento,
                Status = ordem.Status.ToString().ToUpper(),
                ordem.CriadoEm
            },
            _session.Transaction,
            cancellationToken: cancellationToken);

        await _session.Connection.ExecuteAsync(command);
    }

    /// <inheritdoc/>
    public async Task AtualizarAsync(Ordem ordem, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            UPDATE ordem 
            SET status = @Status 
            WHERE id = @Id;";

        var command = new CommandDefinition(
            sql,
            new
            {
                Status = ordem.Status.ToString().ToUpper(),
                ordem.Id
            },
            _session.Transaction,
            cancellationToken: cancellationToken);

        await _session.Connection.ExecuteAsync(command);
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<Ordem>> ObterPendentesAteDataAsync(DateOnly dataBase, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT 
                id AS Id, 
                id_cliente AS IdCliente, 
                id_fundo AS IdFundo, 
                tipo_operacao AS TipoOperacao, 
                quantidade_cotas AS QuantidadeCotas, 
                data_agendamento AS DataAgendamento, 
                status AS Status, 
                criado_em AS CriadoEm 
            FROM ordem 
            WHERE status = 'PENDENTE' AND data_agendamento <= @DataBase 
            ORDER BY criado_em ASC";

        return await _session.Connection.QueryAsync<Ordem>(
            sql,
            new { DataBase = dataBase.ToDateTime(TimeOnly.MinValue) },
            _session.Transaction);
    }
}