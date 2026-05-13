using FundoInvestimento.Domain.DTOs.Response.Posicao;
using FundoInvestimento.Domain.Interfaces.Repositories;
using FundoInvestimento.Domain.Interfaces.UseCases;
using FundoInvestimento.Libs.Utils;

namespace FundoInvestimento.Application.UseCases;

/// <summary>
/// Caso de uso que orquestra a recuperação das posições do cliente na base de dados 
/// e mapeia as informações brutas para o formato de exibição consolidada.
/// </summary>
public class ObterPosicaoConsolidadaUseCase : IObterPosicaoConsolidadaUseCase
{
    private readonly IPosicaoClienteRepository _posicaoRepository;

    public ObterPosicaoConsolidadaUseCase(IPosicaoClienteRepository posicaoRepository)
    {
        _posicaoRepository = posicaoRepository;
    }

    /// <inheritdoc />
    public async Task<Result<PosicaoConsolidadaResponse>> ExecuteAsync(Guid idCliente, CancellationToken cancellationToken = default)
    {
        var posicoesBd = await _posicaoRepository.ObterPosicaoConsolidadaAsync(idCliente, cancellationToken);

        var response = new PosicaoConsolidadaResponse
        {
            IdCliente = idCliente,
            Posicoes = posicoesBd.Select(p => new PosicaoFundoResponse
            {
                IdFundo = p.IdFundo,
                NomeFundo = p.NomeFundo,
                QuantidadeCotas = p.QuantidadeCotas,
                ValorCotaAtual = p.ValorCota
            })
        };

        return Result<PosicaoConsolidadaResponse>.Success(response);
    }
}