using GestAuto.Stock.Application.Interfaces;
using GestAuto.Stock.Application.Vehicles.Dto;
using GestAuto.Stock.Domain.Entities;
using GestAuto.Stock.Domain.Enums;
using GestAuto.Stock.Domain.Exceptions;
using GestAuto.Stock.Domain.History;
using GestAuto.Stock.Domain.Interfaces;

namespace GestAuto.Stock.Application.Vehicles.Queries;

public sealed class GetVehicleHistoryQueryHandler : IQueryHandler<GetVehicleHistoryQuery, VehicleHistoryResponse>
{
    private readonly IVehicleRepository _vehicleRepository;
    private readonly IReservationRepository _reservationRepository;
    private readonly IAuditEntryRepository _auditEntryRepository;

    public GetVehicleHistoryQueryHandler(
        IVehicleRepository vehicleRepository,
        IReservationRepository reservationRepository,
        IAuditEntryRepository auditEntryRepository)
    {
        _vehicleRepository = vehicleRepository;
        _reservationRepository = reservationRepository;
        _auditEntryRepository = auditEntryRepository;
    }

    public async Task<VehicleHistoryResponse> HandleAsync(GetVehicleHistoryQuery query, CancellationToken cancellationToken)
    {
        var vehicle = await _vehicleRepository.GetByIdAsync(query.VehicleId, cancellationToken);
        if (vehicle is null)
        {
            throw new NotFoundException("Vehicle not found.");
        }

        var reservations = await _reservationRepository.ListByVehicleIdAsync(query.VehicleId, cancellationToken);
        var auditEntries = await _auditEntryRepository.ListByVehicleIdAsync(query.VehicleId, cancellationToken);

        var items = new List<VehicleHistoryItemResponse>();

        items.AddRange(vehicle.CheckIns.Select(ToCheckInItem));
        items.AddRange(vehicle.CheckOuts.Select(ToCheckOutItem));
        items.AddRange(vehicle.TestDrives.SelectMany(ToTestDriveItems));
        items.AddRange(reservations.SelectMany(ToReservationItems));
        items.AddRange(auditEntries.Select(ToStatusChangedItem));

        var ordered = items
            .OrderBy(i => i.OccurredAtUtc)
            .ToList();

        return new VehicleHistoryResponse(vehicle.Id, ordered);
    }

    private static VehicleHistoryItemResponse ToCheckInItem(CheckInRecord record)
    {
        return new VehicleHistoryItemResponse(
            Type: "check-in",
            OccurredAtUtc: EnsureUtc(record.OccurredAt),
            UserId: record.ResponsibleUserId,
            Details: new Dictionary<string, object?>
            {
                ["source"] = record.Source.ToString(),
                ["notes"] = record.Notes
            });
    }

    private static VehicleHistoryItemResponse ToCheckOutItem(CheckOutRecord record)
    {
        return new VehicleHistoryItemResponse(
            Type: "check-out",
            OccurredAtUtc: EnsureUtc(record.OccurredAt),
            UserId: record.ResponsibleUserId,
            Details: new Dictionary<string, object?>
            {
                ["reason"] = record.Reason.ToString(),
                ["notes"] = record.Notes
            });
    }

    private static IEnumerable<VehicleHistoryItemResponse> ToTestDriveItems(TestDriveSession session)
    {
        yield return new VehicleHistoryItemResponse(
            Type: "test-drive-started",
            OccurredAtUtc: EnsureUtc(session.StartedAt),
            UserId: session.SalesPersonId,
            Details: new Dictionary<string, object?>
            {
                ["testDriveId"] = session.Id,
                ["customerRef"] = session.CustomerRef
            });

        if (session.EndedAt.HasValue)
        {
            yield return new VehicleHistoryItemResponse(
                Type: "test-drive-completed",
                OccurredAtUtc: EnsureUtc(session.EndedAt.Value),
                UserId: session.SalesPersonId,
                Details: new Dictionary<string, object?>
                {
                    ["testDriveId"] = session.Id,
                    ["outcome"] = session.Outcome?.ToString()
                });
        }
    }

    private static IEnumerable<VehicleHistoryItemResponse> ToReservationItems(Reservation reservation)
    {
        yield return new VehicleHistoryItemResponse(
            Type: "reservation-created",
            OccurredAtUtc: EnsureUtc(reservation.CreatedAtUtc),
            UserId: reservation.SalesPersonId,
            Details: new Dictionary<string, object?>
            {
                ["reservationId"] = reservation.Id,
                ["type"] = reservation.Type.ToString(),
                ["status"] = reservation.Status.ToString(),
                ["expiresAtUtc"] = reservation.ExpiresAtUtc,
                ["bankDeadlineAtUtc"] = reservation.BankDeadlineAtUtc,
                ["contextType"] = reservation.ContextType,
                ["contextId"] = reservation.ContextId
            });

        if (reservation.ExtendedAtUtc.HasValue && reservation.ExtendedByUserId.HasValue)
        {
            yield return new VehicleHistoryItemResponse(
                Type: "reservation-extended",
                OccurredAtUtc: EnsureUtc(reservation.ExtendedAtUtc.Value),
                UserId: reservation.ExtendedByUserId.Value,
                Details: new Dictionary<string, object?>
                {
                    ["reservationId"] = reservation.Id,
                    ["previousExpiresAtUtc"] = reservation.PreviousExpiresAtUtc,
                    ["newExpiresAtUtc"] = reservation.ExpiresAtUtc
                });
        }

        if (reservation.CancelledAtUtc.HasValue && reservation.CancelledByUserId.HasValue)
        {
            yield return new VehicleHistoryItemResponse(
                Type: "reservation-cancelled",
                OccurredAtUtc: EnsureUtc(reservation.CancelledAtUtc.Value),
                UserId: reservation.CancelledByUserId.Value,
                Details: new Dictionary<string, object?>
                {
                    ["reservationId"] = reservation.Id,
                    ["reason"] = reservation.CancelReason
                });
        }

        if (reservation.Status == ReservationStatus.Expired)
        {
            // Expiration is automatic; use the salesperson as the contextual responsible user.
            yield return new VehicleHistoryItemResponse(
                Type: "reservation-expired",
                OccurredAtUtc: EnsureUtc(reservation.UpdatedAt),
                UserId: reservation.SalesPersonId,
                Details: new Dictionary<string, object?>
                {
                    ["reservationId"] = reservation.Id,
                    ["expiresAtUtc"] = reservation.ExpiresAtUtc
                });
        }

        if (reservation.Status == ReservationStatus.Completed)
        {
            yield return new VehicleHistoryItemResponse(
                Type: "reservation-completed",
                OccurredAtUtc: EnsureUtc(reservation.UpdatedAt),
                UserId: reservation.SalesPersonId,
                Details: new Dictionary<string, object?>
                {
                    ["reservationId"] = reservation.Id
                });
        }
    }

    private static VehicleHistoryItemResponse ToStatusChangedItem(AuditEntry entry)
    {
        return new VehicleHistoryItemResponse(
            Type: "status-changed",
            OccurredAtUtc: EnsureUtc(entry.OccurredAtUtc),
            UserId: entry.ResponsibleUserId,
            Details: new Dictionary<string, object?>
            {
                ["previousStatus"] = entry.PreviousStatus.ToString(),
                ["newStatus"] = entry.NewStatus.ToString(),
                ["reason"] = entry.Reason
            });
    }

    private static DateTime EnsureUtc(DateTime value)
    {
        return value.Kind == DateTimeKind.Utc
            ? value
            : DateTime.SpecifyKind(value, DateTimeKind.Utc);
    }
}
