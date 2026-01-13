using GestAuto.Stock.Domain.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client;

namespace GestAuto.Stock.Infra.Messaging;

/// <summary>
/// Configuração centralizada de RabbitMQ.
/// Define exchanges e routing keys para comunicação assíncrona.
/// </summary>
public class RabbitMqConfiguration
{
    public string HostName { get; set; } = "localhost";
    public int Port { get; set; } = 5672;
    public string UserName { get; set; } = "guest";
    public string Password { get; set; } = "guest";
    public string VirtualHost { get; set; } = "/";

    public const string StockExchange = "stock";

    public static class RoutingKeys
    {
        public const string VehicleCheckedIn = "vehicle.checked-in";
        public const string VehicleStatusChanged = "vehicle.status-changed";
        public const string ReservationCreated = "reservation.created";
        public const string ReservationCancelled = "reservation.cancelled";
        public const string ReservationExtended = "reservation.extended";
        public const string ReservationExpired = "reservation.expired";
        public const string VehicleSold = "vehicle.sold";
        public const string VehicleTestDriveStarted = "vehicle.test-drive.started";
        public const string VehicleTestDriveCompleted = "vehicle.test-drive.completed";
        public const string VehicleWrittenOff = "vehicle.written-off";
    }
}

public static class RabbitMqExtensions
{
    /// <summary>
    /// Registra serviços de RabbitMQ no container de injeção de dependência.
    /// Configura a conexão AMQP de forma lazy (apenas quando necessária).
    /// </summary>
    public static IServiceCollection AddRabbitMq(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var config = new RabbitMqConfiguration();
        configuration.GetSection("RabbitMQ").Bind(config);

        services.AddSingleton(config);

        services.AddSingleton<Lazy<IConnection>>(_ =>
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

        services.AddSingleton<IConnection>(sp =>
        {
            var lazy = sp.GetRequiredService<Lazy<IConnection>>();
            return lazy.Value;
        });

        services.AddScoped<IEventPublisher, RabbitMqPublisher>();

        return services;
    }
}
