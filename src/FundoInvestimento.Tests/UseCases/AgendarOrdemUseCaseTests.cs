using AutoFixture;
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
public class AgendarOrdemUseCaseTests
{
    private readonly Mock<IClienteRepository> _clienteRepoMock;
    private readonly Mock<IFundoRepository> _fundoRepoMock;
    private readonly Mock<IPosicaoClienteRepository> _posicaoRepoMock;
    private readonly Mock<IOrdemRepository> _ordemRepoMock;
    private readonly Mock<IProcessadorOperacaoStrategy> _aporteStrategyMock;
    private readonly Mock<IProcessadorOperacaoStrategy> _resgateStrategyMock;
    private readonly Mock<IUnitOfWork> _uowMock;
    private readonly Mock<ILogger<AgendarOrdemUseCase>> _loggerMock;
    private readonly FakeTimeProvider _timeProvider;
    private readonly AgendarOrdemUseCase _useCase;
    private readonly IFixture _fixture;

    private readonly DateTimeOffset _dataAtualFixa = new DateTimeOffset(2026, 5, 12, 10, 0, 0, TimeSpan.FromHours(-3));

    public AgendarOrdemUseCaseTests()
    {
        _fixture = new Fixture();

        _clienteRepoMock = new Mock<IClienteRepository>();
        _fundoRepoMock = new Mock<IFundoRepository>();
        _posicaoRepoMock = new Mock<IPosicaoClienteRepository>();
        _ordemRepoMock = new Mock<IOrdemRepository>();
        _uowMock = new Mock<IUnitOfWork>();
        _loggerMock = new Mock<ILogger<AgendarOrdemUseCase>>();

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
        _timeProvider.SetUtcNow(_dataAtualFixa);

        _useCase = new AgendarOrdemUseCase(
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
    public async Task ExecuteAsync_DeveRetornarFalha404_QuandoFundoNaoExistir()
    {
        // Arrange
        var request = _fixture.Build<AgendarOrdemRequest>()
            .With(r => r.DataAgendamento, DateOnly.FromDateTime(DateTime.UtcNow.Date.AddDays(1)))
            .Create();
        _fundoRepoMock.Setup(r => r.ObterPorIdAsync(request.IdFundo, It.IsAny<CancellationToken>())).ReturnsAsync((Fundo?)null);

        // Act
        var result = await _useCase.ExecuteAsync(request);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(404, result.GetError().StatusCode);
        Assert.Equal("FUNDO_NAO_ENCONTRADO", result.GetError().Code);

        _uowMock.Verify(u => u.BeginTransaction(), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_DeveRetornarFalha404_EFazerRollback_QuandoClienteNaoExistir()
    {
        // Arrange
        var request = _fixture.Build<AgendarOrdemRequest>()
            .With(r => r.TipoOperacao, TipoOperacao.APORTE)
            .With(r => r.DataAgendamento, DateOnly.FromDateTime(DateTime.Today))
            .Create();

        var fundoMock = new Fundo("Fundo Teste", new TimeOnly(14, 0), 10m, 100m, 0m, StatusCaptacao.ABERTO);

        _fundoRepoMock.Setup(r => r.ObterPorIdAsync(request.IdFundo, It.IsAny<CancellationToken>())).ReturnsAsync(fundoMock);
        _clienteRepoMock.Setup(r => r.ObterPorIdAsync(request.IdCliente, It.IsAny<CancellationToken>())).ReturnsAsync((Cliente?)null);

        // Act
        var result = await _useCase.ExecuteAsync(request);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(404, result.GetError().StatusCode);
        Assert.Equal("CLIENTE_NAO_ENCONTRADO", result.GetError().Code);

        _uowMock.Verify(u => u.BeginTransaction(), Times.Once);
        _uowMock.Verify(u => u.Rollback(), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_DeveRetornarFalha_EFazerRollback_QuandoAStrategyRejeitarOAgendamento()
    {
        // Arrange
        var request = _fixture.Build<AgendarOrdemRequest>()
            .With(r => r.TipoOperacao, TipoOperacao.APORTE)
            .With(r => r.DataAgendamento, DateOnly.FromDateTime(DateTime.Now))
            .Create();

        var clienteMock = new Cliente("Joao", "123", 0m);
        var fundoMock = new Fundo("Fundo Teste", new TimeOnly(14, 0), 10m, 100m, 0m, StatusCaptacao.ABERTO);

        _fundoRepoMock.Setup(r => r.ObterPorIdAsync(request.IdFundo, It.IsAny<CancellationToken>())).ReturnsAsync(fundoMock);
        _clienteRepoMock.Setup(r => r.ObterPorIdAsync(request.IdCliente, It.IsAny<CancellationToken>())).ReturnsAsync(clienteMock);

        _aporteStrategyMock
            .Setup(s => s.CriarAgendamento(It.IsAny<Cliente>(), It.IsAny<Fundo>(), It.IsAny<PosicaoCliente?>(), It.IsAny<int>(), It.IsAny<DateOnly>(), It.IsAny<DateOnly>()))
            .Returns(Result<Ordem>.Failure(new CustomError("DATA_AGENDAMENTO_INVALIDA", "Erro na data", 422)));

        // Act
        var result = await _useCase.ExecuteAsync(request);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("DATA_AGENDAMENTO_INVALIDA", result.GetError().Code);

        _uowMock.Verify(u => u.BeginTransaction(), Times.Once);
        _uowMock.Verify(u => u.Rollback(), Times.Once);
        _ordemRepoMock.Verify(r => r.AdicionarAsync(It.IsAny<Ordem>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_DeveCriarOrdemPendenteEComitarTransacao_QuandoDadosForemValidos()
    {
        // Arrange
        var request = _fixture.Build<AgendarOrdemRequest>()
            .With(r => r.QuantidadeCotas, 10)
            .With(r => r.TipoOperacao, TipoOperacao.APORTE)
            .With(r => r.DataAgendamento, new DateOnly(2026, 5, 15))
            .Create();

        var clienteMock = new Cliente("Joao", "123", 0m);
        var fundoMock = new Fundo("Fundo Teste", new TimeOnly(14, 0), 10m, 100m, 0m, StatusCaptacao.ABERTO);

        var ordemCriada = Ordem.CriarAgendada(request.IdCliente, request.IdFundo, TipoOperacao.APORTE, request.QuantidadeCotas, request.DataAgendamento, new DateOnly(2026, 5, 12)).GetSuccess();

        _fundoRepoMock.Setup(r => r.ObterPorIdAsync(request.IdFundo, It.IsAny<CancellationToken>())).ReturnsAsync(fundoMock);
        _clienteRepoMock.Setup(r => r.ObterPorIdAsync(request.IdCliente, It.IsAny<CancellationToken>())).ReturnsAsync(clienteMock);

        _aporteStrategyMock
            .Setup(s => s.CriarAgendamento(It.IsAny<Cliente>(), It.IsAny<Fundo>(), It.IsAny<PosicaoCliente?>(), It.IsAny<int>(), It.IsAny<DateOnly>(), It.IsAny<DateOnly>()))
            .Returns(Result<Ordem>.Success(ordemCriada));

        // Act
        var result = await _useCase.ExecuteAsync(request);

        // Assert
        Assert.True(result.IsSuccess);

        var response = result.GetSuccess();
        Assert.Equal(StatusOrdem.PENDENTE, response.Status);
        Assert.Equal(request.DataAgendamento, response.DataAgendamento);

        _uowMock.Verify(u => u.BeginTransaction(), Times.Once);
        _ordemRepoMock.Verify(r => r.AdicionarAsync(ordemCriada, It.IsAny<CancellationToken>()), Times.Once);
        _uowMock.Verify(u => u.Commit(), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_DeveFazerRollbackERelancarExcecao_QuandoPersistenciaFalhar()
    {
        // Arrange
        var request = _fixture
            .Build<AgendarOrdemRequest>()
            .With(r => r.TipoOperacao, TipoOperacao.APORTE)
            .With(r => r.DataAgendamento, new DateOnly(2026, 5, 15))
            .Create();

        var clienteMock = new Cliente("Joao", "123", 0m);
        var fundoMock = new Fundo("Fundo Teste", new TimeOnly(14, 0), 10m, 100m, 0m, StatusCaptacao.ABERTO);
        var ordemCriada = Ordem.CriarAgendada(request.IdCliente, request.IdFundo, TipoOperacao.APORTE, 10, new DateOnly(2026, 5, 15), new DateOnly(2026, 5, 12)).GetSuccess();

        _clienteRepoMock.Setup(r => r.ObterPorIdAsync(request.IdCliente, It.IsAny<CancellationToken>())).ReturnsAsync(clienteMock);
        _fundoRepoMock.Setup(r => r.ObterPorIdAsync(request.IdFundo, It.IsAny<CancellationToken>())).ReturnsAsync(fundoMock);

        _aporteStrategyMock
            .Setup(s => s.CriarAgendamento(It.IsAny<Cliente>(), It.IsAny<Fundo>(), It.IsAny<PosicaoCliente?>(), It.IsAny<int>(), It.IsAny<DateOnly>(), It.IsAny<DateOnly>()))
            .Returns(Result<Ordem>.Success(ordemCriada));

        _ordemRepoMock
            .Setup(r => r.AdicionarAsync(It.IsAny<Ordem>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Deadlock no banco de dados."));

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() => _useCase.ExecuteAsync(request));

        _uowMock.Verify(u => u.BeginTransaction(), Times.Once);
        _uowMock.Verify(u => u.Commit(), Times.Never);
        _uowMock.Verify(u => u.Rollback(), Times.Once);
    }
}