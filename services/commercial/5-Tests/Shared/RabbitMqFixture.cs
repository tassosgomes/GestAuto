using Testcontainers.RabbitMq;
using RabbitMQ.Client;
using Xunit;

namespace GestAuto.Commercial.Tests.Shared;

public class RabbitMqFixture : IAsyncLifetime
{
    private readonly RabbitMqContainer _container = new RabbitMqBuilder()
        .WithImage("rabbitmq:3.13-management")
        .WithUsername("test")
        .WithPassword("test")
        .Build();

    public string HostName => _container.Hostname;
    public int Port => _container.GetMappedPublicPort(5672);

    public async Task InitializeAsync()
    {
        await _container.StartAsync();
    }

    public async Task DisposeAsync()
    {
        await _container.DisposeAsync();
    }

    public IConnection CreateConnection()
    {
        var factory = new ConnectionFactory
        {
            HostName = HostName,
            Port = Port,
            UserName = "test",
            Password = "test",
            DispatchConsumersAsync = true
        };

        return factory.CreateConnection();
    }

    public void PurgeQueue(string queueName)
    {
        using var connection = CreateConnection();
        using var channel = connection.CreateModel();
        channel.QueuePurge(queueName);
    }
}

[CollectionDefinition("RabbitMq")]
public class RabbitMqCollection : ICollectionFixture<RabbitMqFixture> { }
