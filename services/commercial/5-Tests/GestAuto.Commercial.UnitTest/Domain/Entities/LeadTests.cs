using FluentAssertions;
using GestAuto.Commercial.Domain.Entities;
using GestAuto.Commercial.Domain.Enums;
using GestAuto.Commercial.Domain.Events;
using GestAuto.Commercial.Domain.Services;
using GestAuto.Commercial.Domain.ValueObjects;

namespace GestAuto.Commercial.UnitTest.Domain.Entities;

public class LeadTests
{
    [Fact]
    public void Create_ShouldCreateLeadWithCorrectInitialState()
    {
        // Arrange
        var name = "João Silva";
        var email = new Email("joao@example.com");
        var phone = new Phone("11999999999");
        var source = LeadSource.Showroom;
        var salesPersonId = Guid.NewGuid();

        // Act
        var lead = Lead.Create(name, email, phone, source, salesPersonId);

        // Assert
        lead.Name.Should().Be(name);
        lead.Email.Should().Be(email);
        lead.Phone.Should().Be(phone);
        lead.Source.Should().Be(source);
        lead.Status.Should().Be(LeadStatus.Novo);
        lead.Score.Should().Be(LeadScore.Bronze);
        lead.SalesPersonId.Should().Be(salesPersonId);
        lead.Qualification.Should().BeNull();
        lead.Interactions.Should().BeEmpty();
        lead.DomainEvents.Should().ContainSingle(e => e is LeadCreatedEvent);
    }

    [Fact]
    public void Qualify_ShouldUpdateQualificationAndScore()
    {
        // Arrange
        var lead = Lead.Create("João", new Email("joao@example.com"), new Phone("11999999999"), LeadSource.Showroom, Guid.NewGuid());
        var qualification = new Qualification(
            hasTradeInVehicle: true,
            tradeInVehicle: new TradeInVehicle("Toyota", "Corolla", 2020, 30000, "ABC1234", "Excelente", true),
            PaymentMethod.Financiamento,
            DateTime.UtcNow.AddDays(10));
        var scoringService = new LeadScoringService();

        // Act
        lead.Qualify(qualification, scoringService);

        // Assert
        lead.Qualification.Should().Be(qualification);
        lead.Score.Should().Be(LeadScore.Diamond); // Financiado + Usado + Compra < 15 dias
        lead.Status.Should().Be(LeadStatus.EmNegociacao);
        lead.DomainEvents.Should().Contain(e => e is LeadScoredEvent);
    }

    [Fact]
    public void ChangeStatus_ShouldUpdateStatusAndEmitEvent()
    {
        // Arrange
        var lead = Lead.Create("João", new Email("joao@example.com"), new Phone("11999999999"), LeadSource.Showroom, Guid.NewGuid());

        // Act
        lead.ChangeStatus(LeadStatus.Convertido);

        // Assert
        lead.Status.Should().Be(LeadStatus.Convertido);
        lead.DomainEvents.Should().Contain(e => e is LeadStatusChangedEvent);
    }

    [Fact]
    public void AddInteraction_ShouldAddInteractionToList()
    {
        // Arrange
        var lead = Lead.Create("João", new Email("joao@example.com"), new Phone("11999999999"), LeadSource.Showroom, Guid.NewGuid());
        var interaction = Interaction.Create(lead.Id, "Call", "Discutiu financiamento", DateTime.UtcNow);

        // Act
        lead.AddInteraction(interaction);

        // Assert
        lead.Interactions.Should().Contain(interaction);
    }
}