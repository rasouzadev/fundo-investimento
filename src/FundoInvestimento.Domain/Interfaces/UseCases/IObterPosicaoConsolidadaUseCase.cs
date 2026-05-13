using FundoInvestimento.Domain.DTOs.Response.Posicao;
using FundoInvestimento.Libs.Utils;

namespace FundoInvestimento.Domain.Interfaces.UseCases;

/// <summary>
/// Contrato para o caso de uso responsável por consolidar a carteira de investimentos do cliente.
/// </summary>
public interface IObterPosicaoConsolidadaUseCase
{
    /// <summary>
    /// Executa a busca das posições ativas de um cliente e calcula o seu saldo financeiro total e por fundo.
    /// </summary>
    /// <param name="idCliente">O identificador único do cliente para consulta da carteira.</param>
    /// <param name="cancellationToken">Token para cancelamento assíncrono da operação.</param>
    /// <returns>
    /// Um <see cref="Result{T}"/> contendo a <see cref="PosicaoConsolidadaResponse"/> com os detalhes 
    /// de cada fundo investido e a somatória do patrimônio total.
    /// </returns>
    Task<Result<PosicaoConsolidadaResponse>> ExecuteAsync(Guid idCliente, CancellationToken cancellationToken = default);
}