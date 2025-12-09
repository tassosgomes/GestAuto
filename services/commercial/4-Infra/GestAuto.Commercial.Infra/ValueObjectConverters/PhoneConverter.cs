using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using GestAuto.Commercial.Domain.ValueObjects;

namespace GestAuto.Commercial.Infra.ValueObjectConverters;

public class PhoneConverter : ValueConverter<Phone, string>
{
    public PhoneConverter() 
        : base(
            v => v.Value,
            v => new Phone(v))
    {
    }
}