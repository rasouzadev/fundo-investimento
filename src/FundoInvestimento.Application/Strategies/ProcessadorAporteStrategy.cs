using FundoInvestimento.Domain.Entities;
using FundoInvestimento.Domain.Enums;
using FundoInvestimento.Domain.Interfaces.Strategies;
using FundoInvestimento.Libs.Utils;

namespace FundoInvestimento.Application.Strategies;

public class ProcessadorAporteStrategy : IProcessadorOperacaoStrategy
{
    public TipoOperacao TipoOperacao => TipoOperacao.APORTE;

    public Result<Ordem> CriarAgendamento(Cliente cliente, Fundo fundo, PosicaoCliente? posicaoAtual, int quantidadeCotas, DateOnly dataAgendamento, DateOnly dataAtual)
    {
        var capacidadeResult = fundo.AceitaAgendamentoAporte();
        if (capacidadeResult.IsFailure) return Result<Ordem>.Failure(capacidadeResult.GetError());

        return Ordem.CriarAgendada(cliente.Id, fundo.Id, TipoOperacao.APORTE, quantidadeCotas, dataAgendamento, dataAtual);
    }

    public Result<(Ordem Ordem, PosicaoCliente Posicao)> CriarImediata(Cliente cliente, Fundo fundo, PosicaoCliente? posicaoAtual, int quantidadeCotas, DateOnly dataAtual)
    {
        var valorTotal = quantidadeCotas * fundo.ValorCota;

        var aceitaAporte = fundo.AceitaAporte(valorTotal);
        if (aceitaAporte.IsFailure) return Result<(Ordem, PosicaoCliente)>.Failure(aceitaAporte.GetError());

        var debito = cliente.DebitarSaldo(valorTotal);
        if (debito.IsFailure) return Result<(Ordem, PosicaoCliente)>.Failure(debito.GetError());

        var ordemResult = Ordem.CriarImediata(cliente.Id, fundo.Id, TipoOperacao.APORTE, quantidadeCotas, dataAtual);
        if (ordemResult.IsFailure) return Result<(Ordem, PosicaoCliente)>.Failure(ordemResult.GetError());

        var ordem = ordemResult.GetSuccess();
        var concluirResult = ordem.Concluir();
        if (concluirResult.IsFailure) return Result<(Ordem, PosicaoCliente)>.Failure(concluirResult.GetError());

        var posicao = posicaoAtual ?? new PosicaoCliente(cliente.Id, fundo.Id, 0);
        var adicionarCotasResult = posicao.AdicionarCotas(quantidadeCotas);
        if (adicionarCotasResult.IsFailure) return Result<(Ordem, PosicaoCliente)>.Failure(adicionarCotasResult.GetError());

        return Result<(Ordem, PosicaoCliente)>.Success((ordem, posicao));
    }

    public Result<PosicaoCliente> ProcessarOrdemPendente(Ordem ordem, Cliente cliente, Fundo fundo, PosicaoCliente? posicaoAtual)
    {
        var valorTotal = ordem.QuantidadeCotas * fundo.ValorCota;

        var aceitaAporte = fundo.AceitaAporte(valorTotal);
        if (aceitaAporte.IsFailure) return Result<PosicaoCliente>.Failure(aceitaAporte.GetError());

        var debito = cliente.DebitarSaldo(valorTotal);
        if (debito.IsFailure) return Result<PosicaoCliente>.Failure(debito.GetError());

        var posicao = posicaoAtual ?? new PosicaoCliente(cliente.Id, fundo.Id, 0);
        var adicionarCotasResult = posicao.AdicionarCotas(ordem.QuantidadeCotas);
        if (adicionarCotasResult.IsFailure) return Result<PosicaoCliente>.Failure(adicionarCotasResult.GetError());

        return Result<PosicaoCliente>.Success(posicao);
    }
}