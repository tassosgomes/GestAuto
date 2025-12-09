using GestAuto.Commercial.Domain.Entities;
using GestAuto.Commercial.Domain.Enums;
using GestAuto.Commercial.Domain.ValueObjects;

namespace GestAuto.Commercial.Domain.Services;

public class LeadScoringService
{
    public LeadScore Calculate(Lead lead)
    {
        if (lead.Qualification == null)
            return LeadScore.Bronze;

        var hasTradeIn = lead.Qualification.HasTradeInVehicle;
        var isFinancing = lead.Qualification.PaymentMethod == PaymentMethod.Financiamento;
        var daysUntilPurchase = GetDaysUntilPurchase(lead.Qualification.ExpectedPurchaseDate);

        // Regra base
        var score = CalculateBaseScore(isFinancing, hasTradeIn, daysUntilPurchase);

        // Bonificações
        score = ApplyBonuses(score, lead);

        return score;
    }

    private LeadScore CalculateBaseScore(bool isFinancing, bool hasTradeIn, int daysUntilPurchase)
    {
        // Diamante: Financiado + Usado + Compra < 15 dias
        if (isFinancing && hasTradeIn && daysUntilPurchase < 15)
            return LeadScore.Diamond;

        // Ouro: (À Vista + Usado) OU (Financiado) + Compra < 15 dias
        if ((hasTradeIn || isFinancing) && daysUntilPurchase < 15)
            return LeadScore.Gold;

        // Prata: À Vista puro (sem usado, sem financiamento)
        if (!isFinancing && !hasTradeIn)
            return LeadScore.Silver;

        // Bronze: Compra > 30 dias
        if (daysUntilPurchase > 30)
            return LeadScore.Bronze;

        return LeadScore.Silver;
    }

    private LeadScore ApplyBonuses(LeadScore baseScore, Lead lead)
    {
        var score = baseScore;

        // Origem Showroom ou Telefone: +1 nível
        if (lead.Source == LeadSource.Showroom || lead.Source == LeadSource.Telefone)
            score = PromoteScore(score);

        // Usado com baixa km e revisões na marca: +1 nível
        if (HasHighQualityTradeIn(lead.Qualification?.TradeInVehicle))
            score = PromoteScore(score);

        return score;
    }

    private LeadScore PromoteScore(LeadScore current) => current switch
    {
        LeadScore.Bronze => LeadScore.Silver,
        LeadScore.Silver => LeadScore.Gold,
        LeadScore.Gold => LeadScore.Diamond,
        LeadScore.Diamond => LeadScore.Diamond,
        _ => current
    };

    private int GetDaysUntilPurchase(DateTime expectedPurchaseDate)
    {
        return (int)(expectedPurchaseDate - DateTime.UtcNow).TotalDays;
    }

    private bool HasHighQualityTradeIn(TradeInVehicle? tradeInVehicle)
    {
        if (tradeInVehicle == null) return false;

        // Considera alta qualidade: baixa km (< 50k), condição excelente, histórico de revisões
        return tradeInVehicle.Mileage < 50000 &&
               tradeInVehicle.Condition.ToLower().Contains("excelente") &&
               tradeInVehicle.HasServiceHistory;
    }
}