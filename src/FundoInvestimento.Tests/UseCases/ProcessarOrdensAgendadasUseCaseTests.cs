using FundoInvestimento.Application.UseCases;
using FundoInvestimento.Domain.Entities;
using FundoInvestimento.Domain.Enums;
using FundoInvestimento.Domain.Interfaces.Data;
using FundoInvestimento.Domain.Interfaces.Repositories;
using FundoInvestimento.Domain.Interfaces.Strategies;
using FundoInvestimento.Libs.Utils;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Time.Testing;
using Moq;
using System.Diagnostics.CodeAnalysis;

namespace FundoInvestimento.Tests.UseCases;

[ExcludeFromCodeCoverage]
public class ProcessarOrdensAgendadasUseCaseTests
{
    private readonly Mock<IOrdemRepository> _ordemRepoMock;
    private readonly Mock<IFundoRepository> _fundoRepoMock;
    private readonly Mock<IClienteRepository> _clienteRepoMock;
    private readonly Mock<IPosicaoClienteRepository> _posicaoRepoMock;
    private readonly Mock<IProcessadorOperacaoStrategy> _aporteStrategyMock;
    private readonly Mock<IUnitOfWork> _uowMock;
    private readonly Mock<ILogger<ProcessarOrdensAgendadasUseCase>> _loggerMock;
    private readonly FakeTimeProvider _timeProvider;
    private readonly ProcessarOrdensAgendadasUseCase _useCase;

    private readonly DateOnly _dataWorker = new DateOnly(2026, 5, 14);

    public ProcessarOrdensAgendadasUseCaseTests()
    {
        _ordemRepoMock = new Mock<IOrdemRepository>();
        _fundoRepoMock = new Mock<IFundoRepository>();
        _clienteRepoMock = new Mock<IClienteRepository>();
        _posicaoRepoMock = new Mock<IPosicaoClienteRepository>();
        _uowMock = new Mock<IUnitOfWork>();
        _loggerMock = new Mock<ILogger<ProcessarOrdensAgendadasUseCase>>();

        _aporteStrategyMock = new Mock<IProcessadorOperacaoStrategy>();
        _aporteStrategyMock.SetupGet(s => s.TipoOperacao).Returns(TipoOperacao.APORTE);

        var processadores = new List<IProcessadorOperacaoStrategy> { _aporteStrategyMock.Object };

        _timeProvider = new FakeTimeProvider();
        _timeProvider.SetUtcNow(new DateTimeOffset(_dataWorker.Year, _dataWorker.Month, _dataWorker.Day, 10, 0, 0, TimeSpan.Zero));

        _useCase = new ProcessarOrdensAgendadasUseCase(
            _ordemRepoMock.Object,
            _fundoRepoMock.Object,
            _clienteRepoMock.Object,
            _posicaoRepoMock.Object,
            processadores,
            _uowMock.Object,
            _timeProvider,
            _loggerMock.Object);
    }

