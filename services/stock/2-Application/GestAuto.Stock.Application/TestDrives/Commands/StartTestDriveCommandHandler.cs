using GestAuto.Stock.Application.Interfaces;
using GestAuto.Stock.Application.TestDrives.Dto;
using GestAuto.Stock.Domain.Exceptions;
using GestAuto.Stock.Domain.Interfaces;

namespace GestAuto.Stock.Application.TestDrives.Commands;

public sealed class StartTestDriveCommandHandler : ICommandHandler<StartTestDriveCommand, StartTestDriveResponse>
{
    private readonly IVehicleRepository _vehicleRepository;
    private readonly IUnitOfWork _unitOfWork;

    public StartTestDriveCommandHandler(IVehicleRepository vehicleRepository, IUnitOfWork unitOfWork)
    {
        _vehicleRepository = vehicleRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<StartTestDriveResponse> HandleAsync(StartTestDriveCommand command, CancellationToken cancellationToken)
    {
        var vehicle = await _vehicleRepository.GetByIdAsync(command.VehicleId, cancellationToken);
        if (vehicle is null)
        {
            throw new NotFoundException("Vehicle not found.");
        }

        var startedAt = command.Request.StartedAt ?? DateTime.UtcNow;

        var testDriveId = vehicle.StartTestDrive(
            salesPersonId: command.SalesPersonId,
            customerRef: command.Request.CustomerRef,
            startedAt: startedAt);

        await _vehicleRepository.UpdateAsync(vehicle, cancellationToken);
        await _unitOfWork.CommitAsync(cancellationToken);

        return new StartTestDriveResponse(
            TestDriveId: testDriveId,
            VehicleId: vehicle.Id,
            SalesPersonId: command.SalesPersonId,
            CustomerRef: command.Request.CustomerRef,
            StartedAtUtc: startedAt);
    }
}
