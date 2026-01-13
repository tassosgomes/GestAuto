using FluentAssertions;
using System.Net;
using GestAuto.Stock.IntegrationTest.Shared;
using GestAuto.Stock.Tests.Shared;
using Xunit.Sdk;

namespace GestAuto.Stock.IntegrationTest;

[Collection("Postgres")]
public class HealthCheckTests : IClassFixture<PostgresFixture>
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly PostgresFixture _postgresFixture;

    public HealthCheckTests(PostgresFixture postgresFixture)
    {
        _postgresFixture = postgresFixture;
        _factory = new CustomWebApplicationFactory(postgresFixture);
    }

    [SkippableFact]
    public async Task GetHealth_ShouldReturn200()
    {
        Skip.IfNot(_postgresFixture.IsAvailable, $"Docker/Testcontainers indisponível: {_postgresFixture.UnavailableReason}");

        using var client = _factory.CreateClient();
        var response = await client.GetAsync("/health");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
