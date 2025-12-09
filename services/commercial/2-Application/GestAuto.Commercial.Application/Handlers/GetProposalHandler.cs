using GestAuto.Commercial.Application.Interfaces;
using GestAuto.Commercial.Domain.Interfaces;
using GestAuto.Commercial.Domain.Exceptions;

namespace GestAuto.Commercial.Application.Handlers;

public class GetProposalHandler : IQueryHandler<Queries.GetProposalQuery, DTOs.ProposalResponse>
{
    private readonly IProposalRepository _proposalRepository;

    public GetProposalHandler(IProposalRepository proposalRepository)
    {
        _proposalRepository = proposalRepository;
    }

    public async Task<DTOs.ProposalResponse> HandleAsync(
        Queries.GetProposalQuery query, 
        CancellationToken cancellationToken)
    {
        var proposal = await _proposalRepository.GetByIdAsync(query.ProposalId)
            ?? throw new NotFoundException($"Proposta {query.ProposalId} não encontrada");

        return DTOs.ProposalResponse.FromEntity(proposal);
    }
}
