using GestAuto.Commercial.Application.Interfaces;
using GestAuto.Commercial.Domain.Enums;
using GestAuto.Commercial.Domain.Interfaces;

namespace GestAuto.Commercial.Application.Handlers;

public class ListProposalsHandler : IQueryHandler<Queries.ListProposalsQuery, DTOs.PagedResponse<DTOs.ProposalListItemResponse>>
{
    private readonly IProposalRepository _proposalRepository;

    public ListProposalsHandler(IProposalRepository proposalRepository)
    {
        _proposalRepository = proposalRepository;
    }

    public async Task<DTOs.PagedResponse<DTOs.ProposalListItemResponse>> HandleAsync(
        Queries.ListProposalsQuery query, 
        CancellationToken cancellationToken)
    {
        var status = !string.IsNullOrEmpty(query.Status) 
            ? Enum.Parse<ProposalStatus>(query.Status, ignoreCase: true) 
            : (ProposalStatus?)null;

        var proposals = await _proposalRepository.ListAsync(
            query.SalesPersonId,
            query.LeadId,
            status,
            query.Page,
            query.PageSize,
            cancellationToken);

        var totalCount = await _proposalRepository.CountAsync(
            query.SalesPersonId,
            query.LeadId,
            status,
            cancellationToken);

        var items = proposals.Select(DTOs.ProposalListItemResponse.FromEntity).ToList();

        return new DTOs.PagedResponse<DTOs.ProposalListItemResponse>(
            items, query.Page, query.PageSize, totalCount);
    }
}
