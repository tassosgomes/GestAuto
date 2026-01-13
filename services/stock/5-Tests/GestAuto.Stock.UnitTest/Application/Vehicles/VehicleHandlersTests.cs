using FluentAssertions;
using GestAuto.Stock.Application.Vehicles.Commands;
using GestAuto.Stock.Application.Vehicles.Dto;
using GestAuto.Stock.Domain.Entities;
using GestAuto.Stock.Domain.Enums;
using GestAuto.Stock.Domain.Exceptions;
using GestAuto.Stock.Domain.Interfaces;

namespace GestAuto.Stock.UnitTest.Application.Vehicles;

public sealed class VehicleHandlersTests
{
    [Fact]
    public async Task CreateVehicle_WhenDuplicateVin_ShouldThrow()
    {
        var repo = new FakeVehicleRepository();
        var uow = new FakeUnitOfWork();

        repo.Vehicles.Add(new Vehicle(
            VehicleCategory.New,
            vin: "VIN123",
            make: "Ford",
            model: "Fiesta",
            yearModel: 2024,
            color: "Blue"));

        var handler = new CreateVehicleCommandHandler(repo, uow);

        var command = new CreateVehicleCommand(
            RequestedByUserId: Guid.NewGuid(),
            Request: new VehicleCreate(
                Category: VehicleCategory.New,
                Vin: "VIN123",
                Make: "Ford",
                Model: "Fiesta",
                YearModel: 2024,
                Color: "Blue",
                Plate: null,
                Trim: null,
                MileageKm: null,
                EvaluationId: null,
                DemoPurpose: null,
                IsRegistered: false));

        var act = async () => await handler.HandleAsync(command, CancellationToken.None);

        await act.Should().ThrowAsync<DomainException>();
        uow.CommitCount.Should().Be(0);
    }

    [Fact]
    public async Task CreateVehicle_WhenUsedMissingRequiredFields_ShouldThrow()
    {
        var repo = new FakeVehicleRepository();
        var uow = new FakeUnitOfWork();
        var handler = new CreateVehicleCommandHandler(repo, uow);

        var command = new CreateVehicleCommand(
            RequestedByUserId: Guid.NewGuid(),
            Request: new VehicleCreate(
                Category: VehicleCategory.Used,
                Vin: "VIN999",
                Make: "VW",
                Model: "Gol",
                YearModel: 2020,
                Color: "White",
                Plate: null,
                Trim: null,
                MileageKm: null,
                EvaluationId: null,
                DemoPurpose: null,
                IsRegistered: false));

        var act = async () => await handler.HandleAsync(command, CancellationToken.None);

        await act.Should().ThrowAsync<DomainException>();
        repo.Vehicles.Should().BeEmpty();
        uow.CommitCount.Should().Be(0);
    }

    [Fact]
    public async Task ChangeVehicleStatus_WhenVehicleNotFound_ShouldThrow()
    {
        var repo = new FakeVehicleRepository();
        var uow = new FakeUnitOfWork();
        var handler = new ChangeVehicleStatusCommandHandler(repo, uow);

        var command = new ChangeVehicleStatusCommand(
            VehicleId: Guid.NewGuid(),
            NewStatus: VehicleStatus.InStock,
            Reason: "manual-fix",
            ChangedByUserId: Guid.NewGuid());

        var act = async () => await handler.HandleAsync(command, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
        uow.CommitCount.Should().Be(0);
    }

    [Fact]
    public async Task ChangeVehicleStatus_WhenVehicleSold_ShouldThrow()
    {
        var repo = new FakeVehicleRepository();
        var uow = new FakeUnitOfWork();
        var handler = new ChangeVehicleStatusCommandHandler(repo, uow);

        var responsible = Guid.NewGuid();
        var vehicle = new Vehicle(
            VehicleCategory.Used,
            vin: "VIN-SOLD",
            make: "VW",
            model: "Gol",
            yearModel: 2020,
            color: "White",
            plate: "ABC1234",
            mileageKm: 100,
            evaluationId: Guid.NewGuid());

        vehicle.MarkInStock(responsible, "seed");
        vehicle.CheckOut(CheckOutReason.Sale, DateTime.UtcNow, responsible);

        repo.Vehicles.Add(vehicle);

        var command = new ChangeVehicleStatusCommand(
            VehicleId: vehicle.Id,
            NewStatus: VehicleStatus.InStock,
            Reason: "try-reopen",
            ChangedByUserId: responsible);

        var act = async () => await handler.HandleAsync(command, CancellationToken.None);

        await act.Should().ThrowAsync<DomainException>();
        uow.CommitCount.Should().Be(0);
    }

    [Fact]
    public async Task ChangeVehicleStatus_WhenValid_ShouldCommit()
    {
        var repo = new FakeVehicleRepository();
        var uow = new FakeUnitOfWork();
        var handler = new ChangeVehicleStatusCommandHandler(repo, uow);

        var responsible = Guid.NewGuid();
        var vehicle = new Vehicle(
            VehicleCategory.Used,
            vin: "VIN-OK",
            make: "VW",
            model: "Gol",
            yearModel: 2020,
            color: "White",
            plate: "DEF5678",
            mileageKm: 100,
            evaluationId: Guid.NewGuid());

        vehicle.MarkInStock(responsible, "seed");
        repo.Vehicles.Add(vehicle);

        var command = new ChangeVehicleStatusCommand(
            VehicleId: vehicle.Id,
            NewStatus: VehicleStatus.InPreparation,
            Reason: "prep",
            ChangedByUserId: responsible);

        var result = await handler.HandleAsync(command, CancellationToken.None);

        result.Should().BeTrue();
        vehicle.CurrentStatus.Should().Be(VehicleStatus.InPreparation);
        uow.CommitCount.Should().Be(1);
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
            IEnumerable<Vehicle> q = Vehicles;

            if (status.HasValue)
            {
                q = q.Where(v => v.CurrentStatus == status.Value);
            }

            if (category.HasValue)
            {
                q = q.Where(v => v.Category == category.Value);
            }

            if (!string.IsNullOrWhiteSpace(query))
            {
                var needle = query.Trim();
                q = q.Where(v =>
                    v.Vin.Contains(needle, StringComparison.OrdinalIgnoreCase) ||
                    (v.Plate?.Contains(needle, StringComparison.OrdinalIgnoreCase) ?? false) ||
                    v.Make.Contains(needle, StringComparison.OrdinalIgnoreCase) ||
                    v.Model.Contains(needle, StringComparison.OrdinalIgnoreCase) ||
                    (v.Trim?.Contains(needle, StringComparison.OrdinalIgnoreCase) ?? false));
            }

            var total = q.Count();
            var items = q
                .OrderByDescending(v => v.CreatedAt)
                .Skip(Math.Max(0, (page - 1) * size))
                .Take(Math.Max(1, size))
                .ToList();

            return Task.FromResult(((IReadOnlyList<Vehicle>)items, total));
        }

        public Task AddAsync(Vehicle vehicle, CancellationToken cancellationToken = default)
        {
            Vehicles.Add(vehicle);
            return Task.CompletedTask;
        }

        public Task UpdateAsync(Vehicle vehicle, CancellationToken cancellationToken = default)
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
