using FundoInvestimento.Domain.Entities;

namespace FundoInvestimento.Domain.Interfaces.Repositories;

/// <summary>
/// Contrato para operações da entidade Fundo no catálogo.
/// </summary>
public interface IFundoRepository
{
    /// <summary>
    /// Busca um fundo de investimento pelo seu id.
    /// </summary>
    /// <param name="id">O identificador (UUID) do fundo.</param>
    /// <param name="cancellationToken">Token para cancelamento assíncrono da operação.</param>
    /// <returns>Uma <see cref="Task"/> que representa a operação assíncrona, contendo o <see cref="Fundo"/> encontrado ou nulo se não existir.</returns>
    Task<Fundo?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default);
}