using FluentValidation;
using GestAuto.Commercial.Application.Commands;
using GestAuto.Commercial.Domain.Enums;

namespace GestAuto.Commercial.Application.Validators;

public class CreateProposalValidator : AbstractValidator<CreateProposalCommand>
{
    public CreateProposalValidator()
    {
        RuleFor(x => x.LeadId)
            .NotEmpty().WithMessage("Lead é obrigatório");

        RuleFor(x => x.VehicleModel)
            .NotEmpty().WithMessage("Modelo do veículo é obrigatório")
            .MaximumLength(100).WithMessage("Modelo deve ter no máximo 100 caracteres");

        RuleFor(x => x.VehicleTrim)
            .NotEmpty().WithMessage("Versão do veículo é obrigatória")
            .MaximumLength(100).WithMessage("Versão deve ter no máximo 100 caracteres");

        RuleFor(x => x.VehicleColor)
            .NotEmpty().WithMessage("Cor do veículo é obrigatória")
            .MaximumLength(50).WithMessage("Cor deve ter no máximo 50 caracteres");

        RuleFor(x => x.VehicleYear)
            .InclusiveBetween(2000, DateTime.Now.Year + 2)
            .WithMessage("Ano do veículo inválido");

        RuleFor(x => x.VehiclePrice)
            .GreaterThan(0).WithMessage("Preço do veículo deve ser maior que zero");

        RuleFor(x => x.PaymentMethod)
            .NotEmpty().WithMessage("Forma de pagamento é obrigatória")
            .Must(BeValidPaymentMethod).WithMessage("Forma de pagamento inválida");

        When(x => x.PaymentMethod?.ToLower() == "financing", () =>
        {
            RuleFor(x => x.DownPayment)
                .GreaterThan(0).When(x => x.DownPayment.HasValue)
                .WithMessage("Entrada deve ser maior que zero");

            RuleFor(x => x.Installments)
                .InclusiveBetween(1, 60).When(x => x.Installments.HasValue)
                .WithMessage("Número de parcelas deve ser entre 1 e 60");
        });
    }

    private bool BeValidPaymentMethod(string method)
    {
        return Enum.TryParse<PaymentMethod>(method, ignoreCase: true, out _);
    }
}
