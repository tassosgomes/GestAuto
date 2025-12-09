using GestAuto.Commercial.Application.Commands;
using GestAuto.Commercial.Application.DTOs;
using GestAuto.Commercial.Application.Interfaces;
using GestAuto.Commercial.Domain.Entities;
using GestAuto.Commercial.Domain.Exceptions;
using GestAuto.Commercial.Domain.Interfaces;
using GestAuto.Commercial.Domain.ValueObjects;

namespace GestAuto.Commercial.Application.Handlers;

public class RequestEvaluationHandler : ICommandHandler<RequestEvaluationCommand, EvaluationResponse>
{
    private readonly IUsedVehicleEvaluationRepository _evaluationRepository;
    private readonly IProposalRepository _proposalRepository;
    private readonly IUnitOfWork _unitOfWork;

    public RequestEvaluationHandler(
        IUsedVehicleEvaluationRepository evaluationRepository,
        IProposalRepository proposalRepository,
        IUnitOfWork unitOfWork)
    {
        _evaluationRepository = evaluationRepository;
        _proposalRepository = proposalRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<EvaluationResponse> HandleAsync(
        RequestEvaluationCommand command, 
        CancellationToken cancellationToken)
    {
        var proposal = await _proposalRepository.GetByIdAsync(command.ProposalId)
            ?? throw new NotFoundException($"Proposta {command.ProposalId} não encontrada");

        var usedVehicle = UsedVehicle.Create(
            command.Brand,
            command.Model,
            command.Year,
            command.Mileage,
            new LicensePlate(command.LicensePlate),
            command.Color,
            command.GeneralCondition,
            command.HasDealershipServiceHistory
        );

        var evaluation = UsedVehicleEvaluation.Request(
            command.ProposalId,
            usedVehicle,
            command.RequestedByUserId
        );

        await _evaluationRepository.AddAsync(evaluation, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return EvaluationResponse.FromEntity(evaluation);
    }
}