using FluentAssertions;
using GestAuto.Stock.Application.Vehicles.Queries;
using GestAuto.Stock.Domain.Entities;
using GestAuto.Stock.Domain.Enums;
using GestAuto.Stock.Domain.Exceptions;
using GestAuto.Stock.Domain.History;
using GestAuto.Stock.Domain.Interfaces;

namespace GestAuto.Stock.UnitTest.Application.Vehicles;

public sealed class VehicleHistoryQueryHandlerTests
{
    [Fact]
    public async Task GetVehicleHistory_WhenVehicleNotFound_ShouldThrow()
    {
        var vehicles = new FakeVehicleRepository();
        var reservations = new FakeReservationRepository();
        var audit = new FakeAuditEntryRepository();

        var handler = new GetVehicleHistoryQueryHandler(vehicles, reservations, audit);

        var act = async () => await handler.HandleAsync(new GetVehicleHistoryQuery(Guid.NewGuid()), CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task GetVehicleHistory_ShouldReturnChronologicalTimeline_WithResponsibleUser()
    {
        var vehicles = new FakeVehicleRepository();
        var reservations = new FakeReservationRepository();
        var audit = new FakeAuditEntryRepository();

        var vehicle = new Vehicle(
            VehicleCategory.Used,
            vin: "VIN-HIST",
            make: "VW",
            model: "Gol",
            yearModel: 2020,
            color: "White",
            plate: "ABC1234",
            mileageKm: 100,
            evaluationId: Guid.NewGuid());

        var userA = Guid.NewGuid();
        var userB = Guid.NewGuid();

        vehicle.MarkInStock(userA, "seed");
        vehicle.CheckIn(CheckInSource.CustomerUsedPurchase, new DateTime(2026, 1, 1, 10, 0, 0, DateTimeKind.Utc), userA, "entrada");

        var testDriveId = vehicle.StartTestDrive(userA, customerRef: "C-001", startedAt: new DateTime(2026, 1, 2, 9, 0, 0, DateTimeKind.Utc));
        vehicle.CompleteTestDrive(testDriveId, completedByUserId: userA, endedAt: new DateTime(2026, 1, 2, 11, 0, 0, DateTimeKind.Utc), TestDriveOutcome.ReturnedToStock);

        vehicle.CheckOut(CheckOutReason.Transfer, new DateTime(2026, 1, 3, 8, 0, 0, DateTimeKind.Utc), userB, "transferencia");

        vehicles.Vehicles.Add(vehicle);

        var reservation = new Reservation(
            vehicleId: vehicle.Id,
            type: ReservationType.Standard,
            salesPersonId: userA,
            createdAtUtc: new DateTime(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc),
            contextType: "lead",
            contextId: Guid.NewGuid());

        reservation.Extend(
            extendedByUserId: userB,
            newExpiresAtUtc: new DateTime(2026, 1, 4, 12, 0, 0, DateTimeKind.Utc),
            extendedAtUtc: new DateTime(2026, 1, 2, 12, 0, 0, DateTimeKind.Utc));

        reservations.Reservations.Add(reservation);

        audit.Entries.Add(new AuditEntry(
            vehicleId: vehicle.Id,
            occurredAtUtc: new DateTime(2026, 1, 5, 9, 0, 0, DateTimeKind.Utc),
            responsibleUserId: userB,
            previousStatus: VehicleStatus.InTransit,
            newStatus: VehicleStatus.InStock,
            reason: "manual:fix"));

        var handler = new GetVehicleHistoryQueryHandler(vehicles, reservations, audit);

        var result = await handler.HandleAsync(new GetVehicleHistoryQuery(vehicle.Id), CancellationToken.None);

        result.VehicleId.Should().Be(vehicle.Id);
        result.Items.Should().NotBeEmpty();

        // Garantir ordenação
        result.Items.Select(i => i.OccurredAtUtc).Should().BeInAscendingOrder();

        // Garantir responsável (auditabilidade)
        result.Items.All(i => i.UserId != Guid.Empty).Should().BeTrue();

        // Sanity: deve conter pelo menos um evento de cada categoria principal
        result.Items.Any(i => i.Type == "check-in").Should().BeTrue();
        result.Items.Any(i => i.Type.StartsWith("test-drive")).Should().BeTrue();
        result.Items.Any(i => i.Type.StartsWith("reservation")).Should().BeTrue();
        result.Items.Any(i => i.Type == "check-out").Should().BeTrue();
        result.Items.Any(i => i.Type == "status-changed").Should().BeTrue();
    }

    private sealed class FakeVehicleRepository : IVehicleRepository
    {
        public List<Vehicle> Vehicles { get; } = new();

        public Task<Vehicle?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
            => Task.FromResult(Vehicles.SingleOrDefault(v => v.Id == id));

        public Task<Vehicle?> GetByVinAsync(string vin, CancellationToken cancellationToken = default)
            => Task.FromResult(Vehicles.SingleOrDefault(v => v.Vin.Equals(vin, StringComparison.OrdinalIgnoreCase)));

        public Task<Vehicle?> GetByPlateAsync(string plate, CancellationToken cancellationToken = default)
            => Task.FromResult(Vehicles.SingleOrDefault(v => v.Plate != null && v.Plate.Equals(plate, StringComparison.OrdinalIgnoreCase)));

        public Task<(IReadOnlyList<Vehicle> Items, int Total)> ListAsync(
            int page,
            int size,
            VehicleStatus? status,
            VehicleCategory? category,
            string? query,
            CancellationToken cancellationToken = default)
            => Task.FromResult(((IReadOnlyList<Vehicle>)Vehicles, Vehicles.Count));

        public Task AddAsync(Vehicle vehicle, CancellationToken cancellationToken = default)
        {
            Vehicles.Add(vehicle);
            return Task.CompletedTask;
        }

        public Task UpdateAsync(Vehicle vehicle, CancellationToken cancellationToken = default)
            => Task.CompletedTask;
    }

    private sealed class FakeReservationRepository : IReservationRepository
    {
        public List<Reservation> Reservations { get; } = new();

        public Task<Reservation?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
            => Task.FromResult(Reservations.SingleOrDefault(r => r.Id == id));

        public Task<Reservation?> GetActiveByVehicleIdAsync(Guid vehicleId, CancellationToken cancellationToken = default)
            => Task.FromResult(Reservations.SingleOrDefault(r => r.VehicleId == vehicleId && r.Status == ReservationStatus.Active));

        public Task<IReadOnlyList<Reservation>> ListByVehicleIdAsync(Guid vehicleId, CancellationToken cancellationToken = default)
            => Task.FromResult((IReadOnlyList<Reservation>)Reservations.Where(r => r.VehicleId == vehicleId).ToList());

        public Task<(IReadOnlyList<Reservation> Items, int Total)> ListAsync(
            int page,
            int size,
            ReservationStatus? status,
            ReservationType? type,
            Guid? salesPersonId,
            Guid? vehicleId,
            CancellationToken cancellationToken = default)
        {
            var query = Reservations.AsEnumerable();

            if (status.HasValue)
            {
                query = query.Where(r => r.Status == status.Value);
            }

            if (type.HasValue)
            {
                query = query.Where(r => r.Type == type.Value);
            }

            if (salesPersonId.HasValue && salesPersonId.Value != Guid.Empty)
            {
                query = query.Where(r => r.SalesPersonId == salesPersonId.Value);
            }

            if (vehicleId.HasValue && vehicleId.Value != Guid.Empty)
            {
                query = query.Where(r => r.VehicleId == vehicleId.Value);
            }

            var items = query.ToList();
            return Task.FromResult(((IReadOnlyList<Reservation>)items, items.Count));
        }

        public Task AddAsync(Reservation reservation, CancellationToken cancellationToken = default)
        {
            Reservations.Add(reservation);
            return Task.CompletedTask;
        }

        public Task UpdateAsync(Reservation reservation, CancellationToken cancellationToken = default)
            => Task.CompletedTask;
    }

    private sealed class FakeAuditEntryRepository : IAuditEntryRepository
    {
        public List<AuditEntry> Entries { get; } = new();

        public Task<IReadOnlyList<AuditEntry>> ListByVehicleIdAsync(Guid vehicleId, CancellationToken cancellationToken = default)
            => Task.FromResult((IReadOnlyList<AuditEntry>)Entries.Where(e => e.VehicleId == vehicleId).ToList());

        public Task AddAsync(AuditEntry entry, CancellationToken cancellationToken = default)
        {
            Entries.Add(entry);
            return Task.CompletedTask;
        }
    }
}
