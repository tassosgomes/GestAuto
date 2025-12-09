using GestAuto.Commercial.Application.Interfaces;

namespace GestAuto.Commercial.Application.Commands;

public record CreateLeadCommand(
    string Name,
    string Email,
    string Phone,
    string Source,
    Guid SalesPersonId,
    string? InterestedModel,
    string? InterestedTrim,
    string? InterestedColor
) : ICommand<DTOs.LeadResponse>;