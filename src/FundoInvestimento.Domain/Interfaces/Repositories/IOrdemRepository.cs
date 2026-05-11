using FundoInvestimento.Domain.Entities;

namespace FundoInvestimento.Domain.Interfaces.Repositories;

/// <summary>
/// Contrato para operações do histórico de transações (Ordens).
/// </summary>
public interface IOrdemRepository
{
    /// <summary>
    /// Busca uma ordem específica pelo seu id.
    /// </summary>
    /// <param name="id">O identificador (UUID) da ordem.</param>
    /// <param name="cancellationToken">Token para cancelamento assíncrono da operação.</param>
    /// <returns>Uma <see cref="Task"/> contendo a <see cref="Ordem"/> encontrada, ou nulo se não existir.</returns>
    Task<Ordem?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Lista todo o histórico de ordens associadas a um determinado cliente.
    /// </summary>
    /// <param name="idCliente">O id do cliente.</param>
    /// <param name="cancellationToken">Token para cancelamento assíncrono da operação.</param>
    /// <returns>Uma <see cref="Task"/> contendo uma coleção enumerável das ordens do cliente.</returns>
    Task<IEnumerable<Ordem>> ObterPorClienteIdAsync(Guid idCliente, CancellationToken cancellationToken = default);

    /// <summary>
    /// Insere uma nova solicitação de ordem (imediata ou agendada) no banco de dados.
    /// </summary>
    /// <param name="ordem">A entidade ordem a ser persistida.</param>
    /// <param name="cancellationToken">Token para cancelamento assíncrono da operação.</param>
    /// <returns>Uma <see cref="Task"/> que representa a conclusão da operação de inserção.</returns>
    Task AdicionarAsync(Ordem ordem, CancellationToken cancellationToken = default);

    /// <summary>
    /// Atualiza uma ordem existente
    /// </summary>
    /// <param name="ordem">A entidade ordem com as modificações a serem salvas.</param>
    /// <param name="cancellationToken">Token para cancelamento assíncrono da operação.</param>
    /// <returns>Uma <see cref="Task"/> que representa a conclusão da operação de atualização.</returns>
    Task AtualizarAsync(Ordem ordem, CancellationToken cancellationToken = default);
}