using System.Net;
using System.Net.Http.Json;
using GestAuto.Commercial.Application.DTOs;
using GestAuto.Commercial.Application.Commands;

namespace GestAuto.Commercial.IntegrationTest;

[Collection("Integration Tests")]
public class LeadControllerTests : IAsyncLifetime
{
    private readonly IntegrationTestFixture _fixture;
    private readonly HttpClient _client;

    public LeadControllerTests(IntegrationTestFixture fixture)
    {
        _fixture = fixture;
        _client = fixture.Client;
    }

    public async Task InitializeAsync()
    {
        await Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        await Task.CompletedTask;
    }

    [Fact]
    public async Task CreateLead_WithValidData_EndpointExists()
    {
        // Arrange
        var request = new CreateLeadRequest(
            "João Silva",
            "joao@test.com",
            "11999999999",
            "instagram",
            "Civic",
            "EX",
            "Prata"
        );

        _fixture.SetAuthorizationHeader("VALID_JWT_TOKEN_HERE");

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/leads", request);

        // Assert - endpoint exists (will return Unauthorized or Created, not NotFound)
        Assert.NotEqual(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetLeads_EndpointExists()
    {
        // Arrange
        _fixture.SetAuthorizationHeader("VALID_JWT_TOKEN_HERE");

        // Act
        var response = await _client.GetAsync("/api/v1/leads");

        // Assert - endpoint exists
        Assert.NotEqual(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetLeadById_EndpointExists()
    {
        // Arrange
        var leadId = Guid.NewGuid();
        _fixture.SetAuthorizationHeader("VALID_JWT_TOKEN_HERE");

        // Act
        var response = await _client.GetAsync($"/api/v1/leads/{leadId}");

        // Assert - endpoint exists
        Assert.NotEqual(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task UpdateLead_EndpointExists()
    {
        // Arrange
        var leadId = Guid.NewGuid();
        var request = new UpdateLeadRequest(
            "João Silva Updated",
            "joao.updated@test.com",
            "11999999998",
            "Civic",
            "EX",
            "Branco"
        );

        _fixture.SetAuthorizationHeader("VALID_JWT_TOKEN_HERE");

        // Act
        var response = await _client.PutAsJsonAsync($"/api/v1/leads/{leadId}", request);

        // Assert - endpoint exists
        Assert.NotEqual(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task ChangeLeadStatus_EndpointExists()
    {
        // Arrange
        var leadId = Guid.NewGuid();
        var request = new ChangeLeadStatusRequest("em_contato");

        _fixture.SetAuthorizationHeader("VALID_JWT_TOKEN_HERE");

        // Act
        var response = await _client.PatchAsJsonAsync($"/api/v1/leads/{leadId}/status", request);

        // Assert - endpoint exists
        Assert.NotEqual(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task QualifyLead_EndpointExists()
    {
        // Arrange
        var leadId = Guid.NewGuid();
        var request = new QualifyLeadRequest(
            true,
            new TradeInVehicleDto(
                "Toyota",
                "Corolla",
                2020,
                50000,
                "ABC1234",
                "Preto",
                "bom",
                true
            ),
            "financiamento",
            DateTime.UtcNow.AddMonths(1),
            true
        );

        _fixture.SetAuthorizationHeader("VALID_JWT_TOKEN_HERE");

        // Act
        var response = await _client.PostAsJsonAsync($"/api/v1/leads/{leadId}/qualify", request);

        // Assert - endpoint exists
        Assert.NotEqual(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task RegisterInteraction_EndpointExists()
    {
        // Arrange
        var leadId = Guid.NewGuid();
        var request = new RegisterInteractionRequest(
            "chamada_telefonica",
            "Cliente não atendeu"
        );

        _fixture.SetAuthorizationHeader("VALID_JWT_TOKEN_HERE");

        // Act
        var response = await _client.PostAsJsonAsync($"/api/v1/leads/{leadId}/interactions", request);

        // Assert - endpoint exists
        Assert.NotEqual(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetLeadInteractions_EndpointExists()
    {
        // Arrange
        var leadId = Guid.NewGuid();
        _fixture.SetAuthorizationHeader("VALID_JWT_TOKEN_HERE");

        // Act
        var response = await _client.GetAsync($"/api/v1/leads/{leadId}/interactions");

        // Assert - endpoint exists
        Assert.NotEqual(HttpStatusCode.NotFound, response.StatusCode);
    }
}
