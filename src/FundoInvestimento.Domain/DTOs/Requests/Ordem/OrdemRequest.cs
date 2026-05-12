using FundoInvestimento.Domain.Enums;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace FundoInvestimento.Domain.DTOs.Requests.Ordem;

[Description("Representa a solicitação de uma ordem de investimento.")]
public class OrdemRequest
{
    [Required]
    [Description("Identificador único do cliente que está realizando a operação.")]
    public Guid IdCliente { get; set; }

    [Required]
    [Description("Identificador único do fundo alvo da operação.")]
    public Guid IdFundo { get; set; }

    [Required]
    [Description("Tipo da operação desejada (APORTE ou RESGATE).")]
    public TipoOperacao TipoOperacao { get; set; }

    [Required]
    [Range(1, int.MaxValue)]
    [Description("Quantidade de cotas a serem negociadas. Deve ser maior que zero.")]
    public int QuantidadeCotas { get; set; }
}