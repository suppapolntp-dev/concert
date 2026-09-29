namespace TicketWorld.Domain.Entities;

using TicketWorld.Domain.Common;
using TicketWorld.Domain.Exceptions;

/// <summary>
/// Dynamic system-wide runtime configuration key-value store.
/// Mapped to the 'system_configs' table in PostgreSQL.
/// </summary>
public class SystemConfig : BaseEntity<string>
{
    public string Key => Id;
    public string ValueJson { get; private set; } = null!;
    public string? Description { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    // Parameterless constructor for EF Core
    protected SystemConfig() { }

    public static SystemConfig Create(string key, string valueJson, string? description = null)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new DomainException("Config key is required.");

        if (string.IsNullOrWhiteSpace(valueJson))
            throw new DomainException("Config value JSON is required.");

        return new SystemConfig
        {
            Id = key.Trim(),
            ValueJson = valueJson.Trim(),
            Description = description?.Trim(),
            UpdatedAt = DateTime.UtcNow
        };
    }

    public void UpdateValue(string valueJson, string? description = null)
    {
        if (string.IsNullOrWhiteSpace(valueJson))
            throw new DomainException("Config value JSON cannot be empty.");

        ValueJson = valueJson.Trim();
        if (description != null)
        {
            Description = description.Trim();
        }
        UpdatedAt = DateTime.UtcNow;
    }
}
