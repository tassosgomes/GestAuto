using FluentAssertions;
using GestAuto.Commercial.Domain.Exceptions;
using GestAuto.Commercial.Domain.ValueObjects;
using Xunit;

namespace GestAuto.Commercial.UnitTest.Domain.ValueObjects;

public class PhoneTests
{
    [Theory]
    [InlineData("11999998888", "11", "999998888")] // 11 digits
    [InlineData("1133334444", "11", "33334444")]   // 10 digits
    [InlineData("(11) 99999-8888", "11", "999998888")]
    public void Should_Create_Phone_When_Valid(string input, string expectedDdd, string expectedNumber)
    {
        // Act
        var phone = new Phone(input);

        // Assert
        phone.DDD.Should().Be(expectedDdd);
        phone.Number.Should().Be(expectedNumber);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Should_Throw_Exception_When_Empty(string invalidPhone)
    {
        // Act
        Action act = () => new Phone(invalidPhone);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("Telefone não pode ser vazio");
    }

    [Theory]
    [InlineData("123")]
    [InlineData("119999988888")] // 12 digits
    public void Should_Throw_Exception_When_Length_Invalid(string invalidPhone)
    {
        // Act
        Action act = () => new Phone(invalidPhone);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("Telefone deve ter 10 ou 11 dígitos");
    }

    [Fact]
    public void Should_Format_Correctly()
    {
        // Arrange
        var phone11 = new Phone("11999998888");
        var phone10 = new Phone("1133334444");

        // Assert
        phone11.Formatted.Should().Be("(11) 99999-8888");
        phone10.Formatted.Should().Be("(11) 3333-4444");
    }
}
