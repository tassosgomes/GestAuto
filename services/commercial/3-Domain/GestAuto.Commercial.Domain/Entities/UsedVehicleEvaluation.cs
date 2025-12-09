using GestAuto.Commercial.Domain.Entities;
using GestAuto.Commercial.Domain.Events;
using GestAuto.Commercial.Domain.ValueObjects;

namespace GestAuto.Commercial.Domain.Entities;

public class UsedVehicleEvaluation : BaseEntity
{
    public Guid ProposalId { get; private set; }
    public string Brand { get; private set; } = null!;
    public string Model { get; private set; } = null!;
    public int Year { get; private set; }
    public int Mileage { get; private set; }
    public string LicensePlate { get; private set; } = null!;
    public Money MarketValue { get; private set; } = null!;
    public Money TradeInValue { get; private set; } = null!;
    public string EvaluationNotes { get; private set; } = null!;
    public Guid EvaluatedBy { get; private set; }
    public DateTime EvaluatedAt { get; private set; }

    private UsedVehicleEvaluation() { } // EF Core

    public static UsedVehicleEvaluation Create(
        Guid proposalId,
        string brand,
        string model,
        int year,
        int mileage,
        string licensePlate,
        Money marketValue,
        Money tradeInValue,
        string evaluationNotes,
        Guid evaluatedBy)
    {
        if (string.IsNullOrWhiteSpace(brand))
            throw new ArgumentException("Brand cannot be empty", nameof(brand));

        if (string.IsNullOrWhiteSpace(model))
            throw new ArgumentException("Model cannot be empty", nameof(model));

        if (marketValue.Amount <= 0 || tradeInValue.Amount < 0)
            throw new ArgumentException("Values must be non-negative", nameof(marketValue));

        var evaluation = new UsedVehicleEvaluation
        {
            ProposalId = proposalId,
            Brand = brand,
            Model = model,
            Year = year,
            Mileage = mileage,
            LicensePlate = licensePlate,
            MarketValue = marketValue,
            TradeInValue = tradeInValue,
            EvaluationNotes = evaluationNotes,
            EvaluatedBy = evaluatedBy,
            EvaluatedAt = DateTime.UtcNow
        };

        evaluation.AddEvent(new UsedVehicleEvaluationRequestedEvent(
            evaluation.Id,
            proposalId,
            brand,
            model,
            year,
            mileage,
            licensePlate));

        return evaluation;
    }

    public void UpdateValues(Money marketValue, Money tradeInValue, string notes)
    {
        MarketValue = marketValue;
        TradeInValue = tradeInValue;
        EvaluationNotes = notes;
        UpdatedAt = DateTime.UtcNow;
    }
}