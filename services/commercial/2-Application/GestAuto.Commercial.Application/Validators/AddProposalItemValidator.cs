using FluentValidation;
using GestAuto.Commercial.Application.Commands;

namespace GestAuto.Commercial.Application.Validators;

public class AddProposalItemValidator : AbstractValidator<AddProposalItemCommand>
{
    public AddProposalItemValidator()
    {
        RuleFor(x => x.ProposalId)
            .NotEmpty().WithMessage("Proposta é obrigatória");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Descrição do item é obrigatória")
            .MaximumLength(500).WithMessage("Descrição deve ter no máximo 500 caracteres");

        RuleFor(x => x.Value)
            .GreaterThan(0).WithMessage("Valor do item deve ser maior que zero");
    }
}
