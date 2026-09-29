namespace TicketWorld.Domain.Exceptions;

public class NotFoundException : DomainException
{
    public NotFoundException() : base() { }
    public NotFoundException(string message) : base(message) { }
    public NotFoundException(string name, object key) : base($"Entity '{name}' ({key}) was not found.") { }
}
