using GestAuto.Commercial.Application.Commands;
using GestAuto.Commercial.Application.DTOs;
using GestAuto.Commercial.Application.Handlers;
using GestAuto.Commercial.Domain.Entities;
using GestAuto.Commercial.Domain.Enums;
using GestAuto.Commercial.Domain.Exceptions;
using GestAuto.Commercial.Domain.Interfaces;
using GestAuto.Commercial.Domain.ValueObjects;
using GestAuto.Commercial.Infra.UnitOfWork;
using Moq;
using Xunit;

namespace GestAuto.Commercial.UnitTest.TestDrive;

public class ScheduleTestDriveHandlerTests
{
    private readonly Mock<ITestDriveRepository> _testDriveRepositoryMock;
    private readonly Mock<ILeadRepository> _leadRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly ScheduleTestDriveHandler _handler;

    public ScheduleTestDriveHandlerTests()
    {
        _testDriveRepositoryMock = new Mock<ITestDriveRepository>();
        _leadRepositoryMock = new Mock<ILeadRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _handler = new ScheduleTestDriveHandler(
            _testDriveRepositoryMock.Object,
            _leadRepositoryMock.Object,
            _unitOfWorkMock.Object);
    }

    [Fact]
    public async Task HandleAsync_WithValidCommand_SchedulesTestDrive()
    {
        // Arrange
        var leadId = Guid.NewGuid();
        var vehicleId = Guid.NewGuid();
        var salesPersonId = Guid.NewGuid();
        var scheduledAt = DateTime.UtcNow.AddDays(7);

        var lead = CreateTestLead(leadId, salesPersonId);
        _leadRepositoryMock.Setup(x => x.GetByIdAsync(leadId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(lead);

        _testDriveRepositoryMock.Setup(x => x.CheckVehicleAvailabilityAsync(vehicleId, scheduledAt, It.IsAny<TimeSpan?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _testDriveRepositoryMock.Setup(x => x.AddAsync(It.IsAny<Domain.Entities.TestDrive>(), It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult(Domain.Entities.TestDrive.Schedule(leadId, vehicleId, scheduledAt, salesPersonId, "Test notes")));

        var command = new ScheduleTestDriveCommand(leadId, vehicleId, scheduledAt, salesPersonId, "Test notes");

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(leadId, result.LeadId);
        Assert.Equal(vehicleId, result.VehicleId);
        Assert.Equal("Scheduled", result.Status);
        _testDriveRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Domain.Entities.TestDrive>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WithLeadNotFound_ThrowsNotFoundException()
    {
        // Arrange
        var leadId = Guid.NewGuid();
        var vehicleId = Guid.NewGuid();
        var salesPersonId = Guid.NewGuid();
        var scheduledAt = DateTime.UtcNow.AddDays(7);

        _leadRepositoryMock.Setup(x => x.GetByIdAsync(leadId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Domain.Entities.Lead)null);

        var command = new ScheduleTestDriveCommand(leadId, vehicleId, scheduledAt, salesPersonId, null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => _handler.HandleAsync(command, CancellationToken.None));
    }

    [Fact]
    public async Task HandleAsync_WithVehicleNotAvailable_ThrowsDomainException()
    {
        // Arrange
        var leadId = Guid.NewGuid();
        var vehicleId = Guid.NewGuid();
        var salesPersonId = Guid.NewGuid();
        var scheduledAt = DateTime.UtcNow.AddDays(7);

        var lead = CreateTestLead(leadId, salesPersonId);
        _leadRepositoryMock.Setup(x => x.GetByIdAsync(leadId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(lead);

        _testDriveRepositoryMock.Setup(x => x.CheckVehicleAvailabilityAsync(vehicleId, scheduledAt, It.IsAny<TimeSpan?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var command = new ScheduleTestDriveCommand(leadId, vehicleId, scheduledAt, salesPersonId, null);

        // Act & Assert
        await Assert.ThrowsAsync<DomainException>(() => _handler.HandleAsync(command, CancellationToken.None));
    }

    private static Domain.Entities.Lead CreateTestLead(Guid id, Guid salesPersonId)
    {
        return Domain.Entities.Lead.Create(
            "Test Lead",
            new Email("test@example.com"),
            new Phone("+55 11 99999-9999"),
            LeadSource.Showroom,
            salesPersonId);
    }
}

public class CompleteTestDriveHandlerTests
{
    private readonly Mock<ITestDriveRepository> _testDriveRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly CompleteTestDriveHandler _handler;

    public CompleteTestDriveHandlerTests()
    {
        _testDriveRepositoryMock = new Mock<ITestDriveRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _handler = new CompleteTestDriveHandler(
            _testDriveRepositoryMock.Object,
            _unitOfWorkMock.Object);
    }

    [Fact]
    public async Task HandleAsync_WithValidCommand_CompletesTestDrive()
    {
        // Arrange
        var testDriveId = Guid.NewGuid();
        var leadId = Guid.NewGuid();
        var vehicleId = Guid.NewGuid();
        var salesPersonId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var testDrive = Domain.Entities.TestDrive.Schedule(leadId, vehicleId, DateTime.UtcNow.AddDays(1), salesPersonId, "Test");

        _testDriveRepositoryMock.Setup(x => x.GetByIdAsync(testDriveId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(testDrive);

        var checklistDto = new TestDriveChecklistDto(
            1000m,
            1050m,
            "Full",
            "No issues observed");

        var command = new CompleteTestDriveCommand(
            testDriveId,
            checklistDto,
            "Great experience!",
            userId);

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Completed", result.Status);
        Assert.NotNull(result.Checklist);
        Assert.Equal(1000m, result.Checklist.InitialMileage);
        Assert.Equal(1050m, result.Checklist.FinalMileage);
        Assert.Equal("Full", result.Checklist.FuelLevel);
        Assert.Equal("Great experience!", result.CustomerFeedback);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WithTestDriveNotFound_ThrowsNotFoundException()
    {
        // Arrange
        var testDriveId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        _testDriveRepositoryMock.Setup(x => x.GetByIdAsync(testDriveId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Domain.Entities.TestDrive)null);

        var checklistDto = new TestDriveChecklistDto(1000m, 1050m, "Full", null);
        var command = new CompleteTestDriveCommand(testDriveId, checklistDto, null, userId);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => _handler.HandleAsync(command, CancellationToken.None));
    }

    [Fact]
    public async Task HandleAsync_WithInvalidFuelLevel_ThrowsArgumentException()
    {
        // Arrange
        var testDriveId = Guid.NewGuid();
        var leadId = Guid.NewGuid();
        var vehicleId = Guid.NewGuid();
        var salesPersonId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var testDrive = Domain.Entities.TestDrive.Schedule(leadId, vehicleId, DateTime.UtcNow.AddDays(1), salesPersonId, "Test");

        _testDriveRepositoryMock.Setup(x => x.GetByIdAsync(testDriveId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(testDrive);

        var checklistDto = new TestDriveChecklistDto(
            1000m,
            1050m,
            "InvalidFuelLevel",
            null);

        var command = new CompleteTestDriveCommand(testDriveId, checklistDto, null, userId);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _handler.HandleAsync(command, CancellationToken.None));
    }
}

public class CancelTestDriveHandlerTests
{
    private readonly Mock<ITestDriveRepository> _testDriveRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly CancelTestDriveHandler _handler;

    public CancelTestDriveHandlerTests()
    {
        _testDriveRepositoryMock = new Mock<ITestDriveRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _handler = new CancelTestDriveHandler(
            _testDriveRepositoryMock.Object,
            _unitOfWorkMock.Object);
    }

    [Fact]
    public async Task HandleAsync_WithValidCommand_CancelsTestDrive()
    {
        // Arrange
        var testDriveId = Guid.NewGuid();
        var leadId = Guid.NewGuid();
        var vehicleId = Guid.NewGuid();
        var salesPersonId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var testDrive = Domain.Entities.TestDrive.Schedule(leadId, vehicleId, DateTime.UtcNow.AddDays(1), salesPersonId, "Test");

        _testDriveRepositoryMock.Setup(x => x.GetByIdAsync(testDriveId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(testDrive);

        var command = new CancelTestDriveCommand(testDriveId, "Client requested", userId);

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Cancelled", result.Status);
        Assert.Equal("Client requested", result.CancellationReason);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}

public class TestDriveDomainTests
{
    [Fact]
    public void Schedule_WithValidData_CreatesTestDrive()
    {
        // Arrange
        var leadId = Guid.NewGuid();
        var vehicleId = Guid.NewGuid();
        var salesPersonId = Guid.NewGuid();
        var scheduledAt = DateTime.UtcNow.AddDays(7);

        // Act
        var testDrive = Domain.Entities.TestDrive.Schedule(leadId, vehicleId, scheduledAt, salesPersonId, "Test notes");

        // Assert
        Assert.NotNull(testDrive);
        Assert.Equal(leadId, testDrive.LeadId);
        Assert.Equal(vehicleId, testDrive.VehicleId);
        Assert.Equal(TestDriveStatus.Scheduled, testDrive.Status);
        Assert.Equal(scheduledAt, testDrive.ScheduledAt);
        Assert.Equal(salesPersonId, testDrive.SalesPersonId);
        Assert.Equal("Test notes", testDrive.Notes);
    }

    [Fact]
    public void Schedule_WithPastDate_ThrowsArgumentException()
    {
        // Arrange
        var leadId = Guid.NewGuid();
        var vehicleId = Guid.NewGuid();
        var salesPersonId = Guid.NewGuid();
        var scheduledAt = DateTime.UtcNow.AddDays(-1);

        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            Domain.Entities.TestDrive.Schedule(leadId, vehicleId, scheduledAt, salesPersonId, "Test"));
    }

    [Fact]
    public void Complete_WithValidChecklist_CompletesTestDrive()
    {
        // Arrange
        var leadId = Guid.NewGuid();
        var vehicleId = Guid.NewGuid();
        var salesPersonId = Guid.NewGuid();
        var testDrive = Domain.Entities.TestDrive.Schedule(leadId, vehicleId, DateTime.UtcNow.AddDays(1), salesPersonId, "Test");

        var checklist = new TestDriveChecklist(1000m, 1050m, FuelLevel.Full, "No issues");

        // Act
        testDrive.Complete(checklist, "Great!", Guid.NewGuid());

        // Assert
        Assert.Equal(TestDriveStatus.Completed, testDrive.Status);
        Assert.NotNull(testDrive.CompletedAt);
        Assert.NotNull(testDrive.Checklist);
        Assert.Equal("Great!", testDrive.CustomerFeedback);
    }

    [Fact]
    public void Complete_WithInvalidChecklist_ThrowsArgumentNullException()
    {
        // Arrange
        var leadId = Guid.NewGuid();
        var vehicleId = Guid.NewGuid();
        var salesPersonId = Guid.NewGuid();
        var testDrive = Domain.Entities.TestDrive.Schedule(leadId, vehicleId, DateTime.UtcNow.AddDays(1), salesPersonId, "Test");

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => testDrive.Complete(null));
    }

    [Fact]
    public void Cancel_WithValidTestDrive_CancelsIt()
    {
        // Arrange
        var leadId = Guid.NewGuid();
        var vehicleId = Guid.NewGuid();
        var salesPersonId = Guid.NewGuid();
        var testDrive = Domain.Entities.TestDrive.Schedule(leadId, vehicleId, DateTime.UtcNow.AddDays(1), salesPersonId, "Test");

        // Act
        testDrive.Cancel("Client change");

        // Assert
        Assert.Equal(TestDriveStatus.Cancelled, testDrive.Status);
        Assert.Equal("Client change", testDrive.CancellationReason);
    }

    [Fact]
    public void Cancel_WithCompletedTestDrive_ThrowsInvalidOperationException()
    {
        // Arrange
        var leadId = Guid.NewGuid();
        var vehicleId = Guid.NewGuid();
        var salesPersonId = Guid.NewGuid();
        var testDrive = Domain.Entities.TestDrive.Schedule(leadId, vehicleId, DateTime.UtcNow.AddDays(1), salesPersonId, "Test");
        var checklist = new TestDriveChecklist(1000m, 1050m, FuelLevel.Full, null);
        testDrive.Complete(checklist);

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => testDrive.Cancel("Reason"));
    }

    [Fact]
    public void TestDriveChecklist_WithValidData_CreatesChecklist()
    {
        // Act
        var checklist = new TestDriveChecklist(1000m, 1050m, FuelLevel.Half, "Some observations");

        // Assert
        Assert.NotNull(checklist);
        Assert.Equal(1000m, checklist.InitialMileage);
        Assert.Equal(1050m, checklist.FinalMileage);
        Assert.Equal(FuelLevel.Half, checklist.FuelLevel);
        Assert.Equal("Some observations", checklist.VisualObservations);
        Assert.Equal(50m, checklist.GetMileageDifference());
    }

    [Fact]
    public void TestDriveChecklist_WithFinalMileageLessThanInitial_ThrowsArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            new TestDriveChecklist(1050m, 1000m, FuelLevel.Full, null));
    }
}
