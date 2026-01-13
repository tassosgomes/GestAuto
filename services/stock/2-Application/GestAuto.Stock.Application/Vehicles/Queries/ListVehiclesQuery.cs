using GestAuto.Stock.Application.Common;
using GestAuto.Stock.Application.Interfaces;
using GestAuto.Stock.Application.Vehicles.Dto;
using GestAuto.Stock.Domain.Enums;

namespace GestAuto.Stock.Application.Vehicles.Queries;

public sealed record ListVehiclesQuery(
    int Page,
    int Size,
    VehicleStatus? Status,
    VehicleCategory? Category,
    string? Query) : IQuery<PagedResponse<VehicleListItem>>;
