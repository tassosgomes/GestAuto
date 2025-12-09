using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using GestAuto.Commercial.Domain.ValueObjects;

namespace GestAuto.Commercial.Infra.ValueObjectConverters;

public class LicensePlateConverter : ValueConverter<LicensePlate, string>
{
    public LicensePlateConverter() 
        : base(
            v => v.Value,
            v => new LicensePlate(v))
    {
    }
}