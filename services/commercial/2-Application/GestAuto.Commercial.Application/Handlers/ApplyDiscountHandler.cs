using GestAuto.Commercial.Application.Interfaces;
using GestAuto.Commercial.Domain.ValueObjects;
using GestAuto.Commercial.Domain.Interfaces;
using GestAuto.Commercial.Domain.Exceptions;
using GestAuto.Commercial.Infra.UnitOfWork;

namespace GestAuto.Commercial.Application.Handlers;

public class ApplyDiscountHandler : ICommandHandler<Commands.ApplyDiscountCommand, DTOs.ProposalResponse>
{
    private readonly IProposalRepository _proposalRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ApplyDiscountHandler(
        IProposalRepository proposalRepository,
        IUnitOfWork unitOfWork)
    {
        _proposalRepository = proposalRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<DTOs.ProposalResponse> HandleAsync(
        Commands.ApplyDiscountCommand command, 
        CancellationToken cancellationToken)
    {
        var proposal = await _proposalRepository.GetByIdAsync(command.ProposalId)
            ?? throw new NotFoundException($"Proposta {command.ProposalId} não encontrada");

        var discountAmount = new Money(command.Amount);
        
        // Método ApplyDiscount na entidade verifica se > 5% e muda status para aguardando aprovação
        proposal.ApplyDiscount(discountAmount, command.Reason, command.SalesPersonId);

        await _proposalRepository.UpdateAsync(proposal);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return DTOs.ProposalResponse.FromEntity(proposal);
    }
}
