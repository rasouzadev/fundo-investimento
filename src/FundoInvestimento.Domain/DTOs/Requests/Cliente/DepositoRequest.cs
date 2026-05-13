using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace FundoInvestimento.Domain.DTOs.Requests.Cliente;

/// <summary>
/// Objeto de transferência para solicitação de depósito em conta corrente.
/// </summary>
[Description("Parâmetros para realizar um depósito na conta do cliente.")]
public class DepositoRequest
{
    [Required(ErrorMessage = "O valor do depósito é obrigatório.")]
    [Range(0.01, double.MaxValue, ErrorMessage = "O valor do depósito deve ser maior que zero.")]
    [Description("Valor financeiro a ser creditado na conta.")]
    public decimal Valor { get; set; }
}