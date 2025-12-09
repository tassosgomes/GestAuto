using GestAuto.Commercial.Application.Interfaces;
using GestAuto.Commercial.Domain.Interfaces;
using GestAuto.Commercial.Domain.Exceptions;
using GestAuto.Commercial.Infra.UnitOfWork;

namespace GestAuto.Commercial.Application.Handlers;

public class CloseProposalHandler : ICommandHandler<Commands.CloseProposalCommand, DTOs.ProposalResponse>
{
    private readonly IProposalRepository _proposalRepository;
    private readonly ILeadRepository _leadRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CloseProposalHandler(
        IProposalRepository proposalRepository,
        ILeadRepository leadRepository,
        IUnitOfWork unitOfWork)
    {
        _proposalRepository = proposalRepository;
        _leadRepository = leadRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<DTOs.ProposalResponse> HandleAsync(
        Commands.CloseProposalCommand command, 
        CancellationToken cancellationToken)
    {
        var proposal = await _proposalRepository.GetByIdAsync(command.ProposalId)
            ?? throw new NotFoundException($"Proposta {command.ProposalId} não encontrada");

        // Validações dentro do método Close
        proposal.Close(command.SalesPersonId);

        var lead = await _leadRepository.GetByIdAsync(proposal.LeadId, cancellationToken);
        if (lead != null)
        {
            lead.ChangeStatus(Domain.Enums.LeadStatus.Converted);
            await _leadRepository.UpdateAsync(lead, cancellationToken);
        }

        await _proposalRepository.UpdateAsync(proposal);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return DTOs.ProposalResponse.FromEntity(proposal);
    }
}
