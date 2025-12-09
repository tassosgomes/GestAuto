using FluentValidation;
using GestAuto.Commercial.Application.Commands;

namespace GestAuto.Commercial.Application.Validators;

public class ApplyDiscountValidator : AbstractValidator<ApplyDiscountCommand>
{
    public ApplyDiscountValidator()
    {
        RuleFor(x => x.ProposalId)
            .NotEmpty().WithMessage("Proposta é obrigatória");

        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Valor do desconto deve ser maior que zero");

        RuleFor(x => x.Reason)
            .NotEmpty().WithMessage("Motivo do desconto é obrigatório")
            .MaximumLength(500).WithMessage("Motivo deve ter no máximo 500 caracteres");

        RuleFor(x => x.SalesPersonId)
            .NotEmpty().WithMessage("Vendedor responsável é obrigatório");
    }
}
