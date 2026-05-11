using FundoInvestimento.Domain.Enums;

namespace FundoInvestimento.Domain.DTOs.Response.Ordem;

/// <summary>
/// Representa o resultado do processamento de uma ordem.
/// </summary>
public class OrdemResponse
{
    /// <summary>
    /// Identificador único gerado para a ordem.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Tipo da operação que foi processada.
    /// </summary>
    public TipoOperacao TipoOperacao { get; set; }

    /// <summary>
    /// Status atual da ordem após o processamento.
    /// </summary>
    public StatusOrdem Status { get; set; }

    /// <summary>
    /// Data e hora exata em que a ordem foi registrada no sistema.
    /// </summary>
    public DateTimeOffset CriadoEm { get; set; }
}