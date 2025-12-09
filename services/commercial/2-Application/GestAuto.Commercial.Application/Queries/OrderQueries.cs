using GestAuto.Commercial.Application.DTOs;
using GestAuto.Commercial.Application.Interfaces;

namespace GestAuto.Commercial.Application.Queries;

public record GetOrderQuery(Guid Id) : IQuery<OrderResponse>;

public record ListOrdersQuery(
    int Page = 1,
    int PageSize = 20
) : IQuery<PagedResponse<OrderListItemResponse>>;