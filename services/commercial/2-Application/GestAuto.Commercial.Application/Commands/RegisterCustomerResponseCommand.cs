using GestAuto.Commercial.Application.DTOs;
using GestAuto.Commercial.Application.Interfaces;

namespace GestAuto.Commercial.Application.Commands;

public record RegisterCustomerResponseCommand(
    Guid EvaluationId,
    bool Accepted,
    string? RejectionReason
) : ICommand<EvaluationResponse>;