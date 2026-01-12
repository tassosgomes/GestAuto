using FluentAssertions;
using GestAuto.Stock.Domain.Entities;
using GestAuto.Stock.Domain.Enums;
using GestAuto.Stock.Domain.Exceptions;
using GestAuto.Stock.Domain.History;

namespace GestAuto.Stock.UnitTest.Domain;

public class VehicleTests
{
    [Fact]
    public void CheckIn_WhenNewVehicleAndSourceIsNotManufacturer_ShouldThrow()
    {
        var vehicle = new Vehicle(
            VehicleCategory.New,
            vin: "VIN123",
            make: "Ford",
            model: "Fiesta",
            yearModel: 2024,
            color: "Blue");

        var act = () => vehicle.CheckIn(CheckInSource.CustomerUsedPurchase, DateTime.UtcNow, Guid.NewGuid());

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void CheckIn_WhenUsedVehicleMissingRequiredFields_ShouldThrow()
    {
        var vehicle = new Vehicle(
            VehicleCategory.Used,
            vin: "VIN123",
            make: "VW",
            model: "Gol",
            yearModel: 2020,
            color: "White");

        var act = () => vehicle.CheckIn(CheckInSource.CustomerUsedPurchase, DateTime.UtcNow, Guid.NewGuid());

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void CheckIn_WhenUsedVehicleHasRequiredFields_ShouldSetInStock()
    {
        var vehicle = new Vehicle(
            VehicleCategory.Used,
            vin: "VIN123",
            make: "VW",
            model: "Gol",
            yearModel: 2020,
            color: "White",
            plate: "ABC1234",
            mileageKm: 10000,
            evaluationId: Guid.NewGuid());

        vehicle.CheckIn(CheckInSource.CustomerUsedPurchase, DateTime.UtcNow, Guid.NewGuid());

        vehicle.CurrentStatus.Should().Be(VehicleStatus.InStock);
    }

    [Fact]
    public void CheckIn_WhenDemoVehicleRegisteredButMissingPlate_ShouldThrow()
    {
        var vehicle = new Vehicle(
            VehicleCategory.Demonstration,
            vin: "VIN123",
            make: "Toyota",
            model: "Corolla",
            yearModel: 2023,
            color: "Silver",
            demoPurpose: DemoPurpose.TestDrive,
            isRegistered: true);

        var act = () => vehicle.CheckIn(CheckInSource.InternalFleet, DateTime.UtcNow, Guid.NewGuid());

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Reserve_WhenVehicleNotInStock_ShouldThrow()
    {
        var vehicle = new Vehicle(
            VehicleCategory.New,
            vin: "VIN123",
            make: "Ford",
            model: "Fiesta",
            yearModel: 2024,
            color: "Blue");

        var act = () => vehicle.Reserve(Guid.NewGuid(), Guid.NewGuid());

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void StartAndCompleteTestDrive_ShouldChangeStatusBasedOnOutcome()
    {
        var vehicle = new Vehicle(
            VehicleCategory.Used,
            vin: "VIN123",
            make: "VW",
            model: "Gol",
            yearModel: 2020,
            color: "White",
            plate: "ABC1234",
            mileageKm: 10000,
            evaluationId: Guid.NewGuid());

        var now = DateTime.UtcNow;
        var sellerId = Guid.NewGuid();

        vehicle.CheckIn(CheckInSource.CustomerUsedPurchase, now, sellerId);

        var testDriveId = vehicle.StartTestDrive(sellerId, customerRef: "customer-1", startedAt: now);
        vehicle.CurrentStatus.Should().Be(VehicleStatus.InTestDrive);

        vehicle.CompleteTestDrive(testDriveId, sellerId, now.AddMinutes(30), TestDriveOutcome.ReturnedToStock);
        vehicle.CurrentStatus.Should().Be(VehicleStatus.InStock);
    }
}
