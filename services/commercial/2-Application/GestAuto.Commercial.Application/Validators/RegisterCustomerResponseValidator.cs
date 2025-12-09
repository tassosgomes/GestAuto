using FluentValidation;
using GestAuto.Commercial.Application.Commands;

namespace GestAuto.Commercial.Application.Validators;

public class RegisterCustomerResponseValidator : AbstractValidator<RegisterCustomerResponseCommand>
{
    public RegisterCustomerResponseValidator()
    {
        RuleFor(x => x.EvaluationId)
            .NotEmpty()
            .WithMessage("EvaluationId é obrigatório");

        RuleFor(x => x.RejectionReason)
            .NotEmpty()
            .When(x => !x.Accepted)
            .WithMessage("Motivo da recusa é obrigatório quando a avaliação é rejeitada")
            .MaximumLength(500)
            .WithMessage("Motivo da recusa não pode exceder 500 caracteres");
    }
}