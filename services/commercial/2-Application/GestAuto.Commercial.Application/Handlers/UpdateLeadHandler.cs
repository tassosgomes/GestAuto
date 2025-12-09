using GestAuto.Commercial.Application.Interfaces;
using GestAuto.Commercial.Domain.Entities;
using GestAuto.Commercial.Domain.ValueObjects;
using GestAuto.Commercial.Domain.Interfaces;
using GestAuto.Commercial.Domain.Exceptions;
using GestAuto.Commercial.Infra.UnitOfWork;
using GestAuto.Commercial.Infra.UnitOfWork;

namespace GestAuto.Commercial.Application.Handlers;

public class UpdateLeadHandler : ICommandHandler<Commands.UpdateLeadCommand, DTOs.LeadResponse>
{
    private readonly ILeadRepository _leadRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateLeadHandler(ILeadRepository leadRepository, IUnitOfWork unitOfWork)
    {
        _leadRepository = leadRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<DTOs.LeadResponse> HandleAsync(
        Commands.UpdateLeadCommand command, 
        CancellationToken cancellationToken)
    {
        var lead = await _leadRepository.GetByIdAsync(command.LeadId, cancellationToken)
            ?? throw new NotFoundException($"Lead {command.LeadId} não encontrado");

        if (!string.IsNullOrEmpty(command.Name))
            lead.UpdateName(command.Name);

        if (!string.IsNullOrEmpty(command.Email))
            lead.UpdateEmail(new Email(command.Email));

        if (!string.IsNullOrEmpty(command.Phone))
            lead.UpdatePhone(new Phone(command.Phone));

        lead.UpdateInterest(command.InterestedModel, command.InterestedTrim, command.InterestedColor);

        await _leadRepository.UpdateAsync(lead, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return DTOs.LeadResponse.FromEntity(lead);
    }
}