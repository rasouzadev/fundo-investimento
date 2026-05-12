using FundoInvestimento.Domain.Enums;
using System.ComponentModel;

namespace FundoInvestimento.Domain.DTOs.Response.Ordem;

[Description("Representa o resultado do processamento de uma ordem.")]
public class OrdemResponse
{
    [Description("Identificador único gerado para a ordem.")]
    public Guid Id { get; set; }

    [Description("Tipo da operação que foi processada.")]
    public TipoOperacao TipoOperacao { get; set; }

    [Description("Status atual da ordem após o processamento.")]
    public StatusOrdem Status { get; set; }

    [Description("Data e hora exata em que a ordem foi registrada no sistema.")]
    public DateTimeOffset CriadoEm { get; set; }
}