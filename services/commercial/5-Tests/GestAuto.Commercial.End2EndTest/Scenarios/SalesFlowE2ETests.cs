using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using GestAuto.Commercial.API.Tests.Shared;
using GestAuto.Commercial.Application.DTOs;
using GestAuto.Commercial.Tests.Shared;
using Xunit;

namespace GestAuto.Commercial.End2EndTest.Scenarios;

[Collection("Integration")]
public class SalesFlowE2ETests : IAsyncLifetime
{
    private readonly CustomWebApplicationFactory _factory;
    private HttpClient _client = null!;

    public SalesFlowE2ETests(PostgresFixture postgresFixture, RabbitMqFixture rabbitMqFixture)
    {
        _factory = new CustomWebApplicationFactory(postgresFixture, rabbitMqFixture);
    }

    public async Task InitializeAsync()
    {
        await _factory.ResetStateAsync();
        _client = _factory.CreateClient();
    }

    public Task DisposeAsync()
    {
        _client.Dispose();
        _factory.Dispose();
        return Task.CompletedTask;
    }

    [Fact]
    public async Task CompleteFlow_FromLeadToClosedSale()
    {
        var leadResponse = await _client.PostAsJsonAsync("/api/v1/leads", new
        {
            Name = "Cliente Premium",
            Email = "premium@test.com",
            Phone = "11999995555",
            Source = "showroom",
            InterestedModel = "Camry"
        });
        var lead = await leadResponse.Content.ReadFromJsonAsync<LeadResponse>();
        lead.Should().NotBeNull();
        lead!.Score.Should().Be("Bronze");

        var qualifyResponse = await _client.PostAsJsonAsync($"/api/v1/leads/{lead.Id}/qualify", new
        {
            HasTradeInVehicle = true,
            TradeInVehicle = new
            {
                Brand = "Toyota",
                Model = "Corolla",
                Year = 2021,
                Mileage = 25000,
                LicensePlate = "DEF2G34",
                Color = "Branco",
                GeneralCondition = "Excelente",
                HasDealershipServiceHistory = true
            },
            PaymentMethod = "Financing",
            ExpectedPurchaseDate = DateTime.UtcNow.AddDays(5),
            InterestedInTestDrive = true
        });
        var qualifiedLead = await qualifyResponse.Content.ReadFromJsonAsync<LeadResponse>();
        qualifiedLead!.Score.Should().Be("Diamond");

        var proposalResponse = await _client.PostAsJsonAsync("/api/v1/proposals", new
        {
            LeadId = lead.Id,
            VehicleModel = "Camry",
            VehicleTrim = "XLE V6",
            VehicleColor = "Preto",
            VehicleYear = 2025,
            IsReadyDelivery = true,
            VehiclePrice = 280000,
            PaymentMethod = "Financing",
            DownPayment = 80000,
            Installments = 48
        });
        var proposal = await proposalResponse.Content.ReadFromJsonAsync<ProposalResponse>();
        proposal.Should().NotBeNull();

        await _client.PostAsJsonAsync($"/api/v1/proposals/{proposal!.Id}/items", new
        {
            Description = "Película de proteção solar",
            Value = 1500
        });

        var closeResponse = await _client.PostAsync($"/api/v1/proposals/{proposal.Id}/close", null);
        closeResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var closedProposal = await closeResponse.Content.ReadFromJsonAsync<ProposalResponse>();
        closedProposal!.Status.Should().Be("Closed");

        var finalLeadResponse = await _client.GetAsync($"/api/v1/leads/{lead.Id}");
        var finalLead = await finalLeadResponse.Content.ReadFromJsonAsync<LeadResponse>();
        finalLead!.Status.Should().Be("Converted");
    }
}
