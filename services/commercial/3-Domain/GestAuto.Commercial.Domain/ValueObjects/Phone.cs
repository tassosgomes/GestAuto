using System.Text.RegularExpressions;

namespace GestAuto.Commercial.Domain.ValueObjects;

public record Phone
{
    private static readonly Regex PhoneRegex = new(
        @"^\(?([0-9]{2})\)?[-. ]?([0-9]{4,5})[-. ]?([0-9]{4})$",
        RegexOptions.Compiled);

    public string Value { get; init; }

    public Phone(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Phone cannot be empty", nameof(value));

        var cleanValue = Regex.Replace(value, @"[^\d]", "");
        if (cleanValue.Length < 10 || cleanValue.Length > 11)
            throw new ArgumentException("Invalid phone number format", nameof(value));

        Value = cleanValue;
    }

    public string Formatted => Value.Length == 11
        ? $"({Value[..2]}) {Value[2..7]}-{Value[7..]}"
        : $"({Value[..2]}) {Value[2..6]}-{Value[6..]}";

    public bool IsMobile => Value.Length == 11;
    public bool IsLandline => Value.Length == 10;
}