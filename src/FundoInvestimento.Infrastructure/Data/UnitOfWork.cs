using FundoInvestimento.Domain.Interfaces.Data;
using System.Diagnostics.CodeAnalysis;

namespace FundoInvestimento.Infrastructure.Data;

/// <summary>
/// Implementação do padrão Unit of Work.
/// </summary>
[ExcludeFromCodeCoverage]
public class UnitOfWork : IUnitOfWork
{
    private readonly DbSession _session;

    /// <summary>
    /// Inicializa o Unit of Work com a sessão de banco de dados atual.
    /// </summary>
    /// <param name="session">Sessão injetada contendo a conexão aberta.</param>
    public UnitOfWork(DbSession session)
    {
        _session = session;
    }

    /// <summary>
    /// Inicia uma nova transação associada à conexão da sessão.
    /// </summary>
    public void BeginTransaction()
    {
        _session.Transaction = _session.Connection.BeginTransaction();
    }

    /// <summary>
    /// Confirma todas as operações da transação no banco de dados.
    /// </summary>
    public void Commit()
    {
        _session.Transaction?.Commit();
        DisposeTransaction();
    }

    /// <summary>
    /// Desfaz as operações da transação em caso de erro.
    /// </summary>
    public void Rollback()
    {
        _session.Transaction?.Rollback();
        DisposeTransaction();
    }

    /// <summary>
    /// Libera a transação da memória quando o Unit of Work é descartado.
    /// </summary>
    public void Dispose()
    {
        DisposeTransaction();
        GC.SuppressFinalize(this);
    }

    private void DisposeTransaction()
    {
        _session.Transaction?.Dispose();
        _session.Transaction = null;
    }
}