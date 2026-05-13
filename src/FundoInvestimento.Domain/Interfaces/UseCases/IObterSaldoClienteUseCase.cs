using FundoInvestimento.Domain.DTOs.Response.Cliente;
using FundoInvestimento.Libs.Utils;

namespace FundoInvestimento.Domain.Interfaces.UseCases;

/// <summary>
/// Contrato para o caso de uso responsável por consultar o saldo livre do cliente.
/// </summary>
public interface IObterSaldoClienteUseCase
{
    /// <summary>
    /// Consulta as informações do cliente e retorna apenas o saldo disponível.
    /// </summary>
    /// <param name="idCliente">Identificador único do cliente.</param>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    /// <returns>O saldo do cliente ou erro caso não seja encontrado.</returns>
    Task<Result<SaldoResponse>> ExecuteAsync(Guid idCliente, CancellationToken cancellationToken = default);
}