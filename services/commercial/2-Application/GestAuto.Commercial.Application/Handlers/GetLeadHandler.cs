using GestAuto.Commercial.Application.Interfaces;
using GestAuto.Commercial.Domain.Entities;
using GestAuto.Commercial.Domain.Interfaces;
using GestAuto.Commercial.Domain.Exceptions;

namespace GestAuto.Commercial.Application.Handlers;

public class GetLeadHandler : IQueryHandler<Queries.GetLeadQuery, DTOs.LeadResponse>
{
    private readonly ILeadRepository _leadRepository;

    public GetLeadHandler(ILeadRepository leadRepository)
    {
        _leadRepository = leadRepository;
    }

    public async Task<DTOs.LeadResponse> HandleAsync(
        Queries.GetLeadQuery query, 
        CancellationToken cancellationToken)
    {
        var lead = await _leadRepository.GetByIdAsync(query.LeadId, cancellationToken)
            ?? throw new NotFoundException($"Lead {query.LeadId} não encontrado");

        return DTOs.LeadResponse.FromEntity(lead);
    }
}