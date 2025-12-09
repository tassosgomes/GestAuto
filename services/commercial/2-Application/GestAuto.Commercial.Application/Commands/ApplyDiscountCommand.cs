using GestAuto.Commercial.Application.Interfaces;

namespace GestAuto.Commercial.Application.Commands;

public record ApplyDiscountCommand(
    Guid ProposalId,
    decimal Amount,
    string Reason,
    Guid SalesPersonId
) : ICommand<DTOs.ProposalResponse>;
