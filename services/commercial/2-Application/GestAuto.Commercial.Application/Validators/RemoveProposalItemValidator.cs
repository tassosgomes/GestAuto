using FluentValidation;
using GestAuto.Commercial.Application.Commands;

namespace GestAuto.Commercial.Application.Validators;

public class RemoveProposalItemValidator : AbstractValidator<RemoveProposalItemCommand>
{
    public RemoveProposalItemValidator()
    {
        RuleFor(x => x.ProposalId)
            .NotEmpty().WithMessage("Proposta é obrigatória");

        RuleFor(x => x.ItemId)
            .NotEmpty().WithMessage("Item é obrigatório");
    }
}
