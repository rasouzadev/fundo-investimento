using FundoInvestimento.Application.UseCases;
using FundoInvestimento.Domain.DTOs.Requests.Ordem;
using FundoInvestimento.Domain.Entities;
using FundoInvestimento.Domain.Enums;
using FundoInvestimento.Domain.Interfaces.Data;
using FundoInvestimento.Domain.Interfaces.Repositories;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Time.Testing;
using Moq;
using System.Diagnostics.CodeAnalysis;

namespace FundoInvestimento.Tests.Application.UseCases;

[ExcludeFromCodeCoverage]
public class CriarOrdemImediataUseCaseTests
{
    private readonly Mock<IClienteRepository> _clienteRepoMock;
    private readonly Mock<IFundoRepository> _fundoRepoMock;
    private readonly Mock<IPosicaoClienteRepository> _posicaoRepoMock;
    private readonly Mock<IOrdemRepository> _ordemRepoMock;
    private readonly Mock<IUnitOfWork> _uowMock;
    private readonly Mock<ILogger<CriarOrdemImediataUseCase>> _loggerMock;
    private readonly FakeTimeProvider _timeProvider;
    private readonly CriarOrdemImediataUseCase _useCase;

    public CriarOrdemImediataUseCaseTests()
    {
        _clienteRepoMock = new Mock<IClienteRepository>();
        _fundoRepoMock = new Mock<IFundoRepository>();
        _posicaoRepoMock = new Mock<IPosicaoClienteRepository>();
        _ordemRepoMock = new Mock<IOrdemRepository>();
        _uowMock = new Mock<IUnitOfWork>();
        _loggerMock = new Mock<ILogger<CriarOrdemImediataUseCase>>();

        _timeProvider = new FakeTimeProvider();
        _timeProvider.SetUtcNow(new DateTimeOffset(2026, 5, 11, 10, 0, 0, TimeSpan.Zero));

        _useCase = new CriarOrdemImediataUseCase(
            _clienteRepoMock.Object,
            _fundoRepoMock.Object,
            _posicaoRepoMock.Object,
            _ordemRepoMock.Object,
            _uowMock.Object,
            _timeProvider,
            _loggerMock.Object);
    }

