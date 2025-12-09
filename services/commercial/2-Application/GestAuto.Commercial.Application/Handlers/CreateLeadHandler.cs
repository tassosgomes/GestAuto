using GestAuto.Commercial.Application.Interfaces;
using GestAuto.Commercial.Domain.Entities;
using GestAuto.Commercial.Domain.Enums;
using GestAuto.Commercial.Domain.ValueObjects;
using GestAuto.Commercial.Domain.Interfaces;
using GestAuto.Commercial.Domain.Exceptions;
using GestAuto.Commercial.Infra.UnitOfWork;

namespace GestAuto.Commercial.Application.Handlers;

public class CreateLeadHandler : ICommandHandler<Commands.CreateLeadCommand, DTOs.LeadResponse>
{
    private readonly ILeadRepository _leadRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateLeadHandler(ILeadRepository leadRepository, IUnitOfWork unitOfWork)
    {
        _leadRepository = leadRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<DTOs.LeadResponse> HandleAsync(
        Commands.CreateLeadCommand command, 
        CancellationToken cancellationToken)
    {
        var email = new Email(command.Email);
        var phone = new Phone(command.Phone);
        var source = Enum.Parse<LeadSource>(command.Source, ignoreCase: true);

        var lead = Lead.Create(
            command.Name,
            email,
            phone,
            source,
            command.SalesPersonId
        );

        if (!string.IsNullOrEmpty(command.InterestedModel))
            lead.UpdateInterest(command.InterestedModel, command.InterestedTrim, command.InterestedColor);

        await _leadRepository.AddAsync(lead, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return DTOs.LeadResponse.FromEntity(lead);
    }
}