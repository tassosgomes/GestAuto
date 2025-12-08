using FluentAssertions;
using GestAuto.Commercial.Domain.Entities;
using GestAuto.Commercial.Domain.Enums;
using GestAuto.Commercial.Domain.Services;
using GestAuto.Commercial.Domain.ValueObjects;

namespace GestAuto.Commercial.UnitTest.Domain.Services;

public class LeadScoringServiceTests
{
    private readonly LeadScoringService _scoringService = new();

    [Fact]
    public void Calculate_LeadWithoutQualification_ShouldReturnBronze()
    {
        // Arrange
        var lead = Lead.Create("João", new Email("joao@example.com"), new Phone("11999999999"), LeadSource.Showroom, Guid.NewGuid());

        // Act
        var score = _scoringService.Calculate(lead);

        // Assert
        score.Should().Be(LeadScore.Bronze);
    }

    [Theory]
    [InlineData(true, true, 10, LeadScore.Diamond)] // Financiado + Usado + <15 dias
    [InlineData(true, false, 10, LeadScore.Diamond)] // Financiado + <15 dias + bonus
    [InlineData(false, true, 10, LeadScore.Diamond)] // À Vista + Usado + <15 dias + bonus
    [InlineData(false, false, 10, LeadScore.Gold)] // À Vista puro + bonus
    [InlineData(false, false, 35, LeadScore.Gold)] // Compra >30 dias + bonus
    public void Calculate_VariousScenarios_ShouldReturnCorrectScore(
        bool isFinancing, bool hasTradeIn, int daysUntilPurchase, LeadScore expectedScore)
    {
        // Arrange
        var lead = Lead.Create("João", new Email("joao@example.com"), new Phone("11999999999"), LeadSource.Showroom, Guid.NewGuid());
        var qualification = new Qualification(
            hasTradeInVehicle: hasTradeIn,
            tradeInVehicle: hasTradeIn ? new TradeInVehicle("Toyota", "Corolla", 2020, 30000, "ABC1234", "Excelente", true) : null,
            isFinancing ? PaymentMethod.Financiamento : PaymentMethod.AVista,
            DateTime.UtcNow.AddDays(daysUntilPurchase));
        lead.Qualify(qualification, _scoringService);

        // Act
        var score = _scoringService.Calculate(lead);

        // Assert
        score.Should().Be(expectedScore);
    }

    [Fact]
    public void Calculate_ShowroomSource_ShouldPromoteScore()
    {
        // Arrange
        var lead = Lead.Create("João", new Email("joao@example.com"), new Phone("11999999999"), LeadSource.Showroom, Guid.NewGuid());
        var qualification = new Qualification(
            hasTradeInVehicle: false,
            tradeInVehicle: null,
            PaymentMethod.AVista,
            DateTime.UtcNow.AddDays(20)); // Would be Bronze, but Showroom promotes to Silver
        lead.Qualify(qualification, _scoringService);

        // Act
        var score = _scoringService.Calculate(lead);

        // Assert
        score.Should().Be(LeadScore.Gold);
    }

    [Fact]
    public void Calculate_HighQualityTradeIn_ShouldPromoteScore()
    {
        // Arrange
        var lead = Lead.Create("João", new Email("joao@example.com"), new Phone("11999999999"), LeadSource.Instagram, Guid.NewGuid());
        var qualification = new Qualification(
            hasTradeInVehicle: true,
            tradeInVehicle: new TradeInVehicle("Toyota", "Corolla", 2020, 20000, "ABC1234", "Excelente", true),
            PaymentMethod.AVista,
            DateTime.UtcNow.AddDays(20)); // Would be Gold, but high quality trade-in promotes to Diamond
        lead.Qualify(qualification, _scoringService);

        // Act
        var score = _scoringService.Calculate(lead);

        // Assert
        score.Should().Be(LeadScore.Gold);
    }
}