using GestAuto.Commercial.Infra;
using GestAuto.Commercial.Infra.Messaging;
using GestAuto.Commercial.Tests.Shared;
using GestAuto.Commercial.Domain.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using RabbitMQ.Client;
using Xunit;

namespace GestAuto.Commercial.API.Tests.Shared;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly PostgresFixture _postgresFixture;
    private readonly RabbitMqFixture _rabbitMqFixture;

    public CustomWebApplicationFactory(PostgresFixture postgresFixture, RabbitMqFixture rabbitMqFixture)
    {
        _postgresFixture = postgresFixture;
        _rabbitMqFixture = rabbitMqFixture;
    }

    public async Task ResetStateAsync()
    {
        await _postgresFixture.ResetDatabaseAsync();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");

        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll(typeof(DbContextOptions<CommercialDbContext>));
            services.AddDbContext<CommercialDbContext>(options =>
            {
                options.UseNpgsql(_postgresFixture.ConnectionString, opts =>
                {
                    opts.MigrationsAssembly(typeof(CommercialDbContext).Assembly.FullName);
                });
            });

            services.RemoveAll(typeof(IConnection));
            services.AddSingleton(_rabbitMqFixture.CreateConnection());

            services.RemoveAll(typeof(IEventPublisher));
            services.AddScoped<IEventPublisher, RabbitMqPublisher>();

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = TestAuthHandler.Scheme;
                options.DefaultChallengeScheme = TestAuthHandler.Scheme;
            }).AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(TestAuthHandler.Scheme, _ => { });

            services.AddAuthorization(options =>
            {
                options.AddPolicy("SalesPerson", policy =>
                    policy.RequireClaim("role", "sales_person", "manager"));
                options.AddPolicy("Manager", policy =>
                    policy.RequireClaim("role", "manager"));
            });
        });
    }
}

[CollectionDefinition("Integration")]
public class IntegrationCollection : ICollectionFixture<PostgresFixture>, ICollectionFixture<RabbitMqFixture>
{
}



