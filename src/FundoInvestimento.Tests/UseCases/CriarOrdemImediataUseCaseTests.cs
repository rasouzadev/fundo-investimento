using FundoInvestimento.Application.UseCases;
using FundoInvestimento.Domain.DTOs.Requests.Ordem;
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

namespace FundoInvestimento.Tests.Application.UseCases;

[ExcludeFromCodeCoverage]
public class CriarOrdemImediataUseCaseTests
{
    private readonly Mock<IClienteRepository> _clienteRepoMock;
    private readonly Mock<IFundoRepository> _fundoRepoMock;
    private readonly Mock<IPosicaoClienteRepository> _posicaoRepoMock;
    private readonly Mock<IOrdemRepository> _ordemRepoMock;
    private readonly Mock<IProcessadorOperacaoStrategy> _aporteStrategyMock;
    private readonly Mock<IProcessadorOperacaoStrategy> _resgateStrategyMock;
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

        _aporteStrategyMock = new Mock<IProcessadorOperacaoStrategy>();
        _aporteStrategyMock.SetupGet(s => s.TipoOperacao).Returns(TipoOperacao.APORTE);

        _resgateStrategyMock = new Mock<IProcessadorOperacaoStrategy>();
        _resgateStrategyMock.SetupGet(s => s.TipoOperacao).Returns(TipoOperacao.RESGATE);

        var processadores = new List<IProcessadorOperacaoStrategy>
        {
            _aporteStrategyMock.Object,
            _resgateStrategyMock.Object
        };

        _timeProvider = new FakeTimeProvider();
        _timeProvider.SetUtcNow(new DateTimeOffset(2026, 5, 11, 10, 0, 0, TimeSpan.Zero));

