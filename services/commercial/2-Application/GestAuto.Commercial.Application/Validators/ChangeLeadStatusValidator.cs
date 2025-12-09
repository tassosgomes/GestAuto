using FluentValidation;
using GestAuto.Commercial.Application.Commands;

namespace GestAuto.Commercial.Application.Validators;

public class ChangeLeadStatusValidator : AbstractValidator<ChangeLeadStatusCommand>
{
    public ChangeLeadStatusValidator()
    {
        RuleFor(x => x.LeadId)
            .NotEmpty().WithMessage("ID do lead é obrigatório");

        RuleFor(x => x.Status)
            .NotEmpty().WithMessage("Status é obrigatório")
            .Must(BeValidStatus).WithMessage("Status inválido. Valores permitidos: new, contacted, qualified, proposal, negotiation, won, lost, archived");
    }

    private bool BeValidStatus(string status)
    {
        return Enum.TryParse<Domain.Enums.LeadStatus>(status, ignoreCase: true, out _);
    }
}