using GestAuto.Stock.Domain.Entities;
using GestAuto.Stock.Domain.Enums;

namespace GestAuto.Stock.Domain.History;

public sealed class CheckOutRecord : BaseEntity
{
    public Guid VehicleId { get; private set; }
    public CheckOutReason Reason { get; private set; }
    public DateTime OccurredAt { get; private set; }
    public Guid ResponsibleUserId { get; private set; }
    public string? Notes { get; private set; }

    private CheckOutRecord() { }

    public CheckOutRecord(Guid vehicleId, CheckOutReason reason, DateTime occurredAt, Guid responsibleUserId, string? notes)
    {
        VehicleId = vehicleId;
        Reason = reason;
        OccurredAt = occurredAt;
        ResponsibleUserId = responsibleUserId;
        Notes = notes;
    }
}
