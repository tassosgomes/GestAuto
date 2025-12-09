using GestAuto.Commercial.Domain.ValueObjects;

namespace GestAuto.Commercial.Domain.Entities;

public class UsedVehicle
{
    public Guid Id { get; private set; }
    public string Brand { get; private set; } = null!;
    public string Model { get; private set; } = null!;
    public int Year { get; private set; }
    public int Mileage { get; private set; }
    public string LicensePlate { get; private set; } = null!;
    public string Condition { get; private set; } = null!;
    public bool HasServiceHistory { get; private set; }
    public Money EstimatedValue { get; private set; } = null!;
    public Guid LeadId { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private UsedVehicle() { } // EF Core

    public static UsedVehicle Create(
        string brand,
        string model,
        int year,
        int mileage,
        string licensePlate,
        string condition,
        bool hasServiceHistory,
        Money estimatedValue,
        Guid leadId)
    {
        if (string.IsNullOrWhiteSpace(brand))
            throw new ArgumentException("Brand cannot be empty", nameof(brand));

        if (string.IsNullOrWhiteSpace(model))
            throw new ArgumentException("Model cannot be empty", nameof(model));

        int currentYear = DateTime.Now.Year;
        if (year < 1900 || year > currentYear + 1)
            throw new ArgumentException("Invalid year", nameof(year));

        if (mileage < 0)
            throw new ArgumentException("Mileage cannot be negative", nameof(mileage));

        if (string.IsNullOrWhiteSpace(licensePlate))
            throw new ArgumentException("License plate cannot be empty", nameof(licensePlate));

        if (estimatedValue.Amount <= 0)
            throw new ArgumentException("Estimated value must be positive", nameof(estimatedValue));

        return new UsedVehicle
        {
            Id = Guid.NewGuid(),
            Brand = brand,
            Model = model,
            Year = year,
            Mileage = mileage,
            LicensePlate = licensePlate,
            Condition = condition,
            HasServiceHistory = hasServiceHistory,
            EstimatedValue = estimatedValue,
            LeadId = leadId,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void UpdateEstimatedValue(Money newValue)
    {
        if (newValue.Amount <= 0)
            throw new ArgumentException("Estimated value must be positive", nameof(newValue));

        EstimatedValue = newValue;
    }
}