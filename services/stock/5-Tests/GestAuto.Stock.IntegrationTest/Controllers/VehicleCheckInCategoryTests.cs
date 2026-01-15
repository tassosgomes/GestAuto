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
public class VehicleCheckInCategoryTests
{
    private readonly PostgresFixture _postgresFixture;
    private readonly CustomWebApplicationFactory _factory;

    public VehicleCheckInCategoryTests(PostgresFixture postgresFixture)
    {
        _postgresFixture = postgresFixture;
        _factory = new CustomWebApplicationFactory(postgresFixture);
    }

    [SkippableFact]
    public async Task CreateVehicle_UsedWithoutPlate_ShouldReturn422()
    {
        Skip.IfNot(_postgresFixture.IsAvailable, $"Docker/Testcontainers indisponível: {_postgresFixture.UnavailableReason}");

        await _factory.ResetStateAsync();

        using var client = _factory.CreateClient().WithTestAuth(Guid.NewGuid(), role: "SALES_PERSON");

        var create = new VehicleCreate(
            Category: VehicleCategory.Used,
            Vin: $"VIN-{Guid.NewGuid():N}",
            Make: "Ford",
            Model: "Ka",
            YearModel: 2019,
            Color: "Prata",
            Plate: null,
            Trim: null,
            MileageKm: 10000,
            EvaluationId: Guid.NewGuid(),
            DemoPurpose: null,
            IsRegistered: false);

        var response = await client.PostAsJsonAsync("/api/v1/vehicles", create);
        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }

    [SkippableFact]
    public async Task CreateVehicle_DemonstrationWithoutDemoPurpose_ShouldReturn422()
    {
        Skip.IfNot(_postgresFixture.IsAvailable, $"Docker/Testcontainers indisponível: {_postgresFixture.UnavailableReason}");

        await _factory.ResetStateAsync();

        using var client = _factory.CreateClient().WithTestAuth(Guid.NewGuid(), role: "SALES_PERSON");

        var create = new VehicleCreate(
            Category: VehicleCategory.Demonstration,
            Vin: $"VIN-{Guid.NewGuid():N}",
            Make: "Hyundai",
            Model: "HB20",
            YearModel: 2023,
            Color: "Azul",
            Plate: "XYZ1234",
            Trim: null,
            MileageKm: null,
            EvaluationId: null,
            DemoPurpose: null,
            IsRegistered: true);

        var response = await client.PostAsJsonAsync("/api/v1/vehicles", create);
        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }

    [SkippableFact]
    public async Task CheckIn_NewVehicleWithNonManufacturerSource_ShouldReturn422()
    {
        Skip.IfNot(_postgresFixture.IsAvailable, $"Docker/Testcontainers indisponível: {_postgresFixture.UnavailableReason}");

        await _factory.ResetStateAsync();

        var userId = Guid.NewGuid();
        using var client = _factory.CreateClient().WithTestAuth(userId, role: "SALES_PERSON");

        var create = new VehicleCreate(
            Category: VehicleCategory.New,
            Vin: $"VIN-{Guid.NewGuid():N}",
            Make: "Toyota",
            Model: "Corolla",
            YearModel: 2025,
            Color: "Cinza",
            Plate: null,
            Trim: "XEi",
            MileageKm: null,
            EvaluationId: null,
            DemoPurpose: null,
            IsRegistered: false);

        var createResponse = await client.PostAsJsonAsync("/api/v1/vehicles", create);
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var created = await createResponse.Content.ReadFromJsonAsync<VehicleResponse>();
        created.Should().NotBeNull();

        var checkIn = new CheckInCreateRequest(
            Source: CheckInSource.StoreTransfer,
            OccurredAt: DateTime.UtcNow,
            Notes: "entrada errada");

        var checkInResponse = await client.PostAsJsonAsync($"/api/v1/vehicles/{created!.Id}/check-ins", checkIn);
        checkInResponse.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }

    [SkippableFact]
    public async Task CheckIn_UsedAndDemonstration_ShouldSucceed_AndSetOwner()
    {
        Skip.IfNot(_postgresFixture.IsAvailable, $"Docker/Testcontainers indisponível: {_postgresFixture.UnavailableReason}");

        await _factory.ResetStateAsync();

        var userId = Guid.NewGuid();
        using var client = _factory.CreateClient().WithTestAuth(userId, role: "SALES_PERSON");

        // Used
        var usedCreate = new VehicleCreate(
            Category: VehicleCategory.Used,
            Vin: $"VIN-{Guid.NewGuid():N}",
            Make: "Chevrolet",
            Model: "Onix",
            YearModel: 2020,
            Color: "Branco",
            Plate: $"GHI{Random.Shared.Next(1000, 9999)}",
            Trim: null,
            MileageKm: 80000,
            EvaluationId: Guid.NewGuid(),
            DemoPurpose: null,
            IsRegistered: false);

        var usedResp = await client.PostAsJsonAsync("/api/v1/vehicles", usedCreate);
        usedResp.StatusCode.Should().Be(HttpStatusCode.Created);
        var used = await usedResp.Content.ReadFromJsonAsync<VehicleResponse>();

        var usedCheckIn = new CheckInCreateRequest(CheckInSource.CustomerUsedPurchase, null, "entrada seminovo");
        var usedCheckInResp = await client.PostAsJsonAsync($"/api/v1/vehicles/{used!.Id}/check-ins", usedCheckIn);
        usedCheckInResp.StatusCode.Should().Be(HttpStatusCode.Created);

        var usedCheckInBody = await usedCheckInResp.Content.ReadFromJsonAsync<CheckInResponse>();
        usedCheckInBody!.CurrentStatus.Should().Be(VehicleStatus.InStock);
        usedCheckInBody.CurrentOwnerUserId.Should().Be(userId);

        // Demonstration
        var demoCreate = new VehicleCreate(
            Category: VehicleCategory.Demonstration,
            Vin: $"VIN-{Guid.NewGuid():N}",
            Make: "Honda",
            Model: "Civic",
            YearModel: 2024,
            Color: "Vermelho",
            Plate: "JKL1234",
            Trim: null,
            MileageKm: null,
            EvaluationId: null,
            DemoPurpose: DemoPurpose.TestDrive,
            IsRegistered: true);

        var demoResp = await client.PostAsJsonAsync("/api/v1/vehicles", demoCreate);
        demoResp.StatusCode.Should().Be(HttpStatusCode.Created);
        var demo = await demoResp.Content.ReadFromJsonAsync<VehicleResponse>();

        var demoCheckIn = new CheckInCreateRequest(CheckInSource.InternalFleet, null, "entrada demo");
        var demoCheckInResp = await client.PostAsJsonAsync($"/api/v1/vehicles/{demo!.Id}/check-ins", demoCheckIn);
        demoCheckInResp.StatusCode.Should().Be(HttpStatusCode.Created);

        var demoCheckInBody = await demoCheckInResp.Content.ReadFromJsonAsync<CheckInResponse>();
        demoCheckInBody!.CurrentStatus.Should().Be(VehicleStatus.InStock);
        demoCheckInBody.CurrentOwnerUserId.Should().Be(userId);
    }
}
