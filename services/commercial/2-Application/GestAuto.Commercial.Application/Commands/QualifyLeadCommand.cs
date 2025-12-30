using GestAuto.Commercial.Application.Interfaces;
using GestAuto.Commercial.Domain.Entities;

namespace GestAuto.Commercial.Application.Commands;

public record TradeInVehicleDto(
    string Brand,
    string Model,
    int Year,
    int Mileage,
    string LicensePlate,
    string Color,
    string GeneralCondition,
    bool HasDealershipServiceHistory
);

public record QualifyLeadCommand(
    Guid LeadId,
    bool HasTradeInVehicle,
    TradeInVehicleDto? TradeInVehicle,
    string PaymentMethod,
    decimal? EstimatedMonthlyIncome,
    DateTime? ExpectedPurchaseDate,
    bool InterestedInTestDrive
) : ICommand<DTOs.LeadResponse>;