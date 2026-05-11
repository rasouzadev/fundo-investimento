using FundoInvestimento.Domain.Entities;

namespace FundoInvestimento.Domain.Interfaces.Repositories;

/// <summary>
/// Contrato para operações da entidade Cliente no banco de dados.
/// </summary>
public interface IClienteRepository
{
    /// <summary>
    /// Busca um cliente na base de dados pelo seu id.
    /// </summary>
    /// <param name="id">O identificador (UUID) do cliente.</param>
    /// <param name="cancellationToken">Token para cancelamento assíncrono da operação.</param>
    /// <returns>Uma <see cref="Task"/> que representa a operação assíncrona, contendo o <see cref="Cliente"/> encontrado ou nulo se não existir.</returns>
    Task<Cliente?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Atualiza o estado de um cliente existente na base de dados (ex: alteração de saldo após débito/crédito).
    /// </summary>
    /// <param name="cliente">A entidade cliente com os dados atualizados.</param>
    /// <param name="cancellationToken">Token para cancelamento assíncrono da operação.</param>
    /// <returns>Uma <see cref="Task"/> que representa a conclusão da operação de atualização.</returns>
    Task AtualizarAsync(Cliente cliente, CancellationToken cancellationToken = default);
}