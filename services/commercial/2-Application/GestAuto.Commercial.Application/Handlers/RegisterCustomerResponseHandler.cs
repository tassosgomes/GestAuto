using GestAuto.Commercial.Application.Commands;
using GestAuto.Commercial.Application.DTOs;
using GestAuto.Commercial.Application.Interfaces;
using GestAuto.Commercial.Domain.Exceptions;
using GestAuto.Commercial.Domain.Interfaces;

namespace GestAuto.Commercial.Application.Handlers;

public class RegisterCustomerResponseHandler : ICommandHandler<RegisterCustomerResponseCommand, EvaluationResponse>
{
    private readonly IUsedVehicleEvaluationRepository _evaluationRepository;
    private readonly IProposalRepository _proposalRepository;
    private readonly IUnitOfWork _unitOfWork;

    public RegisterCustomerResponseHandler(
        IUsedVehicleEvaluationRepository evaluationRepository,
        IProposalRepository proposalRepository,
        IUnitOfWork unitOfWork)
    {
        _evaluationRepository = evaluationRepository;
        _proposalRepository = proposalRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<EvaluationResponse> HandleAsync(
        RegisterCustomerResponseCommand command, 
        CancellationToken cancellationToken)
    {
        var evaluation = await _evaluationRepository.GetByIdAsync(command.EvaluationId, cancellationToken)
            ?? throw new NotFoundException($"Avaliação {command.EvaluationId} não encontrada");

        if (evaluation.Status != Domain.Enums.EvaluationStatus.Completed)
            throw new DomainException("Avaliação ainda não foi respondida pelo setor de seminovos");

        if (command.Accepted)
        {
            evaluation.CustomerAccept();

            var proposal = await _proposalRepository.GetByIdAsync(evaluation.ProposalId);
            if (proposal != null && evaluation.EvaluatedValue is not null)
            {
                proposal.SetTradeInValue(evaluation.EvaluatedValue);
                await _proposalRepository.UpdateAsync(proposal);
            }
        }
        else
        {
            evaluation.CustomerReject(command.RejectionReason);
        }

        await _evaluationRepository.UpdateAsync(evaluation, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return EvaluationResponse.FromEntity(evaluation);
    }
}