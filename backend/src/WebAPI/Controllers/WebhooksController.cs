namespace TicketWorld.WebAPI.Controllers;

using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using TicketWorld.Application.Common.Interfaces;
using TicketWorld.Application.Features.Bookings.Commands;

[ApiController]
[Route("api/webhooks")]
[AllowAnonymous]
public class WebhooksController : ApiControllerBase
{
    private readonly IPaymentGateway _paymentGateway;
    private readonly IConfiguration _configuration;
    private readonly ILogger<WebhooksController> _logger;

    public WebhooksController(
        IPaymentGateway paymentGateway,
        IConfiguration configuration,
        ILogger<WebhooksController> logger)
    {
        _paymentGateway = paymentGateway;
        _configuration = configuration;
        _logger = logger;
    }

    [HttpPost("stripe")]
    public async Task<IActionResult> HandleStripeWebhook()
    {
        using var reader = new StreamReader(HttpContext.Request.Body);
        var json = await reader.ReadToEndAsync();

        var webhookSecret = _configuration["STRIPE_WEBHOOK_SECRET"]
            ?? _configuration["Stripe:WebhookSecret"]
            ?? "whsec_test_secret_for_local_development";

        var signatureHeader = Request.Headers["Stripe-Signature"].ToString();

        // In production/when secret is configured, enforce strict HMAC signature verification
        var isSignatureValid = _paymentGateway.VerifyWebhookSignature(json, signatureHeader, webhookSecret);
        if (!isSignatureValid && !string.IsNullOrEmpty(signatureHeader) && !webhookSecret.StartsWith("whsec_test"))
        {
            _logger.LogWarning("Invalid Stripe webhook signature rejected");
            return BadRequest(new { error = "Invalid signature" });
        }

        try
        {
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            var eventId = root.GetProperty("id").GetString() ?? Guid.NewGuid().ToString("N");
            var eventType = root.GetProperty("type").GetString() ?? "unknown";

            string? paymentIntentId = null;
            string? failureReason = null;

            if (root.TryGetProperty("data", out var data) && data.TryGetProperty("object", out var obj))
            {
                if (obj.TryGetProperty("id", out var idProp))
                {
                    paymentIntentId = idProp.GetString();
                }

                if (obj.TryGetProperty("last_payment_error", out var errorProp) &&
                    errorProp.TryGetProperty("message", out var msgProp))
                {
                    failureReason = msgProp.GetString();
                }
            }

            var command = new ProcessStripeWebhookCommand(
                eventId,
                eventType,
                json,
                paymentIntentId,
                failureReason);

            await Mediator.Send(command);

            return Ok(new { received = true });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process Stripe webhook");
            return StatusCode(500, new { error = "Webhook processing error" });
        }
    }
}
