using FluentValidation;
using GestAuto.Commercial.Application.Commands;
using GestAuto.Commercial.Domain.Enums;

namespace GestAuto.Commercial.Application.Validators;

public class UpdateProposalValidator : AbstractValidator<UpdateProposalCommand>
{
    public UpdateProposalValidator()
    {
        RuleFor(x => x.ProposalId)
            .NotEmpty().WithMessage("Proposta é obrigatória");

        RuleFor(x => x.VehicleModel)
            .MaximumLength(100).WithMessage("Modelo deve ter no máximo 100 caracteres")
            .When(x => !string.IsNullOrEmpty(x.VehicleModel));

        RuleFor(x => x.VehicleTrim)
            .MaximumLength(100).WithMessage("Versão deve ter no máximo 100 caracteres")
            .When(x => !string.IsNullOrEmpty(x.VehicleTrim));

        RuleFor(x => x.VehicleColor)
            .MaximumLength(50).WithMessage("Cor deve ter no máximo 50 caracteres")
            .When(x => !string.IsNullOrEmpty(x.VehicleColor));

        RuleFor(x => x.VehicleYear)
            .InclusiveBetween(2000, DateTime.Now.Year + 2).WithMessage("Ano do veículo inválido")
            .When(x => x.VehicleYear.HasValue);

        RuleFor(x => x.VehiclePrice)
            .GreaterThan(0).WithMessage("Preço do veículo deve ser maior que zero")
            .When(x => x.VehiclePrice.HasValue);

        RuleFor(x => x.PaymentMethod)
            .Must(BeValidPaymentMethod).WithMessage("Forma de pagamento inválida")
            .When(x => !string.IsNullOrEmpty(x.PaymentMethod));

        RuleFor(x => x.DownPayment)
            .GreaterThan(0).WithMessage("Entrada deve ser maior que zero")
            .When(x => x.DownPayment.HasValue && x.DownPayment.Value > 0);

        RuleFor(x => x.Installments)
            .InclusiveBetween(1, 60).WithMessage("Número de parcelas deve ser entre 1 e 60")
            .When(x => x.Installments.HasValue);
    }

    private bool BeValidPaymentMethod(string? method)
    {
        return Enum.TryParse<PaymentMethod>(method, ignoreCase: true, out _);
    }
}
