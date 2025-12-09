using GestAuto.Commercial.Application.DTOs;
using GestAuto.Commercial.Application.Interfaces;

namespace GestAuto.Commercial.Application.Queries;

public record GetEvaluationQuery(Guid Id) : IQuery<EvaluationResponse>;