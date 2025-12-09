using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using GestAuto.Commercial.API.Tests.Shared;
using GestAuto.Commercial.Application.DTOs;
using GestAuto.Commercial.Tests.Shared;
using Xunit;

namespace GestAuto.Commercial.IntegrationTest.API;

[Collection("Integration")]
public class LeadApiTests : IAsyncLifetime
{
    private readonly CustomWebApplicationFactory _factory;
    private HttpClient _client = null!;
    private readonly Guid _salesPersonId = Guid.NewGuid();

    public LeadApiTests(PostgresFixture postgresFixture, RabbitMqFixture rabbitMqFixture)
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
    public async Task CreateLead_ShouldReturn201()
    {
        var request = new
        {
            Name = "Maria Santos",
            Email = "maria@email.com",
            Phone = "11999997777",
            Source = "showroom",
            InterestedModel = "Corolla"
        };

        var response = await _client.PostAsJsonAsync("/api/v1/leads", request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var lead = await response.Content.ReadFromJsonAsync<LeadResponse>();
        lead.Should().NotBeNull();
        lead!.Name.Should().Be("Maria Santos");
        lead.Status.Should().Be("New");
    }

    [Fact]
    public async Task QualifyLead_ShouldReturnDiamondScore()
    {
        var createResponse = await _client.PostAsJsonAsync("/api/v1/leads", new
        {
            Name = "Carlos Diamond",
            Email = "carlos@email.com",
            Phone = "11999996666",
            Source = "showroom"
        });

        var created = await createResponse.Content.ReadFromJsonAsync<LeadResponse>();

        var qualifyRequest = new
        {
            HasTradeInVehicle = true,
            TradeInVehicle = new
            {
                Brand = "Honda",
                Model = "Civic",
                Year = 2020,
                Mileage = 30000,
                LicensePlate = "ABC1D23",
                Color = "Prata",
                GeneralCondition = "Bom",
                HasDealershipServiceHistory = true
            },
            PaymentMethod = "Financing",
            ExpectedPurchaseDate = DateTime.UtcNow.AddDays(10),
            InterestedInTestDrive = true
        };

        var response = await _client.PostAsJsonAsync($"/api/v1/leads/{created!.Id}/qualify", qualifyRequest);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var qualified = await response.Content.ReadFromJsonAsync<LeadResponse>();
        qualified!.Score.Should().Be("Diamond");
    }

    [Fact]
    public async Task ListLeads_ShouldApplySalesPersonFilter()
    {
        // lead for salesperson A
        await _client.PostAsJsonAsync("/api/v1/leads", new
        {
            Name = "Lead A",
            Email = "a@test.com",
            Phone = "11999990001",
            Source = "google"
        });

        // lead for salesperson B
        using var clientB = _factory.CreateClient();
        var otherSalesPerson = Guid.NewGuid();
        clientB.DefaultRequestHeaders.Add("X-Test-SalesPersonId", otherSalesPerson.ToString());
        await clientB.PostAsJsonAsync("/api/v1/leads", new
        {
            Name = "Lead B",
            Email = "b@test.com",
            Phone = "11999990002",
            Source = "google"
        });

        var response = await _client.GetAsync("/api/v1/leads?page=1&pageSize=10");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var list = await response.Content.ReadFromJsonAsync<PagedResponse<LeadListItemResponse>>();
        list!.Items.Should().OnlyContain(l => l.SalesPersonId == _salesPersonId);
    }
}
