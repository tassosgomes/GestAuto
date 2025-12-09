using GestAuto.Commercial.Application.Interfaces;

namespace GestAuto.Commercial.Application.Commands;

public record AddProposalItemCommand(
    Guid ProposalId,
    string Description,
    decimal Value
) : ICommand<DTOs.ProposalResponse>;
