using GestAuto.Commercial.Domain.Entities;

namespace GestAuto.Commercial.Domain.Entities;

public class Interaction : BaseEntity
{
    public Guid LeadId { get; private set; }
    public string Type { get; private set; } = null!; // e.g., "Call", "Email", "Meeting"
    public string Description { get; private set; } = null!;
    public DateTime InteractionDate { get; private set; }
    public string? Result { get; private set; } // e.g., "Interested", "Not interested", "Callback requested"

    private Interaction() { } // EF Core

    public static Interaction Create(
        Guid leadId,
        string type,
        string description,
        DateTime interactionDate,
        string? result = null)
    {
        if (string.IsNullOrWhiteSpace(type))
            throw new ArgumentException("Type cannot be empty", nameof(type));

        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Description cannot be empty", nameof(description));

        return new Interaction
        {
            LeadId = leadId,
            Type = type,
            Description = description,
            InteractionDate = interactionDate,
            Result = result
        };
    }
}