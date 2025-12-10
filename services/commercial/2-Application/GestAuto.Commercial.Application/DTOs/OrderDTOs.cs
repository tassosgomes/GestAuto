using GestAuto.Commercial.Domain.Entities;

namespace GestAuto.Commercial.Application.DTOs;

/// <summary>
/// Requisição para adicionar notas a um pedido
/// </summary>
public record AddOrderNotesRequest(
    /// <summary>Observações adicionais do pedido</summary>
    string Notes
);

/// <summary>
/// Resposta com informações completas do pedido
/// </summary>
public record OrderResponse(
    /// <summary>ID único do pedido</summary>
    Guid Id,
    /// <summary>ID externo do pedido no módulo financeiro</summary>
    Guid? ExternalId,
    /// <summary>ID da proposta associada</summary>
    Guid ProposalId,
    /// <summary>ID do lead associado</summary>
    Guid LeadId,
    /// <summary>Número do pedido/ordem</summary>
    string OrderNumber,
    /// <summary>Valor total do pedido</summary>
    decimal TotalValue,
    /// <summary>Status do pedido (AwaitingDocumentation, CreditAnalysis, CreditApproved, CreditRejected, AwaitingVehicle, ReadyForDelivery, Delivered)</summary>
    string Status,
    /// <summary>Data de entrega (quando já entregue)</summary>
    DateTime? DeliveryDate,
    /// <summary>Data estimada para entrega</summary>
    DateTime? EstimatedDeliveryDate,
    /// <summary>Observações do pedido</summary>
    string? Notes,
    /// <summary>Data de criação do pedido</summary>
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

/// <summary>
/// Item de pedido em listagem paginada
/// </summary>
public record OrderListItemResponse(
    /// <summary>ID único do pedido</summary>
    Guid Id,
    /// <summary>Número do pedido/ordem</summary>
    string OrderNumber,
    /// <summary>Valor total</summary>
    decimal TotalValue,
    /// <summary>Status do pedido</summary>
    string Status,
    /// <summary>Data estimada de entrega</summary>
    DateTime? EstimatedDeliveryDate,
    /// <summary>Data de criação</summary>
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