using FundoInvestimento.Domain.DTOs.Requests.Ordem;
using FundoInvestimento.Domain.DTOs.Response.Ordem;
using FundoInvestimento.Libs.Utils;

namespace FundoInvestimento.Domain.Interfaces.UseCases;

/// <summary>
/// Contrato para o caso de uso responsável por registrar agendamentos de ordens de investimento.
/// </summary>
public interface IAgendarOrdemUseCase
{
    /// <summary>
    /// Executa a validação estrutural e registra uma nova ordem com status PENDENTE para execução em data futura.
    /// </summary>
    /// <param name="request">Objeto contendo os dados do agendamento (ID do Cliente, ID do Fundo, Operação, Quantidade e Data).</param>
    /// <param name="cancellationToken">Token para cancelamento assíncrono da operação.</param>
    /// <returns>
    /// Um <see cref="Result{T}"/> contendo os dados da ordem criada (<see cref="OrdemResponse"/>) 
    /// ou os detalhes do erro de negócio em caso de falha nas validações.
    /// </returns>
    Task<Result<OrdemResponse>> ExecuteAsync(AgendarOrdemRequest request, CancellationToken cancellationToken = default);
}