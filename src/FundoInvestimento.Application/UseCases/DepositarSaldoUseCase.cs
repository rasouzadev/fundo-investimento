using FundoInvestimento.Domain.DTOs.Requests.Cliente;
using FundoInvestimento.Domain.DTOs.Response.Cliente;
using FundoInvestimento.Domain.Interfaces.Repositories;
using FundoInvestimento.Domain.Interfaces.UseCases;
using FundoInvestimento.Libs.Utils;

namespace FundoInvestimento.Application.UseCases;

/// <summary>
/// Caso de uso para processamento de entrada de capital (Cash-In) na conta do cliente.
/// </summary>
public class DepositarSaldoUseCase : IDepositarSaldoUseCase
{
    private readonly IClienteRepository _clienteRepository;

    public DepositarSaldoUseCase(IClienteRepository clienteRepository)
    {
        _clienteRepository = clienteRepository;
    }

    /// <inheritdoc />
    public async Task<Result<SaldoResponse>> ExecuteAsync(Guid idCliente, DepositoRequest request, CancellationToken cancellationToken = default)
    {
        var cliente = await _clienteRepository.ObterPorIdAsync(idCliente, cancellationToken);

        if (cliente == null)
        {
            return Result<SaldoResponse>.Failure(new CustomError(
                code: "CLIENTE_NAO_ENCONTRADO",
                message: "O cliente informado não existe na base de dados.",
                statusCode: 404));
        }

        cliente.CreditarSaldo(request.Valor);

        await _clienteRepository.AtualizarAsync(cliente, cancellationToken);

        var response = new SaldoResponse
        {
            IdCliente = cliente.Id,
            SaldoDisponivel = cliente.SaldoDisponivel
        };

        return Result<SaldoResponse>.Success(response);
    }
}