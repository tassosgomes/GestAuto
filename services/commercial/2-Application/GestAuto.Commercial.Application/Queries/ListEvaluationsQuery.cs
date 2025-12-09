using GestAuto.Commercial.Application.DTOs;
using GestAuto.Commercial.Application.Interfaces;

namespace GestAuto.Commercial.Application.Queries;

public record ListEvaluationsQuery(
    Guid? ProposalId,
    string? Status,
    int Page,
    int PageSize
) : IQuery<PagedResponse<EvaluationListItemResponse>>;