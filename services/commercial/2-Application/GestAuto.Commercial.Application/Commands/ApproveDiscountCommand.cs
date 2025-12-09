using GestAuto.Commercial.Application.Interfaces;

namespace GestAuto.Commercial.Application.Commands;

public record ApproveDiscountCommand(
    Guid ProposalId,
    Guid ManagerId
) : ICommand<DTOs.ProposalResponse>;
