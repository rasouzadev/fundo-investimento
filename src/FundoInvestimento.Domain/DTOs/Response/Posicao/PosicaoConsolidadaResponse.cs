using System.ComponentModel;

namespace FundoInvestimento.Domain.DTOs.Response.Posicao;

[Description("Resumo consolidado da carteira de investimentos do cliente.")]
public class PosicaoConsolidadaResponse
{
    [Description("Identificador único do cliente.")]
    public Guid IdCliente { get; set; }

    [Description("Lista de fundos em que o cliente possui cotas.")]
    public IEnumerable<PosicaoFundoResponse> Posicoes { get; set; } = new List<PosicaoFundoResponse>();

    [Description("Soma do saldo financeiro de todos os fundos da carteira.")]
    public decimal PatrimonioTotal => Posicoes.Sum(p => p.SaldoFinanceiro);
}