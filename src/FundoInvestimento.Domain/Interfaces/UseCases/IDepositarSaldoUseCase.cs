using FundoInvestimento.Domain.DTOs.Requests.Cliente;
using FundoInvestimento.Domain.DTOs.Response.Cliente;
using FundoInvestimento.Libs.Utils;

namespace FundoInvestimento.Domain.Interfaces.UseCases;

/// <summary>
/// Contrato para o caso de uso responsável por realizar depósitos na conta do cliente.
/// </summary>
public interface IDepositarSaldoUseCase
{
    /// <summary>
    /// Credita o valor informado no saldo disponível do cliente.
    /// </summary>
    /// <param name="idCliente">Identificador único do cliente.</param>
    /// <param name="request">Dados do depósito contendo o valor.</param>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    /// <returns>O novo saldo atualizado ou erro caso o cliente não seja encontrado.</returns>
    Task<Result<SaldoResponse>> ExecuteAsync(Guid idCliente, DepositoRequest request, CancellationToken cancellationToken = default);
}