using FluentValidation.TestHelper;
using GestAuto.Commercial.Application.Commands;
using GestAuto.Commercial.Application.Validators;

namespace GestAuto.Commercial.UnitTest.Application;

public class QualifyLeadValidatorTests
{
    private readonly QualifyLeadValidator _validator = new();

    [Fact]
    public void Should_Have_Error_When_LeadId_Is_Empty()
    {
        var command = new QualifyLeadCommand(Guid.Empty, false, null, "a_vista", null, false);
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.LeadId);
    }

    [Fact]
    public void Should_Have_Error_When_PaymentMethod_Is_Empty()
    {
        var command = new QualifyLeadCommand(Guid.NewGuid(), false, null, "", null, false);
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.PaymentMethod);
    }

    [Fact]
    public void Should_Have_Error_When_PaymentMethod_Is_Invalid()
    {
        var command = new QualifyLeadCommand(Guid.NewGuid(), false, null, "invalid", null, false);
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.PaymentMethod);
    }

    [Fact]
    public void Should_Have_Error_When_HasTradeIn_But_TradeInVehicle_Brand_Is_Empty()
    {
        var tradeIn = new TradeInVehicleDto("", "Civic", 2020, 30000, "ABC1234", "Preto", "Bom", true);
        var command = new QualifyLeadCommand(Guid.NewGuid(), true, tradeIn, "Financing", null, false);
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.TradeInVehicle!.Brand);
    }

    [Fact]
    public void Should_Have_Error_When_Year_Is_Too_Old()
    {
        var tradeIn = new TradeInVehicleDto("Honda", "Civic", 1800, 30000, "ABC1234", "Preto", "Bom", true);
        var command = new QualifyLeadCommand(Guid.NewGuid(), true, tradeIn, "Financing", null, false);
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.TradeInVehicle!.Year);
    }

    [Fact]
    public void Should_Have_Error_When_Year_Is_In_Future()
    {
        var futureYear = DateTime.Now.Year + 2;
        var tradeIn = new TradeInVehicleDto("Honda", "Civic", futureYear, 30000, "ABC1234", "Preto", "Bom", true);
        var command = new QualifyLeadCommand(Guid.NewGuid(), true, tradeIn, "Financing", null, false);
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.TradeInVehicle!.Year);
    }

    [Fact]
    public void Should_Have_Error_When_Mileage_Is_Negative()
    {
        var tradeIn = new TradeInVehicleDto("Honda", "Civic", 2020, -1000, "ABC1234", "Preto", "Bom", true);
        var command = new QualifyLeadCommand(Guid.NewGuid(), true, tradeIn, "Financing", null, false);
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.TradeInVehicle!.Mileage);
    }

    [Fact]
    public void Should_Have_Error_When_LicensePlate_Is_Invalid_Format()
    {
        var tradeIn = new TradeInVehicleDto("Honda", "Civic", 2020, 30000, "INVALID", "Preto", "Bom", true);
        var command = new QualifyLeadCommand(Guid.NewGuid(), true, tradeIn, "Financing", null, false);
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.TradeInVehicle!.LicensePlate);
    }

    [Fact]
    public void Should_Accept_Valid_Old_Format_LicensePlate()
    {
        var tradeIn = new TradeInVehicleDto("Honda", "Civic", 2020, 30000, "ABC1234", "Preto", "Bom", true);
        var command = new QualifyLeadCommand(Guid.NewGuid(), true, tradeIn, "Financing", null, false);
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveValidationErrorFor(x => x.TradeInVehicle!.LicensePlate);
    }

    [Fact]
    public void Should_Accept_Valid_Mercosul_Format_LicensePlate()
    {
        var tradeIn = new TradeInVehicleDto("Honda", "Civic", 2020, 30000, "ABC1D23", "Preto", "Bom", true);
        var command = new QualifyLeadCommand(Guid.NewGuid(), true, tradeIn, "Financing", null, false);
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveValidationErrorFor(x => x.TradeInVehicle!.LicensePlate);
    }

    [Fact]
    public void Should_Have_Error_When_ExpectedPurchaseDate_Is_In_Past()
    {
        var command = new QualifyLeadCommand(Guid.NewGuid(), false, null, "Cash", DateTime.Now.AddDays(-1), false);
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.ExpectedPurchaseDate!.Value);
    }

    [Fact]
    public void Should_Not_Have_Error_When_Command_Is_Valid()
    {
        var tradeIn = new TradeInVehicleDto("Honda", "Civic", 2020, 30000, "ABC1234", "Preto", "Bom", true);
        var command = new QualifyLeadCommand(Guid.NewGuid(), true, tradeIn, "Financing", DateTime.Now.AddDays(10), true);
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveAnyValidationErrors();
    }
}
