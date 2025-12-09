using FluentAssertions;
using GestAuto.Commercial.Domain.Exceptions;
using GestAuto.Commercial.Domain.ValueObjects;
using Xunit;

namespace GestAuto.Commercial.UnitTest.Domain.ValueObjects;

public class EmailTests
{
    [Theory]
    [InlineData("test@example.com")]
    [InlineData("user.name@domain.co.uk")]
    [InlineData("user+tag@example.com")]
    public void Should_Create_Email_When_Valid(string validEmail)
    {
        // Act
        var email = new Email(validEmail);

        // Assert
        email.Value.Should().Be(validEmail.ToLowerInvariant());
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Should_Throw_Exception_When_Empty(string? invalidEmail)
    {
        // Act
        Action act = () => new Email(invalidEmail!);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("Email não pode ser vazio");
    }

    [Theory]
    [InlineData("invalid-email")]
    [InlineData("user@")]
    [InlineData("@domain.com")]
    public void Should_Throw_Exception_When_Format_Invalid(string invalidEmail)
    {
        // Act
        Action act = () => new Email(invalidEmail);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("Email inválido");
    }

    [Fact]
    public void Should_Be_Equal_When_Values_Are_Equal()
    {
        // Arrange
        var email1 = new Email("test@example.com");
        var email2 = new Email("TEST@example.com");

        // Assert
        email1.Should().Be(email2);
        (email1 == email2).Should().BeTrue();
    }
}
