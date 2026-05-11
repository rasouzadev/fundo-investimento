using FundoInvestimento.Domain.Enums;
using FundoInvestimento.Libs.Utils;

namespace FundoInvestimento.Domain.Entities;

/// <summary>
/// Representa uma solicitação de transação (Aporte ou Resgate) feita por um cliente em um fundo.
/// </summary>
public class Ordem
{
    /// <summary>
    /// Identificador único da ordem.
    /// </summary>
    public Guid Id { get; init; }

    /// <summary>
    /// Identificador do cliente solicitante.
    /// </summary>
    public Guid IdCliente { get; private set; }

    /// <summary>
    /// Identificador do fundo alvo da operação.
    /// </summary>
    public Guid IdFundo { get; private set; }

    /// <summary>
    /// Tipo da operação (Aporte ou Resgate).
    /// </summary>
    public TipoOperacao TipoOperacao { get; private set; }

    /// <summary>
    /// Quantidade de cotas envolvidas na transação.
    /// </summary>
    public int QuantidadeCotas { get; private set; }

    /// <summary>
    /// Data programada para a execução da ordem (nulo para ordens imediatas).
    /// </summary>
    public DateOnly? DataAgendamento { get; private set; }

    /// <summary>
    /// Status atual de processamento da ordem.
    /// </summary>
    public StatusOrdem Status { get; private set; }

    /// <summary>
    /// Data e hora exata em que a ordem foi registrada no sistema.
    /// </summary>
    public DateTimeOffset CriadoEm { get; private set; }

    /// <summary>
    /// Construtor vazio exigido pelo Dapper para materialização do objeto do banco de dados.
    /// </summary>
    protected Ordem() { }

    /// <summary>
    /// Construtor privado para inicialização das propriedades da ordem. Deve ser chamado pelas Factories.
    /// </summary>
    /// <param name="idCliente">Identificador único do cliente.</param>
    /// <param name="idFundo">Identificador único do fundo.</param>
    /// <param name="tipoOperacao">Tipo da operação a ser realizada (Aporte ou Resgate).</param>
    /// <param name="quantidadeCotas">Quantidade de cotas alvo da transação.</param>
    /// <param name="dataAgendamento">Data futura para execução, ou nulo para execução imediata.</param>
    private Ordem(Guid idCliente, Guid idFundo, TipoOperacao tipoOperacao, int quantidadeCotas, DateOnly? dataAgendamento)
    {
        Id = Guid.CreateVersion7();
        IdCliente = idCliente;
        IdFundo = idFundo;
        TipoOperacao = tipoOperacao;
        QuantidadeCotas = quantidadeCotas;
        DataAgendamento = dataAgendamento;
        Status = StatusOrdem.PENDENTE;
        CriadoEm = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Cria uma solicitação de ordem para execução imediata (D+0).
    /// </summary>
    /// <param name="idCliente">Identificador único do cliente.</param>
    /// <param name="idFundo">Identificador único do fundo de destino.</param>
    /// <param name="tipoOperacao">O tipo de operação a ser realizada (Aporte ou Resgate).</param>
    /// <param name="quantidadeCotas">A quantidade de cotas desejada (deve ser maior que zero).</param>
    /// <returns>Um <see cref="Result{Ordem}"/> contendo a instância da ordem se os dados forem válidos, ou um erro de validação.</returns>
    public static Result<Ordem> CriarImediata(Guid idCliente, Guid idFundo, TipoOperacao tipoOperacao, int quantidadeCotas)
    {
        if (quantidadeCotas <= 0)
        {
            return Result<Ordem>.Failure(new CustomError(
                code: "QUANTIDADE_COTAS_INVALIDA",
                message: "A quantidade de cotas para a ordem deve ser maior que zero.",
                statusCode: 422));
        }

        var ordem = new Ordem(idCliente, idFundo, tipoOperacao, quantidadeCotas, null);
        return Result<Ordem>.Success(ordem);
    }

    /// <summary>
    /// Cria uma solicitação de ordem agendada para uma data futura.
    /// </summary>
    /// <param name="idCliente">Identificador único do cliente.</param>
    /// <param name="idFundo">Identificador único do fundo de destino.</param>
    /// <param name="tipoOperacao">O tipo de operação a ser realizada (Aporte ou Resgate).</param>
    /// <param name="quantidadeCotas">A quantidade de cotas desejada (deve ser maior que zero).</param>
    /// <param name="dataAgendamento">A data programada para a execução da ordem.</param>
    /// <param name="dataAtual">A data atual de referência para garantir que o agendamento seja no futuro.</param>
    /// <returns>Um <see cref="Result{Ordem}"/> contendo a instância da ordem agendada, ou um erro se os dados ou a data forem inválidos.</returns>
    public static Result<Ordem> CriarAgendada(Guid idCliente, Guid idFundo, TipoOperacao tipoOperacao, int quantidadeCotas, DateOnly dataAgendamento, DateOnly dataAtual)
    {
        if (quantidadeCotas <= 0)
        {
            return Result<Ordem>.Failure(new CustomError(
                code: "QUANTIDADE_COTAS_INVALIDA",
                message: "A quantidade de cotas para a ordem deve ser maior que zero.",
                statusCode: 422));
        }

        if (dataAgendamento <= dataAtual)
        {
            return Result<Ordem>.Failure(new CustomError(
                code: "DATA_AGENDAMENTO_INVALIDA",
                message: "A data de agendamento deve ser no futuro (maior que a data atual).",
                statusCode: 422));
        }

        var ordem = new Ordem(idCliente, idFundo, tipoOperacao, quantidadeCotas, dataAgendamento);
        return Result<Ordem>.Success(ordem);
    }

    /// <summary>
    /// Marca a ordem como concluída com sucesso.
    /// </summary>
    /// <returns>Um <see cref="Result"/> indicando sucesso, ou falha caso a ordem não esteja com o status Pendente.</returns>
    public Result Concluir()
    {
        if (Status != StatusOrdem.PENDENTE)
        {
            return Result.Failure(new CustomError(
                code: "STATUS_ORDEM_INVALIDO",
                message: $"Não é possível concluir uma ordem que está com status {Status}.",
                statusCode: 422));
        }

        Status = StatusOrdem.CONCLUIDO;
        return Result.Success();
    }

    /// <summary>
    /// Rejeita a ordem, geralmente por quebra de regra de negócio no momento do processamento (ex: capacity atingido no futuro).
    /// </summary>
    /// <returns>Um <see cref="Result"/> indicando sucesso na alteração de status, ou falha caso a ordem não esteja Pendente.</returns>
    public Result Rejeitar()
    {
        if (Status != StatusOrdem.PENDENTE)
        {
            return Result.Failure(new CustomError(
                code: "STATUS_ORDEM_INVALIDO",
                message: $"Não é possível rejeitar uma ordem que está com status {Status}.",
                statusCode: 422));
        }

        Status = StatusOrdem.REJEITADO;
        return Result.Success();
    }
}