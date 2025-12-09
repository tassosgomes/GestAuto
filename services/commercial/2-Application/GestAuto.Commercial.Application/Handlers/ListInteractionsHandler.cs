using GestAuto.Commercial.Application.Interfaces;
using GestAuto.Commercial.Domain.Interfaces;
using GestAuto.Commercial.Domain.Exceptions;

namespace GestAuto.Commercial.Application.Handlers;

public class ListInteractionsHandler : IQueryHandler<Queries.ListInteractionsQuery, IReadOnlyList<DTOs.InteractionResponse>>
{
    private readonly ILeadRepository _leadRepository;

    public ListInteractionsHandler(ILeadRepository leadRepository)
    {
        _leadRepository = leadRepository;
    }

    public async Task<IReadOnlyList<DTOs.InteractionResponse>> HandleAsync(
        Queries.ListInteractionsQuery query, 
        CancellationToken cancellationToken)
    {
        var lead = await _leadRepository.GetByIdAsync(query.LeadId, cancellationToken)
            ?? throw new NotFoundException($"Lead {query.LeadId} não encontrado");

        var interactions = lead.Interactions
            .OrderByDescending(i => i.InteractionDate)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(DTOs.InteractionResponse.FromEntity)
            .ToList();

        return interactions;
    }
}