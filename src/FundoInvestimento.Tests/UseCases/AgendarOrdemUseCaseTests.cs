using AutoFixture;
using FundoInvestimento.Application.UseCases;
using FundoInvestimento.Domain.DTOs.Requests.Ordem;
using FundoInvestimento.Domain.Entities;
using FundoInvestimento.Domain.Enums;
using FundoInvestimento.Domain.Interfaces.Data;
using FundoInvestimento.Domain.Interfaces.Repositories;
using Microsoft.Extensions.Time.Testing;
using Moq;
using System.Diagnostics.CodeAnalysis;

namespace FundoInvestimento.Tests.Application.UseCases;

[ExcludeFromCodeCoverage]
public class AgendarOrdemUseCaseTests
{
    private readonly Mock<IClienteRepository> _clienteRepositoryMock;
    private readonly Mock<IFundoRepository> _fundoRepositoryMock;
    private readonly Mock<IOrdemRepository> _ordemRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly FakeTimeProvider _timeProvider;
    private readonly AgendarOrdemUseCase _useCase;
    private readonly IFixture _fixture;

    private readonly DateTimeOffset _dataAtualFixa = new DateTimeOffset(2026, 5, 12, 10, 0, 0, TimeSpan.FromHours(-3));

    public AgendarOrdemUseCaseTests()
    {
        _fixture = new Fixture();
        _fixture.Register(() =>
        {
            var random = new Random();

            return new DateOnly(
                year: random.Next(2000, 2100),
                month: random.Next(1, 13),
                day: random.Next(1, 28));
        });
        _fixture.Register(() =>
        {
            var random = new Random();

            return new TimeOnly(
                hour: random.Next(0, 24),
                minute: random.Next(0, 60),
                second: random.Next(0, 60));
        });
        _clienteRepositoryMock = new Mock<IClienteRepository>();
        _fundoRepositoryMock = new Mock<IFundoRepository>();
        _ordemRepositoryMock = new Mock<IOrdemRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _timeProvider = new FakeTimeProvider();
        _timeProvider.SetUtcNow(_dataAtualFixa);

        _useCase = new AgendarOrdemUseCase(
            _clienteRepositoryMock.Object,
            _fundoRepositoryMock.Object,
            _ordemRepositoryMock.Object,
            _unitOfWorkMock.Object,
            _timeProvider);
    }