        _useCase = new CriarOrdemImediataUseCase(
            _clienteRepoMock.Object,
            _fundoRepoMock.Object,
            _posicaoRepoMock.Object,
            _ordemRepoMock.Object,
            processadores,
            _uowMock.Object,
            _timeProvider,
            _loggerMock.Object);
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
    }

    [Fact]
    public async Task ExecuteAsync_DeveRetornarFalha_ENaoIniciarTransacao_QuandoForaDoHorarioDeCorte()
    {
        // Arrange
        var request = new OrdemRequest { IdCliente = Guid.NewGuid(), IdFundo = Guid.NewGuid(), TipoOperacao = TipoOperacao.APORTE, QuantidadeCotas = 100 };
        var fundo = new Fundo("Fundo Teste", new TimeOnly(14, 0, 0), 10m, 100m, 50m, StatusCaptacao.ABERTO);

        _timeProvider.SetUtcNow(new DateTimeOffset(2026, 5, 11, 15, 0, 0, TimeSpan.Zero));

        _fundoRepoMock.Setup(r => r.ObterPorIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(fundo);

        // Act
        var result = await _useCase.ExecuteAsync(request);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("FORA_DO_HORARIO_DE_CORTE", result.GetError().Code);
        _uowMock.Verify(u => u.BeginTransaction(), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_DeveRetornarFalha_EFazerRollback_QuandoClienteNaoExistir()
    {
        // Arrange
        var request = new OrdemRequest { IdCliente = Guid.NewGuid(), IdFundo = Guid.NewGuid(), TipoOperacao = TipoOperacao.APORTE, QuantidadeCotas = 100 };
        var fundo = new Fundo("Fundo Teste", new TimeOnly(14, 0, 0), 10m, 100m, 50m, StatusCaptacao.ABERTO);

        _fundoRepoMock.Setup(r => r.ObterPorIdAsync(request.IdFundo, It.IsAny<CancellationToken>())).ReturnsAsync(fundo);
        _clienteRepoMock.Setup(r => r.ObterPorIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((Cliente?)null);

        // Act
        var result = await _useCase.ExecuteAsync(request);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("CLIENTE_NAO_ENCONTRADO", result.GetError().Code);
        _uowMock.Verify(u => u.BeginTransaction(), Times.Once);
        _uowMock.Verify(u => u.Rollback(), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_DeveRetornarFalha_EFazerRollback_QuandoAStrategyFalhar()
    {
        // Arrange
        var clienteId = Guid.NewGuid();
        var fundoId = Guid.NewGuid();
        var request = new OrdemRequest { IdCliente = clienteId, IdFundo = fundoId, TipoOperacao = TipoOperacao.APORTE, QuantidadeCotas = 100 };

        var fundo = new Fundo("Fundo Teste", new TimeOnly(14, 0, 0), 10m, 100m, 50m, StatusCaptacao.ABERTO);
        var cliente = new Cliente("Joao", "123", 0m);

        _fundoRepoMock.Setup(r => r.ObterPorIdAsync(fundoId, It.IsAny<CancellationToken>())).ReturnsAsync(fundo);
        _clienteRepoMock.Setup(r => r.ObterPorIdAsync(clienteId, It.IsAny<CancellationToken>())).ReturnsAsync(cliente);

        _aporteStrategyMock
            .Setup(s => s.CriarImediata(It.IsAny<Cliente>(), It.IsAny<Fundo>(), It.IsAny<PosicaoCliente?>(), It.IsAny<int>(), It.IsAny<DateOnly>()))
            .Returns(Result<(Ordem, PosicaoCliente)>.Failure(new CustomError("SALDO_INSUFICIENTE", "Erro na strategy", 422)));

        // Act
        var result = await _useCase.ExecuteAsync(request);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("SALDO_INSUFICIENTE", result.GetError().Code);
        _uowMock.Verify(u => u.BeginTransaction(), Times.Once);
        _uowMock.Verify(u => u.Rollback(), Times.Once);
        _ordemRepoMock.Verify(r => r.AdicionarAsync(It.IsAny<Ordem>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_DeveRetornarSucessoEFazerCommit_QuandoOrdemForValida()
    {
        // Arrange
        var clienteId = Guid.NewGuid();
        var fundoId = Guid.NewGuid();
        var request = new OrdemRequest { IdCliente = clienteId, IdFundo = fundoId, TipoOperacao = TipoOperacao.APORTE, QuantidadeCotas = 100 };

        var fundo = new Fundo("Fundo Teste", new TimeOnly(14, 0, 0), 10m, 100m, 50m, StatusCaptacao.ABERTO);
        var cliente = new Cliente("Joao", "12345678910", 5000m);
        var dataAtual = new DateOnly(2026, 5, 11);

        var ordemFake = Ordem.CriarImediata(clienteId, fundoId, TipoOperacao.APORTE, 100, dataAtual).GetSuccess();
        var posicaoFake = new PosicaoCliente(clienteId, fundoId, 100);

        _fundoRepoMock.Setup(r => r.ObterPorIdAsync(fundoId, It.IsAny<CancellationToken>())).ReturnsAsync(fundo);
        _clienteRepoMock.Setup(r => r.ObterPorIdAsync(clienteId, It.IsAny<CancellationToken>())).ReturnsAsync(cliente);

        _aporteStrategyMock
            .Setup(s => s.CriarImediata(It.IsAny<Cliente>(), It.IsAny<Fundo>(), It.IsAny<PosicaoCliente?>(), It.IsAny<int>(), It.IsAny<DateOnly>()))
            .Returns(Result<(Ordem, PosicaoCliente)>.Success((ordemFake, posicaoFake)));

        // Act
        var result = await _useCase.ExecuteAsync(request);

        // Assert
        Assert.True(result.IsSuccess);
        _uowMock.Verify(u => u.BeginTransaction(), Times.Once);
        _uowMock.Verify(u => u.Commit(), Times.Once);

        _clienteRepoMock.Verify(r => r.AtualizarAsync(cliente, It.IsAny<CancellationToken>()), Times.Once);
        _posicaoRepoMock.Verify(r => r.AdicionarAsync(posicaoFake, It.IsAny<CancellationToken>()), Times.Once);
        _ordemRepoMock.Verify(r => r.AdicionarAsync(ordemFake, It.IsAny<CancellationToken>()), Times.Once);
    }
}