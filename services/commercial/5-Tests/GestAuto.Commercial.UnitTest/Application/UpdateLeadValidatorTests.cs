using FluentValidation.TestHelper;
using GestAuto.Commercial.Application.Commands;
using GestAuto.Commercial.Application.Validators;

namespace GestAuto.Commercial.UnitTest.Application;

public class UpdateLeadValidatorTests
{
    private readonly UpdateLeadValidator _validator = new();

    [Fact]
    public void Should_Have_Error_When_LeadId_Is_Empty()
    {
        var command = new UpdateLeadCommand(Guid.Empty, "João Silva", null, null, null, null, null);
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.LeadId);
    }

    [Fact]
    public void Should_Have_Error_When_Name_Exceeds_MaxLength()
    {
        var longName = new string('A', 201);
        var command = new UpdateLeadCommand(Guid.NewGuid(), longName, null, null, null, null, null);
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void Should_Have_Error_When_Email_Is_Invalid()
    {
        var command = new UpdateLeadCommand(Guid.NewGuid(), null, "invalid-email", null, null, null, null);
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void Should_Have_Error_When_Phone_Is_Invalid()
    {
        var command = new UpdateLeadCommand(Guid.NewGuid(), null, null, "123", null, null, null);
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Phone);
    }

    [Fact]
    public void Should_Accept_Valid_Phone_With_10_Digits()
    {
        var command = new UpdateLeadCommand(Guid.NewGuid(), null, null, "1199999999", null, null, null);
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveValidationErrorFor(x => x.Phone);
    }

    [Fact]
    public void Should_Accept_Valid_Phone_With_11_Digits()
    {
        var command = new UpdateLeadCommand(Guid.NewGuid(), null, null, "11999999999", null, null, null);
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveValidationErrorFor(x => x.Phone);
    }

    [Fact]
    public void Should_Not_Validate_Empty_Optional_Fields()
    {
        var command = new UpdateLeadCommand(Guid.NewGuid(), null, null, null, null, null, null);
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveValidationErrorFor(x => x.Name);
        result.ShouldNotHaveValidationErrorFor(x => x.Email);
        result.ShouldNotHaveValidationErrorFor(x => x.Phone);
    }

    [Fact]
    public void Should_Not_Have_Error_When_All_Fields_Are_Valid()
    {
        var command = new UpdateLeadCommand(
            Guid.NewGuid(), 
            "João Silva", 
            "joao@test.com", 
            "11999999999", 
            "Civic", 
            "Sport", 
            "Preto");
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveAnyValidationErrors();
    }
}
