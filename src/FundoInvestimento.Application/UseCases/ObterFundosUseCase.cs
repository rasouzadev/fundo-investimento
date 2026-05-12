using FundoInvestimento.Domain.DTOs.Response.Fundo;
using FundoInvestimento.Domain.Enums;
using FundoInvestimento.Domain.Interfaces.Repositories;
using FundoInvestimento.Domain.Interfaces.UseCases;
using FundoInvestimento.Libs.Utils;

namespace FundoInvestimento.Application.UseCases;

/// <summary>
/// Caso de uso que orquestra a recuperação e mapeamento do catálogo de fundos.
/// </summary>
public class ObterFundosUseCase : IObterFundosUseCase
{
    private readonly IFundoRepository _fundoRepository;

    public ObterFundosUseCase(IFundoRepository fundoRepository)
    {
        _fundoRepository = fundoRepository;
    }

    /// <inheritdoc />
    public async Task<Result<IEnumerable<FundoResponse>>> ExecuteAsync(StatusCaptacao? status, CancellationToken cancellationToken = default)
    {
        var fundos = await _fundoRepository.ObterTodosAsync(status, cancellationToken);

        var response = fundos.Select(f => new FundoResponse
        {
            Id = f.Id,
            Nome = f.Nome,
            HorarioCorte = f.HorarioCorte,
            ValorCota = f.ValorCota,
            ValorMinimoAporte = f.ValorMinimoAporte,
            ValorMinimoPermanencia = f.ValorMinimoPermanencia,
            StatusCaptacao = f.StatusCaptacao
        });

        return Result<IEnumerable<FundoResponse>>.Success(response);
    }
}