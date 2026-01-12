using GestAuto.Stock.Domain.Enums;
using GestAuto.Stock.Domain.Events;
using GestAuto.Stock.Domain.Exceptions;

namespace GestAuto.Stock.Domain.Entities;

public sealed class Reservation : BaseEntity
{
    public Guid VehicleId { get; private set; }
    public ReservationType Type { get; private set; }
    public ReservationStatus Status { get; private set; }

    public Guid SalesPersonId { get; private set; }

    public string ContextType { get; private set; } = null!;
    public Guid? ContextId { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }

    public DateTime? ExpiresAtUtc { get; private set; }
    public DateTime? BankDeadlineAtUtc { get; private set; }

    public DateTime? CancelledAtUtc { get; private set; }
    public Guid? CancelledByUserId { get; private set; }
    public string? CancelReason { get; private set; }

    public DateTime? ExtendedAtUtc { get; private set; }
    public Guid? ExtendedByUserId { get; private set; }
    public DateTime? PreviousExpiresAtUtc { get; private set; }

    private Reservation() { }

    public Reservation(
        Guid vehicleId,
        ReservationType type,
        Guid salesPersonId,
        DateTime createdAtUtc,
        string contextType,
        Guid? contextId = null,
        DateTime? bankDeadlineAtUtc = null)
    {
        if (vehicleId == Guid.Empty)
        {
            throw new DomainException("VehicleId is required.");
        }

        if (salesPersonId == Guid.Empty)
        {
            throw new DomainException("SalesPersonId is required.");
        }

        if (string.IsNullOrWhiteSpace(contextType))
        {
            throw new DomainException("ContextType is required.");
        }

        VehicleId = vehicleId;
        Type = type;
        Status = ReservationStatus.Active;
        SalesPersonId = salesPersonId;

        CreatedAtUtc = DateTime.SpecifyKind(createdAtUtc, DateTimeKind.Utc);

        ContextType = contextType.Trim();
        ContextId = contextId;

        BankDeadlineAtUtc = bankDeadlineAtUtc;
        ExpiresAtUtc = DetermineExpiresAtUtc(type, CreatedAtUtc, bankDeadlineAtUtc);

        AddEvent(new ReservationCreatedEvent(Id, vehicleId, salesPersonId));
    }

    public void Cancel(Guid cancelledByUserId, string reason, DateTime cancelledAtUtc)
    {
        if (Status is ReservationStatus.Cancelled or ReservationStatus.Completed or ReservationStatus.Expired)
        {
            throw new DomainException("Reservation cannot be cancelled in its current status.");
        }

        if (cancelledByUserId == Guid.Empty)
        {
            throw new DomainException("CancelledByUserId is required.");
        }

        if (string.IsNullOrWhiteSpace(reason))
        {
            throw new DomainException("Cancel reason is required.");
        }

        Status = ReservationStatus.Cancelled;
        CancelledAtUtc = DateTime.SpecifyKind(cancelledAtUtc, DateTimeKind.Utc);
        CancelledByUserId = cancelledByUserId;
        CancelReason = reason.Trim();

        AddEvent(new ReservationCancelledEvent(Id, VehicleId, cancelledByUserId, CancelledAtUtc.Value));
        Touch();
    }

    public void Complete(Guid completedByUserId, DateTime completedAtUtc)
    {
        if (Status is not ReservationStatus.Active)
        {
            throw new DomainException("Only active reservations can be completed.");
        }

        Status = ReservationStatus.Completed;
        AddEvent(new ReservationCompletedEvent(Id, VehicleId, completedByUserId, DateTime.SpecifyKind(completedAtUtc, DateTimeKind.Utc)));
        Touch();
    }

    public void Extend(Guid extendedByUserId, DateTime newExpiresAtUtc, DateTime extendedAtUtc)
    {
        if (Status is not ReservationStatus.Active)
        {
            throw new DomainException("Only active reservations can be extended.");
        }

        if (Type is ReservationType.PaidDeposit)
        {
            throw new DomainException("Paid-deposit reservations do not have automatic expiration.");
        }

        var newExpiresAt = DateTime.SpecifyKind(newExpiresAtUtc, DateTimeKind.Utc);

        if (newExpiresAt <= CreatedAtUtc)
        {
            throw new DomainException("New expiration must be after reservation creation.");
        }

        PreviousExpiresAtUtc = ExpiresAtUtc;
        ExpiresAtUtc = newExpiresAt;
        ExtendedAtUtc = DateTime.SpecifyKind(extendedAtUtc, DateTimeKind.Utc);
        ExtendedByUserId = extendedByUserId;

        AddEvent(new ReservationExtendedEvent(Id, VehicleId, extendedByUserId, ExpiresAtUtc.Value));
        Touch();
    }

    public void Expire(DateTime expiredAtUtc)
    {
        if (Status is not ReservationStatus.Active)
        {
            return;
        }

        if (!ExpiresAtUtc.HasValue)
        {
            throw new DomainException("Reservation has no expiration to expire.");
        }

        var expiredAt = DateTime.SpecifyKind(expiredAtUtc, DateTimeKind.Utc);
        if (expiredAt < ExpiresAtUtc.Value)
        {
            throw new DomainException("Reservation cannot be expired before its expiration time.");
        }

        Status = ReservationStatus.Expired;
        AddEvent(new ReservationExpiredEvent(Id, VehicleId, expiredAt));
        Touch();
    }

    public bool IsExpiredAt(DateTime nowUtc)
    {
        if (Status is not ReservationStatus.Active)
        {
            return false;
        }

        if (!ExpiresAtUtc.HasValue)
        {
            return false;
        }

        return DateTime.SpecifyKind(nowUtc, DateTimeKind.Utc) >= ExpiresAtUtc.Value;
    }

    private static DateTime? DetermineExpiresAtUtc(ReservationType type, DateTime createdAtUtc, DateTime? bankDeadlineAtUtc)
    {
        return type switch
        {
            ReservationType.Standard => createdAtUtc.AddHours(48),
            ReservationType.PaidDeposit => null,
            ReservationType.WaitingBank => bankDeadlineAtUtc.HasValue
                ? DateTime.SpecifyKind(bankDeadlineAtUtc.Value, DateTimeKind.Utc)
                : throw new DomainException("BankDeadlineAt is required for WaitingBank reservation."),
            _ => throw new DomainException("Invalid reservation type.")
        };
    }
}
