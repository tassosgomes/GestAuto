using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;
using GestAuto.Commercial.Domain.Interfaces;

namespace GestAuto.Commercial.Infra.Messaging;

/// <summary>
/// Configuração centralizada de RabbitMQ.
/// Define exchanges, queues e routing keys para comunicação assíncrona.
/// </summary>
public class RabbitMqConfiguration
{
    /// <summary>
    /// Nome do host onde RabbitMQ está rodando.
    /// </summary>
    public string HostName { get; set; } = "localhost";

    /// <summary>
    /// Porta AMQP do RabbitMQ (padrão: 5672).
    /// </summary>
    public int Port { get; set; } = 5672;

    /// <summary>
    /// Nome de usuário para autenticação RabbitMQ.
    /// </summary>
    public string UserName { get; set; } = "guest";

    /// <summary>
    /// Senha para autenticação RabbitMQ.
    /// </summary>
    public string Password { get; set; } = "guest";

    /// <summary>
    /// Virtual host RabbitMQ (padrão: /).
    /// </summary>
    public string VirtualHost { get; set; } = "/";

    /// <summary>
    /// Nome do exchange de eventos comerciais.
    /// </summary>
    public const string CommercialExchange = "commercial";

    /// <summary>
    /// Routing keys para eventos de domínio do módulo comercial.
    /// Usados para rotear eventos para os consumers corretos.
    /// </summary>
    public static class RoutingKeys
    {
        /// <summary>Lead foi criado no sistema.</summary>
        public const string LeadCreated = "lead.created";

        /// <summary>Lead foi pontuado/qualificado (Diamante, Ouro, Prata, Bronze).</summary>
        public const string LeadScored = "lead.scored";

        /// <summary>Status do lead foi alterado.</summary>
        public const string LeadStatusChanged = "lead.status-changed";

        /// <summary>Proposta comercial foi criada.</summary>
        public const string ProposalCreated = "proposal.created";

        /// <summary>Proposta comercial foi atualizada.</summary>
        public const string ProposalUpdated = "proposal.updated";

        /// <summary>Venda foi fechada com aprovação do cliente.</summary>
        public const string SaleClosed = "sale.closed";

        /// <summary>Test-drive foi agendado.</summary>
        public const string TestDriveScheduled = "test-drive.scheduled";

        /// <summary>Test-drive foi realizado/completado.</summary>
        public const string TestDriveCompleted = "test-drive.completed";

        /// <summary>Solicitação de avaliação de seminovo foi criada.</summary>
        public const string EvaluationRequested = "used-vehicle.evaluation-requested";
    }
}

/// <summary>
/// Extensões de serviço para configuração de RabbitMQ no container de DI.
/// </summary>
public static class RabbitMqExtensions
{
    /// <summary>
    /// Registra serviços de RabbitMQ no container de injeção de dependência.
    /// Configura a conexão AMQP de forma lazy (apenas quando necessária).
    /// </summary>
    /// <param name="services">Coleção de serviços do ASP.NET Core</param>
    /// <param name="configuration">Configuração da aplicação</param>
    /// <returns>Referência aos serviços para encadeamento de chamadas</returns>
    public static IServiceCollection AddRabbitMq(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Obter configuração de RabbitMQ ou usar defaults
        var config = new RabbitMqConfiguration();
        configuration.GetSection("RabbitMQ").Bind(config);

        // Registrar configuração como singleton
        services.AddSingleton(config);

        // Registrar conexão RabbitMQ como lazy singleton
        // Isso permite que a aplicação inicie mesmo se RabbitMQ não estiver disponível
        // A conexão só será estabelecida quando algum serviço realmente a precisar
        services.AddSingleton<Lazy<IConnection>>(sp =>
            new Lazy<IConnection>(() =>
            {
                var factory = new ConnectionFactory
                {
                    HostName = config.HostName,
                    Port = config.Port,
                    UserName = config.UserName,
                    Password = config.Password,
                    VirtualHost = config.VirtualHost,
                    AutomaticRecoveryEnabled = true,
                    NetworkRecoveryInterval = TimeSpan.FromSeconds(10),
                    RequestedConnectionTimeout = TimeSpan.FromSeconds(5)
                };

                return factory.CreateConnection();
            })
        );

        // Registrar factory que resolve a conexão lazy
        services.AddSingleton<IConnection>(sp =>
        {
            var lazy = sp.GetRequiredService<Lazy<IConnection>>();
            return lazy.Value;
        });

        // Registrar publisher de eventos
        services.AddScoped<IEventPublisher, RabbitMqPublisher>();

        return services;
    }
}
