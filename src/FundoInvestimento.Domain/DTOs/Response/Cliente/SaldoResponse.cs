using System.ComponentModel;

namespace FundoInvestimento.Domain.DTOs.Response.Cliente;

/// <summary>
/// Representa o saldo financeiro disponível na conta do cliente.
/// </summary>
[Description("Representa o saldo disponível do cliente para novos investimentos.")]
public class SaldoResponse
{
    [Description("Identificador único do cliente.")]
    public Guid IdCliente { get; set; }

    [Description("Valor monetário livre em conta corrente.")]
    public decimal SaldoDisponivel { get; set; }
}