using GestAuto.Commercial.Application.Interfaces;

namespace GestAuto.Commercial.Application.Commands;

public record UpdateProposalCommand(
    Guid ProposalId,
    string? VehicleModel,
    string? VehicleTrim,
    string? VehicleColor,
    int? VehicleYear,
    bool? IsReadyDelivery,
    decimal? VehiclePrice,
    string? PaymentMethod,
    decimal? DownPayment,
    int? Installments
) : ICommand<DTOs.ProposalResponse>;
