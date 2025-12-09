using System;

namespace GestAuto.Commercial.Domain.ValueObjects;

public record Money(decimal Amount, string Currency = "BRL")
{
    public Money(decimal amount) : this(amount, "BRL") { }

    public static Money Zero => new Money(0);

    public Money Add(Money other)
    {
        if (Currency != other.Currency)
            throw new InvalidOperationException("Cannot add money with different currencies");

        return new Money(Amount + other.Amount, Currency);
    }

    public Money Subtract(Money other)
    {
        if (Currency != other.Currency)
            throw new InvalidOperationException("Cannot subtract money with different currencies");

        return new Money(Amount - other.Amount, Currency);
    }

    public Money Multiply(decimal factor) => new Money(Amount * factor, Currency);

    public bool IsPositive => Amount > 0;
    public bool IsNegative => Amount < 0;
    public bool IsZero => Amount == 0;
}