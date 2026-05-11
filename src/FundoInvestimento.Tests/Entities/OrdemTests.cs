using FundoInvestimento.Domain.Entities;
using FundoInvestimento.Domain.Enums;
using System.Diagnostics.CodeAnalysis;

namespace FundoInvestimento.Tests.Entities;

[ExcludeFromCodeCoverage]
public class OrdemTests
{
    private readonly Guid _idCliente = Guid.NewGuid();
    private readonly Guid _idFundo = Guid.NewGuid();

    [Fact]
    public void CriarImediata_DeveRetornarSucesso_QuandoQuantidadeForValida()
    {
        // Act
        var resultado = Ordem.CriarImediata(_idCliente, _idFundo, TipoOperacao.APORTE, 100);

        // Assert
        Assert.True(resultado.IsSuccess);
        var ordem = resultado.GetSuccess();

        Assert.Null(ordem.DataAgendamento);
        Assert.Equal(StatusOrdem.PENDENTE, ordem.Status);
        Assert.Equal(100, ordem.QuantidadeCotas);
    }

    [Fact]
    public void CriarImediata_DeveRetornarFalha_QuandoQuantidadeForZero()
    {
        // Act
        var resultado = Ordem.CriarImediata(_idCliente, _idFundo, TipoOperacao.RESGATE, 0);

        // Assert
        Assert.True(resultado.IsFailure);
        Assert.Equal("QUANTIDADE_COTAS_INVALIDA", resultado.GetError().Code);
    }

    [Fact]
    public void CriarAgendada_DeveRetornarSucesso_QuandoDataForNoFuturo()
    {
        // Arrange
        var hoje = new DateOnly(2026, 5, 10);
        var amanha = hoje.AddDays(1);

        // Act
        var resultado = Ordem.CriarAgendada(_idCliente, _idFundo, TipoOperacao.APORTE, 100, amanha, hoje);

        // Assert
        Assert.True(resultado.IsSuccess);
        Assert.Equal(amanha, resultado.GetSuccess().DataAgendamento);
    }

    [Fact]
    public void CriarAgendada_DeveRetornarFalha_QuandoDataForNoPassadoOuHoje()
    {
        // Arrange
        var hoje = new DateOnly(2026, 5, 10);
        var passado = hoje.AddDays(-1);

        // Act
        var resultado = Ordem.CriarAgendada(_idCliente, _idFundo, TipoOperacao.APORTE, 100, passado, hoje);

        // Assert
        Assert.True(resultado.IsFailure);
        Assert.Equal("DATA_AGENDAMENTO_INVALIDA", resultado.GetError().Code);
    }

    [Fact]
    public void Concluir_DeveRetornarSucesso_QuandoOrdemEstiverPendente()
    {
        // Arrange
        var ordem = Ordem.CriarImediata(_idCliente, _idFundo, TipoOperacao.APORTE, 100).GetSuccess();

        // Act
        var resultado = ordem.Concluir();

        // Assert
        Assert.True(resultado.IsSuccess);
        Assert.Equal(StatusOrdem.CONCLUIDO, ordem.Status);
    }

    [Fact]
    public void Concluir_DeveRetornarFalha_QuandoOrdemJaEstiverConcluida()
    {
        // Arrange
        var ordem = Ordem.CriarImediata(_idCliente, _idFundo, TipoOperacao.APORTE, 100).GetSuccess();
        ordem.Concluir();

        // Act
        var resultado = ordem.Concluir();

        // Assert
        Assert.True(resultado.IsFailure);
        Assert.Equal("STATUS_ORDEM_INVALIDO", resultado.GetError().Code);
    }
}