using FluentValidation;
using GestAuto.Commercial.Application.Commands;

namespace GestAuto.Commercial.Application.Validators;

public class CloseProposalValidator : AbstractValidator<CloseProposalCommand>
{
    public CloseProposalValidator()
    {
        RuleFor(x => x.ProposalId)
            .NotEmpty().WithMessage("Proposta é obrigatória");

        RuleFor(x => x.SalesPersonId)
            .NotEmpty().WithMessage("Vendedor responsável é obrigatório");
    }
}
