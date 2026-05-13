using FundoInvestimento.Domain.DTOs.Response.Cliente;
using FundoInvestimento.Domain.Interfaces.Repositories;
using FundoInvestimento.Domain.Interfaces.UseCases;
using FundoInvestimento.Libs.Utils;

namespace FundoInvestimento.Application.UseCases;

/// <summary>
/// Caso de uso para recuperação do saldo em conta do cliente.
/// </summary>
public class ObterSaldoClienteUseCase : IObterSaldoClienteUseCase
{
    private readonly IClienteRepository _clienteRepository;

    public ObterSaldoClienteUseCase(IClienteRepository clienteRepository)
    {
        _clienteRepository = clienteRepository;
    }

    /// <inheritdoc />
    public async Task<Result<SaldoResponse>> ExecuteAsync(Guid idCliente, CancellationToken cancellationToken = default)
    {
        var cliente = await _clienteRepository.ObterPorIdAsync(idCliente, cancellationToken);

        if (cliente == null)
        {
            return Result<SaldoResponse>.Failure(new CustomError(
                code: "CLIENTE_NAO_ENCONTRADO",
                message: "O cliente informado não existe na base de dados.",
                statusCode: 404));
        }

        var response = new SaldoResponse
        {
            IdCliente = cliente.Id,
            SaldoDisponivel = cliente.SaldoDisponivel
        };

        return Result<SaldoResponse>.Success(response);
    }
}