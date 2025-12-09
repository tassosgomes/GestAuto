using GestAuto.Commercial.Application.Interfaces;

namespace GestAuto.Commercial.Application.Queries;

public record GetProposalQuery(Guid ProposalId) : IQuery<DTOs.ProposalResponse>;
