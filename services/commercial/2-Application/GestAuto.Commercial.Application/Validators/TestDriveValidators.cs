using FluentValidation;
using GestAuto.Commercial.Application.Commands;

namespace GestAuto.Commercial.Application.Validators;

public class ScheduleTestDriveValidator : AbstractValidator<ScheduleTestDriveCommand>
{
    public ScheduleTestDriveValidator()
    {
        RuleFor(x => x.LeadId)
            .NotEmpty().WithMessage("Lead is required");

        RuleFor(x => x.VehicleId)
            .NotEmpty().WithMessage("Vehicle is required");

        RuleFor(x => x.ScheduledAt)
            .GreaterThan(DateTime.UtcNow).WithMessage("Test-drive date must be in the future")
            .LessThan(DateTime.UtcNow.AddMonths(3)).WithMessage("Test-drive date must be within the next 3 months");

        RuleFor(x => x.SalesPersonId)
            .NotEmpty().WithMessage("SalesPerson is required");
    }
}

public class CompleteTestDriveValidator : AbstractValidator<CompleteTestDriveCommand>
{
    public CompleteTestDriveValidator()
    {
        RuleFor(x => x.TestDriveId)
            .NotEmpty().WithMessage("Test-drive is required");

        RuleFor(x => x.Checklist)
            .NotNull().WithMessage("Checklist is required");

        RuleFor(x => x.Checklist.InitialMileage)
            .GreaterThanOrEqualTo(0).WithMessage("Initial mileage must be non-negative")
            .When(x => x.Checklist != null);

        RuleFor(x => x.Checklist.FinalMileage)
            .GreaterThanOrEqualTo(x => x.Checklist.InitialMileage)
            .WithMessage("Final mileage must be greater than or equal to initial mileage")
            .When(x => x.Checklist != null);

        RuleFor(x => x.Checklist.FuelLevel)
            .Must(BeValidFuelLevel).WithMessage("Invalid fuel level")
            .When(x => x.Checklist != null);

        RuleFor(x => x.CompletedByUserId)
            .NotEmpty().WithMessage("Completed by user is required");
    }

    private bool BeValidFuelLevel(string level)
    {
        return Enum.TryParse<Domain.Enums.FuelLevel>(level, ignoreCase: true, out _);
    }
}

public class CancelTestDriveValidator : AbstractValidator<CancelTestDriveCommand>
{
    public CancelTestDriveValidator()
    {
        RuleFor(x => x.TestDriveId)
            .NotEmpty().WithMessage("Test-drive is required");

        RuleFor(x => x.CancelledByUserId)
            .NotEmpty().WithMessage("Cancelled by user is required");
    }
}
