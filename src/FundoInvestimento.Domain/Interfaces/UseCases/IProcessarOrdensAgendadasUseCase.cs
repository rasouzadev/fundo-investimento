namespace FundoInvestimento.Domain.Interfaces.UseCases;

/// <summary>
/// Contrato para o motor de processamento em lote (Worker) responsável por efetivar ordens agendadas.
/// </summary>
public interface IProcessarOrdensAgendadasUseCase
{
    /// <summary>
    /// Busca as ordens agendadas pendentes do dia e as processa uma a uma, aplicando as regras de débito/crédito 
    /// e garantindo o isolamento transacional de cada operação.
    /// </summary>
    /// <param name="cancellationToken">Token para cancelamento assíncrono da operação em background.</param>
    Task ExecuteAsync(CancellationToken cancellationToken = default);
}