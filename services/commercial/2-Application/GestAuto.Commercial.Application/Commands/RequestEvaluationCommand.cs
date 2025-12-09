using GestAuto.Commercial.Application.DTOs;
using GestAuto.Commercial.Application.Interfaces;

namespace GestAuto.Commercial.Application.Commands;

public record RequestEvaluationCommand(
    Guid ProposalId,
    string Brand,
    string Model,
    int Year,
    int Mileage,
    string LicensePlate,
    string Color,
    string GeneralCondition,
    bool HasDealershipServiceHistory,
    Guid RequestedByUserId
) : ICommand<EvaluationResponse>;