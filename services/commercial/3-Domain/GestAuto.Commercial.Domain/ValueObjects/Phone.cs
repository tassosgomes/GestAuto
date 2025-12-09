using System.Collections.Generic;
using System.Linq;
using GestAuto.Commercial.Domain.Exceptions;

namespace GestAuto.Commercial.Domain.ValueObjects;

public class Phone : ValueObject
{
    public string Value { get; }
    public string DDD { get; }
    public string Number { get; }

    public Phone(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException("Telefone não pode ser vazio");

        var cleanNumber = new string(value.Where(char.IsDigit).ToArray());

        if (cleanNumber.Length < 10 || cleanNumber.Length > 11)
            throw new DomainException("Telefone deve ter 10 ou 11 dígitos");

        DDD = cleanNumber[..2];
        Number = cleanNumber[2..];
        Value = cleanNumber;
    }

    public string Formatted => Number.Length == 9 
        ? $"({DDD}) {Number[..5]}-{Number[5..]}"
        : $"({DDD}) {Number[..4]}-{Number[4..]}";

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Formatted;
}
