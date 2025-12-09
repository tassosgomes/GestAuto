using GestAuto.Commercial.Domain.Entities;
using GestAuto.Commercial.Domain.Events;
using GestAuto.Commercial.Domain.ValueObjects;
using GestAuto.Commercial.Domain.Enums;

namespace GestAuto.Commercial.Domain.Entities;

public class UsedVehicle
{
    public string Brand { get; init; } = null!;
    public string Model { get; init; } = null!;
    public int Year { get; init; }
    public int Mileage { get; init; }
    public LicensePlate LicensePlate { get; init; } = null!;
    public string Color { get; init; } = null!;
    public string GeneralCondition { get; init; } = null!;
    public bool HasDealershipServiceHistory { get; init; }

    public static UsedVehicle Create(
        string brand,
        string model,
        int year,
        int mileage,
        LicensePlate licensePlate,
        string color,
        string generalCondition,
        bool hasDealershipServiceHistory)
    {
        if (string.IsNullOrWhiteSpace(brand))
            throw new ArgumentException("Brand cannot be empty", nameof(brand));

        if (string.IsNullOrWhiteSpace(model))
            throw new ArgumentException("Model cannot be empty", nameof(model));

        if (year < 1900 || year > DateTime.Now.Year + 1)
            throw new ArgumentException("Invalid year", nameof(year));

        if (mileage < 0)
            throw new ArgumentException("Mileage cannot be negative", nameof(mileage));

        return new UsedVehicle
        {
            Brand = brand,
            Model = model,
            Year = year,
            Mileage = mileage,
            LicensePlate = licensePlate,
            Color = color,
            GeneralCondition = generalCondition,
            HasDealershipServiceHistory = hasDealershipServiceHistory
        };
    }
}

public class UsedVehicleEvaluation : BaseEntity
{
    public Guid ProposalId { get; private set; }
    public UsedVehicle Vehicle { get; private set; } = null!;
    public EvaluationStatus Status { get; private set; }
    public Money? EvaluatedValue { get; private set; }
    public string? EvaluationNotes { get; private set; }
    public DateTime RequestedAt { get; private set; }
    public DateTime? RespondedAt { get; private set; }
    public bool? CustomerAccepted { get; private set; }
    public string? CustomerRejectionReason { get; private set; }
    public Guid RequestedBy { get; private set; }

    private UsedVehicleEvaluation() { } // EF Core

    public static UsedVehicleEvaluation Request(
        Guid proposalId,
        UsedVehicle vehicle,
        Guid requestedBy)
    {
        var evaluation = new UsedVehicleEvaluation
        {
            ProposalId = proposalId,
            Vehicle = vehicle,
            Status = EvaluationStatus.Requested,
            RequestedAt = DateTime.UtcNow,
            RequestedBy = requestedBy
        };

        evaluation.AddEvent(new UsedVehicleEvaluationRequestedEvent(
            evaluation.Id,
            proposalId,
            vehicle.Brand,
            vehicle.Model,
            vehicle.Year,
            vehicle.Mileage,
            vehicle.LicensePlate.Value));

        return evaluation;
    }

    public void MarkAsCompleted(Money evaluatedValue, string? notes = null)
    {
        if (Status != EvaluationStatus.Requested)
            throw new InvalidOperationException("Only requested evaluations can be completed");

        Status = EvaluationStatus.Completed;
        EvaluatedValue = evaluatedValue;
        EvaluationNotes = notes;
        RespondedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void CustomerAccept()
    {
        if (Status != EvaluationStatus.Completed)
            throw new InvalidOperationException("Can only accept completed evaluations");

        Status = EvaluationStatus.Accepted;
        CustomerAccepted = true;
        UpdatedAt = DateTime.UtcNow;
    }

    public void CustomerReject(string? reason = null)
    {
        if (Status != EvaluationStatus.Completed)
            throw new InvalidOperationException("Can only reject completed evaluations");

        Status = EvaluationStatus.Rejected;
        CustomerAccepted = false;
        CustomerRejectionReason = reason;
        UpdatedAt = DateTime.UtcNow;
    }
}