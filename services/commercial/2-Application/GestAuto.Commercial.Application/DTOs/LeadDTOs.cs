using GestAuto.Commercial.Domain.ValueObjects;

namespace GestAuto.Commercial.Application.DTOs;

public record CreateLeadRequest(
    string Name,
    string Email,
    string Phone,
    string Source,
    string? InterestedModel,
    string? InterestedTrim,
    string? InterestedColor
);

public record UpdateLeadRequest(
    string Name,
    string Email,
    string Phone,
    string? InterestedModel,
    string? InterestedTrim,
    string? InterestedColor
);

public record ChangeLeadStatusRequest(string Status);

public record QualifyLeadRequest(
    bool HasTradeInVehicle,
    Commands.TradeInVehicleDto? TradeInVehicle,
    string PaymentMethod,
    DateTime? ExpectedPurchaseDate,
    bool InterestedInTestDrive
);

public record RegisterInteractionRequest(
    string Type,
    string Description
);

public record LeadResponse(
    Guid Id,
    string Name,
    string Email,
    string Phone,
    string Source,
    string Status,
    string Score,
    Guid SalesPersonId,
    string? InterestedModel,
    string? InterestedTrim,
    string? InterestedColor,
    QualificationResponse? Qualification,
    DateTime CreatedAt,
    DateTime UpdatedAt
)
{
    public static LeadResponse FromEntity(Domain.Entities.Lead lead) => new(
        lead.Id,
        lead.Name,
        lead.Email.Value,
        lead.Phone.Formatted,
        lead.Source.ToString(),
        lead.Status.ToString(),
        lead.Score.ToString(),
        lead.SalesPersonId,
        lead.InterestedModel,
        lead.InterestedTrim,
        lead.InterestedColor,
        lead.Qualification != null ? QualificationResponse.FromEntity(lead.Qualification) : null,
        lead.CreatedAt,
        lead.UpdatedAt
    );
}

public record QualificationResponse(
    bool HasTradeInVehicle,
    TradeInVehicleResponse? TradeInVehicle,
    string PaymentMethod,
    DateTime? ExpectedPurchaseDate,
    bool InterestedInTestDrive
)
{
    public static QualificationResponse FromEntity(Domain.ValueObjects.Qualification qualification) => new(
        qualification.HasTradeInVehicle,
        qualification.TradeInVehicle != null 
            ? TradeInVehicleResponse.FromEntity(qualification.TradeInVehicle) 
            : null,
        qualification.PaymentMethod.ToString(),
        qualification.ExpectedPurchaseDate,
        qualification.InterestedInTestDrive
    );
}

public record TradeInVehicleResponse(
    string Brand,
    string Model,
    int Year,
    int Mileage,
    string LicensePlate,
    string Color,
    string GeneralCondition,
    bool HasDealershipServiceHistory
)
{
    public static TradeInVehicleResponse FromEntity(Domain.ValueObjects.TradeInVehicle vehicle) => new(
        vehicle.Brand,
        vehicle.Model,
        vehicle.Year,
        vehicle.Mileage,
        vehicle.LicensePlate,
        vehicle.Color,
        vehicle.GeneralCondition,
        vehicle.HasDealershipServiceHistory
    );
}

public record LeadListItemResponse(
    Guid Id,
    string Name,
    string Phone,
    string Source,
    string Status,
    string Score,
    string? InterestedModel,
    DateTime CreatedAt
)
{
    public static LeadListItemResponse FromEntity(Domain.Entities.Lead lead) => new(
        lead.Id,
        lead.Name,
        lead.Phone.Formatted,
        lead.Source.ToString(),
        lead.Status.ToString(),
        lead.Score.ToString(),
        lead.InterestedModel,
        lead.CreatedAt
    );
}

public record PagedResponse<T>(
    IReadOnlyList<T> Items,
    int Page,
    int PageSize,
    int TotalCount
)
{
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
    public bool HasNextPage => Page < TotalPages;
    public bool HasPreviousPage => Page > 1;
}