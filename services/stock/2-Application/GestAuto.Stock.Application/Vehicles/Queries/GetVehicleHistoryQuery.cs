using GestAuto.Stock.Application.Interfaces;
using GestAuto.Stock.Application.Vehicles.Dto;

namespace GestAuto.Stock.Application.Vehicles.Queries;

public sealed record GetVehicleHistoryQuery(Guid VehicleId) : IQuery<VehicleHistoryResponse>;
