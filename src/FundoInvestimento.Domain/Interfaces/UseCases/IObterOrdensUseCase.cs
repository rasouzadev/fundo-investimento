using FundoInvestimento.Domain.DTOs.Requests.Ordem;
using FundoInvestimento.Domain.DTOs.Response.Ordem;
using FundoInvestimento.Libs.Utils;

namespace FundoInvestimento.Domain.Interfaces.UseCases;

/// <summary>
/// Contrato para o caso de uso responsável por obter o histórico de ordens.
/// </summary>
public interface IObterOrdensUseCase
{
    /// <summary>
    /// Executa a busca de ordens aplicando as regras de negócio de leitura e os filtros fornecidos.
    /// </summary>
    /// <param name="request">O DTO <see cref="ListarOrdensRequest"/> contendo os parâmetros de filtro (ID do Cliente, ID do Fundo e Período).</param>
    /// <param name="cancellationToken">Token para cancelamento assíncrono da operação.</param>
    /// <returns>
    /// Um <see cref="Result{T}"/> contendo a coleção de <see cref="OrdemResponse"/> formatadas para exibição, 
    /// ou os detalhes de um erro de negócio caso a solicitação seja inválida.
    /// </returns>
    Task<Result<IEnumerable<OrdemResponse>>> ExecuteAsync(ListarOrdensRequest request, CancellationToken cancellationToken = default);
}