using FluentValidation;
using GestAuto.Commercial.Application.Commands;

namespace GestAuto.Commercial.Application.Validators;

public class ApproveDiscountValidator : AbstractValidator<ApproveDiscountCommand>
{
    public ApproveDiscountValidator()
    {
        RuleFor(x => x.ProposalId)
            .NotEmpty().WithMessage("Proposta é obrigatória");

        RuleFor(x => x.ManagerId)
            .NotEmpty().WithMessage("Gerente responsável é obrigatório");
    }
}
