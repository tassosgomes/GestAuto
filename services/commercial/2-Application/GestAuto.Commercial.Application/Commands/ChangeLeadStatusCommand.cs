using GestAuto.Commercial.Application.Interfaces;

namespace GestAuto.Commercial.Application.Commands;

public record ChangeLeadStatusCommand(
    Guid LeadId,
    string Status
) : ICommand<DTOs.LeadResponse>;