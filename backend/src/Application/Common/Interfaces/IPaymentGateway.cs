namespace TicketWorld.Application.Common.Interfaces;

public sealed record PaymentIntentResult(
    string PaymentIntentId,
    string ClientSecret,
    decimal Amount,
    string Currency,
    string Status);

public sealed record RefundResult(
    string RefundId,
    string PaymentIntentId,
    decimal Amount,
    string Status);

public interface IPaymentGateway
{
    Task<PaymentIntentResult> CreatePaymentIntentAsync(
        decimal amount,
        string currency,
        string bookingReference,
        string idempotencyKey,
        CancellationToken cancellationToken = default);

    Task<RefundResult> ProcessRefundAsync(
        string paymentIntentId,
        decimal amount,
        string reason,
        CancellationToken cancellationToken = default);

    bool VerifyWebhookSignature(string payload, string signatureHeader, string webhookSecret);
}
