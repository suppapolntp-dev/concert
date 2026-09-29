namespace TicketWorld.Domain.Exceptions;

public class ForbiddenException : DomainException
{
    public ForbiddenException() : base() { }
    public ForbiddenException(string message) : base(message) { }
    public ForbiddenException(string message, Exception innerException) : base(message, innerException) { }
}
