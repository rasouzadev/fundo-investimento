using System.Diagnostics.CodeAnalysis;

namespace FundoInvestimento.Libs.Utils;

/// <summary>
/// Representa um erro detalhado de uma operação, projetado para facilitar 
/// o mapeamento direto para respostas HTTP (ProblemDetails).
/// </summary>
[ExcludeFromCodeCoverage]
public class CustomError
{
    /// <summary>
    /// Código interno do erro para identificação sistêmica ou traduções no front-end (ex: "SALDO_INSUFICIENTE").
    /// </summary>
    public string Code { get; }

    /// <summary>
    /// Mensagem descritiva e amigável sobre o motivo do erro.
    /// </summary>
    public string Message { get; }

    /// <summary>
    /// Código de status HTTP sugerido para este erro (ex: 400, 404, 422).
    /// </summary>
    public int StatusCode { get; }

    /// <summary>
    /// Objeto opcional contendo dados adicionais relevantes para o contexto do erro.
    /// </summary>
    public object? Data { get; }

    /// <summary>
    /// Inicializa uma nova instância da classe <see cref="CustomError"/>.
    /// </summary>
    /// <param name="code">Código interno do erro.</param>
    /// <param name="message">Mensagem descritiva do erro.</param>
    /// <param name="statusCode">Código de status HTTP associado (padrão é 400 - Bad Request).</param>
    /// <param name="data">Dados extras opcionais.</param>
    public CustomError(string code, string message, int statusCode = 400, object? data = null)
    {
        Code = code;
        Message = message;
        StatusCode = statusCode;
        Data = data;
    }
}