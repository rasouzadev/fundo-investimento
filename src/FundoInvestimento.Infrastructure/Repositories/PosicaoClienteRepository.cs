using Dapper;
using FundoInvestimento.Domain.Entities;
using FundoInvestimento.Domain.Interfaces.Repositories;
using FundoInvestimento.Infrastructure.Data;
using System.Diagnostics.CodeAnalysis;

namespace FundoInvestimento.Infrastructure.Repositories;

/// <summary>
/// Implementação do repositório da carteira consolidada (PosicaoCliente) utilizando Dapper.
/// </summary>
[ExcludeFromCodeCoverage]
public class PosicaoClienteRepository : IPosicaoClienteRepository
{
    private readonly DbSession _session;

    /// <summary>
    /// Inicializa o repositório com a sessão de banco de dados.
    /// </summary>
    /// <param name="session">Sessão compartilhada gerenciada pela injeção de dependência.</param>
    public PosicaoClienteRepository(DbSession session)
    {
        _session = session;
    }

    /// <inheritdoc/>
    public async Task<PosicaoCliente?> ObterPorIdAsync(Guid idCliente, Guid idFundo, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT id_cliente AS IdCliente, 
                   id_fundo AS IdFundo, 
                   quantidade_cotas AS QuantidadeCotas
            FROM posicao_cliente 
            WHERE id_cliente = @IdCliente 
              AND id_fundo = @IdFundo
            FOR UPDATE;";

        var command = new CommandDefinition(
            sql,
            new { IdCliente = idCliente, IdFundo = idFundo },
            _session.Transaction,
            cancellationToken: cancellationToken);

        return await _session.Connection.QuerySingleOrDefaultAsync<PosicaoCliente>(command);
    }

    /// <inheritdoc/>
    public async Task AdicionarAsync(PosicaoCliente posicao, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            INSERT INTO posicao_cliente (id_cliente, id_fundo, quantidade_cotas) 
            VALUES (@IdCliente, @IdFundo, @QuantidadeCotas);";

        var command = new CommandDefinition(
            sql,
            new
            {
                posicao.IdCliente,
                posicao.IdFundo,
                posicao.QuantidadeCotas
            },
            _session.Transaction,
            cancellationToken: cancellationToken);

        await _session.Connection.ExecuteAsync(command);
    }

    /// <inheritdoc/>
    public async Task AtualizarAsync(PosicaoCliente posicao, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            UPDATE posicao_cliente 
            SET quantidade_cotas = @QuantidadeCotas 
            WHERE id_cliente = @IdCliente 
              AND id_fundo = @IdFundo;";

        var command = new CommandDefinition(
            sql,
            new
            {
                posicao.QuantidadeCotas,
                posicao.IdCliente,
                posicao.IdFundo
            },
            _session.Transaction,
            cancellationToken: cancellationToken);

        await _session.Connection.ExecuteAsync(command);
    }
}