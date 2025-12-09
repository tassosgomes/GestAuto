using GestAuto.Commercial.Application.DTOs;
using GestAuto.Commercial.Application.Interfaces;
using GestAuto.Commercial.Application.Queries;
using GestAuto.Commercial.Domain.Exceptions;
using GestAuto.Commercial.Domain.Interfaces;

namespace GestAuto.Commercial.Application.Handlers;

public class GetEvaluationHandler : IQueryHandler<GetEvaluationQuery, EvaluationResponse>
{
    private readonly IUsedVehicleEvaluationRepository _evaluationRepository;

    public GetEvaluationHandler(IUsedVehicleEvaluationRepository evaluationRepository)
    {
        _evaluationRepository = evaluationRepository;
    }

    public async Task<EvaluationResponse> HandleAsync(
        GetEvaluationQuery query, 
        CancellationToken cancellationToken)
    {
        var evaluation = await _evaluationRepository.GetByIdAsync(query.Id, cancellationToken)
            ?? throw new NotFoundException($"Avaliação {query.Id} não encontrada");

        return EvaluationResponse.FromEntity(evaluation);
    }
}