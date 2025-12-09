using System.Collections.Generic;
using System.Text.RegularExpressions;
using GestAuto.Commercial.Domain.Exceptions;

namespace GestAuto.Commercial.Domain.ValueObjects;

public class LicensePlate : ValueObject
{
    public string Value { get; }
    public bool IsMercosul { get; }

    private static readonly Regex OldPatternRegex = new(@"^[A-Z]{3}[0-9]{4}$", RegexOptions.Compiled);
    private static readonly Regex MercosulPatternRegex = new(@"^[A-Z]{3}[0-9][A-Z][0-9]{2}$", RegexOptions.Compiled);

    public LicensePlate(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException("Placa não pode ser vazia");

        var cleanValue = value.ToUpperInvariant().Replace("-", "").Replace(" ", "");

        if (OldPatternRegex.IsMatch(cleanValue))
        {
            Value = cleanValue;
            IsMercosul = false;
        }
        else if (MercosulPatternRegex.IsMatch(cleanValue))
        {
            Value = cleanValue;
            IsMercosul = true;
        }
        else
        {
            throw new DomainException("Formato de placa inválido. Use AAA-1234 ou AAA1A23 (Mercosul)");
        }
    }

    public string Formatted => IsMercosul 
        ? Value 
        : $"{Value[..3]}-{Value[3..]}";

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Formatted;
}
