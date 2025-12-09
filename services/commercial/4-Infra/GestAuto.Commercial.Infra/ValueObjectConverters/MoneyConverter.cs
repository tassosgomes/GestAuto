using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using GestAuto.Commercial.Domain.ValueObjects;

namespace GestAuto.Commercial.Infra.ValueObjectConverters;

public class MoneyConverter : ValueConverter<Money, decimal>
{
    public MoneyConverter() 
        : base(
            v => v.Amount,
            v => new Money(v, "BRL"))
    {
    }
}