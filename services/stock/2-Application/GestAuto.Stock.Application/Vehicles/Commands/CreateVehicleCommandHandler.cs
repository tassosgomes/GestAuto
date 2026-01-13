using GestAuto.Stock.Application.Interfaces;
using GestAuto.Stock.Application.Vehicles.Dto;
using GestAuto.Stock.Domain.Entities;
using GestAuto.Stock.Domain.Enums;
using GestAuto.Stock.Domain.Exceptions;
using GestAuto.Stock.Domain.Interfaces;

namespace GestAuto.Stock.Application.Vehicles.Commands;

public sealed class CreateVehicleCommandHandler : ICommandHandler<CreateVehicleCommand, VehicleResponse>
{
    private readonly IVehicleRepository _vehicleRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateVehicleCommandHandler(IVehicleRepository vehicleRepository, IUnitOfWork unitOfWork)
    {
        _vehicleRepository = vehicleRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<VehicleResponse> HandleAsync(CreateVehicleCommand command, CancellationToken cancellationToken)
    {
        var request = command.Request;

        ValidateCategoryRequirements(request);

        var existingByVin = await _vehicleRepository.GetByVinAsync(request.Vin.Trim(), cancellationToken);
        if (existingByVin is not null)
        {
            throw new DomainException("Vehicle with the same VIN already exists.");
        }

        if (!string.IsNullOrWhiteSpace(request.Plate))
        {
            var existingByPlate = await _vehicleRepository.GetByPlateAsync(request.Plate.Trim(), cancellationToken);
            if (existingByPlate is not null)
            {
                throw new DomainException("Vehicle with the same plate already exists.");
            }
        }

        var vehicle = new Vehicle(
            category: request.Category,
            vin: request.Vin,
            make: request.Make,
            model: request.Model,
            yearModel: request.YearModel,
            color: request.Color,
            plate: request.Plate,
            trim: request.Trim,
            mileageKm: request.MileageKm,
            evaluationId: request.EvaluationId,
            demoPurpose: request.DemoPurpose,
            isRegistered: request.IsRegistered);

        // Ensure initial status is coherent with the MVP flow.
        if (vehicle.Category is VehicleCategory.Used or VehicleCategory.Demonstration)
        {
            vehicle.MarkInStock(command.RequestedByUserId, "initial-create");
        }

        await _vehicleRepository.AddAsync(vehicle, cancellationToken);
        await _unitOfWork.CommitAsync(cancellationToken);

        return MapToResponse(vehicle);
    }

    private static void ValidateCategoryRequirements(VehicleCreate request)
    {
        if (string.IsNullOrWhiteSpace(request.Vin))
        {
            throw new DomainException("Vin is required.");
        }

        if (request.Category == VehicleCategory.Used)
        {
            if (string.IsNullOrWhiteSpace(request.Plate))
            {
                throw new DomainException("Plate is required for used vehicles.");
            }

            if (!request.MileageKm.HasValue || request.MileageKm.Value < 0)
            {
                throw new DomainException("MileageKm is required for used vehicles.");
            }

            if (!request.EvaluationId.HasValue || request.EvaluationId.Value == Guid.Empty)
            {
                throw new DomainException("EvaluationId is required for used vehicles.");
            }
        }

        if (request.Category == VehicleCategory.Demonstration)
        {
            if (!request.DemoPurpose.HasValue)
            {
                throw new DomainException("DemoPurpose is required for demonstration vehicles.");
            }

            if (request.IsRegistered && string.IsNullOrWhiteSpace(request.Plate))
            {
                throw new DomainException("Plate is required when the demonstration vehicle is registered.");
            }
        }
    }

    private static VehicleResponse MapToResponse(Vehicle vehicle)
    {
        return new VehicleResponse(
            Id: vehicle.Id,
            Category: vehicle.Category,
            CurrentStatus: vehicle.CurrentStatus,
            Vin: vehicle.Vin,
            Plate: vehicle.Plate,
            Make: vehicle.Make,
            Model: vehicle.Model,
            Trim: vehicle.Trim,
            YearModel: vehicle.YearModel,
            Color: vehicle.Color,
            MileageKm: vehicle.MileageKm,
            EvaluationId: vehicle.EvaluationId,
            DemoPurpose: vehicle.DemoPurpose,
            IsRegistered: vehicle.IsRegistered,
            CurrentOwnerUserId: vehicle.CurrentOwnerUserId,
            CreatedAt: vehicle.CreatedAt,
            UpdatedAt: vehicle.UpdatedAt);
    }
}
