using FluentAssertions;
using GestAuto.Commercial.Domain.Exceptions;
using GestAuto.Commercial.Domain.ValueObjects;
using Xunit;

namespace GestAuto.Commercial.UnitTest.Domain.ValueObjects;

public class MoneyTests
{
    [Fact]
    public void Should_Create_Money_When_Valid()
    {
        // Act
        var money = new Money(100.50m);

        // Assert
        money.Amount.Should().Be(100.50m);
        money.Currency.Should().Be("BRL");
    }

    [Fact]
    public void Should_Throw_Exception_When_Negative()
    {
        // Act
        Action act = () => new Money(-10);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("Valor não pode ser negativo");
    }

    [Fact]
    public void Should_Add_Money_Correctly()
    {
        // Arrange
        var m1 = new Money(100);
        var m2 = new Money(50);

        // Act
        var result = m1 + m2;

        // Assert
        result.Amount.Should().Be(150);
    }

    [Fact]
    public void Should_Throw_Exception_When_Adding_Different_Currencies()
    {
        // Arrange
        var m1 = new Money(100, "BRL");
        var m2 = new Money(50, "USD");

        // Act
        Action act = () => { var _ = m1 + m2; };

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("Não é possível operar valores em moedas diferentes");
    }
}
