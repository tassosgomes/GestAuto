using GestAuto.Commercial.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace GestAuto.Commercial.Infra.ValueObjectConverters;

public class NullableMoneyConverter : ValueConverter<Money?, decimal?>
{
    public NullableMoneyConverter()
        : base(
            v => v != null ? v.Amount : (decimal?)null,
            v => v.HasValue ? new Money(v.Value, "BRL") : null)
    {
    }
}
