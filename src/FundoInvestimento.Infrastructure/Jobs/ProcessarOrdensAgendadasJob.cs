using FundoInvestimento.Domain.Interfaces.UseCases;
using Quartz;
using Microsoft.Extensions.Logging;

namespace FundoInvestimento.Infrastructure.Jobs;

[DisallowConcurrentExecution]
public class ProcessarOrdensAgendadasJob : IJob
{
    private readonly IProcessarOrdensAgendadasUseCase _useCase;
    private readonly ILogger<ProcessarOrdensAgendadasJob> _logger;

    public ProcessarOrdensAgendadasJob(
        IProcessarOrdensAgendadasUseCase useCase,
        ILogger<ProcessarOrdensAgendadasJob> logger)
    {
        _useCase = useCase;
        _logger = logger;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        _logger.LogInformation("Job acionado: Iniciando processamento em lote de ordens agendadas.");

        await _useCase.ExecuteAsync(context.CancellationToken);

        _logger.LogInformation("Job finalizado.");
    }
}