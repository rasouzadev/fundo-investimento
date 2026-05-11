using FundoInvestimento.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace FundoInvestimento.Domain.DTOs.Requests.Ordem;

/// <summary>
/// Representa a solicitação de uma ordem de investimento.
/// </summary>
public class OrdemRequest
{
    /// <summary>
    /// Identificador único do cliente que está realizando a operação.
    /// </summary>
    [Required]
    public Guid IdCliente { get; set; }

    /// <summary>
    /// Identificador único do fundo alvo da operação.
    /// </summary>
    [Required]
    public Guid IdFundo { get; set; }

    /// <summary>
    /// Tipo da operação desejada (APORTE ou RESGATE).
    /// </summary>
    [Required]
    public TipoOperacao TipoOperacao { get; set; }

    /// <summary>
    /// Quantidade de cotas a serem negociadas. Deve ser maior que zero.
    /// </summary>
    [Required]
    [Range(1, int.MaxValue)]
    public int QuantidadeCotas { get; set; }

    /// <summary>
    /// Data futura desejada para execução. Envie nulo para execução imediata (D+0).
    /// </summary>
    public DateOnly? DataAgendamento { get; set; }
}