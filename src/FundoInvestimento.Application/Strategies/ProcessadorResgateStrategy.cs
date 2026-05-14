using FundoInvestimento.Domain.Entities;
using FundoInvestimento.Domain.Enums;
using FundoInvestimento.Domain.Interfaces.Strategies;
using FundoInvestimento.Libs.Utils;

namespace FundoInvestimento.Application.Strategies;

public class ProcessadorResgateStrategy : IProcessadorOperacaoStrategy
{
    public TipoOperacao TipoOperacao => TipoOperacao.RESGATE;

    public Result<Ordem> CriarAgendamento(Cliente cliente, Fundo fundo, PosicaoCliente? posicaoAtual, int quantidadeCotas, DateOnly dataAgendamento, DateOnly dataAtual)
    {
        if (posicaoAtual is null)
            return Result<Ordem>.Failure(new CustomError("POSICAO_INEXISTENTE", "O cliente não possui posições ativas neste fundo.", 422));

        var validacaoResult = posicaoAtual.ValidarRegrasDeResgate(quantidadeCotas, fundo);
        if (validacaoResult.IsFailure) return Result<Ordem>.Failure(validacaoResult.GetError());

        return Ordem.CriarAgendada(cliente.Id, fundo.Id, TipoOperacao.RESGATE, quantidadeCotas, dataAgendamento, dataAtual);
    }

    public Result<(Ordem Ordem, PosicaoCliente Posicao)> CriarImediata(Cliente cliente, Fundo fundo, PosicaoCliente? posicaoAtual, int quantidadeCotas, DateOnly dataAtual)
    {
        if (posicaoAtual is null)
            return Result<(Ordem, PosicaoCliente)>.Failure(new CustomError("POSICAO_INEXISTENTE", "O cliente não possui posições ativas neste fundo.", 422));

        var validacaoResult = posicaoAtual.ValidarRegrasDeResgate(quantidadeCotas, fundo);
        if (validacaoResult.IsFailure) return Result<(Ordem, PosicaoCliente)>.Failure(validacaoResult.GetError());

        var valorResgate = quantidadeCotas * fundo.ValorCota;

        var removerCotasResult = posicaoAtual.RemoverCotas(quantidadeCotas);
        if (removerCotasResult.IsFailure) return Result<(Ordem, PosicaoCliente)>.Failure(removerCotasResult.GetError());

        var creditarSaldoResult = cliente.CreditarSaldo(valorResgate);
        if (creditarSaldoResult.IsFailure) return Result<(Ordem, PosicaoCliente)>.Failure(creditarSaldoResult.GetError());

        var ordemResult = Ordem.CriarImediata(cliente.Id, fundo.Id, TipoOperacao.RESGATE, quantidadeCotas, dataAtual);
        if (ordemResult.IsFailure) return Result<(Ordem, PosicaoCliente)>.Failure(ordemResult.GetError());

        var ordem = ordemResult.GetSuccess();
        var concluirResult = ordem.Concluir();
        if (concluirResult.IsFailure) return Result<(Ordem, PosicaoCliente)>.Failure(concluirResult.GetError());

        return Result<(Ordem, PosicaoCliente)>.Success((ordem, posicaoAtual));
    }

    public Result<PosicaoCliente> ProcessarOrdemPendente(Ordem ordem, Cliente cliente, Fundo fundo, PosicaoCliente? posicaoAtual)
    {
        if (posicaoAtual is null)
            return Result<PosicaoCliente>.Failure(new CustomError("POSICAO_INEXISTENTE", "O cliente não possui posições ativas neste fundo.", 422));

        var validacaoResult = posicaoAtual.ValidarRegrasDeResgate(ordem.QuantidadeCotas, fundo);
        if (validacaoResult.IsFailure) return Result<PosicaoCliente>.Failure(validacaoResult.GetError());

        var valorResgate = ordem.QuantidadeCotas * fundo.ValorCota;

        var removerCotasResult = posicaoAtual.RemoverCotas(ordem.QuantidadeCotas);
        if (removerCotasResult.IsFailure) return Result<PosicaoCliente>.Failure(removerCotasResult.GetError());

        var creditarSaldoResult = cliente.CreditarSaldo(valorResgate);
        if (creditarSaldoResult.IsFailure) return Result<PosicaoCliente>.Failure(creditarSaldoResult.GetError());

        return Result<PosicaoCliente>.Success(posicaoAtual);
    }
}