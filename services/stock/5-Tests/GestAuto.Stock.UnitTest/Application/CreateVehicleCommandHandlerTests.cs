using FluentAssertions;
using GestAuto.Stock.Application.Vehicles.Commands;
using GestAuto.Stock.Application.Vehicles.Dto;
using GestAuto.Stock.Domain.Entities;
using GestAuto.Stock.Domain.Enums;
using GestAuto.Stock.Domain.Exceptions;
using GestAuto.Stock.Domain.Interfaces;
using Moq;

namespace GestAuto.Stock.UnitTest.Application;

public sealed class CreateVehicleCommandHandlerTests
{
    private readonly CreateVehicleCommandHandler _handler;
    private readonly Mock<IVehicleRepository> _vehicleRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;

    public CreateVehicleCommandHandlerTests()
    {
        _vehicleRepositoryMock = new Mock<IVehicleRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _handler = new CreateVehicleCommandHandler(
            _vehicleRepositoryMock.Object,
            _unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_UsedVehicleWithoutPlate_ThrowsDomainException()
    {
        // Arrange
        var request = CreateUsedVehicleRequest(plate: null, mileageKm: 1000, evaluationId: Guid.NewGuid());
        var command = CreateCommand(request);

        // Act
        var act = () => _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<DomainException>();
    }

    [Fact]
    public async Task Handle_UsedVehicleWithoutMileageKm_ThrowsDomainException()
    {
        // Arrange
        var request = CreateUsedVehicleRequest(plate: "ABC1234", mileageKm: null, evaluationId: Guid.NewGuid());
        var command = CreateCommand(request);

        // Act
        var act = () => _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<DomainException>();
    }

    [Fact]
    public async Task Handle_UsedVehicleWithoutEvaluationId_ThrowsDomainException()
    {
        // Arrange
        var request = CreateUsedVehicleRequest(plate: "ABC1234", mileageKm: 1000, evaluationId: null);
        var command = CreateCommand(request);

        // Act
        var act = () => _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<DomainException>();
    }

    [Fact]
    public async Task Handle_UsedVehicleWithAllRequiredFields_CreatesVehicleSuccessfully()
    {
        // Arrange
        var request = CreateUsedVehicleRequest(plate: "ABC1234", mileageKm: 1000, evaluationId: Guid.NewGuid());
        var command = CreateCommand(request);

        _vehicleRepositoryMock
            .Setup(repo => repo.GetByVinAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Vehicle?)null);

        _vehicleRepositoryMock
            .Setup(repo => repo.GetByPlateAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Vehicle?)null);

        _unitOfWorkMock
            .Setup(uow => uow.CommitAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Category.Should().Be(VehicleCategory.Used);
        result.Vin.Should().Be(request.Vin);
        result.Plate.Should().Be(request.Plate);

        _vehicleRepositoryMock.Verify(
            repo => repo.AddAsync(It.Is<Vehicle>(vehicle => vehicle.Vin == request.Vin), It.IsAny<CancellationToken>()),
            Times.Once);

        _unitOfWorkMock.Verify(
            uow => uow.CommitAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_NewVehicleWithoutVin_ThrowsDomainException()
    {
        // Arrange
        var request = new VehicleCreate(
            Category: VehicleCategory.New,
            Vin: string.Empty,
            Make: "Ford",
            Model: "Fiesta",
            YearModel: 2024,
            Color: "Blue",
            Plate: null,
            Trim: null,
            MileageKm: null,
            EvaluationId: null,
            DemoPurpose: null,
            IsRegistered: false);

        var command = CreateCommand(request);

        // Act
        var act = () => _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<DomainException>();
    }

    [Fact]
    public async Task Handle_DemonstrationVehicleWithoutDemoPurpose_ThrowsDomainException()
    {
        // Arrange
        var request = new VehicleCreate(
            Category: VehicleCategory.Demonstration,
            Vin: "VIN-DEMO-001",
            Make: "Toyota",
            Model: "Corolla",
            YearModel: 2023,
            Color: "Silver",
            Plate: null,
            Trim: null,
            MileageKm: null,
            EvaluationId: null,
            DemoPurpose: null,
            IsRegistered: false);

        var command = CreateCommand(request);

        // Act
        var act = () => _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<DomainException>();
    }

    [Fact]
    public async Task Handle_DuplicateVin_ThrowsDomainException()
    {
        // Arrange
        var request = new VehicleCreate(
            Category: VehicleCategory.New,
            Vin: "VIN-DUP-001",
            Make: "Ford",
            Model: "Ka",
            YearModel: 2022,
            Color: "Red",
            Plate: null,
            Trim: null,
            MileageKm: null,
            EvaluationId: null,
            DemoPurpose: null,
            IsRegistered: false);

        var command = CreateCommand(request);

        _vehicleRepositoryMock
            .Setup(repo => repo.GetByVinAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Vehicle(
                VehicleCategory.New,
                vin: request.Vin,
                make: request.Make,
                model: request.Model,
                yearModel: request.YearModel,
                color: request.Color));

        // Act
        var act = () => _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<DomainException>();
        _vehicleRepositoryMock.Verify(
            repo => repo.AddAsync(It.IsAny<Vehicle>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    private static CreateVehicleCommand CreateCommand(VehicleCreate request)
        => new(Guid.NewGuid(), request);

    private static VehicleCreate CreateUsedVehicleRequest(string? plate, int? mileageKm, Guid? evaluationId)
        => new(
            Category: VehicleCategory.Used,
            Vin: "VIN-USED-001",
            Make: "Honda",
            Model: "Civic",
            YearModel: 2021,
            Color: "Black",
            Plate: plate,
            Trim: "EX",
            MileageKm: mileageKm,
            EvaluationId: evaluationId,
            DemoPurpose: null,
            IsRegistered: false);
}
