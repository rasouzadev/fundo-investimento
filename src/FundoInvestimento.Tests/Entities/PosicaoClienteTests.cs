using FundoInvestimento.Domain.Entities;
using FundoInvestimento.Domain.Enums;
using System.Diagnostics.CodeAnalysis;

namespace FundoInvestimento.Tests.Entities;

[ExcludeFromCodeCoverage]
public class PosicaoClienteTests
{
    [Theory]
    [InlineData(150, 100, true)]
    [InlineData(150, 150, true)]
    [InlineData(150, 200, false)]
    public void TemCotasSuficientes_DeveRetornarBooleanoCorreto(int cotasAtuais, int cotasNecessarias, bool esperado)
    {
        // Act
        var posicao = new PosicaoCliente(Guid.NewGuid(), Guid.NewGuid(), cotasAtuais);

        // Assert
        Assert.Equal(esperado, posicao.TemCotasSuficientes(cotasNecessarias));
    }

    [Fact]
    public void AdicionarCotas_DeveRetornarSucesso_QuandoQuantidadeForValida()
    {
        // Arrange
        var posicao = new PosicaoCliente(Guid.NewGuid(), Guid.NewGuid(), 100);

        // Act
        var resultado = posicao.AdicionarCotas(50);

        // Assert
        Assert.True(resultado.IsSuccess);
        Assert.Equal(150, posicao.QuantidadeCotas);
    }

    [Fact]
    public void RemoverCotas_DeveRetornarSucessoEReduzirCotas_QuandoHouverCotasSuficientes()
    {
        // Arrange
        var posicao = new PosicaoCliente(Guid.NewGuid(), Guid.NewGuid(), 200);

        // Act
        var resultado = posicao.RemoverCotas(50);

        // Assert
        Assert.True(resultado.IsSuccess);
        Assert.Equal(150, posicao.QuantidadeCotas);
    }

    [Fact]
    public void ValidarRegrasDeResgate_DeveFalhar_QuandoCotasForemInsuficientes()
    {
        // Arrange
        var posicao = new PosicaoCliente(Guid.NewGuid(), Guid.NewGuid(), 50);
        var fundo = new Fundo("Fundo Teste", new TimeOnly(14, 0), 10m, 100m, 0m, StatusCaptacao.ABERTO);

        // Act
        var resultado = posicao.ValidarRegrasDeResgate(60, fundo);

        // Assert
        Assert.True(resultado.IsFailure);
        Assert.Equal("COTAS_INSUFICIENTES", resultado.GetError().Code);
    }

    [Fact]
    public void ValidarRegrasDeResgate_DeveFalhar_QuandoViolarSaldoDePermanencia()
    {
        // Arrange
        var posicao = new PosicaoCliente(Guid.NewGuid(), Guid.NewGuid(), 15);
        var fundo = new Fundo("Fundo Teste", new TimeOnly(14, 0), 10m, 100m, 100m, StatusCaptacao.ABERTO);

        // Act
        var resultado = posicao.ValidarRegrasDeResgate(10, fundo);

        // Assert
        Assert.True(resultado.IsFailure);
        Assert.Equal("SALDO_PERMANENCIA_INVALIDO", resultado.GetError().Code);
    }

    [Fact]
    public void ValidarRegrasDeResgate_DeveRetornarSucesso_QuandoRegrasForemRespeitadas()
    {
        // Arrange
        var posicao = new PosicaoCliente(Guid.NewGuid(), Guid.NewGuid(), 25);
        var fundo = new Fundo("Fundo Teste", new TimeOnly(14, 0), 10m, 100m, 100m, StatusCaptacao.ABERTO);

        // Act
        var resultado = posicao.ValidarRegrasDeResgate(10, fundo);

        // Assert
        Assert.True(resultado.IsSuccess);
    }
}