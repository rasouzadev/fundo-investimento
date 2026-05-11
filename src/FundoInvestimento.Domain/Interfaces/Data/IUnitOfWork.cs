namespace FundoInvestimento.Domain.Interfaces.Data;

/// <summary>
/// Contrato para o padrão Unit of Work, responsável por gerenciar transações atômicas no banco de dados.
/// </summary>
public interface IUnitOfWork : IDisposable
{
    /// <summary>
    /// Inicia uma nova transação no banco de dados.
    /// </summary>
    void BeginTransaction();

    /// <summary>
    /// Confirma todas as operações realizadas dentro da transação atual.
    /// </summary>
    void Commit();

    /// <summary>
    /// Desfaz todas as operações realizadas dentro da transação atual em caso de falha.
    /// </summary>
    void Rollback();
}