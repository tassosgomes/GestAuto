using GestAuto.Commercial.Application.Interfaces;

namespace GestAuto.Commercial.Application.Queries;

public record ListProposalsQuery(
    Guid? SalesPersonId,
    Guid? LeadId,
    string? Status,
    int Page = 1,
    int PageSize = 20
) : IQuery<DTOs.PagedResponse<DTOs.ProposalListItemResponse>>;
