using FundoInvestimento.Application.Strategies;
using FundoInvestimento.Domain.Entities;
using FundoInvestimento.Domain.Enums;
using System.Diagnostics.CodeAnalysis;

namespace FundoInvestimento.Tests.Application.Strategies;

[ExcludeFromCodeCoverage]
public class ProcessadorAporteStrategyTests
{
    private readonly ProcessadorAporteStrategy _strategy;

    public ProcessadorAporteStrategyTests()
    {
        _strategy = new ProcessadorAporteStrategy();
    }

    private Cliente CriarClienteComSaldo(decimal saldo)
    {
        return new Cliente("Joao", "12345678910", saldo);
    }

    [Fact]
    public void CriarAgendamento_DeveFalhar_QuandoFundoEstiverFechado()
    {
        // Arrange
        var cliente = CriarClienteComSaldo(1000m);
        var fundo = new Fundo("Teste", new TimeOnly(14, 0), 10m, 100m, 0m, StatusCaptacao.FECHADO);

        // Act
        var result = _strategy.CriarAgendamento(cliente, fundo, null, 10, new DateOnly(2026, 5, 20), new DateOnly(2026, 5, 10));

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("FUNDO_FECHADO", result.GetError().Code);
    }

    [Fact]
    public void CriarAgendamento_DeveRetornarOrdem_QuandoValido()
    {
        // Arrange
        var cliente = CriarClienteComSaldo(1000m);
        var fundo = new Fundo("Teste", new TimeOnly(14, 0), 10m, 100m, 0m, StatusCaptacao.ABERTO);
        var dataAgendamento = new DateOnly(2026, 5, 20);
        var dataAtual = new DateOnly(2026, 5, 13);

        // Act
        var result = _strategy.CriarAgendamento(cliente, fundo, null, 10, dataAgendamento, dataAtual);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(StatusOrdem.PENDENTE, result.GetSuccess().Status);
        Assert.Equal(dataAgendamento, result.GetSuccess().DataAgendamento);
    }

    [Fact]
    public void CriarImediata_DeveFalhar_QuandoSaldoForInsuficiente()
    {
        // Arrange
        var cliente = CriarClienteComSaldo(50m);
        var fundo = new Fundo("Teste", new TimeOnly(14, 0), 10m, 100m, 0m, StatusCaptacao.ABERTO);

        // Act
        var result = _strategy.CriarImediata(cliente, fundo, null, 10, new DateOnly(2026, 5, 13));

        // Assert
        Assert.True(result.IsFailure);
    }

    [Fact]
    public void CriarImediata_DeveDebitarSaldoEAdicionarCotas_QuandoValido()
    {
        // Arrange
        var cliente = CriarClienteComSaldo(1500m);
        var fundo = new Fundo("Alpha", new TimeOnly(14, 0), 10m, 100m, 0m, StatusCaptacao.ABERTO);
        var dataAtual = new DateOnly(2026, 5, 13);

        // Act
        var result = _strategy.CriarImediata(cliente, fundo, null, 10, dataAtual);

        // Assert
        Assert.True(result.IsSuccess);

        var (ordem, posicao) = result.GetSuccess();
        Assert.Equal(StatusOrdem.CONCLUIDO, ordem.Status);
        Assert.Equal(10, posicao.QuantidadeCotas);
    }

    [Fact]
    public void ProcessarOrdemPendente_DeveAdicionarCotas_QuandoAprovadoPeloWorker()
    {
        // Arrange
        var cliente = CriarClienteComSaldo(1000m);
        var fundo = new Fundo("Alpha", new TimeOnly(14, 0), 10m, 100m, 0m, StatusCaptacao.ABERTO);
        var ordem = Ordem.CriarAgendada(cliente.Id, fundo.Id, TipoOperacao.APORTE, 10, new DateOnly(2026, 5, 20), new DateOnly(2026, 5, 13)).GetSuccess();
        var posicaoExistente = new PosicaoCliente(cliente.Id, fundo.Id, 5);

        // Act
        var result = _strategy.ProcessarOrdemPendente(ordem, cliente, fundo, posicaoExistente);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(15, result.GetSuccess().QuantidadeCotas);
    }
}