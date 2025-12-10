using GestAuto.Commercial.Domain.ValueObjects;

namespace GestAuto.Commercial.Application.DTOs;

/// <summary>
/// Resposta com informações de uma interação com o lead
/// </summary>
public record InteractionResponse(
    /// <summary>ID único da interação</summary>
    Guid Id,
    /// <summary>Tipo de interação (Call, Email, Visit, Message, Other)</summary>
    string Type,
    /// <summary>Descrição/conteúdo da interação</summary>
    string Description,
    /// <summary>Data e hora quando ocorreu a interação</summary>
    DateTime OccurredAt,
    /// <summary>Data de registro da interação no sistema</summary>
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