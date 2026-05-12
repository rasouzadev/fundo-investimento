using System.ComponentModel;

namespace FundoInvestimento.Domain.DTOs.Requests.Ordem;

/// <summary>
/// Objeto de transferência para filtragem do histórico de ordens.
/// </summary>
[Description("Parâmetros para filtragem do histórico de ordens.")]
public class ListarOrdensRequest
{
    /// <summary>
    /// Identificador único do cliente.
    /// </summary>
    [Description("Identificador único do cliente para filtrar o histórico.")]
    public Guid? IdCliente { get; set; }

    /// <summary>
    /// Identificador único do fundo de investimento.
    /// </summary>
    [Description("Identificador único do fundo para filtrar o histórico.")]
    public Guid? IdFundo { get; set; }

    /// <summary>
    /// Data inicial do período de busca.
    /// </summary>
    [Description("Data de início do período (ISO 8601).")]
    public DateOnly? DataInicio { get; set; }

    /// <summary>
    /// Data final do período de busca.
    /// </summary>
    [Description("Data de fim do período (ISO 8601).")]
    public DateOnly? DataFim { get; set; }
}