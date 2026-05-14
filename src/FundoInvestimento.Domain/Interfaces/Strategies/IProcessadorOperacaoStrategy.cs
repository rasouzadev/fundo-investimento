using FundoInvestimento.Domain.Entities;
using FundoInvestimento.Domain.Enums;
using FundoInvestimento.Libs.Utils;

namespace FundoInvestimento.Domain.Interfaces.Strategies;

/// <summary>
/// Centraliza as regras de negócio para criação e execução de ordens (Aporte e Resgate).
/// </summary>
public interface IProcessadorOperacaoStrategy
{
    TipoOperacao TipoOperacao { get; }

    /// <summary>
    /// Regras de Agendamento: Valida Capacity (Aporte) ou Posse de Cotas (Resgate) e gera a Ordem PENDENTE.
    /// Não altera saldo financeiro.
    /// </summary>
    Result<Ordem> CriarAgendamento(Cliente cliente, Fundo fundo, PosicaoCliente? posicaoAtual, int quantidadeCotas, DateOnly dataAgendamento, DateOnly dataAtual);

    /// <summary>
    /// Regras de Ordem Imediata: Valida Capacity, Saldo e Permanência. Gera a Ordem CONCLUÍDA e atualiza a posição e conta.
    /// </summary>
    Result<(Ordem Ordem, PosicaoCliente Posicao)> CriarImediata(Cliente cliente, Fundo fundo, PosicaoCliente? posicaoAtual, int quantidadeCotas, DateOnly dataAtual);

    /// <summary>
    /// Usado pelo Worker de processamento em lote. Efetiva uma ordem que estava pendente.
    /// </summary>
    Result<PosicaoCliente> ProcessarOrdemPendente(Ordem ordem, Cliente cliente, Fundo fundo, PosicaoCliente? posicaoAtual);
}