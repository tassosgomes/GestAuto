using FluentValidation;
using GestAuto.Commercial.Application.Commands;

namespace GestAuto.Commercial.Application.Validators;

public class RequestEvaluationValidator : AbstractValidator<RequestEvaluationCommand>
{
    public RequestEvaluationValidator()
    {
        RuleFor(x => x.ProposalId)
            .NotEmpty()
            .WithMessage("ProposalId é obrigatório");

        RuleFor(x => x.Brand)
            .NotEmpty()
            .WithMessage("Marca é obrigatória")
            .MaximumLength(50)
            .WithMessage("Marca não pode exceder 50 caracteres");

        RuleFor(x => x.Model)
            .NotEmpty()
            .WithMessage("Modelo é obrigatório")
            .MaximumLength(100)
            .WithMessage("Modelo não pode exceder 100 caracteres");

        RuleFor(x => x.Year)
            .GreaterThan(1900)
            .WithMessage("Ano deve ser maior que 1900")
            .LessThanOrEqualTo(DateTime.Now.Year + 1)
            .WithMessage("Ano não pode ser futuro");

        RuleFor(x => x.Mileage)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Quilometragem não pode ser negativa");

        RuleFor(x => x.LicensePlate)
            .NotEmpty()
            .WithMessage("Placa é obrigatória")
            .Must(BeValidLicensePlate)
            .WithMessage("Formato de placa inválido");

        RuleFor(x => x.Color)
            .NotEmpty()
            .WithMessage("Cor é obrigatória")
            .MaximumLength(50)
            .WithMessage("Cor não pode exceder 50 caracteres");

        RuleFor(x => x.GeneralCondition)
            .NotEmpty()
            .WithMessage("Estado geral é obrigatório")
            .MaximumLength(200)
            .WithMessage("Estado geral não pode exceder 200 caracteres");

        RuleFor(x => x.RequestedByUserId)
            .NotEmpty()
            .WithMessage("Usuário solicitante é obrigatório");
    }

    private static bool BeValidLicensePlate(string plate)
    {
        if (string.IsNullOrWhiteSpace(plate)) 
            return false;
        
        var cleanPlate = plate.ToUpperInvariant().Replace("-", "").Replace(" ", "");
        
        // Formato antigo: AAA1234
        if (System.Text.RegularExpressions.Regex.IsMatch(cleanPlate, @"^[A-Z]{3}[0-9]{4}$"))
            return true;
            
        // Formato Mercosul: AAA1A23
        if (System.Text.RegularExpressions.Regex.IsMatch(cleanPlate, @"^[A-Z]{3}[0-9][A-Z][0-9]{2}$"))
            return true;
            
        return false;
    }
}