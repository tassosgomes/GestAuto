using FluentValidation;
using GestAuto.Commercial.Application.Commands;

namespace GestAuto.Commercial.Application.Validators;

public class CreateLeadValidator : AbstractValidator<CreateLeadCommand>
{
    public CreateLeadValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Nome é obrigatório")
            .MaximumLength(200).WithMessage("Nome deve ter no máximo 200 caracteres");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("E-mail é obrigatório")
            .EmailAddress().WithMessage("E-mail inválido");

        RuleFor(x => x.Phone)
            .NotEmpty().WithMessage("Telefone é obrigatório")
            .Must(BeValidPhone).WithMessage("Telefone deve ter 10 ou 11 dígitos");

        RuleFor(x => x.Source)
            .NotEmpty().WithMessage("Origem é obrigatória")
            .Must(BeValidSource).WithMessage("Origem inválida. Valores permitidos: instagram, indicacao, google, loja, telefone, showroom, portal_classificados, outros");

        RuleFor(x => x.SalesPersonId)
            .NotEmpty().WithMessage("Vendedor responsável é obrigatório");
    }

    private bool BeValidPhone(string phone)
    {
        var digits = new string(phone.Where(char.IsDigit).ToArray());
        return digits.Length >= 10 && digits.Length <= 11;
    }

    private bool BeValidSource(string source)
    {
        return Enum.TryParse<Domain.Enums.LeadSource>(source, ignoreCase: true, out _);
    }
}