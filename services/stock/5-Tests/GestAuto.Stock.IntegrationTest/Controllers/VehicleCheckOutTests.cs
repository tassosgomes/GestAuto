using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using GestAuto.Stock.Application.Vehicles.Dto;
using GestAuto.Stock.Domain.Enums;
using GestAuto.Stock.IntegrationTest.Shared;
using GestAuto.Stock.Tests.Shared;
using Xunit;

namespace GestAuto.Stock.IntegrationTest.Controllers;

[Collection("Postgres")]
public class VehicleCheckOutTests
{
    private readonly PostgresFixture _postgresFixture;
    private readonly CustomWebApplicationFactory _factory;

    public VehicleCheckOutTests(PostgresFixture postgresFixture)
    {
        _postgresFixture = postgresFixture;
        _factory = new CustomWebApplicationFactory(postgresFixture);
    }

    [SkippableFact]
    public async Task CheckOut_Sale_ShouldSetStatusSold()
    {
        Skip.IfNot(_postgresFixture.IsAvailable, $"Docker/Testcontainers indisponível: {_postgresFixture.UnavailableReason}");

        await _factory.ResetStateAsync();

        var userId = Guid.NewGuid();
        using var client = _factory.CreateClient().WithTestAuth(userId, role: "SALES_PERSON");

        var vehicleId = await CreateUsedVehicleAsync(client);

        var request = new CheckOutCreateRequest(CheckOutReason.Sale, null, "venda confirmada");
        var response = await client.PostAsJsonAsync($"/api/v1/vehicles/{vehicleId}/check-outs", request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var body = await response.Content.ReadFromJsonAsync<CheckOutResponse>();
        body.Should().NotBeNull();
        body!.CurrentStatus.Should().Be(VehicleStatus.Sold);
    }

    [SkippableFact]
    public async Task CheckOut_TotalLoss_ShouldRequireManagerRole()
    {
        Skip.IfNot(_postgresFixture.IsAvailable, $"Docker/Testcontainers indisponível: {_postgresFixture.UnavailableReason}");

        await _factory.ResetStateAsync();

        var vehicleId = await CreateUsedVehicleAsync(_factory.CreateClient().WithTestAuth(Guid.NewGuid(), role: "SALES_PERSON"));

        using var salesClient = _factory.CreateClient().WithTestAuth(Guid.NewGuid(), role: "SALES_PERSON");
        var denied = await salesClient.PostAsJsonAsync(
            $"/api/v1/vehicles/{vehicleId}/check-outs",
            new CheckOutCreateRequest(CheckOutReason.TotalLoss, null, "sinistro"));

        denied.StatusCode.Should().Be(HttpStatusCode.Forbidden);

        using var managerClient = _factory.CreateClient().WithTestAuth(Guid.NewGuid(), role: "MANAGER");
        var allowed = await managerClient.PostAsJsonAsync(
            $"/api/v1/vehicles/{vehicleId}/check-outs",
            new CheckOutCreateRequest(CheckOutReason.TotalLoss, null, "sinistro"));

        allowed.StatusCode.Should().Be(HttpStatusCode.Created);

        var body = await allowed.Content.ReadFromJsonAsync<CheckOutResponse>();
        body!.CurrentStatus.Should().Be(VehicleStatus.WrittenOff);
    }

    private static async Task<Guid> CreateUsedVehicleAsync(HttpClient client)
    {
        var create = new VehicleCreate(
            Category: VehicleCategory.Used,
            Vin: $"VIN-{Guid.NewGuid():N}",
            Make: "Nissan",
            Model: "Kicks",
            YearModel: 2020,
            Color: "Prata",
            Plate: $"MNO{Random.Shared.Next(1000, 9999)}",
            Trim: null,
            MileageKm: 40000,
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
