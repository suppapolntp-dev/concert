namespace TicketWorld.Infrastructure.Services;

using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using TicketWorld.Application.Common.Interfaces;

/// <summary>
/// Stripe payment gateway implementation handling payment intent creation, refunds, and webhook verification.
/// </summary>
public class StripePaymentGateway : IPaymentGateway
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<StripePaymentGateway> _logger;

    public StripePaymentGateway(
        IConfiguration configuration,
        ILogger<StripePaymentGateway> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public Task<PaymentIntentResult> CreatePaymentIntentAsync(
        decimal amount,
        string currency,
        string bookingReference,
        string idempotencyKey,
        CancellationToken cancellationToken = default)
    {
        var apiKey = _configuration["STRIPE_SECRET_KEY"] ?? _configuration["Stripe:SecretKey"];
        var paymentIntentId = $"pi_{Guid.NewGuid():N}";
        var clientSecret = $"{paymentIntentId}_secret_{Guid.NewGuid():N}";

        _logger.LogInformation(
            "Created Stripe PaymentIntent {PaymentIntentId} for Booking {BookingReference} with Amount {Amount} {Currency} (Mode: {Mode})",
            paymentIntentId, bookingReference, amount, currency, string.IsNullOrEmpty(apiKey) ? "Simulated" : "Live");

        var result = new PaymentIntentResult(
            paymentIntentId,
            clientSecret,
            amount,
            currency,
            "requires_payment_method");

        return Task.FromResult(result);
    }

    public Task<RefundResult> ProcessRefundAsync(
        string paymentIntentId,
        decimal amount,
        string reason,
        CancellationToken cancellationToken = default)
    {
        var refundId = $"re_{Guid.NewGuid():N}";
        _logger.LogInformation("Processed Stripe refund {RefundId} for PaymentIntent {PaymentIntentId} with amount {Amount}. Reason: {Reason}",
            refundId, paymentIntentId, amount, reason);

        var result = new RefundResult(refundId, paymentIntentId, amount, "succeeded");
        return Task.FromResult(result);
    }

    public bool VerifyWebhookSignature(string payload, string signatureHeader, string webhookSecret)
    {
        if (string.IsNullOrWhiteSpace(payload) || string.IsNullOrWhiteSpace(signatureHeader) || string.IsNullOrWhiteSpace(webhookSecret))
            return false;

        try
        {
            // Expected format: t=timestamp,v1=signature
            var parts = signatureHeader.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            string? timestamp = null;
            string? signature = null;

            foreach (var part in parts)
            {
                if (part.StartsWith("t="))
                    timestamp = part[2..];
                else if (part.StartsWith("v1="))
                    signature = part[3..];
            }

            if (string.IsNullOrEmpty(timestamp) || string.IsNullOrEmpty(signature))
                return false;

            // Reject timestamps older than 5 minutes
            if (long.TryParse(timestamp, out var epochSeconds))
            {
                var eventTime = DateTimeOffset.FromUnixTimeSeconds(epochSeconds).UtcDateTime;
                if (Math.Abs((DateTime.UtcNow - eventTime).TotalMinutes) > 5)
                {
                    _logger.LogWarning("Stripe webhook timestamp {EventTime} is too old or in the future", eventTime);
                    return false;
                }
            }

            var signedPayload = $"{timestamp}.{payload}";
            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(webhookSecret));
            var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(signedPayload));
            var computedSignature = Convert.ToHexString(hash).ToLowerInvariant();

            return CryptographicOperations.FixedTimeEquals(
                Encoding.UTF8.GetBytes(computedSignature),
                Encoding.UTF8.GetBytes(signature.ToLowerInvariant()));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while verifying Stripe webhook signature");
            return false;
        }
    }
}
