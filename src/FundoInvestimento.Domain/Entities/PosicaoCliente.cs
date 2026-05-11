using FundoInvestimento.Libs.Utils;

namespace FundoInvestimento.Domain.Entities;

/// <summary>
/// Representa a carteira consolidada de um cliente em um fundo de investimento específico.
/// </summary>
public class PosicaoCliente
{
    /// <summary>
    /// Identificador do cliente detentor da posição.
    /// </summary>
    public Guid IdCliente { get; init; }

    /// <summary>
    /// Identificador do fundo de investimento no qual o cliente possui cotas.
    /// </summary>
    public Guid IdFundo { get; init; }

    /// <summary>
    /// Quantidade total de cotas que o cliente detém neste fundo no momento atual.
    /// </summary>
    public int QuantidadeCotas { get; private set; }

    /// <summary>
    /// Construtor vazio exigido por ferramentas de ORM.
    /// </summary>
    protected PosicaoCliente() { }

    /// <summary>
    /// Inicializa a posição consolidada de um cliente em um fundo.
    /// </summary>
    /// <param name="idCliente">Identificador único do cliente.</param>
    /// <param name="idFundo">Identificador único do fundo de investimento.</param>
    /// <param name="quantidadeInicial">Quantidade inicial de cotas (opcional, padrão zero).</param>
    public PosicaoCliente(Guid idCliente, Guid idFundo, int quantidadeInicial = 0)
    {
        IdCliente = idCliente;
        IdFundo = idFundo;
        QuantidadeCotas = quantidadeInicial;
    }

    /// <summary>
    /// Verifica se o cliente possui a quantidade mínima de cotas solicitada para uma operação.
    /// </summary>
    /// <param name="quantidadeNecessaria">A quantidade de cotas exigida (ex: para um resgate).</param>
    /// <returns>Retorna <c>true</c> se o cliente possuir cotas iguais ou superiores à solicitada; caso contrário, <c>false</c>.</returns>
    public bool TemCotasSuficientes(int quantidadeNecessaria)
    {
        return QuantidadeCotas >= quantidadeNecessaria;
    }

    /// <summary>
    /// Adiciona novas cotas à carteira do cliente, geralmente como consequência da liquidação de uma ordem de aporte.
    /// </summary>
    /// <param name="quantidade">Número de cotas a ser adicionado.</param>
    /// <returns>Um <see cref="Result"/> indicando sucesso ou detalhando a falha na validação da quantidade.</returns>
    public Result AdicionarCotas(int quantidade)
    {
        if (quantidade <= 0)
        {
            return Result.Failure(new CustomError(
                code: "QUANTIDADE_COTAS_INVALIDA",
                message: "A quantidade de cotas a adicionar na posição deve ser maior que zero.",
                statusCode: 422));
        }

        QuantidadeCotas += quantidade;
        return Result.Success();
    }

    /// <summary>
    /// Deduz cotas da carteira do cliente, geralmente como consequência de uma ordem de resgate.
    /// </summary>
    /// <param name="quantidade">Número de cotas a ser retirado.</param>
    /// <returns>Um <see cref="Result"/> indicando sucesso ou detalhando a falha por insuficiência de cotas.</returns>
    public Result RemoverCotas(int quantidade)
    {
        if (!TemCotasSuficientes(quantidade))
        {
            return Result.Failure(new CustomError(
                code: "COTAS_INSUFICIENTES",
                message: "O cliente não possui a quantidade de cotas necessária para realizar este resgate.",
                statusCode: 422));
        }

        QuantidadeCotas -= quantidade;
        return Result.Success();
    }
}