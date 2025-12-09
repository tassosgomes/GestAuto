using GestAuto.Commercial.Domain.ValueObjects;

namespace GestAuto.Commercial.Application.DTOs;

public record InteractionResponse(
    Guid Id,
    string Type,
    string Description,
    DateTime OccurredAt,
    DateTime CreatedAt
)
{
    public static InteractionResponse FromEntity(Domain.Entities.Interaction interaction) => new(
        interaction.Id,
        interaction.Type,
        interaction.Description,
        interaction.InteractionDate,
        interaction.CreatedAt
    );
}