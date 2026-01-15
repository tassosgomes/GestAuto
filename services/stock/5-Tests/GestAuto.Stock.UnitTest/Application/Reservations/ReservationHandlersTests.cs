using FluentAssertions;
using GestAuto.Stock.Application.Reservations.Commands;
using GestAuto.Stock.Application.Reservations.Dto;
using GestAuto.Stock.Domain.Entities;
using GestAuto.Stock.Domain.Enums;
using GestAuto.Stock.Domain.Exceptions;
using GestAuto.Stock.Domain.Interfaces;

namespace GestAuto.Stock.UnitTest.Application.Reservations;

public sealed class ReservationHandlersTests
{
    [Fact]
    public async Task CreateReservation_WhenVehicleHasActiveReservation_ShouldReturnConflict()
    {
        var vehicles = new FakeVehicleRepository();
        var reservations = new FakeReservationRepository();
        var uow = new FakeUnitOfWork();

        var userId = Guid.NewGuid();
        var vehicle = SeedInStockVehicle(userId);
        vehicles.Vehicles.Add(vehicle);

        reservations.Reservations.Add(new Reservation(
            vehicleId: vehicle.Id,
            type: ReservationType.Standard,
            salesPersonId: userId,
            createdAtUtc: DateTime.UtcNow,
            contextType: "lead",
            contextId: Guid.NewGuid()));

        var handler = new CreateReservationCommandHandler(vehicles, reservations, uow);

        var act = async () => await handler.HandleAsync(
            new CreateReservationCommand(
                VehicleId: vehicle.Id,
                RequestedByUserId: userId,
                Request: new CreateReservationRequest(
                    Type: ReservationType.Standard,
                    ContextType: "lead",
                    ContextId: Guid.NewGuid(),
                    BankDeadlineAtUtc: null)),
            CancellationToken.None);

        await act.Should().ThrowAsync<ConflictException>();
        uow.CommitCount.Should().Be(0);
    }

    [Fact]
    public async Task CreateReservation_WhenWaitingBankWithoutDeadline_ShouldThrowDomainException()
    {
        var vehicles = new FakeVehicleRepository();
        var reservations = new FakeReservationRepository();
        var uow = new FakeUnitOfWork();

        var userId = Guid.NewGuid();
        var vehicle = SeedInStockVehicle(userId);
        vehicles.Vehicles.Add(vehicle);

        var handler = new CreateReservationCommandHandler(vehicles, reservations, uow);

        var act = async () => await handler.HandleAsync(
            new CreateReservationCommand(
                VehicleId: vehicle.Id,
                RequestedByUserId: userId,
                Request: new CreateReservationRequest(
                    Type: ReservationType.WaitingBank,
                    ContextType: "proposal",
                    ContextId: Guid.NewGuid(),
                    BankDeadlineAtUtc: null)),
            CancellationToken.None);

        await act.Should().ThrowAsync<DomainException>();
        uow.CommitCount.Should().Be(0);
    }

    [Fact]
    public async Task CancelReservation_WhenNotOwnerAndNotManager_ShouldThrowForbidden()
    {
        var vehicles = new FakeVehicleRepository();
        var reservations = new FakeReservationRepository();
        var uow = new FakeUnitOfWork();

        var ownerId = Guid.NewGuid();
        var vehicle = SeedInStockVehicle(ownerId);
        vehicles.Vehicles.Add(vehicle);

        var reservation = new Reservation(
            vehicleId: vehicle.Id,
            type: ReservationType.Standard,
            salesPersonId: ownerId,
            createdAtUtc: DateTime.UtcNow,
            contextType: "lead",
            contextId: Guid.NewGuid());

        // Simulate reservation-created effect on vehicle
        vehicle.ChangeStatusManually(VehicleStatus.Reserved, ownerId, "seed");

        reservations.Reservations.Add(reservation);

        var handler = new CancelReservationCommandHandler(reservations, vehicles, uow);

        var otherUser = Guid.NewGuid();
        var act = async () => await handler.HandleAsync(
            new CancelReservationCommand(
                ReservationId: reservation.Id,
                CancelledByUserId: otherUser,
                CanCancelOthers: false,
                Request: new CancelReservationRequest("cliente desistiu")),
            CancellationToken.None);

        await act.Should().ThrowAsync<ForbiddenException>();
        uow.CommitCount.Should().Be(0);
    }

    [Fact]
    public async Task CancelReservation_WhenOwner_ShouldCancelAndReleaseVehicle()
    {
        var vehicles = new FakeVehicleRepository();
        var reservations = new FakeReservationRepository();
        var uow = new FakeUnitOfWork();

        var ownerId = Guid.NewGuid();
        var vehicle = SeedInStockVehicle(ownerId);
        vehicles.Vehicles.Add(vehicle);

        var reservation = new Reservation(
            vehicleId: vehicle.Id,
            type: ReservationType.Standard,
            salesPersonId: ownerId,
            createdAtUtc: DateTime.UtcNow,
            contextType: "lead",
            contextId: Guid.NewGuid());

        vehicle.ChangeStatusManually(VehicleStatus.Reserved, ownerId, "seed");
        reservations.Reservations.Add(reservation);

        var handler = new CancelReservationCommandHandler(reservations, vehicles, uow);

        var result = await handler.HandleAsync(
            new CancelReservationCommand(
                ReservationId: reservation.Id,
                CancelledByUserId: ownerId,
                CanCancelOthers: false,
                Request: new CancelReservationRequest("cliente desistiu")),
            CancellationToken.None);

        result.Status.Should().Be(ReservationStatus.Cancelled);
        vehicle.CurrentStatus.Should().Be(VehicleStatus.InStock);
        uow.CommitCount.Should().Be(1);
    }

    private static Vehicle SeedInStockVehicle(Guid responsibleUserId)
    {
        var vehicle = new Vehicle(
            category: VehicleCategory.Used,
            vin: Guid.NewGuid().ToString("N")[..10],
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
