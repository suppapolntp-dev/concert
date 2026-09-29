namespace TicketWorld.Domain.Entities;

using TicketWorld.Domain.Common;
using TicketWorld.Domain.Enums;
using TicketWorld.Domain.Exceptions;

/// <summary>
/// Idempotent ingestion ledger for incoming payment gateway webhooks.
/// Mapped to the 'webhook_events' table in PostgreSQL.
/// </summary>
public class WebhookEvent : BaseEntity, IAuditableEntity
{
    public PaymentGateway Gateway { get; private set; } = PaymentGateway.Stripe;
    public string EventId { get; private set; } = null!;
    public string EventType { get; private set; } = null!;
    public string PayloadJson { get; private set; } = null!;
    public bool IsProcessed { get; private set; }
    public DateTime? ProcessedAt { get; private set; }
    public string? ErrorMessage { get; private set; }
    public int RetryCount { get; private set; }

    // IAuditableEntity
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Parameterless constructor for EF Core
    protected WebhookEvent() { }

    public static WebhookEvent Create(
        PaymentGateway gateway,
        string eventId,
        string eventType,
        string payloadJson)
    {
        if (string.IsNullOrWhiteSpace(eventId))
            throw new DomainException("Gateway event ID is required.");

        if (string.IsNullOrWhiteSpace(eventType))
            throw new DomainException("Event type is required.");

        if (string.IsNullOrWhiteSpace(payloadJson))
            throw new DomainException("Payload JSON is required.");

        return new WebhookEvent
        {
            Id = Guid.NewGuid(),
            Gateway = gateway,
            EventId = eventId.Trim(),
            EventType = eventType.Trim(),
            PayloadJson = payloadJson.Trim(),
            IsProcessed = false,
            RetryCount = 0,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    public void MarkProcessed()
    {
        IsProcessed = true;
        ProcessedAt = DateTime.UtcNow;
        ErrorMessage = null;
        UpdatedAt = DateTime.UtcNow;
    }

    public void RecordFailure(string errorMessage)
    {
        RetryCount++;
        ErrorMessage = errorMessage.Trim();
        UpdatedAt = DateTime.UtcNow;
    }
}
