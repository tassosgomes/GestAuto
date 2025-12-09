using GestAuto.Commercial.Application.Interfaces;
using GestAuto.Commercial.Domain.Interfaces;
using GestAuto.Commercial.Domain.Exceptions;
using GestAuto.Commercial.Infra.UnitOfWork;

namespace GestAuto.Commercial.Application.Handlers;

public class RemoveProposalItemHandler : ICommandHandler<Commands.RemoveProposalItemCommand, DTOs.ProposalResponse>
{
    private readonly IProposalRepository _proposalRepository;
    private readonly IUnitOfWork _unitOfWork;

    public RemoveProposalItemHandler(
        IProposalRepository proposalRepository,
        IUnitOfWork unitOfWork)
    {
        _proposalRepository = proposalRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<DTOs.ProposalResponse> HandleAsync(
        Commands.RemoveProposalItemCommand command, 
        CancellationToken cancellationToken)
    {
        var proposal = await _proposalRepository.GetByIdAsync(command.ProposalId)
            ?? throw new NotFoundException($"Proposta {command.ProposalId} não encontrada");

        if (proposal.Status == Domain.Enums.ProposalStatus.Closed)
            throw new DomainException("Não é possível remover itens em proposta fechada");

        proposal.RemoveItem(command.ItemId);

        await _proposalRepository.UpdateAsync(proposal);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return DTOs.ProposalResponse.FromEntity(proposal);
    }
}
