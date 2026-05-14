using FundoInvestimento.Domain.Entities;
using FundoInvestimento.Domain.Enums;
using System.Diagnostics.CodeAnalysis;

namespace FundoInvestimento.Tests.Entities;

[ExcludeFromCodeCoverage]
public class FundoTests
{
    [Theory]
    [InlineData(100, 100, StatusCaptacao.ABERTO, true)]
    [InlineData(150, 100, StatusCaptacao.ABERTO, true)]
    [InlineData(99, 100, StatusCaptacao.ABERTO, false)]
    [InlineData(100, 100, StatusCaptacao.FECHADO, false)]
    public void AceitaAporte_DeveRetornarEsperado_BaseadoNoStatusEValorMinimo(
        decimal valorAporte, decimal valorMinimoAporte, StatusCaptacao status, bool deveSerSucesso)
    {
        // Arrange
        var fundo = new Fundo("Fundo Teste", new TimeOnly(14, 0), 10, valorMinimoAporte, 50, status);

        // Act
        var resultado = fundo.AceitaAporte(valorAporte);

        // Assert
        Assert.Equal(deveSerSucesso, resultado.IsSuccess);
    }

    [Theory]
    [InlineData(StatusCaptacao.ABERTO, true)]
    [InlineData(StatusCaptacao.FECHADO, false)]
    public void AceitaAgendamentoAporte_DeveRetornarEsperado_BaseadoNoStatus(StatusCaptacao status, bool deveSerSucesso)
    {
        // Arrange
        var fundo = new Fundo("Fundo Teste", new TimeOnly(14, 0), 10, 100, 50, status);

        // Act
        var resultado = fundo.AceitaAgendamentoAporte();

        // Assert
        Assert.Equal(deveSerSucesso, resultado.IsSuccess);
    }

    [Theory]
    [InlineData("13:59:59", "14:00:00", true)]
    [InlineData("14:00:00", "14:00:00", true)]
    [InlineData("14:00:01", "14:00:00", false)]
    public void DentroDoHorarioDeCorte_DeveRetornarEsperado_BaseadoNoHorario(string horaAtualStr, string horarioCorteStr, bool deveSerSucesso)
    {
        // Arrange
        var horaAtual = TimeOnly.Parse(horaAtualStr);
        var horarioCorte = TimeOnly.Parse(horarioCorteStr);
        var fundo = new Fundo("Fundo Teste", horarioCorte, 10m, 100m, 50m, StatusCaptacao.ABERTO);

        // Act
        var resultado = fundo.DentroDoHorarioDeCorte(horaAtual);

        // Assert
        Assert.Equal(deveSerSucesso, resultado.IsSuccess);
    }

    [Theory]
    [InlineData(100, 100, 50, true)]
    [InlineData(100, 50, 50, true)]
    [InlineData(100, 40, 50, true)]
    [InlineData(100, 60, 50, false)]
    public void ResgateDeixaSaldoValido_DeveRetornarEsperado_BaseadoNoSaldoRemanescente(
        decimal saldoAtual, decimal valorResgate, decimal valorMinimoPermanencia, bool deveSerSucesso)
    {
        // Arrange
        var fundo = new Fundo("Fundo Teste", new TimeOnly(14, 0), 10, 100, valorMinimoPermanencia, StatusCaptacao.ABERTO);

        // Act
        var resultado = fundo.ResgateDeixaSaldoValido(saldoAtual, valorResgate);

        // Assert
        Assert.Equal(deveSerSucesso, resultado.IsSuccess);
    }
}