using GestAuto.Commercial.Domain.Entities;
using GestAuto.Commercial.Domain.Enums;
using GestAuto.Commercial.Domain.Events;
using GestAuto.Commercial.Domain.Exceptions;
using GestAuto.Commercial.Domain.ValueObjects;

namespace GestAuto.Commercial.Domain.Entities;

public class Proposal : BaseEntity
{
    public Guid LeadId { get; private set; }
    public ProposalStatus Status { get; private set; }

    // Veículo
    public string VehicleModel { get; private set; } = null!;
    public string VehicleTrim { get; private set; } = null!;
    public string VehicleColor { get; private set; } = null!;
    public int VehicleYear { get; private set; }
    public bool IsReadyDelivery { get; private set; }

    // Valores
    public Money VehiclePrice { get; private set; } = null!;
    public Money DiscountAmount { get; private set; } = null!;
    public string? DiscountReason { get; private set; }
    public Guid? DiscountApproverId { get; private set; }
    public Money TradeInValue { get; private set; } = null!;

    // Pagamento
    public PaymentMethod PaymentMethod { get; private set; }
    public Money? DownPayment { get; private set; }
    public int? Installments { get; private set; }

    // Itens extras
    public List<ProposalItem> Items { get; private set; } = new();

    // Avaliação de seminovo
    public Guid? UsedVehicleEvaluationId { get; private set; }

    public Money TotalValue => CalculateTotalValue();

    private Proposal() { } // EF Core

    public static Proposal Create(
        Guid leadId,
        string vehicleModel,
        string vehicleTrim,
        string vehicleColor,
        int vehicleYear,
        bool isReadyDelivery,
        Money vehiclePrice,
        Money tradeInValue,
        PaymentMethod paymentMethod,
        Money? downPayment = null,
        int? installments = null)
    {
        if (string.IsNullOrWhiteSpace(vehicleModel))
            throw new ArgumentException("Vehicle model cannot be empty", nameof(vehicleModel));

        if (vehiclePrice.Amount <= 0)
            throw new ArgumentException("Vehicle price must be positive", nameof(vehiclePrice));

        var proposal = new Proposal
        {
            LeadId = leadId,
            Status = ProposalStatus.AwaitingCustomer,
            VehicleModel = vehicleModel,
            VehicleTrim = vehicleTrim,
            VehicleColor = vehicleColor,
            VehicleYear = vehicleYear,
            IsReadyDelivery = isReadyDelivery,
            VehiclePrice = vehiclePrice,
            DiscountAmount = Money.Zero,
            TradeInValue = tradeInValue,
            PaymentMethod = paymentMethod,
            DownPayment = downPayment,
            Installments = installments
        };

        proposal.AddEvent(new ProposalCreatedEvent(proposal.Id, leadId));
        return proposal;
    }

    public void SetAwaitingEvaluation(Guid evaluationId)
    {
        Status = ProposalStatus.AwaitingUsedVehicleEvaluation;
        UsedVehicleEvaluationId = evaluationId;
        UpdatedAt = DateTime.UtcNow;
        AddEvent(new ProposalUpdatedEvent(Id, "Avaliação de seminovo solicitada"));
    }

    public void ApplyEvaluationResult(Money evaluatedValue)
    {
        TradeInValue = evaluatedValue;
        Status = ProposalStatus.AwaitingCustomer;
        UpdatedAt = DateTime.UtcNow;
        AddEvent(new ProposalUpdatedEvent(Id, "Avaliação de seminovo concluída"));
    }

    public void SetTradeInValue(Money tradeInValue)
    {
        TradeInValue = tradeInValue;
        UpdatedAt = DateTime.UtcNow;
        AddEvent(new ProposalUpdatedEvent(Id, "Valor de seminovo confirmado"));
    }

    public void ApplyDiscount(Money amount, string reason, Guid salesPersonId)
    {
        var discountPercentage = amount.Amount / VehiclePrice.Amount * 100;

        DiscountAmount = amount;
        DiscountReason = reason;

        if (discountPercentage > 5)
        {
            Status = ProposalStatus.AwaitingDiscountApproval;
            // DiscountApproverId permanece null até aprovação
        }

        UpdatedAt = DateTime.UtcNow;
        AddEvent(new ProposalUpdatedEvent(Id, "Desconto aplicado"));
    }

    public void ApproveDiscount(Guid managerId)
    {
        if (Status != ProposalStatus.AwaitingDiscountApproval)
            throw new DomainException("Proposta não está aguardando aprovação de desconto");

        DiscountApproverId = managerId;
        Status = ProposalStatus.AwaitingCustomer;
        UpdatedAt = DateTime.UtcNow;

        AddEvent(new ProposalUpdatedEvent(Id, "Desconto aprovado"));
    }

    public void Close(Guid salesPersonId)
    {
        ValidateCanClose();

        Status = ProposalStatus.Closed;
        UpdatedAt = DateTime.UtcNow;

        AddEvent(new SaleClosedEvent(Id, LeadId, TotalValue));
    }

    public void UpdateVehicleInfo(string vehicleModel, string vehicleTrim, string vehicleColor, int vehicleYear, bool isReadyDelivery)
    {
        VehicleModel = vehicleModel;
        VehicleTrim = vehicleTrim;
        VehicleColor = vehicleColor;
        VehicleYear = vehicleYear;
        IsReadyDelivery = isReadyDelivery;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdatePaymentInfo(Money vehiclePrice, PaymentMethod paymentMethod, Money? downPayment, int? installments)
    {
        VehiclePrice = vehiclePrice;
        PaymentMethod = paymentMethod;
        DownPayment = downPayment;
        Installments = installments;
        UpdatedAt = DateTime.UtcNow;
    }

    public void AddItem(ProposalItem item)
    {
        Items.Add(item);
        UpdatedAt = DateTime.UtcNow;
        AddEvent(new ProposalUpdatedEvent(Id, $"Item adicionado: {item.Description}"));
    }

    public void RemoveItem(Guid itemId)
    {
        var item = Items.FirstOrDefault(i => i.Id == itemId);
        if (item != null)
        {
            Items.Remove(item);
            UpdatedAt = DateTime.UtcNow;
            AddEvent(new ProposalUpdatedEvent(Id, $"Item removido: {item.Description}"));
        }
    }

    public void SetUsedVehicleEvaluationId(Guid evaluationId)
    {
        UsedVehicleEvaluationId = evaluationId;
        UpdatedAt = DateTime.UtcNow;
    }

    private void ValidateCanClose()
    {
        if (Status == ProposalStatus.AwaitingDiscountApproval)
            throw new DomainException("Não é possível fechar proposta com desconto pendente de aprovação");

        if (DiscountAmount.Amount > VehiclePrice.Amount * 0.1M) // 10% max discount
            throw new DomainException("Desconto excede o limite permitido");
    }

    private Money CalculateTotalValue()
    {
        var itemsTotal = Items.Sum(item => item.Price.Amount);
        var netVehiclePrice = VehiclePrice - DiscountAmount;
        var total = netVehiclePrice + new Money(itemsTotal) - TradeInValue;

        return total.Amount > 0 ? total : Money.Zero;
    }
}