using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using GestAuto.Commercial.Domain.ValueObjects;

namespace GestAuto.Commercial.Infra.ValueObjectConverters;

public class EmailConverter : ValueConverter<Email, string>
{
    public EmailConverter() 
        : base(
            v => v.Value,
            v => new Email(v))
    {
    }
}