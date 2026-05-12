using FundoInvestimento.Domain.Entities;
using FundoInvestimento.Domain.Enums;

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

    /// <summary>
    /// Lista os fundos de investimento disponíveis no catálogo, com filtro opcional por status.
    /// </summary>
    /// <param name="status">Filtro opcional. Se preenchido, retorna apenas fundos com o status especificado.</param>
    /// <param name="cancellationToken">Token para cancelamento assíncrono da operação.</param>
    /// <returns>Uma coleção de entidades <see cref="Fundo"/> ordenadas alfabeticamente.</returns>
    Task<IEnumerable<Fundo>> ObterTodosAsync(StatusCaptacao? status = null, CancellationToken cancellationToken = default);
}