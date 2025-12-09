using GestAuto.Commercial.Application.Interfaces;

namespace GestAuto.Commercial.Application.Queries;

public record GetLeadQuery(
    Guid LeadId
) : IQuery<DTOs.LeadResponse>;