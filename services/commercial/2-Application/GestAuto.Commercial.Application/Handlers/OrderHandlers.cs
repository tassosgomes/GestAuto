using GestAuto.Commercial.Application.DTOs;
using GestAuto.Commercial.Application.Interfaces;
using GestAuto.Commercial.Application.Queries;
using GestAuto.Commercial.Domain.Exceptions;
using GestAuto.Commercial.Domain.Interfaces;

namespace GestAuto.Commercial.Application.Handlers;

public class GetOrderHandler : IQueryHandler<GetOrderQuery, OrderResponse>
{
    private readonly IOrderRepository _orderRepository;

    public GetOrderHandler(IOrderRepository orderRepository)
    {
        _orderRepository = orderRepository;
    }

    public async Task<OrderResponse> HandleAsync(
        GetOrderQuery query, 
        CancellationToken cancellationToken)
    {
        var order = await _orderRepository.GetByIdAsync(query.Id, cancellationToken)
            ?? throw new NotFoundException($"Pedido {query.Id} não encontrado");

        return OrderResponse.FromEntity(order);
    }
}

public class ListOrdersHandler : IQueryHandler<ListOrdersQuery, PagedResponse<OrderListItemResponse>>
{
    private readonly IOrderRepository _orderRepository;

    public ListOrdersHandler(IOrderRepository orderRepository)
    {
        _orderRepository = orderRepository;
    }

    public async Task<PagedResponse<OrderListItemResponse>> HandleAsync(
        ListOrdersQuery query, 
        CancellationToken cancellationToken)
    {
        var orders = await _orderRepository.GetAllAsync(cancellationToken);
        
        // Aplicar paginação
        var totalCount = orders.Count();
        var pagedOrders = orders
            .OrderByDescending(x => x.CreatedAt)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToList();

        var orderResponses = pagedOrders.Select(OrderListItemResponse.FromEntity).ToList();

        return new PagedResponse<OrderListItemResponse>(
            orderResponses,
            query.Page,
            query.PageSize,
            totalCount);
    }
}