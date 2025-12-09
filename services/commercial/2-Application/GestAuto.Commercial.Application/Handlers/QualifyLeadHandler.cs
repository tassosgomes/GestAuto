using GestAuto.Commercial.Application.Interfaces;
using GestAuto.Commercial.Domain.Entities;
using GestAuto.Commercial.Domain.Enums;
using GestAuto.Commercial.Domain.Services;
using GestAuto.Commercial.Domain.ValueObjects;
using GestAuto.Commercial.Domain.Interfaces;
using GestAuto.Commercial.Domain.Exceptions;
using GestAuto.Commercial.Infra.UnitOfWork;
using GestAuto.Commercial.Infra.UnitOfWork;

namespace GestAuto.Commercial.Application.Handlers;

public class QualifyLeadHandler : ICommandHandler<Commands.QualifyLeadCommand, DTOs.LeadResponse>
{
    private readonly ILeadRepository _leadRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly LeadScoringService _scoringService;

    public QualifyLeadHandler(
        ILeadRepository leadRepository, 
        IUnitOfWork unitOfWork,
        LeadScoringService scoringService)
    {
        _leadRepository = leadRepository;
        _unitOfWork = unitOfWork;
        _scoringService = scoringService;
    }

    public async Task<DTOs.LeadResponse> HandleAsync(
        Commands.QualifyLeadCommand command, 
        CancellationToken cancellationToken)
    {
        var lead = await _leadRepository.GetByIdAsync(command.LeadId, cancellationToken)
            ?? throw new NotFoundException($"Lead {command.LeadId} não encontrado");

        Domain.ValueObjects.TradeInVehicle? tradeInVehicle = null;
        if (command.HasTradeInVehicle && command.TradeInVehicle != null)
        {
            tradeInVehicle = new Domain.ValueObjects.TradeInVehicle(
                command.TradeInVehicle.Brand,
                command.TradeInVehicle.Model,
                command.TradeInVehicle.Year,
                command.TradeInVehicle.Mileage,
                command.TradeInVehicle.LicensePlate,
                command.TradeInVehicle.Color,
                command.TradeInVehicle.GeneralCondition,
                command.TradeInVehicle.HasDealershipServiceHistory
            );
        }

        var paymentMethod = Enum.Parse<PaymentMethod>(command.PaymentMethod, ignoreCase: true);

        var qualification = new Qualification(
            command.HasTradeInVehicle,
            tradeInVehicle,
            paymentMethod,
            command.ExpectedPurchaseDate ?? DateTime.Now.AddDays(30),
            command.InterestedInTestDrive
        );

        lead.Qualify(qualification, _scoringService);

        await _leadRepository.UpdateAsync(lead, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return DTOs.LeadResponse.FromEntity(lead);
    }
}