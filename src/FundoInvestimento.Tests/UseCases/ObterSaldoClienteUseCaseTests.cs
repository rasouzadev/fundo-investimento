using AutoFixture;
using FundoInvestimento.Application.UseCases;
using FundoInvestimento.Domain.Entities;
using FundoInvestimento.Domain.Interfaces.Repositories;
using Moq;
using System.Diagnostics.CodeAnalysis;

namespace FundoInvestimento.Tests.UseCases;

[ExcludeFromCodeCoverage]
public class ObterSaldoClienteUseCaseTests
{
    private readonly Mock<IClienteRepository> _clienteRepositoryMock;
    private readonly ObterSaldoClienteUseCase _useCase;
    private readonly IFixture _fixture;

    public ObterSaldoClienteUseCaseTests()
    {
        _fixture = new Fixture();
        _clienteRepositoryMock = new Mock<IClienteRepository>();
        _useCase = new ObterSaldoClienteUseCase(_clienteRepositoryMock.Object);
    }

    [Fact]
    public async Task ExecuteAsync_DeveRetornarSaldo_QuandoClienteExistir()
    {
        // Arrange
        var idCliente = Guid.NewGuid();
        var saldoEsperado = 15000.50m;

        var clienteMock = new Cliente(
            nome: _fixture.Create<string>(),
            cpf: _fixture.Create<string>(),
            saldoInicial: saldoEsperado);

        _clienteRepositoryMock
            .Setup(repo => repo.ObterPorIdAsync(idCliente, It.IsAny<CancellationToken>()))
            .ReturnsAsync(clienteMock);

        // Act
        var result = await _useCase.ExecuteAsync(idCliente);

        // Assert
        Assert.True(result.IsSuccess);

        var response = result.GetSuccess();

        Assert.Equal(clienteMock.Id, response.IdCliente);
        Assert.Equal(saldoEsperado, response.SaldoDisponivel);

        _clienteRepositoryMock.Verify(
            repo => repo.ObterPorIdAsync(idCliente, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_DeveRetornarFalha404_QuandoClienteNaoForEncontrado()
    {
        // Arrange
        var idCliente = Guid.NewGuid();

        _clienteRepositoryMock
            .Setup(repo => repo.ObterPorIdAsync(idCliente, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Cliente?)null);

        // Act
        var result = await _useCase.ExecuteAsync(idCliente);

        // Assert
        Assert.True(result.IsFailure);

        var error = result.GetError();
        Assert.Equal(404, error.StatusCode);
        Assert.Equal("CLIENTE_NAO_ENCONTRADO", error.Code);
    }
}