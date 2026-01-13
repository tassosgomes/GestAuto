using System.Collections.Generic;
using GestAuto.Stock.Infra;
using GestAuto.Stock.Tests.Shared;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace GestAuto.Stock.IntegrationTest.Shared;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly PostgresFixture _postgresFixture;

    public CustomWebApplicationFactory(PostgresFixture postgresFixture)
    {
        _postgresFixture = postgresFixture;
    }

    public async Task ResetStateAsync()
    {
        await _postgresFixture.ResetDatabaseAsync();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");

        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:StockDatabase"] = _postgresFixture.ConnectionString
            });
        });

        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll(typeof(DbContextOptions<StockDbContext>));
            services.AddDbContext<StockDbContext>(options =>
            {
                options.UseNpgsql(_postgresFixture.ConnectionString, opts =>
                {
                    opts.MigrationsAssembly(typeof(StockDbContext).Assembly.FullName);
                });
            });
        });
    }
}
