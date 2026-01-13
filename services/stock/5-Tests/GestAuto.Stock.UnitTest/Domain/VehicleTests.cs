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

    [Fact]
    public void CheckOut_WhenSale_ShouldSetSoldAndAddRecord()
    {
        var vehicle = new Vehicle(
            VehicleCategory.Used,
            vin: "VIN-SALE",
            make: "VW",
            model: "Gol",
            yearModel: 2020,
            color: "White",
            plate: "AAA1234",
            mileageKm: 100,
            evaluationId: Guid.NewGuid());

        var userId = Guid.NewGuid();
        vehicle.MarkInStock(userId, "seed");

        var now = DateTime.UtcNow;
        vehicle.CheckOut(CheckOutReason.Sale, now, userId, notes: "sold");

        vehicle.CurrentStatus.Should().Be(VehicleStatus.Sold);
        vehicle.CheckOuts.Should().HaveCount(1);
        vehicle.CheckOuts.First().Reason.Should().Be(CheckOutReason.Sale);
    }

    [Fact]
    public void CheckOut_WhenTotalLoss_ShouldSetWrittenOff()
    {
        var vehicle = new Vehicle(
            VehicleCategory.Used,
            vin: "VIN-TL",
            make: "VW",
            model: "Gol",
            yearModel: 2020,
            color: "White",
            plate: "BBB2345",
            mileageKm: 100,
            evaluationId: Guid.NewGuid());

        var userId = Guid.NewGuid();
        vehicle.MarkInStock(userId, "seed");

        vehicle.CheckOut(CheckOutReason.TotalLoss, DateTime.UtcNow, userId);

        vehicle.CurrentStatus.Should().Be(VehicleStatus.WrittenOff);
    }

    [Fact]
    public void CheckOut_WhenAlreadySold_ShouldThrow()
    {
        var vehicle = new Vehicle(
            VehicleCategory.Used,
            vin: "VIN-FINAL",
            make: "VW",
            model: "Gol",
            yearModel: 2020,
            color: "White",
            plate: "CCC3456",
            mileageKm: 100,
            evaluationId: Guid.NewGuid());

        var userId = Guid.NewGuid();
        vehicle.MarkInStock(userId, "seed");
        vehicle.CheckOut(CheckOutReason.Sale, DateTime.UtcNow, userId);

        var act = () => vehicle.CheckOut(CheckOutReason.Transfer, DateTime.UtcNow, userId);

        act.Should().Throw<DomainException>();
    }
}
