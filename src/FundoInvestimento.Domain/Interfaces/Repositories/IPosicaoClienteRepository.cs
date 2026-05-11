using FundoInvestimento.Domain.Entities;

namespace FundoInvestimento.Domain.Interfaces.Repositories;

/// <summary>
/// Contrato para operações da carteira consolidada (Posição) do cliente.
/// </summary>
public interface IPosicaoClienteRepository
{
    /// <summary>
    /// Busca a posição consolidada de um cliente em um fundo de investimento específico.
    /// </summary>
    /// <param name="idCliente">O id do cliente.</param>
    /// <param name="idFundo">O id do fundo de investimento.</param>
    /// <param name="cancellationToken">Token para cancelamento assíncrono da operação.</param>
    /// <returns>Uma <see cref="Task"/> contendo a <see cref="PosicaoCliente"/> se o cliente já tiver operado neste fundo, ou nulo caso a posição ainda não exista.</returns>
    Task<PosicaoCliente?> ObterPorIdAsync(Guid idCliente, Guid idFundo, CancellationToken cancellationToken = default);

    /// <summary>
    /// Insere uma nova posição no banco de dados para a primeira operação do cliente neste fundo.
    /// </summary>
    /// <param name="posicao">A nova entidade de posição a ser registrada.</param>
    /// <param name="cancellationToken">Token para cancelamento assíncrono da operação.</param>
    /// <returns>Uma <see cref="Task"/> que representa a conclusão da operação de inserção.</returns>
    Task AdicionarAsync(PosicaoCliente posicao, CancellationToken cancellationToken = default);

    /// <summary>
    /// Atualiza a quantidade de cotas de uma posição previamente existente no banco de dados.
    /// </summary>
    /// <param name="posicao">A entidade posição contendo a nova quantidade de cotas.</param>
    /// <param name="cancellationToken">Token para cancelamento assíncrono da operação.</param>
    /// <returns>Uma <see cref="Task"/> que representa a conclusão da operação de atualização.</returns>
    Task AtualizarAsync(PosicaoCliente posicao, CancellationToken cancellationToken = default);
}