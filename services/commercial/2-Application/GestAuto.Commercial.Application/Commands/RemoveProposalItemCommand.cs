using GestAuto.Commercial.Application.Interfaces;

namespace GestAuto.Commercial.Application.Commands;

public record RemoveProposalItemCommand(
    Guid ProposalId,
    Guid ItemId
) : ICommand<DTOs.ProposalResponse>;
