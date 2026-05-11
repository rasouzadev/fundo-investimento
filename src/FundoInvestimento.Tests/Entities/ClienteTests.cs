using FundoInvestimento.Domain.Entities;
using System.Diagnostics.CodeAnalysis;

namespace FundoInvestimento.Tests.Entities;

[ExcludeFromCodeCoverage]
public class ClienteTests
{
    [Theory]
    [InlineData(1000, 500, true)]
    [InlineData(1000, 1000, true)]
    [InlineData(1000, 1500, false)]
    public void TemSaldoSuficiente_DeveRetornarBooleanoCorreto_BaseadoNoSaldo(decimal saldoAtual, decimal valorNecessario, bool esperado)
    {
        // Arrange
        var cliente = new Cliente("Joao", "12345678901", saldoAtual);

        // Act
        var resultado = cliente.TemSaldoSuficiente(valorNecessario);

        // Assert
        Assert.Equal(esperado, resultado);
    }

    [Fact]
    public void DebitarSaldo_DeveRetornarSucessoEReduzirSaldo_QuandoSaldoForSuficiente()
    {
        // Arrange
        var cliente = new Cliente("Joao", "12345678901", 1000m);

        // Act
        var resultado = cliente.DebitarSaldo(400m);

        // Assert
        Assert.True(resultado.IsSuccess);
        Assert.Equal(600m, cliente.SaldoDisponivel);
    }

    [Fact]
    public void DebitarSaldo_DeveRetornarFalhaEErrorCorreto_QuandoSaldoForInsuficiente()
    {
        // Arrange
        var cliente = new Cliente("Joao", "12345678901", 500m);

        // Act
        var resultado = cliente.DebitarSaldo(600m);

        // Assert
        Assert.True(resultado.IsFailure);
        Assert.Equal("SALDO_INSUFICIENTE", resultado.GetError().Code);
        Assert.Equal(422, resultado.GetError().StatusCode);
        Assert.Equal(500m, cliente.SaldoDisponivel);
    }

    [Fact]
    public void CreditarSaldo_DeveRetornarSucessoEAumentarSaldo_QuandoValorForValido()
    {
        // Arrange
        var cliente = new Cliente("Joao", "12345678901", 100m);

        // Act
        var resultado = cliente.CreditarSaldo(300m);

        // Assert
        Assert.True(resultado.IsSuccess);
        Assert.Equal(400m, cliente.SaldoDisponivel);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-50)]
    public void CreditarSaldo_DeveRetornarFalha_QuandoValorForMenorOuIgualAZero(decimal valorInvalido)
    {
        // Arrange
        var cliente = new Cliente("Joao", "12345678901", 100m);

        // Act
        var resultado = cliente.CreditarSaldo(valorInvalido);

        // Assert
        Assert.True(resultado.IsFailure);
        Assert.Equal("VALOR_CREDITO_INVALIDO", resultado.GetError().Code);
        Assert.Equal(100m, cliente.SaldoDisponivel);
    }
}