using GestAuto.Commercial.Domain.Entities;

namespace GestAuto.Commercial.Application.DTOs;

public record AddOrderNotesRequest(string Notes);

public record OrderResponse(
    Guid Id,
    Guid? ExternalId,
    Guid ProposalId,
    Guid LeadId,
    string OrderNumber,
    decimal TotalValue,
    string Status,
    DateTime? DeliveryDate,
    DateTime? EstimatedDeliveryDate,
    string? Notes,
    DateTime CreatedAt
)
{
    public static OrderResponse FromEntity(Order order) => new(
        order.Id,
        order.ExternalId,
        order.ProposalId,
        order.LeadId,
        order.OrderNumber,
        order.TotalValue.Amount,
        order.Status.ToString(),
        order.DeliveryDate,
        order.EstimatedDeliveryDate,
        order.Notes,
        order.CreatedAt
    );
}

public record OrderListItemResponse(
    Guid Id,
    string OrderNumber,
    decimal TotalValue,
    string Status,
    DateTime? EstimatedDeliveryDate,
    DateTime CreatedAt
)
{
    public static OrderListItemResponse FromEntity(Order order) => new(
        order.Id,
        order.OrderNumber,
        order.TotalValue.Amount,
        order.Status.ToString(),
        order.EstimatedDeliveryDate,
        order.CreatedAt
    );
}