using GestAuto.Commercial.Domain.Entities;

namespace GestAuto.Commercial.Application.DTOs;

public record RequestEvaluationRequest(
    Guid ProposalId,
    string Brand,
    string Model,
    int Year,
    int Mileage,
    string LicensePlate,
    string Color,
    string GeneralCondition,
    bool HasDealershipServiceHistory
);

public record CustomerResponseRequest(
    bool Accepted,
    string? RejectionReason
);

public record EvaluationResponse(
    Guid Id,
    Guid ProposalId,
    string Status,
    UsedVehicleResponse Vehicle,
    decimal? EvaluatedValue,
    string? EvaluationNotes,
    DateTime RequestedAt,
    DateTime? RespondedAt,
    bool? CustomerAccepted,
    string? CustomerRejectionReason
)
{
    public static EvaluationResponse FromEntity(UsedVehicleEvaluation evaluation) => new(
        evaluation.Id,
        evaluation.ProposalId,
        evaluation.Status.ToString(),
        UsedVehicleResponse.FromEntity(evaluation.Vehicle),
        evaluation.EvaluatedValue?.Amount,
        evaluation.EvaluationNotes,
        evaluation.RequestedAt,
        evaluation.RespondedAt,
        evaluation.CustomerAccepted,
        evaluation.CustomerRejectionReason
    );
}

public record EvaluationListItemResponse(
    Guid Id,
    Guid ProposalId,
    string Status,
    string VehicleInfo,
    decimal? EvaluatedValue,
    DateTime RequestedAt
)
{
    public static EvaluationListItemResponse FromEntity(UsedVehicleEvaluation evaluation) => new(
        evaluation.Id,
        evaluation.ProposalId,
        evaluation.Status.ToString(),
        $"{evaluation.Vehicle.Brand} {evaluation.Vehicle.Model} {evaluation.Vehicle.Year}",
        evaluation.EvaluatedValue?.Amount,
        evaluation.RequestedAt
    );
}

public record UsedVehicleResponse(
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
    public static UsedVehicleResponse FromEntity(UsedVehicle vehicle) => new(
        vehicle.Brand,
        vehicle.Model,
        vehicle.Year,
        vehicle.Mileage,
        vehicle.LicensePlate.Formatted,
        vehicle.Color,
        vehicle.GeneralCondition,
        vehicle.HasDealershipServiceHistory
    );
}