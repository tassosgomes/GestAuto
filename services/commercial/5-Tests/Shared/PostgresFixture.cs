using Testcontainers.PostgreSql;
using GestAuto.Commercial.Infra;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace GestAuto.Commercial.Tests.Shared;

public class PostgresFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder()
        .WithImage("postgres:16-alpine")
        .WithDatabase("commercial_test")
        .WithUsername("test")
        .WithPassword("test")
        .Build();

    public string ConnectionString => _container.GetConnectionString();

    public async Task InitializeAsync()
    {
        await _container.StartAsync();
        await ApplyMigrationsAsync();
    }

    public async Task DisposeAsync()
    {
        await _container.DisposeAsync();
    }

    public CommercialDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<CommercialDbContext>()
            .UseNpgsql(ConnectionString, opts =>
            {
                opts.MigrationsAssembly(typeof(CommercialDbContext).Assembly.FullName);
            })
            .Options;

        return new CommercialDbContext(options);
    }

    public async Task ResetDatabaseAsync()
    {
        await using var context = CreateContext();
        await context.Database.EnsureDeletedAsync();
        await context.Database.MigrateAsync();
    }

    private async Task ApplyMigrationsAsync()
    {
        await using var context = CreateContext();
        await context.Database.MigrateAsync();
    }
}

[CollectionDefinition("Postgres")]
public class PostgresCollection : ICollectionFixture<PostgresFixture> { }