    [Fact]
    public async Task ExecuteAsync_DeveRetornarSucessoEFazerCommit_QuandoAporteForValido()
    {
        // Arrange
        var clienteId = Guid.NewGuid();
        var fundoId = Guid.NewGuid();

        var cliente = new Cliente("Joao", "12345678910", 5000m);
        var fundo = new Fundo("Fundo Teste", new TimeSpan(14, 0, 0), 10m, 100m, 50m, StatusCaptacao.ABERTO);

        var request = new OrdemRequest { IdCliente = clienteId, IdFundo = fundoId, TipoOperacao = TipoOperacao.APORTE, QuantidadeCotas = 100 };

        _fundoRepoMock.Setup(r => r.ObterPorIdAsync(fundoId, It.IsAny<CancellationToken>())).ReturnsAsync(fundo);
        _clienteRepoMock.Setup(r => r.ObterPorIdAsync(clienteId, It.IsAny<CancellationToken>())).ReturnsAsync(cliente);
        _posicaoRepoMock.Setup(r => r.ObterPorIdAsync(clienteId, fundoId, It.IsAny<CancellationToken>())).ReturnsAsync((PosicaoCliente?)null);

        // Act
        var result = await _useCase.ExecuteAsync(request);

        // Assert
        Assert.True(result.IsSuccess);
        _uowMock.Verify(u => u.BeginTransaction(), Times.Once);
        _uowMock.Verify(u => u.Commit(), Times.Once);
        _uowMock.Verify(u => u.Rollback(), Times.Never);

        _clienteRepoMock.Verify(r => r.AtualizarAsync(It.IsAny<Cliente>(), It.IsAny<CancellationToken>()), Times.Once);
        _posicaoRepoMock.Verify(r => r.AdicionarAsync(It.IsAny<PosicaoCliente>(), It.IsAny<CancellationToken>()), Times.Once);
        _ordemRepoMock.Verify(r => r.AdicionarAsync(It.IsAny<Ordem>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_DeveRetornarSucessoEFazerCommit_QuandoResgateForValido()
    {
        // Arrange
        var clienteId = Guid.NewGuid();
        var fundoId = Guid.NewGuid();

        var cliente = new Cliente("Joao", "12345678910", 100m);
        var fundo = new Fundo("Fundo Teste", new TimeSpan(14, 0, 0), 10m, 100m, 50m, StatusCaptacao.ABERTO);
        var posicaoExistente = new PosicaoCliente(clienteId, fundoId, 200);

        var request = new OrdemRequest { IdCliente = clienteId, IdFundo = fundoId, TipoOperacao = TipoOperacao.RESGATE, QuantidadeCotas = 100 };

        _fundoRepoMock.Setup(r => r.ObterPorIdAsync(fundoId, It.IsAny<CancellationToken>())).ReturnsAsync(fundo);
        _clienteRepoMock.Setup(r => r.ObterPorIdAsync(clienteId, It.IsAny<CancellationToken>())).ReturnsAsync(cliente);
        _posicaoRepoMock.Setup(r => r.ObterPorIdAsync(clienteId, fundoId, It.IsAny<CancellationToken>())).ReturnsAsync(posicaoExistente);

        // Act
        var result = await _useCase.ExecuteAsync(request);

        // Assert
        Assert.True(result.IsSuccess);
        _uowMock.Verify(u => u.BeginTransaction(), Times.Once);
        _uowMock.Verify(u => u.Commit(), Times.Once);

        _posicaoRepoMock.Verify(r => r.AtualizarAsync(It.IsAny<PosicaoCliente>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_DeveRetornarFalha_ENaoIniciarTransacao_QuandoFundoNaoExistir()
    {
        // Arrange
        var request = new OrdemRequest { IdCliente = Guid.NewGuid(), IdFundo = Guid.NewGuid(), TipoOperacao = TipoOperacao.APORTE, QuantidadeCotas = 100 };

        _fundoRepoMock.Setup(r => r.ObterPorIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((Fundo?)null);

        // Act
        var result = await _useCase.ExecuteAsync(request);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("FUNDO_NAO_ENCONTRADO", result.GetError().Code);

        _uowMock.Verify(u => u.BeginTransaction(), Times.Never);
        _uowMock.Verify(u => u.Rollback(), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_DeveRetornarFalha_EFazerRollback_QuandoClienteNaoExistir()
    {
        // Arrange
        var fundoId = Guid.NewGuid();
        var fundo = new Fundo("Fundo Teste", new TimeSpan(14, 0, 0), 10m, 100m, 50m, StatusCaptacao.ABERTO);

        var request = new OrdemRequest { IdCliente = Guid.NewGuid(), IdFundo = fundoId, TipoOperacao = TipoOperacao.APORTE, QuantidadeCotas = 100 };

        _fundoRepoMock.Setup(r => r.ObterPorIdAsync(fundoId, It.IsAny<CancellationToken>())).ReturnsAsync(fundo);
        _clienteRepoMock.Setup(r => r.ObterPorIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((Cliente?)null);

        // Act
        var result = await _useCase.ExecuteAsync(request);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("CLIENTE_NAO_ENCONTRADO", result.GetError().Code);

        _uowMock.Verify(u => u.BeginTransaction(), Times.Once);
        _uowMock.Verify(u => u.Rollback(), Times.Once);
        _uowMock.Verify(u => u.Commit(), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_DeveRetornarFalha_EFazerRollback_QuandoSaldoForInsuficiente()
    {
        // Arrange
        var clienteId = Guid.NewGuid();
        var fundoId = Guid.NewGuid();

        var cliente = new Cliente("Joao", "123", 0m);
        var fundo = new Fundo("Fundo Teste", new TimeSpan(14, 0, 0), 10m, 100m, 50m, StatusCaptacao.ABERTO);

        var request = new OrdemRequest { IdCliente = clienteId, IdFundo = fundoId, TipoOperacao = TipoOperacao.APORTE, QuantidadeCotas = 100 };

        _fundoRepoMock.Setup(r => r.ObterPorIdAsync(fundoId, It.IsAny<CancellationToken>())).ReturnsAsync(fundo);
        _clienteRepoMock.Setup(r => r.ObterPorIdAsync(clienteId, It.IsAny<CancellationToken>())).ReturnsAsync(cliente);
        _posicaoRepoMock.Setup(r => r.ObterPorIdAsync(clienteId, fundoId, It.IsAny<CancellationToken>())).ReturnsAsync((PosicaoCliente?)null);

        // Act
        var result = await _useCase.ExecuteAsync(request);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("SALDO_INSUFICIENTE", result.GetError().Code);

        _uowMock.Verify(u => u.BeginTransaction(), Times.Once);
        _uowMock.Verify(u => u.Rollback(), Times.Once);
        _uowMock.Verify(u => u.Commit(), Times.Never);

        _ordemRepoMock.Verify(r => r.AdicionarAsync(It.IsAny<Ordem>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}