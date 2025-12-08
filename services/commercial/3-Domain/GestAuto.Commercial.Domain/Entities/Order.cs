using GestAuto.Commercial.Domain.Entities;
using GestAuto.Commercial.Domain.ValueObjects;

namespace GestAuto.Commercial.Domain.Entities;

public class Order : BaseEntity
{
    public Guid ProposalId { get; private set; }
    public Guid LeadId { get; private set; }
    public string OrderNumber { get; private set; }
    public Money TotalValue { get; private set; }
    public DateTime? DeliveryDate { get; private set; }
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
            DeliveryDate = deliveryDate,
            Notes = notes,
            CreatedBy = createdBy
        };
    }

    public void UpdateDeliveryDate(DateTime deliveryDate)
    {
        DeliveryDate = deliveryDate;
        UpdatedAt = DateTime.UtcNow;
    }

    public void AddNotes(string notes)
    {
        Notes = notes;
        UpdatedAt = DateTime.UtcNow;
    }

    private static string GenerateOrderNumber()
    {
        // Simple order number generation: ORD + timestamp
        return $"ORD{DateTime.UtcNow:yyyyMMddHHmmss}";
    }
}