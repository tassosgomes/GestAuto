using GestAuto.Stock.Application.Common;
using GestAuto.Stock.Application.Interfaces;
using GestAuto.Stock.Application.Reservations.Dto;
using GestAuto.Stock.Domain.Enums;

namespace GestAuto.Stock.Application.Reservations.Queries;

public sealed record ListReservationsQuery(
    int Page,
    int Size,
    ReservationStatus? Status,
    ReservationType? Type,
    Guid? SalesPersonId,
    Guid? VehicleId
) : IQuery<PagedResponse<ReservationListItem>>;
