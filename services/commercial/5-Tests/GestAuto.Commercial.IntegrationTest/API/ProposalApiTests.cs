using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using GestAuto.Commercial.API.Tests.Shared;
using GestAuto.Commercial.Application.DTOs;
using GestAuto.Commercial.Tests.Shared;
using Xunit;

namespace GestAuto.Commercial.IntegrationTest.API;

[Collection("Integration")]
public class ProposalApiTests : IAsyncLifetime
{
    private readonly CustomWebApplicationFactory _factory;
    private HttpClient _client = null!;
    private readonly Guid _salesPersonId = Guid.NewGuid();

    public ProposalApiTests(PostgresFixture postgresFixture, RabbitMqFixture rabbitMqFixture)
    {
        _factory = new CustomWebApplicationFactory(postgresFixture, rabbitMqFixture);
    }

    public async Task InitializeAsync()
    {
        await _factory.ResetStateAsync();
        _client = _factory.CreateClient();
        _client.DefaultRequestHeaders.Add("X-Test-SalesPersonId", _salesPersonId.ToString());
    }

    public Task DisposeAsync()
    {
        _client.Dispose();
        _factory.Dispose();
        return Task.CompletedTask;
    }

    [Fact]
    public async Task ApplyDiscount_Above5Percent_ShouldRequireApproval()
    {
        var leadId = await CreateLeadAsync();
        var proposalId = await CreateProposalAsync(leadId, 100_000);

        var response = await _client.PostAsJsonAsync($"/api/v1/proposals/{proposalId}/discount", new
        {
            Amount = 6_000m,
            Reason = "Fidelizacao"
        });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var proposal = await response.Content.ReadFromJsonAsync<ProposalResponse>();
        proposal!.Status.Should().Be("AwaitingDiscountApproval");
    }

    [Fact]
    public async Task ApproveDiscount_AsSalesPerson_ShouldReturnForbidden()
    {
        var leadId = await CreateLeadAsync();
        var proposalId = await CreateProposalAsync(leadId, 100_000);

        await _client.PostAsJsonAsync($"/api/v1/proposals/{proposalId}/discount", new { Amount = 6_000m, Reason = "Teste" });

        var response = await _client.PostAsync($"/api/v1/proposals/{proposalId}/approve-discount", null);
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task CloseProposal_ShouldMarkLeadConverted()
    {
        var leadId = await CreateLeadAsync();
        var proposalId = await CreateProposalAsync(leadId, 150_000);

        var response = await _client.PostAsync($"/api/v1/proposals/{proposalId}/close", null);
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var leadResponse = await _client.GetAsync($"/api/v1/leads/{leadId}");
        var lead = await leadResponse.Content.ReadFromJsonAsync<LeadResponse>();
        lead!.Status.Should().Be("Converted");
    }

    private async Task<Guid> CreateLeadAsync()
    {
        var response = await _client.PostAsJsonAsync("/api/v1/leads", new
        {
            Name = "Lead API",
            Email = $"api{Guid.NewGuid()}@test.com",
            Phone = "11999998888",
            Source = "google"
        });

        var lead = await response.Content.ReadFromJsonAsync<LeadResponse>();
        return lead!.Id;
    }

    private async Task<Guid> CreateProposalAsync(Guid leadId, decimal vehiclePrice)
    {
        var response = await _client.PostAsJsonAsync("/api/v1/proposals", new
        {
            LeadId = leadId,
            VehicleModel = "Corolla",
            VehicleTrim = "XEi",
            VehicleColor = "Prata",
            VehicleYear = 2025,
            IsReadyDelivery = true,
            VehiclePrice = vehiclePrice,
            PaymentMethod = "Cash"
        });

        var proposal = await response.Content.ReadFromJsonAsync<ProposalResponse>();
        return proposal!.Id;
    }
}
