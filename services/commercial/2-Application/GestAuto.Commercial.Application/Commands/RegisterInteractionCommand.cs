using GestAuto.Commercial.Application.Interfaces;

namespace GestAuto.Commercial.Application.Commands;

public record RegisterInteractionCommand(
    Guid LeadId,
    string Type,
    string Description,
    DateTime OccurredAt
) : ICommand<DTOs.InteractionResponse>;