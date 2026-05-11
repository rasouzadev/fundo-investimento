using FundoInvestimento.Domain.Enums;

namespace FundoInvestimento.Domain.Entities;

public class Fundo
{
    public Guid Id { get; init; }
    public string Nome { get; private set; }
    public TimeSpan HorarioCorte { get; private set; }
    public decimal ValorCota { get; private set; }
    public decimal ValorMinimoAporte { get; private set; }
    public decimal ValorMinimoPermanencia { get; private set; }
    public StatusCaptacao StatusCaptacao { get; private set; }

    protected Fundo() { }

    public Fundo(string nome, TimeSpan horarioCorte, decimal valorCota, decimal valorMinimoAporte, decimal valorMinimoPermanencia, StatusCaptacao statusCaptacao)
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
    /// Verifica se o fundo aceita novos aportes com base no status de captação (Capacity) e no valor mínimo exigido.
    /// </summary>
    /// <param name="valorAporte">O valor financeiro que o cliente deseja aportar no fundo.</param>
    /// <returns>Verdadeiro se o fundo estiver aberto e o valor for igual ou superior ao mínimo; caso contrário, falso.</returns>
    public bool AceitaAporte(decimal valorAporte)
    {
        if (StatusCaptacao == StatusCaptacao.FECHADO)
            return false;

        if (valorAporte < ValorMinimoAporte)
            return false;

        return true;
    }

    /// <summary>
    /// Valida se uma operação imediata está sendo solicitada dentro do horário de funcionamento (Cut-off time) do fundo.
    /// </summary>
    /// <param name="horaAtual">O horário exato em que a solicitação de ordem está sendo processada.</param>
    /// <returns>Verdadeiro se a hora informada for menor ou igual ao horário de corte do fundo.</returns>
    public bool DentroDoHorarioDeCorte(TimeSpan horaAtual)
    {
        return horaAtual <= HorarioCorte;
    }

    /// <summary>
    /// Valida se o resgate de um determinado valor deixará a conta do cliente com um saldo residual aceitável.
    /// </summary>
    /// <param name="saldoAtual">O saldo total atual que o cliente possui aplicado neste fundo.</param>
    /// <param name="valorResgate">O valor financeiro que o cliente deseja resgatar.</param>
    /// <returns>Verdadeiro se for um resgate total (saldo zera) ou se o saldo remanescente for maior ou igual ao valor mínimo de permanência exigido.</returns>
    public bool ResgateDeixaSaldoValido(decimal saldoAtual, decimal valorResgate)
    {
        var saldoRemanescente = saldoAtual - valorResgate;

        return saldoRemanescente == 0 || saldoRemanescente >= ValorMinimoPermanencia;
    }
}