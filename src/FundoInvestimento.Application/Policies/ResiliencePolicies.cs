using Polly;
using Polly.Retry;
using System.Data.Common;

namespace FundoInvestimento.Application.Policies;

/// <summary>
/// Centraliza as políticas de resiliência da aplicação.
/// </summary>
public static class ResiliencePolicies
{
    /// <summary>
    /// Política de Retry com Backoff Exponencial
    /// específica para falhas transientes de banco de dados ou timeouts.
    /// </summary>
    public static AsyncRetryPolicy DbRetryPolicy =>
        Policy
            .Handle<DbException>()
            .Or<TimeoutException>()
            .WaitAndRetryAsync(
                retryCount: 3,
                sleepDurationProvider: attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt)),
                onRetry: (exception, timeSpan, retryCount, context) =>
                {
                    Console.WriteLine($"Falha no banco de dados. Tentativa {retryCount} de 3. Aguardando {timeSpan.TotalSeconds}s. Erro: {exception.Message}");
                });
}