using FundoInvestimento.Domain.Entities;
using FundoInvestimento.Domain.Enums;
using System.Diagnostics.CodeAnalysis;

namespace FundoInvestimento.Tests.Entities;

[ExcludeFromCodeCoverage]
public class FundoTests
{
    [Theory]
    [InlineData(100, 100, StatusCaptacao.ABERTO, true)]  // Valor igual ao mínimo em fundo aberto
    [InlineData(150, 100, StatusCaptacao.ABERTO, true)]  // Valor maior que mínimo em fundo aberto
    [InlineData(99, 100, StatusCaptacao.ABERTO, false)]  // Valor menor que mínimo em fundo aberto
    [InlineData(100, 100, StatusCaptacao.FECHADO, false)] // Fundo fechado
    public void AceitaAporte_DeveRetornarEsperado_BaseadoNoStatusEValorMinimo(
        decimal valorAporte, decimal valorMinimoAporte, StatusCaptacao status, bool deveSerSucesso)
    {
        // Arrange
        var fundo = new Fundo(
            nome: "Fundo Teste",
            horarioCorte: new TimeSpan(14, 0, 0),
            valorCota: 10,
            valorMinimoAporte: valorMinimoAporte,
            valorMinimoPermanencia: 50,
            statusCaptacao: status);

        // Act
        var resultado = fundo.AceitaAporte(valorAporte);

        // Assert
        Assert.Equal(deveSerSucesso, resultado.IsSuccess);
    }

    [Theory]
    [InlineData("13:59:59", "14:00:00", true)]  // Antes do horário
    [InlineData("14:00:00", "14:00:00", true)]  // Horário limite exato
    [InlineData("14:00:01", "14:00:00", false)] // Um segundo depois do horário
    public void DentroDoHorarioDeCorte_DeveRetornarEsperado_BaseadoNoHorario(
        string horaAtualStr, string horarioCorteStr, bool deveSerSucesso)
    {
        // Arrange
        var horaAtual = TimeSpan.Parse(horaAtualStr);
        var horarioCorte = TimeSpan.Parse(horarioCorteStr);

        var fundo = new Fundo("Fundo Teste", horarioCorte, 10m, 100m, 50m, StatusCaptacao.ABERTO);

        // Act
        var resultado = fundo.DentroDoHorarioDeCorte(horaAtual);

        // Assert
        Assert.Equal(deveSerSucesso, resultado.IsSuccess);
    }

    [Theory]
    [InlineData(100, 100, 50, true)]  // Resgate total (Saldo zero é sempre permitido)
    [InlineData(100, 50, 50, true)]   // Resgate parcial, saldo remanescente igual ao mínimo exigido
    [InlineData(100, 40, 50, true)]   // Resgate parcial, saldo remanescente maior que o mínimo exigido
    [InlineData(100, 60, 50, false)]  // Resgate parcial, saldo remanescente (40) menor que mínimo (50)
    public void ResgateDeixaSaldoValido_DeveRetornarEsperado_BaseadoNoSaldoRemanescente(
        decimal saldoAtual, decimal valorResgate, decimal valorMinimoPermanencia, bool deveSerSucesso)
    {
        // Arrange
        var fundo = new Fundo(
            nome: "Fundo Teste",
            horarioCorte: new TimeSpan(14, 0, 0),
            valorCota: 10,
            valorMinimoAporte: 100,
            valorMinimoPermanencia: valorMinimoPermanencia,
            statusCaptacao: StatusCaptacao.ABERTO);

        // Act
        var resultado = fundo.ResgateDeixaSaldoValido(saldoAtual, valorResgate);

        // Assert
        Assert.Equal(deveSerSucesso, resultado.IsSuccess);
    }
}