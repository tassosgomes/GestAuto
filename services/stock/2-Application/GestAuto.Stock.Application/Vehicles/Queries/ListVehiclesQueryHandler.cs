using GestAuto.Stock.Application.Common;
using GestAuto.Stock.Application.Interfaces;
using GestAuto.Stock.Application.Vehicles.Dto;
using GestAuto.Stock.Domain.Interfaces;

namespace GestAuto.Stock.Application.Vehicles.Queries;

public sealed class ListVehiclesQueryHandler : IQueryHandler<ListVehiclesQuery, PagedResponse<VehicleListItem>>
{
    private readonly IVehicleRepository _vehicleRepository;

    public ListVehiclesQueryHandler(IVehicleRepository vehicleRepository)
    {
        _vehicleRepository = vehicleRepository;
    }

    public async Task<PagedResponse<VehicleListItem>> HandleAsync(ListVehiclesQuery query, CancellationToken cancellationToken)
    {
        var (items, total) = await _vehicleRepository.ListAsync(
            page: query.Page,
            size: query.Size,
            status: query.Status,
            category: query.Category,
            query: query.Query,
            cancellationToken: cancellationToken);

        var data = items
            .Select(v => new VehicleListItem(
                Id: v.Id,
                Category: v.Category,
                CurrentStatus: v.CurrentStatus,
                Vin: v.Vin,
                Plate: v.Plate,
                Make: v.Make,
                Model: v.Model,
                Trim: v.Trim,
                YearModel: v.YearModel,
                Color: v.Color,
                CreatedAt: v.CreatedAt,
                UpdatedAt: v.UpdatedAt))
            .ToList();

        var totalPages = query.Size <= 0 ? 0 : (int)Math.Ceiling(total / (double)query.Size);
        var pagination = new PaginationMetadata(query.Page, query.Size, total, totalPages);

        return new PagedResponse<VehicleListItem>(data, pagination);
    }
}
