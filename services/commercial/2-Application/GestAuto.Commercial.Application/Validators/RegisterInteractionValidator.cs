using FluentValidation;
using GestAuto.Commercial.Application.Commands;

namespace GestAuto.Commercial.Application.Validators;

public class RegisterInteractionValidator : AbstractValidator<RegisterInteractionCommand>
{
    public RegisterInteractionValidator()
    {
        RuleFor(x => x.LeadId)
            .NotEmpty().WithMessage("ID do lead é obrigatório");

        RuleFor(x => x.Type)
            .NotEmpty().WithMessage("Tipo de interação é obrigatório")
            .Must(BeValidType).WithMessage("Tipo inválido. Valores permitidos: phone_call, email, whatsapp, in_person, test_drive, showroom_visit, website_contact");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Descrição é obrigatória")
            .MaximumLength(1000).WithMessage("Descrição deve ter no máximo 1000 caracteres");

        RuleFor(x => x.OccurredAt)
            .LessThanOrEqualTo(DateTime.Now).WithMessage("Data da interação não pode ser no futuro");
    }

    private bool BeValidType(string type)
    {
        return Enum.TryParse<Domain.Enums.InteractionType>(type, ignoreCase: true, out _);
    }
}