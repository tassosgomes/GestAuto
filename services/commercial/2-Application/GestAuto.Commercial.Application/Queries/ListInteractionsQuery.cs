using GestAuto.Commercial.Application.Interfaces;

namespace GestAuto.Commercial.Application.Queries;

public record ListInteractionsQuery(
    Guid LeadId,
    int Page = 1,
    int PageSize = 20
) : IQuery<IReadOnlyList<DTOs.InteractionResponse>>;