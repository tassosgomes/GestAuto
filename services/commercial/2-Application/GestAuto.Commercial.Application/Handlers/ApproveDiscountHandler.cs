using GestAuto.Commercial.Application.Interfaces;
using GestAuto.Commercial.Domain.Enums;
using GestAuto.Commercial.Domain.Interfaces;
using GestAuto.Commercial.Domain.Exceptions;
using GestAuto.Commercial.Infra.UnitOfWork;

namespace GestAuto.Commercial.Application.Handlers;

public class ApproveDiscountHandler : ICommandHandler<Commands.ApproveDiscountCommand, DTOs.ProposalResponse>
{
    private readonly IProposalRepository _proposalRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ApproveDiscountHandler(
        IProposalRepository proposalRepository,
        IUnitOfWork unitOfWork)
    {
        _proposalRepository = proposalRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<DTOs.ProposalResponse> HandleAsync(
        Commands.ApproveDiscountCommand command, 
        CancellationToken cancellationToken)
    {
        var proposal = await _proposalRepository.GetByIdAsync(command.ProposalId)
            ?? throw new NotFoundException($"Proposta {command.ProposalId} não encontrada");

        if (proposal.Status != ProposalStatus.AwaitingDiscountApproval)
            throw new DomainException("Proposta não está aguardando aprovação de desconto");

        proposal.ApproveDiscount(command.ManagerId);

        await _proposalRepository.UpdateAsync(proposal);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return DTOs.ProposalResponse.FromEntity(proposal);
    }
}
