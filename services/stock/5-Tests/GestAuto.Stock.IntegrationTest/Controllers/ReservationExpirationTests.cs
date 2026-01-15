using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using GestAuto.Stock.API.Services;
using GestAuto.Stock.Application.Reservations.Dto;
using GestAuto.Stock.Application.Vehicles.Dto;
using GestAuto.Stock.Domain.Enums;
using GestAuto.Stock.IntegrationTest.Shared;
using GestAuto.Stock.Tests.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace GestAuto.Stock.IntegrationTest.Controllers;

[Collection("Postgres")]
public class ReservationExpirationTests
{
    private readonly PostgresFixture _postgresFixture;
    private readonly CustomWebApplicationFactory _factory;

    public ReservationExpirationTests(PostgresFixture postgresFixture)
    {
        _postgresFixture = postgresFixture;
        _factory = new CustomWebApplicationFactory(postgresFixture);
    }

    [SkippableFact]
    public async Task ReservationExpirationRunner_ShouldExpireStandardReservation_WhenExpiresAtIsPast()
    {
        Skip.IfNot(_postgresFixture.IsAvailable, $"Docker/Testcontainers indisponível: {_postgresFixture.UnavailableReason}");

        await _factory.ResetStateAsync();

        var salesPersonId = Guid.NewGuid();
        using var client = _factory.CreateClient().WithTestAuth(salesPersonId, role: "SALES_PERSON");

        var vehicleId = await CreateUsedVehicleAsync(client);

        var createReservation = new CreateReservationRequest(
            Type: ReservationType.Standard,
            ContextType: "lead",
            ContextId: Guid.NewGuid(),
            BankDeadlineAtUtc: null);

        var reservationResponse = await client.PostAsJsonAsync($"/api/v1/vehicles/{vehicleId}/reservations", createReservation);
        reservationResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var reservation = await reservationResponse.Content.ReadFromJsonAsync<ReservationResponse>();
        reservation.Should().NotBeNull();

        // Force expiration time to the past (simulate time) without depending on real clock.
        var past = DateTime.UtcNow.AddMinutes(-5);

        await using (var ctx = _postgresFixture.CreateContext())
        {
            await ctx.Database.ExecuteSqlRawAsync(
                "UPDATE stock.reservations SET expires_at_utc = {0} WHERE id = {1}",
                past,
                reservation!.Id);
        }

        using (var scope = _factory.Services.CreateScope())
        {
            var runner = scope.ServiceProvider.GetRequiredService<ReservationExpirationRunner>();
            var expired = await runner.ExpireDueReservationsOnceAsync(DateTime.UtcNow, batchSize: 50, CancellationToken.None);
            expired.Should().BeGreaterThan(0);
        }

        await using (var ctx = _postgresFixture.CreateContext())
        {
            var storedReservation = await ctx.Reservations.SingleAsync(r => r.Id == reservation!.Id);
            storedReservation.Status.Should().Be(ReservationStatus.Expired);

            var storedVehicle = await ctx.Vehicles.SingleAsync(v => v.Id == vehicleId);
            storedVehicle.CurrentStatus.Should().Be(VehicleStatus.InStock);

            var outboxTypes = await ctx.OutboxMessages
                .Select(m => m.EventType)
                .ToListAsync();

            outboxTypes.Should().Contain(t => t.Contains("ReservationExpiredEvent"));
            outboxTypes.Should().Contain(t => t.Contains("VehicleStatusChangedEvent"));
        }
    }

    private static async Task<Guid> CreateUsedVehicleAsync(HttpClient client)
    {
        var create = new VehicleCreate(
            Category: VehicleCategory.Used,
            Vin: $"VIN-{Guid.NewGuid():N}",
            Make: "Volkswagen",
            Model: "Polo",
            YearModel: 2021,
            Color: "Branco",
            Plate: $"DEF{Random.Shared.Next(1000, 9999)}",
            Trim: "Highline",
            MileageKm: 54321,
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
