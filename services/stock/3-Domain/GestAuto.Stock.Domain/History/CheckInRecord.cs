using GestAuto.Stock.Domain.Entities;
using GestAuto.Stock.Domain.Enums;

namespace GestAuto.Stock.Domain.History;

public sealed class CheckInRecord : BaseEntity
{
    public Guid VehicleId { get; private set; }
    public CheckInSource Source { get; private set; }
    public DateTime OccurredAt { get; private set; }
    public Guid ResponsibleUserId { get; private set; }
    public string? Notes { get; private set; }

    private CheckInRecord() { }

    public CheckInRecord(Guid vehicleId, CheckInSource source, DateTime occurredAt, Guid responsibleUserId, string? notes)
    {
        VehicleId = vehicleId;
        Source = source;
        OccurredAt = occurredAt;
        ResponsibleUserId = responsibleUserId;
        Notes = notes;
    }
}
