using FundoInvestimento.Application.UseCases;
using FundoInvestimento.Domain.DTOs.Requests.Ordem;
using FundoInvestimento.Domain.Entities;
using FundoInvestimento.Domain.Enums;
using FundoInvestimento.Domain.Interfaces.Repositories;
using Moq;
using System.Diagnostics.CodeAnalysis;

namespace FundoInvestimento.Tests.UseCases;

[ExcludeFromCodeCoverage]
public class ObterOrdensUseCaseTests
{
    private readonly Mock<IOrdemRepository> _ordemRepositoryMock;
    private readonly ObterOrdensUseCase _useCase;

    public ObterOrdensUseCaseTests()
    {
        _ordemRepositoryMock = new Mock<IOrdemRepository>();
        _useCase = new ObterOrdensUseCase(_ordemRepositoryMock.Object);
    }

    [Fact]
    public async Task ExecuteAsync_DeveRetornarListaDeOrdens_MapeadaCorretamente_QuandoExistiremDados()
    {
        // Arrange
        var clienteId = Guid.NewGuid();
        var fundoId = Guid.NewGuid();
        var request = new ListarOrdensRequest { IdCliente = clienteId, IdFundo = fundoId };

        var ordemAporte = Ordem.CriarImediata(clienteId, fundoId, TipoOperacao.APORTE, 100, DateOnly.MaxValue).GetSuccess();
        var ordemResgate = Ordem.CriarImediata(clienteId, fundoId, TipoOperacao.RESGATE, 50, DateOnly.MaxValue).GetSuccess();

        var ordensMock = new List<Ordem> { ordemAporte, ordemResgate };

        _ordemRepositoryMock.Setup(repo => repo.ObterHistoricoAsync(
                request.IdCliente,
                request.IdFundo,
                request.DataInicio,
                request.DataFim,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(ordensMock);

        // Act
        var result = await _useCase.ExecuteAsync(request);

        // Assert
        Assert.True(result.IsSuccess);

        var responseList = result.GetSuccess().ToList();
        Assert.Equal(2, responseList.Count);

        Assert.Equal(ordemAporte.Id, responseList[0].Id);
        Assert.Equal(ordemAporte.TipoOperacao, responseList[0].TipoOperacao);
        Assert.Equal(ordemAporte.Status, responseList[0].Status);
        Assert.Equal(ordemAporte.CriadoEm, responseList[0].CriadoEm);

        Assert.Equal(ordemResgate.Id, responseList[1].Id);
        Assert.Equal(ordemResgate.TipoOperacao, responseList[1].TipoOperacao);

        _ordemRepositoryMock.Verify(repo => repo.ObterHistoricoAsync(
            clienteId, fundoId, null, null, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_DeveRetornarListaVazia_QuandoNaoHouveremRegistrosParaOFiltro()
    {
        // Arrange
        var request = new ListarOrdensRequest
        {
            DataInicio = new DateOnly(2026, 1, 1),
            DataFim = new DateOnly(2026, 1, 31)
        };

        _ordemRepositoryMock.Setup(repo => repo.ObterHistoricoAsync(
                It.IsAny<Guid?>(),
                It.IsAny<Guid?>(),
                It.IsAny<DateOnly?>(),
                It.IsAny<DateOnly?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Ordem>());

        // Act
        var result = await _useCase.ExecuteAsync(request);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Empty(result.GetSuccess());

        _ordemRepositoryMock.Verify(repo => repo.ObterHistoricoAsync(
            null, null, request.DataInicio, request.DataFim, It.IsAny<CancellationToken>()), Times.Once);
    }
}