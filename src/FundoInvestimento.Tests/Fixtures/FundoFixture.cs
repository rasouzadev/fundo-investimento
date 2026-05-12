using AutoFixture;
using FundoInvestimento.Domain.Entities;
using FundoInvestimento.Domain.Enums;

namespace FundoInvestimento.Tests.Fixtures;

public static class FundoFixture
{
    public static Fundo Criar(
        IFixture fixture,
        StatusCaptacao statusCaptacao = StatusCaptacao.ABERTO,
        string? nome = null,
        TimeOnly? horarioCorte = null,
        decimal? valorCota = null,
        decimal? valorMinimoAporte = null,
        decimal? valorMinimoPermanencia = null)
    {
        return new Fundo(
            nome: nome ?? fixture.Create<string>(),
            horarioCorte: horarioCorte ?? new TimeOnly(14, 0),
            valorCota: valorCota ?? 10m,
            valorMinimoAporte: valorMinimoAporte ?? 100m,
            valorMinimoPermanencia: valorMinimoPermanencia ?? 50m,
            statusCaptacao: statusCaptacao);
    }
}