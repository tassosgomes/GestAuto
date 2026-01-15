using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using GestAuto.Stock.Application.Reservations.Dto;
using GestAuto.Stock.Application.Vehicles.Dto;
using GestAuto.Stock.Domain.Enums;
using GestAuto.Stock.IntegrationTest.Shared;
using GestAuto.Stock.Tests.Shared;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace GestAuto.Stock.IntegrationTest.Controllers;

[Collection("Postgres")]
public class OutboxFromHttpOperationsTests
{
    private readonly PostgresFixture _postgresFixture;
    private readonly CustomWebApplicationFactory _factory;

    public OutboxFromHttpOperationsTests(PostgresFixture postgresFixture)
    {
        _postgresFixture = postgresFixture;
        _factory = new CustomWebApplicationFactory(postgresFixture);
    }

    [SkippableFact]
    public async Task CreateReservation_And_CheckOutSale_ShouldWriteExpectedOutboxMessages()
    {
        Skip.IfNot(_postgresFixture.IsAvailable, $"Docker/Testcontainers indisponível: {_postgresFixture.UnavailableReason}");

        await _factory.ResetStateAsync();

        var userId = Guid.NewGuid();
        using var client = _factory.CreateClient().WithTestAuth(userId, role: "SALES_PERSON");

        var vehicleId = await CreateUsedVehicleAsync(client);

        var reservationRequest = new CreateReservationRequest(
            Type: ReservationType.Standard,
            ContextType: "lead",
            ContextId: Guid.NewGuid(),
            BankDeadlineAtUtc: null);

        var reservationResponse = await client.PostAsJsonAsync($"/api/v1/vehicles/{vehicleId}/reservations", reservationRequest);
        reservationResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var checkOutResponse = await client.PostAsJsonAsync(
            $"/api/v1/vehicles/{vehicleId}/check-outs",
            new CheckOutCreateRequest(CheckOutReason.Sale, null, "venda"));

        checkOutResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        await using var ctx = _postgresFixture.CreateContext();
        var eventTypes = await ctx.OutboxMessages.Select(m => m.EventType).ToListAsync();

        eventTypes.Should().Contain(t => t.Contains("ReservationCreatedEvent"));
        eventTypes.Should().Contain(t => t.Contains("VehicleStatusChangedEvent"));
        eventTypes.Should().Contain(t => t.Contains("VehicleSoldEvent"));
    }

    private static async Task<Guid> CreateUsedVehicleAsync(HttpClient client)
    {
        var create = new VehicleCreate(
            Category: VehicleCategory.Used,
            Vin: $"VIN-{Guid.NewGuid():N}",
            Make: "Renault",
            Model: "Duster",
            YearModel: 2020,
            Color: "Verde",
            Plate: $"PQR{Random.Shared.Next(1000, 9999)}",
            Trim: null,
            MileageKm: 25000,
            EvaluationId: Guid.NewGuid(),
            DemoPurpose: null,
            IsRegistered: false);

        var response = await client.PostAsJsonAsync("/api/v1/vehicles", create);
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var body = await response.Content.ReadFromJsonAsync<VehicleResponse>();
        body.Should().NotBeNull();

        return body!.Id;
    }
}
