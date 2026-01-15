using GestAuto.Stock.Domain.Enums;
using GestAuto.Stock.Domain.Exceptions;

namespace GestAuto.Stock.Domain.Entities;

public sealed class AuditEntry : BaseEntity
{
    public Guid VehicleId { get; private set; }
    public DateTime OccurredAtUtc { get; private set; }
    public Guid ResponsibleUserId { get; private set; }

    public VehicleStatus PreviousStatus { get; private set; }
    public VehicleStatus NewStatus { get; private set; }
    public string Reason { get; private set; } = null!;

    private AuditEntry() { }

    public AuditEntry(
        Guid vehicleId,
        DateTime occurredAtUtc,
        Guid responsibleUserId,
        VehicleStatus previousStatus,
        VehicleStatus newStatus,
        string reason)
    {
        if (vehicleId == Guid.Empty)
        {
            throw new DomainException("VehicleId is required.");
        }

        if (responsibleUserId == Guid.Empty)
        {
            throw new DomainException("ResponsibleUserId is required.");
        }

        if (string.IsNullOrWhiteSpace(reason))
        {
            throw new DomainException("Reason is required.");
        }

        VehicleId = vehicleId;
        OccurredAtUtc = DateTime.SpecifyKind(occurredAtUtc, DateTimeKind.Utc);
        ResponsibleUserId = responsibleUserId;
        PreviousStatus = previousStatus;
        NewStatus = newStatus;
        Reason = reason.Trim();
    }
}
