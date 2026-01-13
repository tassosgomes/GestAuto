using GestAuto.Stock.Application.Interfaces;
using GestAuto.Stock.Application.Vehicles.Dto;
using GestAuto.Stock.Domain.Exceptions;
using GestAuto.Stock.Domain.Interfaces;

namespace GestAuto.Stock.Application.Vehicles.Commands;

public sealed class CreateCheckOutCommandHandler : ICommandHandler<CreateCheckOutCommand, CheckOutResponse>
{
    private readonly IVehicleRepository _vehicleRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateCheckOutCommandHandler(IVehicleRepository vehicleRepository, IUnitOfWork unitOfWork)
    {
        _vehicleRepository = vehicleRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<CheckOutResponse> HandleAsync(CreateCheckOutCommand command, CancellationToken cancellationToken)
    {
        var vehicle = await _vehicleRepository.GetByIdAsync(command.VehicleId, cancellationToken);
        if (vehicle is null)
        {
            throw new NotFoundException("Vehicle not found.");
        }

        var occurredAt = command.Request.OccurredAt ?? DateTime.UtcNow;

        vehicle.CheckOut(
            reason: command.Request.Reason,
            occurredAt: occurredAt,
            responsibleUserId: command.ResponsibleUserId,
            notes: command.Request.Notes);

        var record = vehicle.CheckOuts.Last();

        await _vehicleRepository.UpdateAsync(vehicle, cancellationToken);
        await _unitOfWork.CommitAsync(cancellationToken);

        return new CheckOutResponse(
            Id: record.Id,
            VehicleId: record.VehicleId,
            Reason: record.Reason,
            OccurredAt: record.OccurredAt,
            ResponsibleUserId: record.ResponsibleUserId,
            Notes: record.Notes,
            CurrentStatus: vehicle.CurrentStatus);
    }
}
