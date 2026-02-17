namespace GroceryStore.Domain.Exceptions;

public class DomainException : Exception
{
    public DomainException(string message) : base(message) { }
}
public sealed class ValidationException : DomainException
{
    public ValidationException(string message) : base(message) { }
}