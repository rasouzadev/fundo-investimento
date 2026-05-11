using FundoInvestimento.Domain.Entities;
using System.Diagnostics.CodeAnalysis;

namespace FundoInvestimento.Tests.Entities;

[ExcludeFromCodeCoverage]
public class PosicaoClienteTests
{
    [Theory]
    [InlineData(150, 100, true)]
    [InlineData(150, 150, true)]
    [InlineData(150, 200, false)]
    public void TemCotasSuficientes_DeveRetornarBooleanoCorreto_BaseadoNasCotasAtuais(int cotasAtuais, int cotasNecessarias, bool esperado)
    {
        // Arrange
        var posicao = new PosicaoCliente(Guid.NewGuid(), Guid.NewGuid(), cotasAtuais);

        // Act
        var resultado = posicao.TemCotasSuficientes(cotasNecessarias);

        // Assert
        Assert.Equal(esperado, resultado);
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

    [Theory]
    [InlineData(0)]
    [InlineData(-10)]
    public void AdicionarCotas_DeveRetornarFalha_QuandoQuantidadeForMenorOuIgualAZero(int quantidadeInvalida)
    {
        // Arrange
        var posicao = new PosicaoCliente(Guid.NewGuid(), Guid.NewGuid(), 100);

        // Act
        var resultado = posicao.AdicionarCotas(quantidadeInvalida);

        // Assert
        Assert.True(resultado.IsFailure);
        Assert.Equal("QUANTIDADE_COTAS_INVALIDA", resultado.GetError().Code);
        Assert.Equal(100, posicao.QuantidadeCotas);
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
    public void RemoverCotas_DeveRetornarFalhaEErrorCorreto_QuandoCotasForemInsuficientes()
    {
        // Arrange
        var posicao = new PosicaoCliente(Guid.NewGuid(), Guid.NewGuid(), 30);

        // Act
        var resultado = posicao.RemoverCotas(50);

        // Assert
        Assert.True(resultado.IsFailure);
        Assert.Equal("COTAS_INSUFICIENTES", resultado.GetError().Code);
        Assert.Equal(422, resultado.GetError().StatusCode);
        Assert.Equal(30, posicao.QuantidadeCotas);
    }
}