using System;
using System.Collections.Generic;
using GestAuto.Commercial.Domain.Exceptions;

namespace GestAuto.Commercial.Domain.ValueObjects;

public class Money : ValueObject
{
    public decimal Amount { get; }
    public string Currency { get; }

    public static Money Zero => new(0);

    public Money(decimal amount, string currency = "BRL")
    {
        if (amount < 0)
            throw new DomainException("Valor não pode ser negativo");

        Amount = Math.Round(amount, 2);
        Currency = currency;
    }

    public static Money operator +(Money a, Money b)
    {
        ValidateSameCurrency(a, b);
        return new Money(a.Amount + b.Amount, a.Currency);
    }

    public static Money operator -(Money a, Money b)
    {
        ValidateSameCurrency(a, b);
        return new Money(a.Amount - b.Amount, a.Currency);
    }

    public static Money operator *(Money a, decimal multiplier)
    {
        return new Money(a.Amount * multiplier, a.Currency);
    }

    public static bool operator >(Money a, Money b)
    {
        ValidateSameCurrency(a, b);
        return a.Amount > b.Amount;
    }

    public static bool operator <(Money a, Money b)
    {
        ValidateSameCurrency(a, b);
        return a.Amount < b.Amount;
    }

    private static void ValidateSameCurrency(Money a, Money b)
    {
        if (a.Currency != b.Currency)
            throw new DomainException("Não é possível operar valores em moedas diferentes");
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Amount;
        yield return Currency;
    }

    public override string ToString() => $"{Currency} {Amount:N2}";
}
