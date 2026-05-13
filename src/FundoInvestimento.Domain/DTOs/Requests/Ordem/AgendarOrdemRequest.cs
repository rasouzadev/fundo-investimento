using FundoInvestimento.Domain.Enums;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace FundoInvestimento.Domain.DTOs.Requests.Ordem;

/// <summary>
/// Objeto de transferência para solicitação de um agendamento de ordem.
/// </summary>
[Description("Parâmetros para agendar um aporte ou resgate em uma data futura.")]
public class AgendarOrdemRequest
{
    [Required]
    [Description("Identificador único do cliente.")]
    public Guid IdCliente { get; set; }

    [Required]
    [Description("Identificador único do fundo de investimento.")]
    public Guid IdFundo { get; set; }

    [Required]
    [Description("Tipo da operação (APORTE ou RESGATE).")]
    public TipoOperacao TipoOperacao { get; set; }

    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "A quantidade de cotas deve ser maior que zero.")]
    [Description("Quantidade de cotas desejadas para a operação.")]
    public int QuantidadeCotas { get; set; }

    [Required]
    [Description("Data futura em que a ordem deverá ser executada.")]
    public DateOnly DataAgendamento { get; set; }
}