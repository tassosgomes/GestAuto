namespace GestAuto.Stock.Domain.Exceptions;

public sealed class ConflictException : DomainException
{
    public ConflictException(string message) : base(message)
    {
    }
}
