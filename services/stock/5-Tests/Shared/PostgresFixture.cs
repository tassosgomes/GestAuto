using GestAuto.Stock.Infra;
using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;
using Xunit;

namespace GestAuto.Stock.Tests.Shared;

public class PostgresFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder()
        .WithImage("postgres:16-alpine")
        .WithDatabase("stock_test")
        .WithUsername("test")
        .WithPassword("test")
        .Build();

    public bool IsAvailable { get; private set; } = true;
    public string? UnavailableReason { get; private set; }

    public string ConnectionString => _container.GetConnectionString();

    public async Task InitializeAsync()
    {
        try
        {
            await _container.StartAsync();
            await ApplyMigrationsAsync();
            IsAvailable = true;
        }
        catch (Exception ex)
        {
            IsAvailable = false;
            UnavailableReason = ex.Message;
        }
    }

    public async Task DisposeAsync()
    {
        if (!IsAvailable)
        {
            return;
        }

        await _container.DisposeAsync();
    }

    public StockDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<StockDbContext>()
            .UseNpgsql(ConnectionString, opts =>
            {
                opts.MigrationsAssembly(typeof(StockDbContext).Assembly.FullName);
            })
            .Options;

        return new StockDbContext(options);
    }

    public async Task ResetDatabaseAsync()
    {
        if (!IsAvailable)
        {
            return;
        }

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
