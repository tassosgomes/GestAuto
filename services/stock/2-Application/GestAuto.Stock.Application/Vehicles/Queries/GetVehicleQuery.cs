using GestAuto.Stock.Application.Interfaces;
using GestAuto.Stock.Application.Vehicles.Dto;

namespace GestAuto.Stock.Application.Vehicles.Queries;

public sealed record GetVehicleQuery(Guid VehicleId) : IQuery<VehicleResponse>;
