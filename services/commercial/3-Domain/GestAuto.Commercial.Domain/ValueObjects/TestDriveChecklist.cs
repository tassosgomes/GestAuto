using GestAuto.Commercial.Domain.Enums;

namespace GestAuto.Commercial.Domain.ValueObjects;

public record TestDriveChecklist
{
    public decimal InitialMileage { get; init; }
    public decimal FinalMileage { get; init; }
    public FuelLevel FuelLevel { get; init; }
    public string? VisualObservations { get; init; }

    public TestDriveChecklist(
        decimal initialMileage,
        decimal finalMileage,
        FuelLevel fuelLevel,
        string? visualObservations = null)
    {
        if (initialMileage < 0)
            throw new ArgumentException("Initial mileage cannot be negative", nameof(initialMileage));

        if (finalMileage < initialMileage)
            throw new ArgumentException("Final mileage cannot be less than initial mileage", nameof(finalMileage));

        InitialMileage = initialMileage;
        FinalMileage = finalMileage;
        FuelLevel = fuelLevel;
        VisualObservations = visualObservations;
    }

    public decimal GetMileageDifference() => FinalMileage - InitialMileage;
}
