using GestAuto.Commercial.Domain.ValueObjects;

namespace GestAuto.Commercial.Application.DTOs;

public record CreateProposalRequest(
    Guid LeadId,
    string VehicleModel,
    string VehicleTrim,
    string VehicleColor,
    int VehicleYear,
    bool IsReadyDelivery,
    decimal VehiclePrice,
    string PaymentMethod,
    decimal? DownPayment,
    int? Installments
);

public record UpdateProposalRequest(
    string? VehicleModel,
    string? VehicleTrim,
    string? VehicleColor,
    int? VehicleYear,
    bool? IsReadyDelivery,
    decimal? VehiclePrice,
    string? PaymentMethod,
    decimal? DownPayment,
    int? Installments
);

public record ApplyDiscountRequest(
    decimal Amount,
    string Reason
);

public record ApproveDiscountRequest(
    Guid ManagerId
);

public record CloseProposalRequest(
    Guid SalesPersonId
);

public record AddProposalItemRequest(
    string Description,
    decimal Value
);

public record ProposalItemResponse(
    Guid Id,
    string Description,
    decimal Value
)
{
    public static ProposalItemResponse FromEntity(Domain.Entities.ProposalItem item) => new(
        item.Id,
        item.Description,
        item.Price.Amount
    );
}

public record ProposalResponse(
    Guid Id,
    Guid LeadId,
    string Status,
    string VehicleModel,
    string VehicleTrim,
    string VehicleColor,
    int VehicleYear,
    bool IsReadyDelivery,
    decimal VehiclePrice,
    decimal DiscountAmount,
    string DiscountReason,
    bool DiscountApproved,
    Guid DiscountApproverId,
    decimal TradeInValue,
    string PaymentMethod,
    decimal DownPayment,
    int Installments,
    List<ProposalItemResponse> Items,
    Guid UsedVehicleEvaluationId,
    decimal TotalValue,
    DateTime CreatedAt,
    DateTime UpdatedAt
)
{
    public static ProposalResponse FromEntity(Domain.Entities.Proposal proposal) => new(
        proposal.Id,
        proposal.LeadId,
        proposal.Status.ToString(),
        proposal.VehicleModel,
        proposal.VehicleTrim,
        proposal.VehicleColor,
        proposal.VehicleYear,
        proposal.IsReadyDelivery,
        proposal.VehiclePrice.Amount,
        proposal.DiscountAmount.Amount,
        proposal.DiscountReason ?? string.Empty,
        proposal.DiscountApproverId.HasValue,
        proposal.DiscountApproverId ?? Guid.Empty,
        proposal.TradeInValue.Amount,
        proposal.PaymentMethod.ToString(),
        proposal.DownPayment?.Amount ?? 0,
        proposal.Installments ?? 0,
        proposal.Items.Select(ProposalItemResponse.FromEntity).ToList(),
        proposal.UsedVehicleEvaluationId ?? Guid.Empty,
        proposal.TotalValue.Amount,
        proposal.CreatedAt,
        proposal.UpdatedAt
    );
}

public record ProposalListItemResponse(
    Guid Id,
    Guid LeadId,
    string Status,
    string VehicleModel,
    decimal TotalValue,
    DateTime CreatedAt
)
{
    public static ProposalListItemResponse FromEntity(Domain.Entities.Proposal proposal) => new(
        proposal.Id,
        proposal.LeadId,
        proposal.Status.ToString(),
        proposal.VehicleModel,
        proposal.TotalValue.Amount,
        proposal.CreatedAt
    );
}
