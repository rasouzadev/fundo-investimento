using FundoInvestimento.Domain.Enums;
using FundoInvestimento.Libs.Utils;

namespace FundoInvestimento.Domain.Entities;

/// <summary>
/// Representa um fundo de investimento disponível para aplicação e resgate.
/// </summary>
public class Fundo
{
    /// <summary>
    /// Identificador único do fundo.
    /// </summary>
    public Guid Id { get; init; }

    /// <summary>
    /// Nome descritivo do fundo de investimento.
    /// </summary>
    public string Nome { get; private set; }

    /// <summary>
    /// Horário limite diário (Cut-off time) para que ordens imediatas sejam aceitas e processadas no mesmo dia.
    /// </summary>
    public TimeOnly HorarioCorte { get; private set; }

    /// <summary>
    /// Valor atualizado da cota do fundo.
    /// </summary>
    public decimal ValorCota { get; private set; }

    /// <summary>
    /// Valor financeiro mínimo exigido para realizar um aporte neste fundo.
    /// </summary>
    public decimal ValorMinimoAporte { get; private set; }

    /// <summary>
    /// Valor financeiro residual mínimo exigido em conta caso o cliente realize um resgate parcial.
    /// </summary>
    public decimal ValorMinimoPermanencia { get; private set; }

    /// <summary>
    /// Status atual de captação do fundo (Aberto para novos investimentos ou Fechado/Capacity).
    /// </summary>
    public StatusCaptacao StatusCaptacao { get; private set; }

    /// <summary>
    /// Construtor vazio exigido por ferramentas de ORM.
    /// </summary>
    protected Fundo() { }

    /// <summary>
    /// Inicializa uma nova instância da entidade Fundo.
    /// </summary>
    /// <param name="nome">Nome do fundo.</param>
    /// <param name="horarioCorte">Horário limite para operações.</param>
    /// <param name="valorCota">Valor da cota unitária.</param>
    /// <param name="valorMinimoAporte">Valor mínimo para investir.</param>
    /// <param name="valorMinimoPermanencia">Valor mínimo exigido para permanência após resgates.</param>
    /// <param name="statusCaptacao">Status indicando se o fundo recebe captação.</param>
    public Fundo(string nome, TimeOnly horarioCorte, decimal valorCota, decimal valorMinimoAporte, decimal valorMinimoPermanencia, StatusCaptacao statusCaptacao)
    {
        Id = Guid.CreateVersion7();
        Nome = nome;
        HorarioCorte = horarioCorte;
        ValorCota = valorCota;
        ValorMinimoAporte = valorMinimoAporte;
        ValorMinimoPermanencia = valorMinimoPermanencia;
        StatusCaptacao = statusCaptacao;
    }

    /// <summary>
    /// Verifica se o fundo aceita novos aportes validando o capacity (Status de Captação) e o valor financeiro mínimo.
    /// </summary>
    /// <param name="valorAporte">O valor financeiro que o cliente deseja aportar no fundo.</param>
    /// <returns>Um <see cref="Result"/> indicando sucesso ou a violação da regra de negócio detalhada.</returns>
    public Result AceitaAporte(decimal valorAporte)
    {
        if (StatusCaptacao == StatusCaptacao.FECHADO)
        {
            return Result.Failure(new CustomError(
                code: "FUNDO_FECHADO",
                message: "Este fundo encontra-se fechado para novas captações (Capacity atingido).",
                statusCode: 422));
        }

        if (valorAporte < ValorMinimoAporte)
        {
            return Result.Failure(new CustomError(
                code: "APORTE_ABAIXO_DO_MINIMO",
                message: $"O valor do aporte não atinge o mínimo exigido pelo fundo de {ValorMinimoAporte:C}.",
                statusCode: 422));
        }

        return Result.Success();
    }

    /// <summary>
    /// Valida se uma operação imediata está sendo solicitada dentro do horário de funcionamento (Cut-off time) do fundo.
    /// </summary>
    /// <param name="horaAtual">O horário exato em que a solicitação de ordem está sendo processada.</param>
    /// <returns>Um <see cref="Result"/> indicando sucesso ou informando que o horário limite foi excedido.</returns>
    public Result DentroDoHorarioDeCorte(TimeOnly horaAtual)
    {
        if (horaAtual > HorarioCorte)
        {
            return Result.Failure(new CustomError(
                code: "FORA_DO_HORARIO_DE_CORTE",
                message: $"A operação excedeu o horário limite (Cut-off) do fundo, que é às {HorarioCorte}.",
                statusCode: 422));
        }

        return Result.Success();
    }

    /// <summary>
    /// Valida se o resgate de um determinado valor deixará a conta do cliente com um saldo residual aceitável.
    /// </summary>
    /// <param name="saldoAtual">O saldo total atual que o cliente possui aplicado neste fundo.</param>
    /// <param name="valorResgate">O valor financeiro que o cliente deseja resgatar.</param>
    /// <returns>Um <see cref="Result"/> indicando sucesso ou detalhando a regra de saldo de permanência violada.</returns>
    public Result ResgateDeixaSaldoValido(decimal saldoAtual, decimal valorResgate)
    {
        var saldoRemanescente = saldoAtual - valorResgate;

        // Se sacou tudo (saldo zero), é válido. Se sobrou dinheiro, tem que ser maior que o mínimo.
        if (saldoRemanescente != 0 && saldoRemanescente < ValorMinimoPermanencia)
        {
            return Result.Failure(new CustomError(
                code: "SALDO_PERMANENCIA_INVALIDO",
                message: $"O resgate parcial deixará um saldo remanescente inferior ao mínimo de permanência exigido de {ValorMinimoPermanencia:C}.",
                statusCode: 422));
        }

        return Result.Success();
    }
}