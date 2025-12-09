using GestAuto.Commercial.Domain.Enums;

namespace GestAuto.Commercial.Domain.ValueObjects;

public record Qualification
{
    public bool HasTradeInVehicle { get; private set; }
    public TradeInVehicle? TradeInVehicle { get; private set; }
    public PaymentMethod PaymentMethod { get; private set; }
    public DateTime ExpectedPurchaseDate { get; private set; }
    public bool InterestedInTestDrive { get; private set; }
    public string? Notes { get; private set; }

    private Qualification() { } // EF Core

    public Qualification(
        bool hasTradeInVehicle,
        TradeInVehicle? tradeInVehicle,
        PaymentMethod paymentMethod,
        DateTime expectedPurchaseDate,
        bool interestedInTestDrive,
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
        InterestedInTestDrive = interestedInTestDrive;
        Notes = notes;
    }
}

public record TradeInVehicle
{
    public string Brand { get; private set; }
    public string Model { get; private set; }
    public int Year { get; private set; }
    public int Mileage { get; private set; }
    public string LicensePlate { get; private set; }
    public string Color { get; private set; }
    public string GeneralCondition { get; private set; }
    public bool HasDealershipServiceHistory { get; private set; }

    private TradeInVehicle() { } // EF Core

    public TradeInVehicle(
        string brand,
        string model,
        int year,
        int mileage,
        string licensePlate,
        string color,
        string generalCondition,
        bool hasDealershipServiceHistory)
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
        Color = color;
        GeneralCondition = generalCondition;
        HasDealershipServiceHistory = hasDealershipServiceHistory;
    }
}