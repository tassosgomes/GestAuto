using GestAuto.Stock.Application.Interfaces;
using GestAuto.Stock.Application.Reservations.Dto;
using GestAuto.Stock.Application.TestDrives.Dto;
using GestAuto.Stock.Domain.Entities;
using GestAuto.Stock.Domain.Exceptions;
using GestAuto.Stock.Domain.History;
using GestAuto.Stock.Domain.Interfaces;

namespace GestAuto.Stock.Application.TestDrives.Commands;

public sealed class CompleteTestDriveCommandHandler : ICommandHandler<CompleteTestDriveCommand, CompleteTestDriveResponse>
{
    private readonly IVehicleRepository _vehicleRepository;
    private readonly ITestDriveRepository _testDriveRepository;
    private readonly IReservationRepository _reservationRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CompleteTestDriveCommandHandler(
        IVehicleRepository vehicleRepository,
        ITestDriveRepository testDriveRepository,
        IReservationRepository reservationRepository,
        IUnitOfWork unitOfWork)
    {
        _vehicleRepository = vehicleRepository;
        _testDriveRepository = testDriveRepository;
        _reservationRepository = reservationRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<CompleteTestDriveResponse> HandleAsync(CompleteTestDriveCommand command, CancellationToken cancellationToken)
    {
        var testDrive = await _testDriveRepository.GetByIdAsync(command.TestDriveId, cancellationToken);
        if (testDrive is null)
        {
            throw new NotFoundException("Test-drive session not found.");
        }

        var vehicle = await _vehicleRepository.GetByIdAsync(testDrive.VehicleId, cancellationToken);
        if (vehicle is null)
        {
            throw new NotFoundException("Vehicle not found.");
        }

        var endedAt = command.Request.EndedAt ?? DateTime.UtcNow;
        var outcome = command.Request.Outcome;

        var existingActiveReservation = await _reservationRepository.GetActiveByVehicleIdAsync(vehicle.Id, cancellationToken);

        if (outcome == TestDriveOutcome.ReturnedToStock && existingActiveReservation is not null)
        {
            throw new ConflictException("Vehicle has an active reservation and cannot be returned to stock.");
        }

        vehicle.CompleteTestDrive(
            testDriveId: command.TestDriveId,
            completedByUserId: command.CompletedByUserId,
            endedAt: endedAt,
            outcome: outcome);

        Guid? reservationId = null;

        if (outcome == TestDriveOutcome.ConvertedToReservation)
        {
            if (existingActiveReservation is not null)
            {
                reservationId = existingActiveReservation.Id;
            }
            else
            {
                var request = command.Request.Reservation ?? DefaultReservationRequest(command.TestDriveId);
                var reservation = new Reservation(
                    vehicleId: vehicle.Id,
                    type: request.Type,
                    salesPersonId: command.CompletedByUserId,
                    createdAtUtc: DateTime.UtcNow,
                    contextType: request.ContextType,
                    contextId: request.ContextId ?? command.TestDriveId,
                    bankDeadlineAtUtc: request.BankDeadlineAtUtc);

                reservationId = reservation.Id;
                await _reservationRepository.AddAsync(reservation, cancellationToken);
            }
        }

        await _vehicleRepository.UpdateAsync(vehicle, cancellationToken);
        await _unitOfWork.CommitAsync(cancellationToken);

        return new CompleteTestDriveResponse(
            TestDriveId: command.TestDriveId,
            VehicleId: vehicle.Id,
            Outcome: outcome,
            EndedAtUtc: endedAt,
            CurrentStatus: vehicle.CurrentStatus,
            ReservationId: reservationId);
    }

    private static CreateReservationRequest DefaultReservationRequest(Guid testDriveId)
    {
        return new CreateReservationRequest(
            Type: GestAuto.Stock.Domain.Enums.ReservationType.Standard,
            ContextType: "test-drive",
            ContextId: testDriveId,
            BankDeadlineAtUtc: null);
    }
}
