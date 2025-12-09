using GestAuto.Commercial.Application.Interfaces;
using GestAuto.Commercial.Domain.Entities;
using GestAuto.Commercial.Domain.Enums;
using GestAuto.Commercial.Domain.Interfaces;
using GestAuto.Commercial.Domain.ValueObjects;
using GestAuto.Commercial.Domain.Exceptions;
using GestAuto.Commercial.Infra.UnitOfWork;

namespace GestAuto.Commercial.Application.Handlers;

public class ScheduleTestDriveHandler : ICommandHandler<Commands.ScheduleTestDriveCommand, DTOs.TestDriveResponse>
{
    private readonly ITestDriveRepository _testDriveRepository;
    private readonly ILeadRepository _leadRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ScheduleTestDriveHandler(
        ITestDriveRepository testDriveRepository,
        ILeadRepository leadRepository,
        IUnitOfWork unitOfWork)
    {
        _testDriveRepository = testDriveRepository;
        _leadRepository = leadRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<DTOs.TestDriveResponse> HandleAsync(
        Commands.ScheduleTestDriveCommand command,
        CancellationToken cancellationToken)
    {
        // Verify lead exists
        var lead = await _leadRepository.GetByIdAsync(command.LeadId, cancellationToken)
            ?? throw new NotFoundException($"Lead {command.LeadId} not found");

        // Check vehicle availability
        var isAvailable = await _testDriveRepository.CheckVehicleAvailabilityAsync(
            command.VehicleId,
            command.ScheduledAt,
            cancellationToken: cancellationToken);

        if (!isAvailable)
            throw new DomainException("Vehicle is not available at the requested time");

        // Create and schedule test drive
        var testDrive = TestDrive.Schedule(
            command.LeadId,
            command.VehicleId,
            command.ScheduledAt,
            command.SalesPersonId,
            command.Notes
        );

        await _testDriveRepository.AddAsync(testDrive, cancellationToken);

        // Update lead status
        lead.ChangeStatus(LeadStatus.TestDriveScheduled);
        await _leadRepository.UpdateAsync(lead, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return DTOs.TestDriveResponse.FromEntity(testDrive);
    }
}

public class CompleteTestDriveHandler : ICommandHandler<Commands.CompleteTestDriveCommand, DTOs.TestDriveResponse>
{
    private readonly ITestDriveRepository _testDriveRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CompleteTestDriveHandler(
        ITestDriveRepository testDriveRepository,
        IUnitOfWork unitOfWork)
    {
        _testDriveRepository = testDriveRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<DTOs.TestDriveResponse> HandleAsync(
        Commands.CompleteTestDriveCommand command,
        CancellationToken cancellationToken)
    {
        var testDrive = await _testDriveRepository.GetByIdAsync(command.TestDriveId, cancellationToken)
            ?? throw new NotFoundException($"Test-drive {command.TestDriveId} not found");

        // Convert checklist DTO to value object
        var fuelLevel = Enum.Parse<FuelLevel>(command.Checklist.FuelLevel, ignoreCase: true);
        var checklist = new TestDriveChecklist(
            command.Checklist.InitialMileage,
            command.Checklist.FinalMileage,
            fuelLevel,
            command.Checklist.VisualObservations
        );

        testDrive.Complete(checklist, command.CustomerFeedback, command.CompletedByUserId);

        await _testDriveRepository.UpdateAsync(testDrive, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return DTOs.TestDriveResponse.FromEntity(testDrive);
    }
}

public class CancelTestDriveHandler : ICommandHandler<Commands.CancelTestDriveCommand, DTOs.TestDriveResponse>
{
    private readonly ITestDriveRepository _testDriveRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CancelTestDriveHandler(
        ITestDriveRepository testDriveRepository,
        IUnitOfWork unitOfWork)
    {
        _testDriveRepository = testDriveRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<DTOs.TestDriveResponse> HandleAsync(
        Commands.CancelTestDriveCommand command,
        CancellationToken cancellationToken)
    {
        var testDrive = await _testDriveRepository.GetByIdAsync(command.TestDriveId, cancellationToken)
            ?? throw new NotFoundException($"Test-drive {command.TestDriveId} not found");

        testDrive.Cancel(command.Reason);

        await _testDriveRepository.UpdateAsync(testDrive, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return DTOs.TestDriveResponse.FromEntity(testDrive);
    }
}
