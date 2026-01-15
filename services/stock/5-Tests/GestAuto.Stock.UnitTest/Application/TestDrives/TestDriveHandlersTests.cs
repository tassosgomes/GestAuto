using FluentAssertions;
using GestAuto.Stock.Application.Reservations.Dto;
using GestAuto.Stock.Application.TestDrives.Commands;
using GestAuto.Stock.Application.TestDrives.Dto;
using GestAuto.Stock.Domain.Entities;
using GestAuto.Stock.Domain.Enums;
using GestAuto.Stock.Domain.Exceptions;
using GestAuto.Stock.Domain.History;
using GestAuto.Stock.Domain.Interfaces;

namespace GestAuto.Stock.UnitTest.Application.TestDrives;

public sealed class TestDriveHandlersTests
{
    [Fact]
    public async Task StartTestDrive_WhenVehicleNotFound_ShouldThrow()
    {
        var vehicles = new FakeVehicleRepository();
        var uow = new FakeUnitOfWork();
        var handler = new StartTestDriveCommandHandler(vehicles, uow);

        var command = new StartTestDriveCommand(
            VehicleId: Guid.NewGuid(),
            SalesPersonId: Guid.NewGuid(),
            Request: new StartTestDriveRequest(CustomerRef: "C1", StartedAt: null));

        var act = async () => await handler.HandleAsync(command, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
        uow.CommitCount.Should().Be(0);
    }

    [Fact]
    public async Task StartTestDrive_WhenValid_ShouldCreateSessionAndCommit()
    {
        var vehicles = new FakeVehicleRepository();
        var uow = new FakeUnitOfWork();
        var handler = new StartTestDriveCommandHandler(vehicles, uow);

        var salesPerson = Guid.NewGuid();
        var vehicle = SeedVehicleInStock(salesPerson);
        vehicles.Vehicles.Add(vehicle);

        var result = await handler.HandleAsync(
            new StartTestDriveCommand(vehicle.Id, salesPerson, new StartTestDriveRequest(CustomerRef: "customer", StartedAt: null)),
            CancellationToken.None);

        result.VehicleId.Should().Be(vehicle.Id);
        vehicle.CurrentStatus.Should().Be(VehicleStatus.InTestDrive);
        vehicle.TestDrives.Should().ContainSingle(s => s.Id == result.TestDriveId);
        uow.CommitCount.Should().Be(1);
    }

    [Fact]
    public async Task CompleteTestDrive_WhenReturningToStock_ShouldSetInStockAndCommit()
    {
        var vehicles = new FakeVehicleRepository();
        var reservations = new FakeReservationRepository();
        var testDrives = new FakeTestDriveRepository(vehicles);
        var uow = new FakeUnitOfWork();

        var handler = new CompleteTestDriveCommandHandler(vehicles, testDrives, reservations, uow);

        var salesPerson = Guid.NewGuid();
        var vehicle = SeedVehicleInStock(salesPerson);
        vehicles.Vehicles.Add(vehicle);

        var testDriveId = vehicle.StartTestDrive(salesPerson, customerRef: "c", startedAt: DateTime.UtcNow.AddMinutes(-20));
        vehicle.CurrentStatus.Should().Be(VehicleStatus.InTestDrive);

        var result = await handler.HandleAsync(
            new CompleteTestDriveCommand(
                TestDriveId: testDriveId,
                CompletedByUserId: salesPerson,
                Request: new CompleteTestDriveRequest(
                    Outcome: TestDriveOutcome.ReturnedToStock,
                    EndedAt: DateTime.UtcNow,
                    Reservation: null)),
            CancellationToken.None);

        result.CurrentStatus.Should().Be(VehicleStatus.InStock);
        vehicle.CurrentStatus.Should().Be(VehicleStatus.InStock);
        result.ReservationId.Should().BeNull();
        uow.CommitCount.Should().Be(1);
    }

    [Fact]
    public async Task CompleteTestDrive_WhenConvertedToReservationWithoutExisting_ShouldCreateReservation()
    {
        var vehicles = new FakeVehicleRepository();
        var reservations = new FakeReservationRepository();
        var testDrives = new FakeTestDriveRepository(vehicles);
        var uow = new FakeUnitOfWork();

        var handler = new CompleteTestDriveCommandHandler(vehicles, testDrives, reservations, uow);

        var salesPerson = Guid.NewGuid();
        var vehicle = SeedVehicleInStock(salesPerson);
        vehicles.Vehicles.Add(vehicle);

        var testDriveId = vehicle.StartTestDrive(salesPerson, customerRef: "c", startedAt: DateTime.UtcNow.AddMinutes(-20));

        var result = await handler.HandleAsync(
            new CompleteTestDriveCommand(
                TestDriveId: testDriveId,
                CompletedByUserId: salesPerson,
                Request: new CompleteTestDriveRequest(
                    Outcome: TestDriveOutcome.ConvertedToReservation,
                    EndedAt: DateTime.UtcNow,
                    Reservation: null)),
            CancellationToken.None);

        result.CurrentStatus.Should().Be(VehicleStatus.Reserved);
        result.ReservationId.Should().NotBeNull();
        reservations.Reservations.Should().ContainSingle(r => r.Id == result.ReservationId);
        uow.CommitCount.Should().Be(1);
    }

    [Fact]
    public async Task CompleteTestDrive_WhenConvertedToReservationWithExisting_ShouldNotCreateNewReservation()
    {
        var vehicles = new FakeVehicleRepository();
        var reservations = new FakeReservationRepository();
        var testDrives = new FakeTestDriveRepository(vehicles);
        var uow = new FakeUnitOfWork();

        var handler = new CompleteTestDriveCommandHandler(vehicles, testDrives, reservations, uow);

        var salesPerson = Guid.NewGuid();
        var vehicle = SeedVehicleInStock(salesPerson);
        vehicles.Vehicles.Add(vehicle);

        var existingReservation = new Reservation(
            vehicleId: vehicle.Id,
            type: ReservationType.Standard,
            salesPersonId: salesPerson,
            createdAtUtc: DateTime.UtcNow,
            contextType: "seed",
            contextId: null,
            bankDeadlineAtUtc: null);

        reservations.Reservations.Add(existingReservation);

        vehicle.Reserve(existingReservation.Id, salesPerson);

        var testDriveId = vehicle.StartTestDrive(salesPerson, customerRef: "c", startedAt: DateTime.UtcNow.AddMinutes(-20));
        reservations.Reservations.Should().HaveCount(1);

        var result = await handler.HandleAsync(
            new CompleteTestDriveCommand(
                TestDriveId: testDriveId,
                CompletedByUserId: salesPerson,
                Request: new CompleteTestDriveRequest(
                    Outcome: TestDriveOutcome.ConvertedToReservation,
                    EndedAt: DateTime.UtcNow,
                    Reservation: new CreateReservationRequest(
                        Type: ReservationType.PaidDeposit,
                        ContextType: "ignored",
                        ContextId: null,
                        BankDeadlineAtUtc: null))),
            CancellationToken.None);

        result.CurrentStatus.Should().Be(VehicleStatus.Reserved);
        result.ReservationId.Should().Be(existingReservation.Id);
        reservations.Reservations.Should().HaveCount(1);
        uow.CommitCount.Should().Be(1);
    }

    [Fact]
    public async Task CompleteTestDrive_WhenReturningToStockButHasActiveReservation_ShouldThrowConflict()
    {
        var vehicles = new FakeVehicleRepository();
        var reservations = new FakeReservationRepository();
        var testDrives = new FakeTestDriveRepository(vehicles);
        var uow = new FakeUnitOfWork();

        var handler = new CompleteTestDriveCommandHandler(vehicles, testDrives, reservations, uow);

        var salesPerson = Guid.NewGuid();
        var vehicle = SeedVehicleInStock(salesPerson);
        vehicles.Vehicles.Add(vehicle);

        var activeReservation = new Reservation(
            vehicleId: vehicle.Id,
            type: ReservationType.Standard,
            salesPersonId: salesPerson,
            createdAtUtc: DateTime.UtcNow,
            contextType: "seed",
            contextId: null,
            bankDeadlineAtUtc: null);
        reservations.Reservations.Add(activeReservation);

        vehicle.Reserve(activeReservation.Id, salesPerson);

        var testDriveId = vehicle.StartTestDrive(salesPerson, customerRef: "c", startedAt: DateTime.UtcNow.AddMinutes(-20));

        var act = async () => await handler.HandleAsync(
            new CompleteTestDriveCommand(
                TestDriveId: testDriveId,
                CompletedByUserId: salesPerson,
                Request: new CompleteTestDriveRequest(
                    Outcome: TestDriveOutcome.ReturnedToStock,
                    EndedAt: DateTime.UtcNow,
                    Reservation: null)),
            CancellationToken.None);

        await act.Should().ThrowAsync<ConflictException>();
        uow.CommitCount.Should().Be(0);
    }

    private static Vehicle SeedVehicleInStock(Guid responsibleUserId)
    {
        var vehicle = new Vehicle(
            VehicleCategory.Used,
            vin: $"VIN-{Guid.NewGuid():N}",
            make: "VW",
            model: "Gol",
            yearModel: 2020,
            color: "White",
            plate: "ABC1234",
            mileageKm: 100,
            evaluationId: Guid.NewGuid());

        vehicle.MarkInStock(responsibleUserId, "seed");
        return vehicle;
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
        {
            var total = Vehicles.Count;
            return Task.FromResult(((IReadOnlyList<Vehicle>)Vehicles, total));
        }

        public Task AddAsync(Vehicle vehicle, CancellationToken cancellationToken = default)
        {
            Vehicles.Add(vehicle);
            return Task.CompletedTask;
        }

        public Task UpdateAsync(Vehicle vehicle, CancellationToken cancellationToken = default)
            => Task.CompletedTask;
    }

    private sealed class FakeTestDriveRepository : ITestDriveRepository
    {
        private readonly FakeVehicleRepository _vehicles;

        public FakeTestDriveRepository(FakeVehicleRepository vehicles)
        {
            _vehicles = vehicles;
        }

        public Task<TestDriveSession?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var session = _vehicles.Vehicles
                .SelectMany(v => v.TestDrives)
                .SingleOrDefault(t => t.Id == id);

            return Task.FromResult(session);
        }

        public Task AddAsync(TestDriveSession testDrive, CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public Task UpdateAsync(TestDriveSession testDrive, CancellationToken cancellationToken = default)
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

        public Task AddAsync(Reservation reservation, CancellationToken cancellationToken = default)
        {
            Reservations.Add(reservation);
            return Task.CompletedTask;
        }

        public Task UpdateAsync(Reservation reservation, CancellationToken cancellationToken = default)
            => Task.CompletedTask;
    }

    private sealed class FakeUnitOfWork : IUnitOfWork
    {
        public int CommitCount { get; private set; }

        public Task<int> CommitAsync(CancellationToken cancellationToken = default)
        {
            CommitCount++;
            return Task.FromResult(1);
        }
    }
}
