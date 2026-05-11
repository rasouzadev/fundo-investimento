using FundoInvestimento.Libs.Utils;

namespace FundoInvestimento.Domain.Entities;

/// <summary>
/// Representa um cliente investidor dentro do sistema financeiro.
/// </summary>
public class Cliente
{
    /// <summary>
    /// Identificador único do cliente.
    /// </summary>
    public Guid Id { get; init; }

    /// <summary>
    /// Nome completo do cliente.
    /// </summary>
    public string Nome { get; private set; }

    /// <summary>
    /// Documento de identificação (CPF) do cliente.
    /// </summary>
    public string Cpf { get; private set; }

    /// <summary>
    /// Saldo financeiro disponível em conta corrente para realização de novos aportes.
    /// </summary>
    public decimal SaldoDisponivel { get; private set; }

    /// <summary>
    /// Construtor vazio exigido por ferramentas de ORM.
    /// </summary>
    protected Cliente() { }

    /// <summary>
    /// Inicializa uma nova instância de um Cliente.
    /// </summary>
    /// <param name="nome">Nome do cliente.</param>
    /// <param name="cpf">CPF do cliente (apenas números).</param>
    /// <param name="saldoInicial">Saldo inicial na criação da conta (opcional, padrão zero).</param>
    public Cliente(string nome, string cpf, decimal saldoInicial = 0)
    {
        Id = Guid.CreateVersion7();
        Nome = nome;
        Cpf = cpf;
        SaldoDisponivel = saldoInicial;
    }

    /// <summary>
    /// Verifica se o cliente possui saldo disponível em conta suficiente para cobrir um determinado valor.
    /// </summary>
    /// <param name="valorNecessario">O valor financeiro a ser validado.</param>
    /// <returns>Retorna <c>true</c> se o saldo for maior ou igual ao valor necessário; caso contrário, <c>false</c>.</returns>
    public bool TemSaldoSuficiente(decimal valorNecessario)
    {
        return SaldoDisponivel >= valorNecessario;
    }

    /// <summary>
    /// Deduz um valor financeiro do saldo disponível do cliente, simulando o pagamento de um aporte.
    /// </summary>
    /// <param name="valor">Valor a ser debitado da conta.</param>
    /// <returns>Um <see cref="Result"/> indicando sucesso ou detalhando a falha de regra de negócio.</returns>
    public Result DebitarSaldo(decimal valor)
    {
        if (!TemSaldoSuficiente(valor))
        {
            return Result.Failure(new CustomError(
                code: "SALDO_INSUFICIENTE",
                message: "O cliente não possui saldo disponível suficiente para realizar esta operação.",
                statusCode: 422));
        }

        SaldoDisponivel -= valor;
        return Result.Success();
    }

    /// <summary>
    /// Adiciona um valor financeiro ao saldo disponível do cliente, simulando a liquidação de um resgate.
    /// </summary>
    /// <param name="valor">Valor a ser creditado na conta.</param>
    /// <returns>Um <see cref="Result"/> indicando sucesso ou detalhando a falha na validação do valor.</returns>
    public Result CreditarSaldo(decimal valor)
    {
        if (valor <= 0)
        {
            return Result.Failure(new CustomError(
                code: "VALOR_CREDITO_INVALIDO",
                message: "O valor a ser creditado na conta do cliente deve ser maior que zero.",
                statusCode: 422));
        }

        SaldoDisponivel += valor;
        return Result.Success();
    }
}