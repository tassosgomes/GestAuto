using GestAuto.Commercial.Application.Interfaces;

namespace GestAuto.Commercial.Application.Queries;

public record GetLeadQuery(
    Guid LeadId,
    Guid? SalesPersonId
) : IQuery<DTOs.LeadResponse>;