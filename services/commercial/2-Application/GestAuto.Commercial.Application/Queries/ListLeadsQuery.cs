using GestAuto.Commercial.Application.Interfaces;

namespace GestAuto.Commercial.Application.Queries;

public record ListLeadsQuery(
    Guid? SalesPersonId, // null = gerente vê todos
    string? Status,
    string? Score,
    string? Search,
    DateTime? CreatedFrom,
    DateTime? CreatedTo,
    int Page = 1,
    int PageSize = 20
) : IQuery<DTOs.PagedResponse<DTOs.LeadListItemResponse>>;