    [Fact]
    public async Task ExecuteAsync_DeveCriarOrdemPendenteEComitarTransacao_QuandoDadosForemValidos()
    {
        // Arrange
        var request = _fixture.Build<AgendarOrdemRequest>()
            .With(r => r.QuantidadeCotas, 10)
            .With(r => r.DataAgendamento, new DateOnly(2026, 5, 15))
            .Create();

        var clienteMock = _fixture.Build<Cliente>().With(c => c.Id, request.IdCliente).Create();
        var fundoMock = _fixture.Build<Fundo>().With(f => f.Id, request.IdFundo).Create();

        _clienteRepositoryMock.Setup(r => r.ObterPorIdAsync(request.IdCliente, It.IsAny<CancellationToken>())).ReturnsAsync(clienteMock);
        _fundoRepositoryMock.Setup(r => r.ObterPorIdAsync(request.IdFundo, It.IsAny<CancellationToken>())).ReturnsAsync(fundoMock);

        // Act
        var result = await _useCase.ExecuteAsync(request);

        // Assert
        Assert.True(result.IsSuccess);

        var response = result.GetSuccess();
        Assert.Equal(StatusOrdem.PENDENTE, response.Status);
        Assert.Equal(request.DataAgendamento, response.DataAgendamento);

        _unitOfWorkMock.Verify(u => u.BeginTransaction(), Times.Once);
        _ordemRepositoryMock.Verify(r => r.AdicionarAsync(It.IsAny<Ordem>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.Commit(), Times.Once);
        _unitOfWorkMock.Verify(u => u.Rollback(), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_DeveRetornarFalha_QuandoDataAgendamentoForNoPassadoOuPresente()
    {
        // Arrange
        var request = _fixture.Build<AgendarOrdemRequest>()
            .With(r => r.QuantidadeCotas, 10)
            .With(r => r.DataAgendamento, new DateOnly(2026, 5, 12))
            .Create();

        var clienteMock = _fixture.Build<Cliente>().With(c => c.Id, request.IdCliente).Create();
        var fundoMock = _fixture.Build<Fundo>().With(f => f.Id, request.IdFundo).Create();

        _clienteRepositoryMock.Setup(r => r.ObterPorIdAsync(request.IdCliente, It.IsAny<CancellationToken>())).ReturnsAsync(clienteMock);
        _fundoRepositoryMock.Setup(r => r.ObterPorIdAsync(request.IdFundo, It.IsAny<CancellationToken>())).ReturnsAsync(fundoMock);

        // Act
        var result = await _useCase.ExecuteAsync(request);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(422, result.GetError().StatusCode);
        Assert.Equal("DATA_AGENDAMENTO_INVALIDA", result.GetError().Code);

        _unitOfWorkMock.Verify(u => u.BeginTransaction(), Times.Never);
        _ordemRepositoryMock.Verify(r => r.AdicionarAsync(It.IsAny<Ordem>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_DeveFazerRollbackERelancarExcecao_QuandoPersistenciaFalhar()
    {
        // Arrange
        var request = _fixture.Build<AgendarOrdemRequest>()
            .With(r => r.QuantidadeCotas, 10)
            .With(r => r.DataAgendamento, new DateOnly(2026, 5, 15))
            .Create();

        var clienteMock = _fixture.Build<Cliente>().Create();
        var fundoMock = _fixture.Build<Fundo>().Create();

        _clienteRepositoryMock.Setup(r => r.ObterPorIdAsync(request.IdCliente, It.IsAny<CancellationToken>())).ReturnsAsync(clienteMock);
        _fundoRepositoryMock.Setup(r => r.ObterPorIdAsync(request.IdFundo, It.IsAny<CancellationToken>())).ReturnsAsync(fundoMock);

        _ordemRepositoryMock
            .Setup(r => r.AdicionarAsync(It.IsAny<Ordem>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Erro no banco de dados."));

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() => _useCase.ExecuteAsync(request));

        _unitOfWorkMock.Verify(u => u.BeginTransaction(), Times.Once);
        _unitOfWorkMock.Verify(u => u.Commit(), Times.Never);
        _unitOfWorkMock.Verify(u => u.Rollback(), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_DeveRetornarFalha404_QuandoClienteNaoExistir()
    {
        // Arrange
        var request = _fixture.Create<AgendarOrdemRequest>();
        _clienteRepositoryMock.Setup(r => r.ObterPorIdAsync(request.IdCliente, It.IsAny<CancellationToken>())).ReturnsAsync((Cliente?)null);

        // Act
        var result = await _useCase.ExecuteAsync(request);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(404, result.GetError().StatusCode);
        Assert.Equal("CLIENTE_NAO_ENCONTRADO", result.GetError().Code);
    }

    [Fact]
    public async Task ExecuteAsync_DeveRetornarFalha404_QuandoFundoNaoExistir()
    {
        // Arrange
        var request = _fixture.Create<AgendarOrdemRequest>();
        var clienteMock = _fixture.Build<Cliente>().Create();

        _clienteRepositoryMock.Setup(r => r.ObterPorIdAsync(request.IdCliente, It.IsAny<CancellationToken>())).ReturnsAsync(clienteMock);
        _fundoRepositoryMock.Setup(r => r.ObterPorIdAsync(request.IdFundo, It.IsAny<CancellationToken>())).ReturnsAsync((Fundo?)null);

        // Act
        var result = await _useCase.ExecuteAsync(request);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(404, result.GetError().StatusCode);
        Assert.Equal("FUNDO_NAO_ENCONTRADO", result.GetError().Code);
    }
}