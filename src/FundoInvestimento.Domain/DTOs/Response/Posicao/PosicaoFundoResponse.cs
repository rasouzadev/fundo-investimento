using System.ComponentModel;

namespace FundoInvestimento.Domain.DTOs.Response.Posicao;

[Description("Detalhes da posição do cliente em um fundo específico.")]
public class PosicaoFundoResponse
{
    [Description("Identificador único do fundo.")]
    public Guid IdFundo { get; set; }

    [Description("Nome do fundo de investimento.")]
    public string NomeFundo { get; set; } = string.Empty;

    [Description("Quantidade de cotas que o cliente possui neste fundo.")]
    public int QuantidadeCotas { get; set; }

    [Description("Valor unitário atual da cota.")]
    public decimal ValorCotaAtual { get; set; }

    [Description("Saldo financeiro total neste fundo (Quantidade * Valor da Cota).")]
    public decimal SaldoFinanceiro => Math.Round(QuantidadeCotas * ValorCotaAtual, 2);
}