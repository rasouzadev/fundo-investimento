using FundoInvestimento.Domain.Enums;
using System.ComponentModel;

namespace FundoInvestimento.Domain.DTOs.Response.Fundo;

/// <summary>
/// Representa os dados detalhados de um fundo de investimento para exibição no catálogo.
/// </summary>
[Description("Representa os detalhes de um fundo de investimento.")]
public class FundoResponse
{
    [Description("Identificador único do fundo.")]
    public Guid Id { get; set; }

    [Description("Nome de exibição do fundo.")]
    public string Nome { get; set; } = string.Empty;

    [Description("Horário limite (cut-off) para que aplicações e resgates sejam processados no mesmo dia útil.")]
    public TimeOnly HorarioCorte { get; set; }

    [Description("Valor unitário atual da cota do fundo.")]
    public decimal ValorCota { get; set; }

    [Description("Valor financeiro mínimo exigido para o primeiro aporte.")]
    public decimal ValorMinimoAporte { get; set; }

    [Description("Valor financeiro mínimo que deve permanecer no fundo após um resgate parcial.")]
    public decimal ValorMinimoPermanencia { get; set; }

    [Description("Status atual de captação do fundo (ABERTO ou FECHADO para novos aportes).")]
    public StatusCaptacao StatusCaptacao { get; set; }
}