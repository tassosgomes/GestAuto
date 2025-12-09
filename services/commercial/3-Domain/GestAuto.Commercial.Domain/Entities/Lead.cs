using GestAuto.Commercial.Domain.Entities;
using GestAuto.Commercial.Domain.Enums;
using GestAuto.Commercial.Domain.Events;
using GestAuto.Commercial.Domain.Services;
using GestAuto.Commercial.Domain.ValueObjects;

namespace GestAuto.Commercial.Domain.Entities;

public class Lead : BaseEntity
{
    public string Name { get; private set; } = null!;
    public Email Email { get; private set; } = null!;
    public Phone Phone { get; private set; } = null!;
    public LeadSource Source { get; private set; }
    public LeadStatus Status { get; private set; }
    public LeadScore Score { get; private set; }
    public Guid SalesPersonId { get; private set; }
    public Qualification? Qualification { get; private set; }
    public List<Interaction> Interactions { get; private set; } = new();

    // Campos opcionais de interesse
    public string? InterestedModel { get; private set; }
    public string? InterestedTrim { get; private set; }
    public string? InterestedColor { get; private set; }

    private Lead() { } // EF Core

    public static Lead Create(
        string name,
        Email email,
        Phone phone,
        LeadSource source,
        Guid salesPersonId,
        string? interestedModel = null,
        string? interestedTrim = null,
        string? interestedColor = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name cannot be empty", nameof(name));

        var lead = new Lead
        {
            Name = name,
            Email = email,
            Phone = phone,
            Source = source,
            Status = LeadStatus.New,
            Score = LeadScore.Bronze, // Score inicial
            SalesPersonId = salesPersonId,
            InterestedModel = interestedModel,
            InterestedTrim = interestedTrim,
            InterestedColor = interestedColor
        };

        lead.AddEvent(new LeadCreatedEvent(lead.Id, name, source));
        return lead;
    }

    public void Qualify(Qualification qualification, LeadScoringService scoringService)
    {
        Qualification = qualification;
        Score = scoringService.Calculate(this);
        UpdatedAt = DateTime.UtcNow;
        Status = LeadStatus.InNegotiation;

        AddEvent(new LeadScoredEvent(Id, Score));
    }

    public void ChangeStatus(LeadStatus newStatus)
    {
        Status = newStatus;
        UpdatedAt = DateTime.UtcNow;
        AddEvent(new LeadStatusChangedEvent(Id, newStatus));
    }

    public void AddInteraction(Interaction interaction)
    {
        Interactions.Add(interaction);
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateInterest(string? model, string? trim, string? color)
    {
        InterestedModel = model;
        InterestedTrim = trim;
        InterestedColor = color;
        UpdatedAt = DateTime.UtcNow;
    }
}