using GestAuto.Commercial.Application.Interfaces;
using GestAuto.Commercial.Domain.Entities;
using GestAuto.Commercial.Domain.Enums;
using GestAuto.Commercial.Domain.Interfaces;
using GestAuto.Commercial.Domain.Exceptions;
using GestAuto.Commercial.Infra.UnitOfWork;
using GestAuto.Commercial.Infra.UnitOfWork;

namespace GestAuto.Commercial.Application.Handlers;

public class RegisterInteractionHandler : ICommandHandler<Commands.RegisterInteractionCommand, DTOs.InteractionResponse>
{
    private readonly ILeadRepository _leadRepository;
    private readonly IUnitOfWork _unitOfWork;

    public RegisterInteractionHandler(ILeadRepository leadRepository, IUnitOfWork unitOfWork)
    {
        _leadRepository = leadRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<DTOs.InteractionResponse> HandleAsync(
        Commands.RegisterInteractionCommand command, 
        CancellationToken cancellationToken)
    {
        var lead = await _leadRepository.GetByIdAsync(command.LeadId, cancellationToken)
            ?? throw new NotFoundException($"Lead {command.LeadId} não encontrado");

        var type = Enum.Parse<InteractionType>(command.Type, ignoreCase: true);
        var interaction = lead.RegisterInteraction(type, command.Description, command.OccurredAt);

        await _leadRepository.UpdateAsync(lead, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return DTOs.InteractionResponse.FromEntity(interaction);
    }
}