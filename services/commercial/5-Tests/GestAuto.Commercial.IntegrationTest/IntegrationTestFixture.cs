using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using GestAuto.Commercial.Infra;
using GestAuto.Commercial.Application.DTOs;

namespace GestAuto.Commercial.IntegrationTest;

public class IntegrationTestFixture : IAsyncLifetime
{
    private WebApplicationFactory<Program> _factory = null!;
    public HttpClient Client { get; private set; } = null!;
    public IServiceScope ServiceScope { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        _factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    // Remove the production database context
                    var descriptor = services.SingleOrDefault(d =>
                        d.ServiceType == typeof(DbContextOptions<CommercialDbContext>));

                    if (descriptor != null)
                    {
                        services.Remove(descriptor);
                    }

                    // Add in-memory database for testing
                    services.AddDbContext<CommercialDbContext>(options =>
                    {
                        options.UseInMemoryDatabase("TestDatabase");
                    });
                });
            });

        Client = _factory.CreateClient();
        ServiceScope = _factory.Services.CreateScope();

        // Initialize database
        var dbContext = ServiceScope.ServiceProvider.GetRequiredService<CommercialDbContext>();
        await dbContext.Database.EnsureCreatedAsync();
    }

    public async Task DisposeAsync()
    {
        var dbContext = ServiceScope.ServiceProvider.GetRequiredService<CommercialDbContext>();
        await dbContext.Database.EnsureDeletedAsync();
        ServiceScope.Dispose();
        _factory?.Dispose();
    }

    public void SetAuthorizationHeader(string token)
    {
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }

    public void ClearAuthorizationHeader()
    {
        Client.DefaultRequestHeaders.Authorization = null;
    }
}

[CollectionDefinition("Integration Tests")]
public class IntegrationTestCollection : ICollectionFixture<IntegrationTestFixture>
{
    // This has no code, and never creates an instance of IntegrationTestFixture
    // It's just used to define the collection
}
