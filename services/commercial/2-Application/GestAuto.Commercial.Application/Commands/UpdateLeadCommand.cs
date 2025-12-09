using GestAuto.Commercial.Application.Interfaces;

namespace GestAuto.Commercial.Application.Commands;

public record UpdateLeadCommand(
    Guid LeadId,
    string? Name,
    string? Email,
    string? Phone,
    string? InterestedModel,
    string? InterestedTrim,
    string? InterestedColor
) : ICommand<DTOs.LeadResponse>;