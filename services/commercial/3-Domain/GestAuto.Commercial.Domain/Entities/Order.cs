using GestAuto.Commercial.Domain.Entities;
using GestAuto.Commercial.Domain.ValueObjects;
using GestAuto.Commercial.Domain.Enums;

namespace GestAuto.Commercial.Domain.Entities;

public class Order : BaseEntity
{
    public Guid? ExternalId { get; private set; } // ID do módulo financeiro
    public Guid ProposalId { get; private set; }
    public Guid LeadId { get; private set; }
    public string OrderNumber { get; private set; } = null!;
    public Money TotalValue { get; private set; } = null!;
    public OrderStatus Status { get; private set; }
    public DateTime? DeliveryDate { get; private set; }
    public DateTime? EstimatedDeliveryDate { get; private set; }
    public string? Notes { get; private set; }
    public Guid CreatedBy { get; private set; }

    private Order() { } // EF Core

    public static Order Create(
        Guid proposalId,
        Guid leadId,
        Money totalValue,
        Guid createdBy,
        DateTime? deliveryDate = null,
        string? notes = null)
    {
        if (totalValue.Amount <= 0)
            throw new ArgumentException("Total value must be positive", nameof(totalValue));

        var orderNumber = GenerateOrderNumber();

        return new Order
        {
            ProposalId = proposalId,
            LeadId = leadId,
            OrderNumber = orderNumber,
            TotalValue = totalValue,
            Status = OrderStatus.AwaitingDocumentation,
            DeliveryDate = deliveryDate,
            Notes = notes,
            CreatedBy = createdBy
        };
    }

    public static Order Create(
        Guid externalId,
        Guid proposalId,
        OrderStatus status)
    {
        return new Order
        {
            ExternalId = externalId,
            ProposalId = proposalId,
            LeadId = Guid.Empty, // Will be set when we have proposal data
            OrderNumber = GenerateOrderNumber(),
            TotalValue = new Money(0), // Will be updated from external system
            Status = status,
            CreatedBy = Guid.Empty // External creation
        };
    }

    public void UpdateStatus(OrderStatus newStatus, DateTime? estimatedDeliveryDate = null)
    {
        Status = newStatus;
        if (estimatedDeliveryDate.HasValue)
        {
            EstimatedDeliveryDate = estimatedDeliveryDate;
        }
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateDeliveryDate(DateTime deliveryDate)
    {
        DeliveryDate = deliveryDate;
        UpdatedAt = DateTime.UtcNow;
    }

    public void AddNotes(string notes)
    {
        Notes = string.IsNullOrEmpty(Notes) ? notes : $"{Notes}\n{notes}";
        UpdatedAt = DateTime.UtcNow;
    }

    private static string GenerateOrderNumber()
    {
        // Simple order number generation: ORD + timestamp
        return $"ORD{DateTime.UtcNow:yyyyMMddHHmmss}";
    }
}