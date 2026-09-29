namespace TicketWorld.Domain.Entities;

using TicketWorld.Domain.Common;
using TicketWorld.Domain.Exceptions;

/// <summary>
/// Immutable audit trail record of security-critical and financial actions.
/// Mapped to the 'audit_logs' table in PostgreSQL.
/// </summary>
public class AuditLog : BaseEntity
{
    public Guid? ActorId { get; private set; }
    public string Action { get; private set; } = null!;
    public string EntityName { get; private set; } = null!;
    public string? EntityId { get; private set; }
    public string? OldValuesJson { get; private set; }
    public string? NewValuesJson { get; private set; }
    public string? IpAddress { get; private set; }
    public string? UserAgent { get; private set; }
    public DateTime CreatedAt { get; private set; }

    // Navigation Properties
    public virtual User? Actor { get; private set; }

    // Parameterless constructor for EF Core
    protected AuditLog() { }

    public static AuditLog Create(
        string action,
        string entityName,
        string? entityId = null,
        Guid? actorId = null,
        string? oldValuesJson = null,
        string? newValuesJson = null,
        string? ipAddress = null,
        string? userAgent = null)
    {
        if (string.IsNullOrWhiteSpace(action))
            throw new DomainException("Action is required.");

        if (string.IsNullOrWhiteSpace(entityName))
            throw new DomainException("EntityName is required.");

        return new AuditLog
        {
            Id = Guid.NewGuid(),
            ActorId = actorId,
            Action = action.Trim(),
            EntityName = entityName.Trim(),
            EntityId = entityId?.Trim(),
            OldValuesJson = oldValuesJson,
            NewValuesJson = newValuesJson,
            IpAddress = ipAddress?.Trim(),
            UserAgent = userAgent?.Trim(),
            CreatedAt = DateTime.UtcNow
        };
    }
}