    [Fact]
    public async Task ExecuteAsync_DeveRejeitarOrdem_QuandoEstiverExpirada_DevidoFalhaDeInfraAnterior()
    {
        // Arrange
        var clienteId = Guid.NewGuid();
        var fundoId = Guid.NewGuid();
        var dataAgendamentoPassada = _dataWorker.AddDays(-1);

        var ordemAtrasada = Ordem.CriarAgendada(clienteId, fundoId, TipoOperacao.APORTE, 10, dataAgendamentoPassada, dataAgendamentoPassada.AddDays(-2)).GetSuccess();

        _ordemRepoMock
            .Setup(r => r.ObterPendentesAteDataAsync(_dataWorker, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Ordem> { ordemAtrasada });

        // Act
        await _useCase.ExecuteAsync();

        // Assert
        Assert.Equal(StatusOrdem.REJEITADO, ordemAtrasada.Status);

        _uowMock.Verify(u => u.BeginTransaction(), Times.Once);
        _ordemRepoMock.Verify(r => r.AtualizarAsync(ordemAtrasada, It.IsAny<CancellationToken>()), Times.Once);
        _uowMock.Verify(u => u.Commit(), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_DeveRejeitarOrdem_EComitar_QuandoStrategyFalhar()
    {
        // Arrange
        var cliente = new Cliente("Joao", "123", 0m); 
        var fundo = new Fundo("Fundo Teste", new TimeOnly(14, 0), 10m, 100m, 0m, StatusCaptacao.ABERTO);

        var ordem = Ordem.CriarAgendada(cliente.Id, fundo.Id, TipoOperacao.APORTE, 10, _dataWorker, _dataWorker.AddDays(-2)).GetSuccess();

        _ordemRepoMock.Setup(r => r.ObterPendentesAteDataAsync(_dataWorker, It.IsAny<CancellationToken>())).ReturnsAsync(new List<Ordem> { ordem });
        _fundoRepoMock.Setup(r => r.ObterPorIdAsync(fundo.Id, It.IsAny<CancellationToken>())).ReturnsAsync(fundo);
        _clienteRepoMock.Setup(r => r.ObterPorIdAsync(cliente.Id, It.IsAny<CancellationToken>())).ReturnsAsync(cliente);

        _aporteStrategyMock
            .Setup(s => s.ProcessarOrdemPendente(ordem, cliente, fundo, null))
            .Returns(Result<PosicaoCliente>.Failure(new CustomError("SALDO_INSUFICIENTE", "Falta saldo", 422)));

        // Act
        await _useCase.ExecuteAsync();

        // Assert
        Assert.Equal(StatusOrdem.REJEITADO, ordem.Status);
        _ordemRepoMock.Verify(r => r.AtualizarAsync(ordem, It.IsAny<CancellationToken>()), Times.Once);
        _uowMock.Verify(u => u.Commit(), Times.Once);
        _posicaoRepoMock.Verify(r => r.AdicionarAsync(It.IsAny<PosicaoCliente>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_DeveConcluirOrdem_EAdicionarPosicao_QuandoValido()
    {
        // Arrange
        var cliente = new Cliente("Joao", "123", 5000m);
        var fundo = new Fundo("Fundo Teste", new TimeOnly(14, 0), 10m, 100m, 0m, StatusCaptacao.ABERTO);

        var ordem = Ordem.CriarAgendada(cliente.Id, fundo.Id, TipoOperacao.APORTE, 10, _dataWorker, _dataWorker.AddDays(-2)).GetSuccess();
        var posicaoNova = new PosicaoCliente(cliente.Id, fundo.Id, 10);

        _ordemRepoMock.Setup(r => r.ObterPendentesAteDataAsync(_dataWorker, It.IsAny<CancellationToken>())).ReturnsAsync(new List<Ordem> { ordem });
        _fundoRepoMock.Setup(r => r.ObterPorIdAsync(fundo.Id, It.IsAny<CancellationToken>())).ReturnsAsync(fundo);
        _clienteRepoMock.Setup(r => r.ObterPorIdAsync(cliente.Id, It.IsAny<CancellationToken>())).ReturnsAsync(cliente);
        _posicaoRepoMock.Setup(r => r.ObterPorIdAsync(cliente.Id, fundo.Id, It.IsAny<CancellationToken>())).ReturnsAsync((PosicaoCliente?)null);

        _aporteStrategyMock
            .Setup(s => s.ProcessarOrdemPendente(ordem, cliente, fundo, null))
            .Returns(Result<PosicaoCliente>.Success(posicaoNova));

        // Act
        await _useCase.ExecuteAsync();

        // Assert
        Assert.Equal(StatusOrdem.CONCLUIDO, ordem.Status);

        _uowMock.Verify(u => u.BeginTransaction(), Times.Once);
        _clienteRepoMock.Verify(r => r.AtualizarAsync(cliente, It.IsAny<CancellationToken>()), Times.Once);
        _posicaoRepoMock.Verify(r => r.AdicionarAsync(posicaoNova, It.IsAny<CancellationToken>()), Times.Once);
        _ordemRepoMock.Verify(r => r.AtualizarAsync(ordem, It.IsAny<CancellationToken>()), Times.Once);
        _uowMock.Verify(u => u.Commit(), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_DeveFazerRollbackApenasDaOrdemComErro_EContinuarLoop()
    {
        // Arrange
        var cliente = new Cliente("Joao", "123", 5000m);
        var fundo = new Fundo("Fundo Teste", new TimeOnly(14, 0), 10m, 100m, 0m, StatusCaptacao.ABERTO);

        var ordemComErro = Ordem.CriarAgendada(cliente.Id, fundo.Id, TipoOperacao.APORTE, 10, _dataWorker, _dataWorker.AddDays(-2)).GetSuccess();
        var ordemSucesso = Ordem.CriarAgendada(cliente.Id, fundo.Id, TipoOperacao.APORTE, 20, _dataWorker, _dataWorker.AddDays(-2)).GetSuccess();

        _ordemRepoMock.Setup(r => r.ObterPendentesAteDataAsync(_dataWorker, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Ordem> { ordemComErro, ordemSucesso });

        _fundoRepoMock.Setup(r => r.ObterPorIdAsync(fundo.Id, It.IsAny<CancellationToken>())).ReturnsAsync(fundo);
        _clienteRepoMock.Setup(r => r.ObterPorIdAsync(cliente.Id, It.IsAny<CancellationToken>())).ReturnsAsync(cliente);

        _ordemRepoMock.Setup(r => r.AtualizarAsync(ordemComErro, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Deadlock"));

        var posicaoNova = new PosicaoCliente(cliente.Id, fundo.Id, 20);
        _aporteStrategyMock.Setup(s => s.ProcessarOrdemPendente(It.IsAny<Ordem>(), cliente, fundo, null))
            .Returns(Result<PosicaoCliente>.Success(posicaoNova));

        // Act
        await _useCase.ExecuteAsync();

        // Assert
        _uowMock.Verify(u => u.BeginTransaction(), Times.Exactly(2));
        _uowMock.Verify(u => u.Rollback(), Times.Once);
        _uowMock.Verify(u => u.Commit(), Times.Once);

        Assert.Equal(StatusOrdem.CONCLUIDO, ordemSucesso.Status);
    }
}