using GestAuto.Commercial.Application.Interfaces;
using GestAuto.Commercial.Domain.Enums;
using GestAuto.Commercial.Domain.ValueObjects;
using GestAuto.Commercial.Domain.Interfaces;
using GestAuto.Commercial.Domain.Exceptions;
using GestAuto.Commercial.Infra.UnitOfWork;

namespace GestAuto.Commercial.Application.Handlers;

public class UpdateProposalHandler : ICommandHandler<Commands.UpdateProposalCommand, DTOs.ProposalResponse>
{
    private readonly IProposalRepository _proposalRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateProposalHandler(
        IProposalRepository proposalRepository,
        IUnitOfWork unitOfWork)
    {
        _proposalRepository = proposalRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<DTOs.ProposalResponse> HandleAsync(
        Commands.UpdateProposalCommand command, 
        CancellationToken cancellationToken)
    {
        var proposal = await _proposalRepository.GetByIdAsync(command.ProposalId)
            ?? throw new NotFoundException($"Proposta {command.ProposalId} não encontrada");

        // Não permite atualizar proposta fechada ou perdida
        if (proposal.Status == ProposalStatus.Closed || proposal.Status == ProposalStatus.Lost)
            throw new DomainException("Não é possível atualizar proposta fechada ou perdida");

        // Atualiza informações do veículo se fornecidas
        if (!string.IsNullOrEmpty(command.VehicleModel) || 
            !string.IsNullOrEmpty(command.VehicleTrim) || 
            !string.IsNullOrEmpty(command.VehicleColor) || 
            command.VehicleYear.HasValue || 
            command.IsReadyDelivery.HasValue)
        {
            proposal.UpdateVehicleInfo(
                command.VehicleModel ?? proposal.VehicleModel,
                command.VehicleTrim ?? proposal.VehicleTrim,
                command.VehicleColor ?? proposal.VehicleColor,
                command.VehicleYear ?? proposal.VehicleYear,
                command.IsReadyDelivery ?? proposal.IsReadyDelivery
            );
        }

        // Atualiza informações de pagamento se fornecidas
        if (command.VehiclePrice.HasValue || 
            !string.IsNullOrEmpty(command.PaymentMethod) || 
            command.DownPayment.HasValue || 
            command.Installments.HasValue)
        {
            var paymentMethod = !string.IsNullOrEmpty(command.PaymentMethod)
                ? Enum.Parse<PaymentMethod>(command.PaymentMethod, ignoreCase: true)
                : proposal.PaymentMethod;

            var vehiclePrice = command.VehiclePrice.HasValue ? new Money(command.VehiclePrice.Value) : proposal.VehiclePrice;
            var downPayment = command.DownPayment.HasValue ? (command.DownPayment.Value > 0 ? new Money(command.DownPayment.Value) : null) : proposal.DownPayment;
            var installments = command.Installments ?? proposal.Installments;

            proposal.UpdatePaymentInfo(vehiclePrice, paymentMethod, downPayment, installments);
        }

        await _proposalRepository.UpdateAsync(proposal);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return DTOs.ProposalResponse.FromEntity(proposal);
    }
}
