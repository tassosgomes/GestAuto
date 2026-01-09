using GestAuto.Commercial.Application.Interfaces;
using GestAuto.Commercial.Domain.Entities;
using GestAuto.Commercial.Domain.Enums;
using GestAuto.Commercial.Domain.Interfaces;

namespace GestAuto.Commercial.Application.Handlers;

public class ListLeadsHandler : IQueryHandler<Queries.ListLeadsQuery, DTOs.PagedResponse<DTOs.LeadListItemResponse>>
{
    private readonly ILeadRepository _leadRepository;

    public ListLeadsHandler(ILeadRepository leadRepository)
    {
        _leadRepository = leadRepository;
    }

    public async Task<DTOs.PagedResponse<DTOs.LeadListItemResponse>> HandleAsync(
        Queries.ListLeadsQuery query, 
        CancellationToken cancellationToken)
    {
        IReadOnlyCollection<LeadStatus>? statuses = null;
        if (!string.IsNullOrWhiteSpace(query.Status))
        {
            statuses = query.Status
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(value => Enum.Parse<LeadStatus>(value, ignoreCase: true))
                .Distinct()
                .ToArray();
        }

        var score = !string.IsNullOrEmpty(query.Score) 
            ? Enum.Parse<LeadScore>(query.Score, ignoreCase: true) 
            : (LeadScore?)null;

        IReadOnlyList<Lead> leads;
        int totalCount;

        if (query.SalesPersonId.HasValue)
        {
            leads = await _leadRepository.ListBySalesPersonAsync(
                query.SalesPersonId.Value,
                statuses,
                score,
                query.Search,
                query.CreatedFrom,
                query.CreatedTo,
                query.Page,
                query.PageSize,
                cancellationToken);
            totalCount = await _leadRepository.CountBySalesPersonAsync(
                query.SalesPersonId.Value,
                statuses,
                score,
                query.Search,
                query.CreatedFrom,
                query.CreatedTo,
                cancellationToken);
        }
        else
        {
            leads = await _leadRepository.ListAllAsync(
                statuses,
                score,
                query.Search,
                query.CreatedFrom,
                query.CreatedTo,
                query.Page,
                query.PageSize,
                cancellationToken);
            totalCount = await _leadRepository.CountAllAsync(
                statuses,
                score,
                query.Search,
                query.CreatedFrom,
                query.CreatedTo,
                cancellationToken);
        }

        var items = leads.Select(DTOs.LeadListItemResponse.FromEntity).ToList();

        return new DTOs.PagedResponse<DTOs.LeadListItemResponse>(
            items, query.Page, query.PageSize, totalCount);
    }
}