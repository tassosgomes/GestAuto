using GestAuto.Stock.Application.Interfaces;
using GestAuto.Stock.Application.Vehicles.Dto;
using GestAuto.Stock.Domain.Exceptions;
using GestAuto.Stock.Domain.Interfaces;

namespace GestAuto.Stock.Application.Vehicles.Queries;

public sealed class GetVehicleQueryHandler : IQueryHandler<GetVehicleQuery, VehicleResponse>
{
    private readonly IVehicleRepository _vehicleRepository;

    public GetVehicleQueryHandler(IVehicleRepository vehicleRepository)
    {
        _vehicleRepository = vehicleRepository;
    }

    public async Task<VehicleResponse> HandleAsync(GetVehicleQuery query, CancellationToken cancellationToken)
    {
        var vehicle = await _vehicleRepository.GetByIdAsync(query.VehicleId, cancellationToken);
        if (vehicle is null)
        {
            throw new NotFoundException("Vehicle not found.");
        }

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
