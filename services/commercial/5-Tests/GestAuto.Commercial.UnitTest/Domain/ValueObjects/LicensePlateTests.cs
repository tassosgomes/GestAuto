using FluentAssertions;
using GestAuto.Commercial.Domain.Exceptions;
using GestAuto.Commercial.Domain.ValueObjects;
using Xunit;

namespace GestAuto.Commercial.UnitTest.Domain.ValueObjects;

public class LicensePlateTests
{
    [Theory]
    [InlineData("ABC-1234", false)]
    [InlineData("ABC1234", false)]
    [InlineData("ABC1C34", true)]
    public void Should_Create_LicensePlate_When_Valid(string input, bool isMercosul)
    {
        // Act
        var plate = new LicensePlate(input);

        // Assert
        plate.IsMercosul.Should().Be(isMercosul);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Should_Throw_Exception_When_Empty(string invalidPlate)
    {
        // Act
        Action act = () => new LicensePlate(invalidPlate);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("Placa não pode ser vazia");
    }

    [Theory]
    [InlineData("INVALID")]
    [InlineData("123-ABCD")]
    public void Should_Throw_Exception_When_Format_Invalid(string invalidPlate)
    {
        // Act
        Action act = () => new LicensePlate(invalidPlate);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("Formato de placa inválido. Use AAA-1234 ou AAA1A23 (Mercosul)");
    }

    [Fact]
    public void Should_Format_Correctly()
    {
        // Arrange
        var oldPlate = new LicensePlate("ABC1234");
        var mercosulPlate = new LicensePlate("ABC1C34");

        // Assert
        oldPlate.Formatted.Should().Be("ABC-1234");
        mercosulPlate.Formatted.Should().Be("ABC1C34");
    }
}
