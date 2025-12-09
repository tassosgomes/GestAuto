using FluentValidation.TestHelper;
using GestAuto.Commercial.Application.Commands;
using GestAuto.Commercial.Application.Validators;

namespace GestAuto.Commercial.UnitTest.Application;

public class CreateLeadValidatorTests
{
    private readonly CreateLeadValidator _validator = new();

    [Fact]
    public void Should_Have_Error_When_Name_Is_Empty()
    {
        var command = new CreateLeadCommand("", "email@test.com", "11999999999", "instagram", Guid.NewGuid(), null, null, null);
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void Should_Have_Error_When_Email_Is_Invalid()
    {
        var command = new CreateLeadCommand("João Silva", "invalid-email", "11999999999", "instagram", Guid.NewGuid(), null, null, null);
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void Should_Have_Error_When_Phone_Is_Invalid()
    {
        var command = new CreateLeadCommand("João Silva", "email@test.com", "123", "instagram", Guid.NewGuid(), null, null, null);
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Phone);
    }

    [Fact]
    public void Should_Have_Error_When_Source_Is_Invalid()
    {
        var command = new CreateLeadCommand("João Silva", "email@test.com", "11999999999", "invalid", Guid.NewGuid(), null, null, null);
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Source);
    }

    [Fact]
    public void Should_Have_Error_When_SalesPersonId_Is_Empty()
    {
        var command = new CreateLeadCommand("João Silva", "email@test.com", "11999999999", "instagram", Guid.Empty, null, null, null);
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.SalesPersonId);
    }

    [Fact]
    public void Should_Not_Have_Error_When_Command_Is_Valid()
    {
        var command = new CreateLeadCommand("João Silva", "email@test.com", "11999999999", "instagram", Guid.NewGuid(), null, null, null);
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveAnyValidationErrors();
    }
}