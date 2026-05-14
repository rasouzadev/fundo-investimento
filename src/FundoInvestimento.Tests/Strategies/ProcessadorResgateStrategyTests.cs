using FundoInvestimento.Application.Strategies;
using FundoInvestimento.Domain.Entities;
using FundoInvestimento.Domain.Enums;
using System.Diagnostics.CodeAnalysis;

namespace FundoInvestimento.Tests.Application.Strategies;

[ExcludeFromCodeCoverage]
public class ProcessadorResgateStrategyTests
{
    private readonly ProcessadorResgateStrategy _strategy;

    public ProcessadorResgateStrategyTests()
    {
        _strategy = new ProcessadorResgateStrategy();
    }

    private Cliente CriarClienteVazio()
    {
        return new Cliente("Joao", "12345678910");
    }

    [Fact]
    public void CriarAgendamento_DeveFalhar_QuandoNaoHouverPosicao()
    {
        // Arrange
        var cliente = CriarClienteVazio();
        var fundo = new Fundo("Teste", new TimeOnly(14, 0), 10m, 0m, 50m, StatusCaptacao.ABERTO);

        // Act
        var result = _strategy.CriarAgendamento(cliente, fundo, null, 10, new DateOnly(2026, 5, 20), new DateOnly(2026, 5, 13));

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("POSICAO_INEXISTENTE", result.GetError().Code);
    }

    [Fact]
    public void CriarAgendamento_DeveFalhar_QuandoQuebrarRegraDePermanencia()
    {
        // Arrange
        var cliente = CriarClienteVazio();
        var fundo = new Fundo("Teste", new TimeOnly(14, 0), 10m, 0m, 100m, StatusCaptacao.ABERTO);
        var posicao = new PosicaoCliente(cliente.Id, fundo.Id, 15);

        // Act
        var result = _strategy.CriarAgendamento(cliente, fundo, posicao, 10, new DateOnly(2026, 5, 20), new DateOnly(2026, 5, 13));

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("SALDO_PERMANENCIA_INVALIDO", result.GetError().Code);
    }

    [Fact]
    public void CriarAgendamento_DeveRetornarOrdem_QuandoResgateTotalOuParcialValido()
    {
        // Arrange
        var cliente = CriarClienteVazio();
        var fundo = new Fundo("Teste", new TimeOnly(14, 0), 10m, 0m, 100m, StatusCaptacao.ABERTO);

        var posicao = new PosicaoCliente(cliente.Id, fundo.Id, 25);
        var dataAgendamento = new DateOnly(2026, 5, 20);
        var dataAtual = new DateOnly(2026, 5, 13);

        // Act
        var result = _strategy.CriarAgendamento(cliente, fundo, posicao, 10, dataAgendamento, dataAtual);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(StatusOrdem.PENDENTE, result.GetSuccess().Status);
        Assert.Equal(TipoOperacao.RESGATE, result.GetSuccess().TipoOperacao);
    }

    [Fact]
    public void CriarImediata_DeveCreditarSaldoERemoverCotas_QuandoValido()
    {
        // Arrange
        var cliente = CriarClienteVazio();
        var fundo = new Fundo("Teste", new TimeOnly(14, 0), 10m, 0m, 0m, StatusCaptacao.ABERTO);
        var posicao = new PosicaoCliente(cliente.Id, fundo.Id, 10);
        var dataAtual = new DateOnly(2026, 5, 13);

        // Act
        var result = _strategy.CriarImediata(cliente, fundo, posicao, 5, dataAtual);

        // Assert
        Assert.True(result.IsSuccess);

        var (ordem, posAtualizada) = result.GetSuccess();
        Assert.Equal(StatusOrdem.CONCLUIDO, ordem.Status);
        Assert.Equal(5, posAtualizada.QuantidadeCotas);
    }

    [Fact]
    public void ProcessarOrdemPendente_DeveFalhar_QuandoClienteResgatouCotasAntesDoWorker()
    {
        // Arrange
        var cliente = CriarClienteVazio();
        var fundo = new Fundo("Teste", new TimeOnly(14, 0), 10m, 0m, 0m, StatusCaptacao.ABERTO);
        var ordem = Ordem.CriarAgendada(cliente.Id, fundo.Id, TipoOperacao.RESGATE, 10, new DateOnly(2026, 5, 20), new DateOnly(2026, 5, 13)).GetSuccess();
        var posicaoExistente = new PosicaoCliente(cliente.Id, fundo.Id, 5);

        // Act
        var result = _strategy.ProcessarOrdemPendente(ordem, cliente, fundo, posicaoExistente);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("COTAS_INSUFICIENTES", result.GetError().Code);
    }
}