using FundoInvestimento.Domain.DTOs.Requests.Ordem;
using FundoInvestimento.Domain.DTOs.Response.Ordem;
using FundoInvestimento.Libs.Utils;

namespace FundoInvestimento.Domain.Interfaces.UseCases;

/// <summary>
/// Contrato para o caso de uso responsável por processar aportes e resgates de execução imediata (D+0).
/// </summary>
public interface ICriarOrdemImediataUseCase
{
    /// <summary>
    /// Executa o fluxo de criação e liquidação de uma ordem imediata, aplicando as validações de negócio 
    /// e garantindo a consistência transacional (ACID).
    /// </summary>
    /// <param name="request">Os dados da solicitação de ordem enviados pelo cliente.</param>
    /// <param name="cancellationToken">Token para cancelamento assíncrono da operação.</param>
    /// <returns>Um <see cref="Result{OrdemResponse}"/> indicando o sucesso da transação ou os detalhes do erro de negócio.</returns>
    Task<Result<OrdemResponse>> ExecuteAsync(OrdemRequest request, CancellationToken cancellationToken = default);
}