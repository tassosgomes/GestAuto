using GestAuto.Stock.Application.Interfaces;
using GestAuto.Stock.Application.Vehicles.Dto;
using GestAuto.Stock.Domain.Exceptions;
using GestAuto.Stock.Domain.Interfaces;

namespace GestAuto.Stock.Application.Vehicles.Commands;

public sealed class CreateCheckInCommandHandler : ICommandHandler<CreateCheckInCommand, CheckInResponse>
{
    private readonly IVehicleRepository _vehicleRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateCheckInCommandHandler(IVehicleRepository vehicleRepository, IUnitOfWork unitOfWork)
    {
        _vehicleRepository = vehicleRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<CheckInResponse> HandleAsync(CreateCheckInCommand command, CancellationToken cancellationToken)
    {
        var vehicle = await _vehicleRepository.GetByIdAsync(command.VehicleId, cancellationToken);
        if (vehicle is null)
        {
            throw new NotFoundException("Vehicle not found.");
        }

        var occurredAt = command.Request.OccurredAt ?? DateTime.UtcNow;

        vehicle.CheckIn(
            source: command.Request.Source,
            occurredAt: occurredAt,
            responsibleUserId: command.ResponsibleUserId,
            notes: command.Request.Notes);

        var record = vehicle.CheckIns.Last();

        await _vehicleRepository.UpdateAsync(vehicle, cancellationToken);
        await _unitOfWork.CommitAsync(cancellationToken);

        return new CheckInResponse(
            Id: record.Id,
            VehicleId: record.VehicleId,
            Source: record.Source,
            OccurredAt: record.OccurredAt,
            ResponsibleUserId: record.ResponsibleUserId,
            Notes: record.Notes,
            CurrentStatus: vehicle.CurrentStatus,
            CurrentOwnerUserId: vehicle.CurrentOwnerUserId);
    }
}
