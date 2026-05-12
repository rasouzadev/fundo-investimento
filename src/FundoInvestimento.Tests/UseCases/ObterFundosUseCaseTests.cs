using AutoFixture;
using FundoInvestimento.Application.UseCases;
using FundoInvestimento.Domain.Entities;
using FundoInvestimento.Domain.Enums;
using FundoInvestimento.Domain.Interfaces.Repositories;
using FundoInvestimento.Tests.Fixtures;
using Moq;
using System.Diagnostics.CodeAnalysis;

namespace FundoInvestimento.Tests.UseCases;

[ExcludeFromCodeCoverage]
public class ObterFundosUseCaseTests
{
    private readonly Mock<IFundoRepository> _fundoRepositoryMock;
    private readonly ObterFundosUseCase _useCase;
    private readonly IFixture _fixture;

    public ObterFundosUseCaseTests()
    {
        _fixture = new Fixture();

        _fundoRepositoryMock = new Mock<IFundoRepository>();

        _useCase = new ObterFundosUseCase(
            _fundoRepositoryMock.Object);
    }

    [Fact]
    public async Task ExecuteAsync_DeveRetornarListaMapeada_QuandoExistiremFundos()
    {
        // Arrange
        var fundoAberto = FundoFixture.Criar(
            _fixture,
            statusCaptacao: StatusCaptacao.ABERTO);

        var fundoFechado = FundoFixture.Criar(
            _fixture,
            statusCaptacao: StatusCaptacao.FECHADO);

        _fundoRepositoryMock
            .Setup(repo => repo.ObterTodosAsync(null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Fundo>
            {
                fundoAberto,
                fundoFechado
            });

        // Act
        var result = await _useCase.ExecuteAsync(null);

        // Assert
        Assert.True(result.IsSuccess);

        var responseList = result.GetSuccess().ToList();

        Assert.Equal(2, responseList.Count);

        Assert.Equal(fundoAberto.Id, responseList[0].Id);
        Assert.Equal(fundoAberto.Nome, responseList[0].Nome);
        Assert.Equal(fundoAberto.HorarioCorte, responseList[0].HorarioCorte);
        Assert.Equal(fundoAberto.ValorCota, responseList[0].ValorCota);
        Assert.Equal(fundoAberto.ValorMinimoAporte, responseList[0].ValorMinimoAporte);
        Assert.Equal(fundoAberto.ValorMinimoPermanencia, responseList[0].ValorMinimoPermanencia);
        Assert.Equal(fundoAberto.StatusCaptacao, responseList[0].StatusCaptacao);
    }

    [Fact]
    public async Task ExecuteAsync_DeveRepassarFiltroDeStatusAoRepositorio()
    {
        // Arrange
        var statusFiltro = _fixture.Create<StatusCaptacao>();

        _fundoRepositoryMock
            .Setup(repo => repo.ObterTodosAsync(statusFiltro, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Fundo>());

        // Act
        var result = await _useCase.ExecuteAsync(statusFiltro);

        // Assert
        Assert.True(result.IsSuccess);

        _fundoRepositoryMock.Verify(
            repo => repo.ObterTodosAsync(statusFiltro, It.IsAny<CancellationToken>()),
            Times.Once);
    }
}