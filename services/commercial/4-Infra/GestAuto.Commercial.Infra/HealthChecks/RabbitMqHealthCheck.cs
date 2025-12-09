using Microsoft.Extensions.Diagnostics.HealthChecks;
using RabbitMQ.Client;

namespace GestAuto.Commercial.Infra.HealthChecks;

/// <summary>
/// Health check para verificar a conexão com RabbitMQ.
/// Usado por orquestradores (Kubernetes) e load balancers para determinar
/// se a instância está saudável e pronta para processar requisições.
/// </summary>
public class RabbitMqHealthCheck : IHealthCheck
{
    private readonly IConnection _connection;

    public RabbitMqHealthCheck(IConnection connection)
    {
        _connection = connection ?? throw new ArgumentNullException(nameof(connection));
    }

    /// <summary>
    /// Verifica se a conexão RabbitMQ está aberta e saudável.
    /// </summary>
    /// <param name="context">Contexto do health check</param>
    /// <param name="cancellationToken">Token para cancelamento</param>
    /// <returns>Resultado do health check (Healthy, Degraded ou Unhealthy)</returns>
    public Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (_connection.IsOpen)
            {
                return Task.FromResult(
                    HealthCheckResult.Healthy("Conexão RabbitMQ está aberta e funcionando"));
            }

            return Task.FromResult(
                HealthCheckResult.Unhealthy("Conexão RabbitMQ está fechada"));
        }
        catch (Exception ex)
        {
            return Task.FromResult(
                HealthCheckResult.Unhealthy(
                    "Erro ao verificar saúde de RabbitMQ",
                    ex));
        }
    }
}
