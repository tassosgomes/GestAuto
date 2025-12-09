using GestAuto.Commercial.Domain.ValueObjects;

namespace GestAuto.Commercial.Domain.Entities;

public class ProposalItem
{
    public Guid Id { get; private set; }
    public string Description { get; private set; } = null!;
    public Money Price { get; private set; } = null!;
    public bool IsOptional { get; private set; }

    private ProposalItem() { } // EF Core

    public static ProposalItem Create(string description, Money price, bool isOptional = false)
    {
        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Description cannot be empty", nameof(description));

        return new ProposalItem
        {
            Id = Guid.NewGuid(),
            Description = description,
            Price = price,
            IsOptional = isOptional
        };
    }
}