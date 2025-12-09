using GestAuto.Commercial.Application.Interfaces;

namespace GestAuto.Commercial.Application.Commands;

public record CloseProposalCommand(
    Guid ProposalId,
    Guid SalesPersonId
) : ICommand<DTOs.ProposalResponse>;
