using FluentValidation;
using GestAuto.Commercial.Application.Commands;

namespace GestAuto.Commercial.Application.Validators;

public class QualifyLeadValidator : AbstractValidator<QualifyLeadCommand>
{
    public QualifyLeadValidator()
    {
        RuleFor(x => x.LeadId)
            .NotEmpty().WithMessage("ID do lead é obrigatório");

        RuleFor(x => x.PaymentMethod)
            .NotEmpty().WithMessage("Método de pagamento é obrigatório")
            .Must(BeValidPaymentMethod).WithMessage("Método de pagamento inválido. Valores permitidos: a_vista, financiamento, consorcio, leasing");

        When(x => x.HasTradeInVehicle && x.TradeInVehicle != null, () =>
        {
            RuleFor(x => x.TradeInVehicle!.Brand)
                .NotEmpty().WithMessage("Marca do veículo usado é obrigatória");

            RuleFor(x => x.TradeInVehicle!.Model)
                .NotEmpty().WithMessage("Modelo do veículo usado é obrigatório");

            RuleFor(x => x.TradeInVehicle!.Year)
                .GreaterThan(1900).WithMessage("Ano deve ser maior que 1900")
                .LessThanOrEqualTo(DateTime.Now.Year + 1).WithMessage("Ano não pode ser no futuro");

            RuleFor(x => x.TradeInVehicle!.Mileage)
                .GreaterThanOrEqualTo(0).WithMessage("Quilometragem deve ser maior ou igual a 0");

            RuleFor(x => x.TradeInVehicle!.LicensePlate)
                .NotEmpty().WithMessage("Placa é obrigatória")
                .Matches(@"^[A-Z]{3}\d{4}$").WithMessage("Placa deve estar no formato AAA1234");

            RuleFor(x => x.TradeInVehicle!.Color)
                .NotEmpty().WithMessage("Cor é obrigatória");

            RuleFor(x => x.TradeInVehicle!.GeneralCondition)
                .NotEmpty().WithMessage("Condição geral é obrigatória");
        });

        When(x => x.ExpectedPurchaseDate.HasValue, () =>
        {
            RuleFor(x => x.ExpectedPurchaseDate!.Value)
                .GreaterThan(DateTime.Now.Date).WithMessage("Data esperada de compra deve ser no futuro");
        });
    }

    private bool BeValidPaymentMethod(string paymentMethod)
    {
        return Enum.TryParse<Domain.Enums.PaymentMethod>(paymentMethod, ignoreCase: true, out _);
    }
}