namespace GestAuto.Commercial.Domain.Events;

public record UsedVehicleEvaluationRequestedEvent(
    Guid EvaluationId,
    Guid ProposalId,
    string Brand,
    string Model,
    int Year,
    int Mileage,
    string LicensePlate) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
}