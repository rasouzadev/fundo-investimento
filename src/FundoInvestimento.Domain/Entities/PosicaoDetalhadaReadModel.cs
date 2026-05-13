namespace FundoInvestimento.Domain.Entities;

/// <summary>
/// Modelo de leitura para mapear o JOIN entre posicao_cliente e fundo.
/// </summary>
public class PosicaoDetalhadaReadModel
{
    public Guid IdFundo { get; set; }
    public string NomeFundo { get; set; } = string.Empty;
    public int QuantidadeCotas { get; set; }
    public decimal ValorCota { get; set; }
}