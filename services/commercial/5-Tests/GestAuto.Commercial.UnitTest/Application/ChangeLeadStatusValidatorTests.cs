using FluentValidation.TestHelper;
using GestAuto.Commercial.Application.Commands;
using GestAuto.Commercial.Application.Validators;

namespace GestAuto.Commercial.UnitTest.Application;

public class ChangeLeadStatusValidatorTests
{
    private readonly ChangeLeadStatusValidator _validator = new();

    [Fact]
    public void Should_Have_Error_When_LeadId_Is_Empty()
    {
        var command = new ChangeLeadStatusCommand(Guid.Empty, "InContact");
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.LeadId);
    }

    [Fact]
    public void Should_Have_Error_When_Status_Is_Empty()
    {
        var command = new ChangeLeadStatusCommand(Guid.NewGuid(), "");
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Status);
    }

    [Fact]
    public void Should_Have_Error_When_Status_Is_Invalid()
    {
        var command = new ChangeLeadStatusCommand(Guid.NewGuid(), "invalid_status");
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Status);
    }

    [Theory]
    [InlineData("New")]
    [InlineData("InContact")]
    [InlineData("InNegotiation")]
    [InlineData("TestDriveScheduled")]
    [InlineData("ProposalSent")]
    [InlineData("Lost")]
    [InlineData("Converted")]
    public void Should_Accept_All_Valid_Statuses(string status)
    {
        var command = new ChangeLeadStatusCommand(Guid.NewGuid(), status);
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveValidationErrorFor(x => x.Status);
    }

    [Fact]
    public void Should_Not_Have_Error_When_Command_Is_Valid()
    {
        var command = new ChangeLeadStatusCommand(Guid.NewGuid(), "InContact");
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveAnyValidationErrors();
    }
}
