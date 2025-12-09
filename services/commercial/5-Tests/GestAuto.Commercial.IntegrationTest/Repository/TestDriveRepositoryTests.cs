using System.Linq;
using FluentAssertions;
using GestAuto.Commercial.Domain.Entities;
using GestAuto.Commercial.Domain.Enums;
using GestAuto.Commercial.Domain.ValueObjects;
using GestAuto.Commercial.Infra.Repositories;
using GestAuto.Commercial.Tests.Shared;
using Xunit;

namespace GestAuto.Commercial.IntegrationTest.Repository;

[Collection("Postgres")]
public class TestDriveRepositoryTests
{
    private readonly PostgresFixture _postgresFixture;

    public TestDriveRepositoryTests(PostgresFixture postgresFixture)
    {
        _postgresFixture = postgresFixture;
    }

    [Fact]
    public async Task AddAsync_AndGetById_ShouldPersist()
    {
        await _postgresFixture.ResetDatabaseAsync();

        await using var context = _postgresFixture.CreateContext();
        var repository = new TestDriveRepository(context);

        var testDrive = TestDrive.Schedule(
            leadId: Guid.NewGuid(),
            vehicleId: Guid.NewGuid(),
            scheduledAt: DateTime.UtcNow.AddHours(2),
            salesPersonId: Guid.NewGuid(),
            notes: "Primeira visita");

        await repository.AddAsync(testDrive, CancellationToken.None);
        await context.SaveChangesAsync();

        var saved = await repository.GetByIdAsync(testDrive.Id, CancellationToken.None);
        saved.Should().NotBeNull();
        saved!.Status.Should().Be(TestDriveStatus.Scheduled);
        saved.Notes.Should().Be("Primeira visita");
    }

    [Fact]
    public async Task CheckVehicleAvailability_ShouldDetectOverlap()
    {
        await _postgresFixture.ResetDatabaseAsync();

        await using var context = _postgresFixture.CreateContext();
        var repository = new TestDriveRepository(context);

        var vehicleId = Guid.NewGuid();
        var start = DateTime.UtcNow.AddHours(3);

        var scheduled = TestDrive.Schedule(
            leadId: Guid.NewGuid(),
            vehicleId: vehicleId,
            scheduledAt: start,
            salesPersonId: Guid.NewGuid());

        await repository.AddAsync(scheduled, CancellationToken.None);
        await context.SaveChangesAsync();

        var available = await repository.CheckVehicleAvailabilityAsync(
            vehicleId,
            start.AddMinutes(30),
            TimeSpan.FromMinutes(60),
            CancellationToken.None);

        available.Should().BeFalse();

        var differentVehicleAvailable = await repository.CheckVehicleAvailabilityAsync(
            Guid.NewGuid(),
            start.AddMinutes(30),
            TimeSpan.FromMinutes(60),
            CancellationToken.None);

        differentVehicleAvailable.Should().BeTrue();
    }

    [Fact]
    public async Task ListAsync_ShouldFilterBySalesPersonAndStatus()
    {
        await _postgresFixture.ResetDatabaseAsync();

        await using var context = _postgresFixture.CreateContext();
        var repository = new TestDriveRepository(context);

        var salesPerson = Guid.NewGuid();
        var otherSalesPerson = Guid.NewGuid();

        var td1 = TestDrive.Schedule(Guid.NewGuid(), Guid.NewGuid(), DateTime.UtcNow.AddHours(1), salesPerson);
        var td2 = TestDrive.Schedule(Guid.NewGuid(), Guid.NewGuid(), DateTime.UtcNow.AddHours(2), salesPerson);
        var td3 = TestDrive.Schedule(Guid.NewGuid(), Guid.NewGuid(), DateTime.UtcNow.AddHours(3), otherSalesPerson);

        td2.Complete(new TestDriveChecklist(10, 20, FuelLevel.Full, null));

        await repository.AddAsync(td1, CancellationToken.None);
        await repository.AddAsync(td2, CancellationToken.None);
        await repository.AddAsync(td3, CancellationToken.None);
        await context.SaveChangesAsync();

        var list = await repository.ListAsync(
            salesPersonId: salesPerson,
            leadId: null,
            status: TestDriveStatus.Completed.ToString(),
            fromDate: null,
            toDate: null,
            page: 1,
            pageSize: 10,
            cancellationToken: CancellationToken.None);

        list.Should().HaveCount(1);
        list.First().Id.Should().Be(td2.Id);
    }
}
