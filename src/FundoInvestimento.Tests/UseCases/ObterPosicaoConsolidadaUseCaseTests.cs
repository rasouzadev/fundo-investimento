using AutoFixture;
using FundoInvestimento.Application.UseCases;
using FundoInvestimento.Domain.Entities;
using FundoInvestimento.Domain.Interfaces.Repositories;
using Moq;
using System.Diagnostics.CodeAnalysis;

namespace FundoInvestimento.Tests.UseCases;

[ExcludeFromCodeCoverage]
public class ObterPosicaoConsolidadaUseCaseTests
{
    private readonly Mock<IPosicaoClienteRepository> _posicaoRepositoryMock;
    private readonly ObterPosicaoConsolidadaUseCase _useCase;
    private readonly IFixture _fixture;

    public ObterPosicaoConsolidadaUseCaseTests()
    {
        _fixture = new Fixture();
        _posicaoRepositoryMock = new Mock<IPosicaoClienteRepository>();
        _useCase = new ObterPosicaoConsolidadaUseCase(_posicaoRepositoryMock.Object);
    }

    [Fact]
    public async Task ExecuteAsync_DeveMapearPosicoes_E_CalcularPatrimonio_QuandoClienteTiverCotas()
    {
        // Arrange
        var idCliente = Guid.NewGuid();

        var posicaoFundoA = _fixture.Build<PosicaoDetalhadaReadModel>()
            .With(p => p.QuantidadeCotas, 10)
            .With(p => p.ValorCota, 10m)
            .Create();

        var posicaoFundoB = _fixture.Build<PosicaoDetalhadaReadModel>()
            .With(p => p.QuantidadeCotas, 5)
            .With(p => p.ValorCota, 20m)
            .Create();

        _posicaoRepositoryMock
            .Setup(repo => repo.ObterPosicaoConsolidadaAsync(idCliente, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<PosicaoDetalhadaReadModel> { posicaoFundoA, posicaoFundoB });

        // Act
        var result = await _useCase.ExecuteAsync(idCliente);

        // Assert
        Assert.True(result.IsSuccess);

        var response = result.GetSuccess();
        Assert.Equal(idCliente, response.IdCliente);
        Assert.Equal(2, response.Posicoes.Count());

        Assert.Equal(200m, response.PatrimonioTotal);

        var primeiraPosicao = response.Posicoes.First();
        Assert.Equal(posicaoFundoA.IdFundo, primeiraPosicao.IdFundo);
        Assert.Equal(posicaoFundoA.NomeFundo, primeiraPosicao.NomeFundo);
        Assert.Equal(posicaoFundoA.QuantidadeCotas, primeiraPosicao.QuantidadeCotas);
        Assert.Equal(posicaoFundoA.ValorCota, primeiraPosicao.ValorCotaAtual);

        Assert.Equal(100m, primeiraPosicao.SaldoFinanceiro);

        _posicaoRepositoryMock.Verify(repo => repo.ObterPosicaoConsolidadaAsync(idCliente, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_DeveRetornarCarteiraVazia_E_PatrimonioZerado_QuandoClienteNaoTiverPosicao()
    {
        // Arrange
        var idCliente = Guid.NewGuid();

        _posicaoRepositoryMock
            .Setup(repo => repo.ObterPosicaoConsolidadaAsync(idCliente, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<PosicaoDetalhadaReadModel>());

        // Act
        var result = await _useCase.ExecuteAsync(idCliente);

        // Assert
        Assert.True(result.IsSuccess);

        var response = result.GetSuccess();
        Assert.Equal(idCliente, response.IdCliente);
        Assert.Empty(response.Posicoes);
        Assert.Equal(0m, response.PatrimonioTotal);
    }
}