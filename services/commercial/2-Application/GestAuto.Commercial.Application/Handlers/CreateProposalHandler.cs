using GestAuto.Commercial.Application.Interfaces;
using GestAuto.Commercial.Domain.Entities;
using GestAuto.Commercial.Domain.Enums;
using GestAuto.Commercial.Domain.ValueObjects;
using GestAuto.Commercial.Domain.Interfaces;
using GestAuto.Commercial.Domain.Exceptions;
using GestAuto.Commercial.Infra.UnitOfWork;

namespace GestAuto.Commercial.Application.Handlers;

public class CreateProposalHandler : ICommandHandler<Commands.CreateProposalCommand, DTOs.ProposalResponse>
{
    private readonly IProposalRepository _proposalRepository;
    private readonly ILeadRepository _leadRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateProposalHandler(
        IProposalRepository proposalRepository,
        ILeadRepository leadRepository,
        IUnitOfWork unitOfWork)
    {
        _proposalRepository = proposalRepository;
        _leadRepository = leadRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<DTOs.ProposalResponse> HandleAsync(
        Commands.CreateProposalCommand command, 
        CancellationToken cancellationToken)
    {
        // Verifica se lead existe
        var lead = await _leadRepository.GetByIdAsync(command.LeadId)
            ?? throw new NotFoundException($"Lead {command.LeadId} não encontrado");

        var paymentMethod = Enum.Parse<PaymentMethod>(command.PaymentMethod, ignoreCase: true);

        var proposal = Proposal.Create(
            command.LeadId,
            command.VehicleModel,
            command.VehicleTrim,
            command.VehicleColor,
            command.VehicleYear,
            command.IsReadyDelivery,
            new Money(command.VehiclePrice),
            Money.Zero, // TradeInValue inicial é zero
            paymentMethod,
            command.DownPayment.HasValue ? new Money(command.DownPayment.Value) : null,
            command.Installments
        );

        await _proposalRepository.AddAsync(proposal);
        
        // Atualiza status do lead
        lead.ChangeStatus(LeadStatus.ProposalSent);
        await _leadRepository.UpdateAsync(lead);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return DTOs.ProposalResponse.FromEntity(proposal);
    }
}
