using FundoInvestimento.Domain.Entities;
using FundoInvestimento.Domain.Enums;
using System.Diagnostics.CodeAnalysis;

namespace FundoInvestimento.Tests.Entities;

[ExcludeFromCodeCoverage]
public class OrdemTests
{
    private readonly Guid _idCliente = Guid.NewGuid();
    private readonly Guid _idFundo = Guid.NewGuid();

    private readonly DateOnly _quartaFeira = new DateOnly(2026, 5, 13);
    private readonly DateOnly _quintaFeira = new DateOnly(2026, 5, 14);
    private readonly DateOnly _sabado = new DateOnly(2026, 5, 16);

    [Fact]
    public void CriarImediata_DeveRetornarSucesso_QuandoQuantidadeForValida_EDiaForUtil()
    {
        // Act
        var resultado = Ordem.CriarImediata(_idCliente, _idFundo, TipoOperacao.APORTE, 100, _quartaFeira);

        // Assert
        Assert.True(resultado.IsSuccess);
        var ordem = resultado.GetSuccess();
        Assert.Null(ordem.DataAgendamento);
        Assert.Equal(StatusOrdem.PENDENTE, ordem.Status);
    }

    [Fact]
    public void CriarImediata_DeveRetornarFalha_QuandoForFimDeSemana()
    {
        // Act
        var resultado = Ordem.CriarImediata(_idCliente, _idFundo, TipoOperacao.APORTE, 100, _sabado);

        // Assert
        Assert.True(resultado.IsFailure);
        Assert.Equal("DATA_FIM_DE_SEMANA", resultado.GetError().Code);
    }

    [Fact]
    public void CriarAgendada_DeveRetornarSucesso_QuandoDataForNoFuturo_EDiaForUtil()
    {
        // Act
        var resultado = Ordem.CriarAgendada(_idCliente, _idFundo, TipoOperacao.APORTE, 100, _quintaFeira, _quartaFeira);

        // Assert
        Assert.True(resultado.IsSuccess);
        Assert.Equal(_quintaFeira, resultado.GetSuccess().DataAgendamento);
    }

    [Fact]
    public void CriarAgendada_DeveRetornarFalha_QuandoDataAgendamentoForFimDeSemana()
    {
        // Act
        var resultado = Ordem.CriarAgendada(_idCliente, _idFundo, TipoOperacao.APORTE, 100, _sabado, _quartaFeira);

        // Assert
        Assert.True(resultado.IsFailure);
        Assert.Equal("DATA_AGENDAMENTO_FIM_DE_SEMANA", resultado.GetError().Code);
    }

    [Fact]
    public void CriarAgendada_DeveRetornarFalha_QuandoDataForNoPassadoOuHoje()
    {
        // Act
        var resultado = Ordem.CriarAgendada(_idCliente, _idFundo, TipoOperacao.APORTE, 100, _quartaFeira, _quartaFeira);

        // Assert
        Assert.True(resultado.IsFailure);
        Assert.Equal("DATA_AGENDAMENTO_INVALIDA", resultado.GetError().Code);
    }

    [Fact]
    public void Concluir_DeveRetornarSucesso_QuandoOrdemEstiverPendente()
    {
        var ordem = Ordem.CriarImediata(_idCliente, _idFundo, TipoOperacao.APORTE, 100, _quartaFeira).GetSuccess();
        var resultado = ordem.Concluir();

        Assert.True(resultado.IsSuccess);
        Assert.Equal(StatusOrdem.CONCLUIDO, ordem.Status);
    }

    [Fact]
    public void Rejeitar_DeveRetornarSucesso_QuandoOrdemEstiverPendente()
    {
        var ordem = Ordem.CriarImediata(_idCliente, _idFundo, TipoOperacao.APORTE, 100, _quartaFeira).GetSuccess();
        var resultado = ordem.Rejeitar();

        Assert.True(resultado.IsSuccess);
        Assert.Equal(StatusOrdem.REJEITADO, ordem.Status);
    }
}