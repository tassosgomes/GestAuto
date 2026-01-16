using GestAuto.Stock.Application.Common;
using GestAuto.Stock.Application.Interfaces;
using GestAuto.Stock.Application.Reservations.Dto;
using GestAuto.Stock.Domain.Entities;
using GestAuto.Stock.Domain.Interfaces;

namespace GestAuto.Stock.Application.Reservations.Queries;

public sealed class ListReservationsQueryHandler : IQueryHandler<ListReservationsQuery, PagedResponse<ReservationListItem>>
{
    private readonly IReservationRepository _reservationRepository;
    private readonly IVehicleRepository _vehicleRepository;

    public ListReservationsQueryHandler(
        IReservationRepository reservationRepository,
        IVehicleRepository vehicleRepository)
    {
        _reservationRepository = reservationRepository;
        _vehicleRepository = vehicleRepository;
    }

    public async Task<PagedResponse<ReservationListItem>> HandleAsync(ListReservationsQuery query, CancellationToken cancellationToken)
    {
        var (items, total) = await _reservationRepository.ListAsync(
            page: query.Page,
            size: query.Size,
            status: query.Status,
            type: query.Type,
            salesPersonId: query.SalesPersonId,
            vehicleId: query.VehicleId,
            cancellationToken: cancellationToken);

        // Get all unique vehicle IDs to fetch vehicle details
        var vehicleIds = items.Select(r => r.VehicleId).Distinct().ToList();
        var vehicles = new Dictionary<Guid, Vehicle>();
        foreach (var vehicleId in vehicleIds)
        {
            var vehicle = await _vehicleRepository.GetByIdAsync(vehicleId, cancellationToken);
            if (vehicle != null)
            {
                vehicles[vehicleId] = vehicle;
            }
        }

        // Map to list items
        var data = items
            .Select(r =>
            {
                vehicles.TryGetValue(r.VehicleId, out var vehicle);
                return new ReservationListItem(
                    Id: r.Id,
                    VehicleId: r.VehicleId,
                    VehicleMake: vehicle?.Make ?? "N/A",
                    VehicleModel: vehicle?.Model ?? "N/A",
                    VehicleTrim: vehicle?.Trim,
                    VehicleYearModel: vehicle?.YearModel ?? 0,
                    VehiclePlate: vehicle?.Plate,
                    Type: r.Type,
                    Status: r.Status,
                    SalesPersonId: r.SalesPersonId,
                    SalesPersonName: r.SalesPersonId.ToString(), // TODO: Fetch from user service or use claim
                    CreatedAtUtc: r.CreatedAtUtc,
                    ExpiresAtUtc: r.ExpiresAtUtc,
                    BankDeadlineAtUtc: r.BankDeadlineAtUtc);
            })
            .ToList();

        var totalPages = query.Size <= 0 ? 0 : (int)Math.Ceiling(total / (double)query.Size);
        var pagination = new PaginationMetadata(query.Page, query.Size, total, totalPages);

        return new PagedResponse<ReservationListItem>(data, pagination);
    }
}
