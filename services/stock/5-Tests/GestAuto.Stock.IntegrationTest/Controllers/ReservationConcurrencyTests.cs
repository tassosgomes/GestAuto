using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using GestAuto.Stock.Application.Reservations.Dto;
using GestAuto.Stock.Application.Vehicles.Dto;
using GestAuto.Stock.Domain.Enums;
using GestAuto.Stock.Infra;
using GestAuto.Stock.IntegrationTest.Shared;
using GestAuto.Stock.Tests.Shared;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace GestAuto.Stock.IntegrationTest.Controllers;

[Collection("Postgres")]
public class ReservationConcurrencyTests
{
    private readonly PostgresFixture _postgresFixture;
    private readonly CustomWebApplicationFactory _factory;

    public ReservationConcurrencyTests(PostgresFixture postgresFixture)
    {
        _postgresFixture = postgresFixture;
        _factory = new CustomWebApplicationFactory(postgresFixture);
    }

    [SkippableFact]
    public async Task CreateReservation_ConcurrentRequests_ShouldAllowOnlyOneActiveReservation()
    {
        Skip.IfNot(_postgresFixture.IsAvailable, $"Docker/Testcontainers indisponível: {_postgresFixture.UnavailableReason}");

        await _factory.ResetStateAsync();

        var user1 = Guid.NewGuid();
        var user2 = Guid.NewGuid();

        using var client1 = _factory.CreateClient().WithTestAuth(user1, role: "SALES_PERSON");
        using var client2 = _factory.CreateClient().WithTestAuth(user2, role: "SALES_PERSON");

        var vehicleId = await CreateUsedVehicleAsync(client1);

        var request = new CreateReservationRequest(
            Type: ReservationType.Standard,
            ContextType: "lead",
            ContextId: Guid.NewGuid(),
            BankDeadlineAtUtc: null);

        var t1 = client1.PostAsJsonAsync($"/api/v1/vehicles/{vehicleId}/reservations", request);
        var t2 = client2.PostAsJsonAsync($"/api/v1/vehicles/{vehicleId}/reservations", request);

        await Task.WhenAll(t1, t2);

        var codes = new[] { t1.Result.StatusCode, t2.Result.StatusCode };
        codes.Should().Contain(HttpStatusCode.Created);
        codes.Should().Contain(HttpStatusCode.Conflict);

        await using var ctx = _postgresFixture.CreateContext();
        var activeCount = await ctx.Reservations.CountAsync(
            r => r.VehicleId == vehicleId && r.Status == ReservationStatus.Active);

        activeCount.Should().Be(1);
    }

    private static async Task<Guid> CreateUsedVehicleAsync(HttpClient client)
    {
        var create = new VehicleCreate(
            Category: VehicleCategory.Used,
            Vin: $"VIN-{Guid.NewGuid():N}",
            Make: "Fiat",
            Model: "Argo",
            YearModel: 2022,
            Color: "Preto",
            Plate: $"ABC{Random.Shared.Next(1000, 9999)}",
            Trim: "Drive",
            MileageKm: 12345,
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
