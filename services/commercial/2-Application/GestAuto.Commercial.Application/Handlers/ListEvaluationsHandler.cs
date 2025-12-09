using GestAuto.Commercial.Application.DTOs;
using GestAuto.Commercial.Application.Interfaces;
using GestAuto.Commercial.Application.Queries;
using GestAuto.Commercial.Domain.Enums;
using GestAuto.Commercial.Domain.Interfaces;

namespace GestAuto.Commercial.Application.Handlers;

public class ListEvaluationsHandler : IQueryHandler<ListEvaluationsQuery, PagedResponse<EvaluationListItemResponse>>
{
    private readonly IUsedVehicleEvaluationRepository _evaluationRepository;

    public ListEvaluationsHandler(IUsedVehicleEvaluationRepository evaluationRepository)
    {
        _evaluationRepository = evaluationRepository;
    }

    public async Task<PagedResponse<EvaluationListItemResponse>> HandleAsync(
        ListEvaluationsQuery query, 
        CancellationToken cancellationToken)
    {
        var status = !string.IsNullOrEmpty(query.Status) && Enum.TryParse<EvaluationStatus>(query.Status, true, out var parsedStatus)
            ? parsedStatus
            : (EvaluationStatus?)null;

        var (items, totalCount) = await _evaluationRepository.GetPagedAsync(
            query.ProposalId,
            status,
            query.Page,
            query.PageSize,
            cancellationToken);

        var evaluationResponses = items.Select(EvaluationListItemResponse.FromEntity).ToList();

        return new PagedResponse<EvaluationListItemResponse>(
            evaluationResponses,
            query.Page,
            query.PageSize,
            totalCount);
    }
}