using GestAuto.Commercial.Domain.Enums;

namespace GestAuto.Commercial.Domain.ValueObjects;

public record Qualification
{
    public bool HasTradeInVehicle { get; init; }
    public TradeInVehicle? TradeInVehicle { get; init; }
    public PaymentMethod PaymentMethod { get; init; }
    public DateTime ExpectedPurchaseDate { get; init; }
    public string? Notes { get; init; }

    public Qualification(
        bool hasTradeInVehicle,
        TradeInVehicle? tradeInVehicle,
        PaymentMethod paymentMethod,
        DateTime expectedPurchaseDate,
        string? notes = null)
    {
        if (hasTradeInVehicle && tradeInVehicle == null)
            throw new ArgumentException("TradeInVehicle must be provided when HasTradeInVehicle is true");

        if (!hasTradeInVehicle && tradeInVehicle != null)
            throw new ArgumentException("TradeInVehicle should not be provided when HasTradeInVehicle is false");

        HasTradeInVehicle = hasTradeInVehicle;
        TradeInVehicle = tradeInVehicle;
        PaymentMethod = paymentMethod;
        ExpectedPurchaseDate = expectedPurchaseDate;
        Notes = notes;
    }
}

public record TradeInVehicle
{
    public string Brand { get; init; }
    public string Model { get; init; }
    public int Year { get; init; }
    public int Mileage { get; init; }
    public string LicensePlate { get; init; }
    public string Condition { get; init; } // e.g., "Excelente", "Bom", "Regular"
    public bool HasServiceHistory { get; init; }

    public TradeInVehicle(
        string brand,
        string model,
        int year,
        int mileage,
        string licensePlate,
        string condition,
        bool hasServiceHistory)
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

        Brand = brand;
        Model = model;
        Year = year;
        Mileage = mileage;
        LicensePlate = licensePlate;
        Condition = condition;
        HasServiceHistory = hasServiceHistory;
    }
}