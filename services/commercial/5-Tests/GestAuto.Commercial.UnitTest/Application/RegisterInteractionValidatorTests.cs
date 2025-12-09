using FluentValidation.TestHelper;
using GestAuto.Commercial.Application.Commands;
using GestAuto.Commercial.Application.Validators;

namespace GestAuto.Commercial.UnitTest.Application;

public class RegisterInteractionValidatorTests
{
    private readonly RegisterInteractionValidator _validator = new();

    [Fact]
    public void Should_Have_Error_When_LeadId_Is_Empty()
    {
        var command = new RegisterInteractionCommand(Guid.Empty, "Ligação", "Descrição", DateTime.Now);
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.LeadId);
    }

    [Fact]
    public void Should_Have_Error_When_Type_Is_Empty()
    {
        var command = new RegisterInteractionCommand(Guid.NewGuid(), "", "Descrição", DateTime.Now);
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Type);
    }

    [Fact]
    public void Should_Have_Error_When_Type_Exceeds_MaxLength()
    {
        var longType = new string('A', 101);
        var command = new RegisterInteractionCommand(Guid.NewGuid(), longType, "Descrição", DateTime.Now);
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Type);
    }

    [Fact]
    public void Should_Have_Error_When_Description_Is_Empty()
    {
        var command = new RegisterInteractionCommand(Guid.NewGuid(), "Ligação", "", DateTime.Now);
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Description);
    }

    [Fact]
    public void Should_Have_Error_When_Description_Exceeds_MaxLength()
    {
        var longDescription = new string('A', 1001);
        var command = new RegisterInteractionCommand(Guid.NewGuid(), "Ligação", longDescription, DateTime.Now);
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Description);
    }

    [Fact]
    public void Should_Have_Error_When_OccurredAt_Is_In_Future()
    {
        var command = new RegisterInteractionCommand(Guid.NewGuid(), "Ligação", "Descrição", DateTime.Now.AddDays(1));
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.OccurredAt);
    }

    [Fact]
    public void Should_Not_Have_Error_When_Command_Is_Valid()
    {
        var command = new RegisterInteractionCommand(Guid.NewGuid(), "Call", "Cliente interessado", DateTime.Now.AddMinutes(-5));
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveAnyValidationErrors();
    }
}
