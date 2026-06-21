using LexiQuest.Core.Domain.Enums;
using LexiQuest.Infrastructure.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Stripe;

namespace LexiQuest.Api.Controllers;

[ApiController]
[Route("api/v1/webhooks")]
public class WebhookController : ControllerBase
{
    private readonly StripeSubscriptionService _subscriptionService;
    private readonly StripeSettings _stripeSettings;
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<WebhookController> _logger;

    public WebhookController(
        StripeSubscriptionService subscriptionService,
        IOptions<StripeSettings> stripeSettings,
        IWebHostEnvironment environment,
        ILogger<WebhookController> logger)
    {
        _subscriptionService = subscriptionService;
        _stripeSettings = stripeSettings.Value;
        _environment = environment;
        _logger = logger;
    }

    [HttpPost("stripe")]
    public async Task<IActionResult> HandleStripeWebhook()
    {
        try
        {
            using var reader = new StreamReader(Request.Body);
            var json = await reader.ReadToEndAsync();

            // Verify Stripe signature if webhook secret is configured
            Event stripeEvent;
            if (!string.IsNullOrEmpty(_stripeSettings.WebhookSecret) && 
                !IsTestWebhookSecret(_stripeSettings.WebhookSecret))
            {
                var signatureHeader = Request.Headers["Stripe-Signature"].ToString();
                stripeEvent = EventUtility.ConstructEvent(json, signatureHeader, _stripeSettings.WebhookSecret);
                _logger.LogInformation("Verified Stripe webhook signature for event {EventId}", stripeEvent.Id);
            }
            else
            {
                // In test mode, parse without signature verification
                stripeEvent = EventUtility.ParseEvent(json);
                _logger.LogInformation("Parsed Stripe webhook event (test mode): {EventType}", stripeEvent.Type);
            }

            await ProcessStripeEventAsync(stripeEvent);

            return Ok();
        }
        catch (StripeException ex)
        {
            _logger.LogError(ex, "Stripe webhook error: {Message}", ex.Message);
            return BadRequest(new { error = "Invalid Stripe webhook" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing Stripe webhook");
            return BadRequest(new { error = "Webhook processing failed" });
        }
    }

    [HttpPost("stripe/e2e")]
    public async Task<IActionResult> HandleE2EStripeWebhook([FromBody] E2EStripeWebhookRequest request)
    {
        if (!_environment.IsEnvironment("E2E"))
            return NotFound();

        if (string.IsNullOrWhiteSpace(request.Type))
            return BadRequest();

        switch (request.Type)
        {
            case "checkout.session.completed":
                if (string.IsNullOrWhiteSpace(request.StripeCustomerId) ||
                    string.IsNullOrWhiteSpace(request.StripeSubscriptionId))
                {
                    return BadRequest();
                }

                await _subscriptionService.HandleCheckoutCompletedAsync(
                    request.StripeCustomerId,
                    request.StripeSubscriptionId,
                    ParseE2EPlan(request.Plan));
                break;

            case "invoice.paid":
                if (string.IsNullOrWhiteSpace(request.StripeSubscriptionId))
                    return BadRequest();

                await _subscriptionService.HandleInvoicePaidAsync(
                    request.StripeSubscriptionId,
                    request.ExpiresAt ?? DateTime.UtcNow.AddMonths(1));
                break;

            case "invoice.payment_failed":
                if (string.IsNullOrWhiteSpace(request.StripeSubscriptionId))
                    return BadRequest();

                await _subscriptionService.HandleInvoiceFailedAsync(request.StripeSubscriptionId);
                break;

            case "customer.subscription.deleted":
                if (string.IsNullOrWhiteSpace(request.StripeSubscriptionId))
                    return BadRequest();

                await _subscriptionService.HandleSubscriptionCancelledAsync(request.StripeSubscriptionId);
                break;

            default:
                _logger.LogInformation("Unhandled E2E Stripe event type: {EventType}", request.Type);
                break;
        }

        return Ok();
    }

    private async Task ProcessStripeEventAsync(Event stripeEvent)
    {
        switch (stripeEvent.Type)
        {
            case "checkout.session.completed":
                await HandleCheckoutCompleted(stripeEvent);
                break;
            case "invoice.paid":
                await HandleInvoicePaid(stripeEvent);
                break;
            case "invoice.payment_failed":
                await HandleInvoiceFailed(stripeEvent);
                break;
            case "customer.subscription.deleted":
                await HandleSubscriptionCancelled(stripeEvent);
                break;
            default:
                _logger.LogInformation("Unhandled Stripe event type: {EventType}", stripeEvent.Type);
                break;
        }
    }

    private async Task HandleCheckoutCompleted(Event stripeEvent)
    {
        var session = stripeEvent.Data.Object as Stripe.Checkout.Session;
        if (session == null) return;

        var customerId = session.CustomerId;
        var subscriptionId = session.SubscriptionId;
        
        _logger.LogInformation("Processing checkout.session.completed for customer {CustomerId}", customerId);

        // Get plan from metadata or subscription
        var plan = ExtractPlanFromMetadata(session.Metadata);
        
        await _subscriptionService.HandleCheckoutCompletedAsync(customerId, subscriptionId, plan);
        _logger.LogInformation("Successfully processed checkout.completed for customer {CustomerId}", customerId);
    }

    private async Task HandleInvoicePaid(Event stripeEvent)
    {
        var invoice = stripeEvent.Data.Object as Stripe.Invoice;
        if (invoice == null) return;

        var subscriptionId = invoice.Parent?.SubscriptionDetails?.SubscriptionId;

        _logger.LogInformation("Processing invoice.paid for subscription {SubscriptionId}", subscriptionId);

        // Calculate new expiration date based on subscription interval
        var expiresAt = CalculateExpirationDate(invoice);

        await _subscriptionService.HandleInvoicePaidAsync(subscriptionId, expiresAt);
        _logger.LogInformation("Successfully processed invoice.paid for subscription {SubscriptionId}", subscriptionId);
    }

    private async Task HandleInvoiceFailed(Event stripeEvent)
    {
        var invoice = stripeEvent.Data.Object as Stripe.Invoice;
        if (invoice == null) return;

        var subscriptionId = invoice.Parent?.SubscriptionDetails?.SubscriptionId;
        
        _logger.LogWarning("Processing invoice.payment_failed for subscription {SubscriptionId}", subscriptionId);
        
        await _subscriptionService.HandleInvoiceFailedAsync(subscriptionId);
        _logger.LogInformation("Successfully processed invoice.payment_failed for subscription {SubscriptionId}", subscriptionId);
    }

    private async Task HandleSubscriptionCancelled(Event stripeEvent)
    {
        var subscription = stripeEvent.Data.Object as Stripe.Subscription;
        if (subscription == null) return;

        var subscriptionId = subscription.Id;
        
        _logger.LogInformation("Processing customer.subscription.deleted for subscription {SubscriptionId}", subscriptionId);
        
        await _subscriptionService.HandleSubscriptionCancelledAsync(subscriptionId);
        _logger.LogInformation("Successfully processed subscription.deleted for subscription {SubscriptionId}", subscriptionId);
    }

    private static SubscriptionPlan ExtractPlanFromMetadata(Dictionary<string, string>? metadata)
    {
        if (metadata != null && metadata.TryGetValue("Plan", out var planValue))
        {
            if (Enum.TryParse<SubscriptionPlan>(planValue, out var plan))
            {
                return plan;
            }
        }
        
        // Default to Monthly if not specified
        return SubscriptionPlan.Monthly;
    }

    private static DateTime CalculateExpirationDate(Stripe.Invoice invoice)
    {
        // Use period end from the invoice if available
        var periodEnd = invoice.Lines?.Data?.FirstOrDefault()?.Period?.End;
        if (periodEnd.HasValue)
        {
            return periodEnd.Value;
        }
        
        // Default: add 1 month
        return DateTime.UtcNow.AddMonths(1);
    }

    private static bool IsTestWebhookSecret(string webhookSecret)
    {
        return webhookSecret.Contains("dummy", StringComparison.OrdinalIgnoreCase) ||
               webhookSecret.Contains("test", StringComparison.OrdinalIgnoreCase);
    }

    private static SubscriptionPlan ParseE2EPlan(string? plan)
    {
        return Enum.TryParse<SubscriptionPlan>(plan, ignoreCase: true, out var parsed)
            ? parsed
            : SubscriptionPlan.Monthly;
    }

    public sealed record E2EStripeWebhookRequest(
        string Type,
        string? StripeCustomerId,
        string? StripeSubscriptionId,
        string? Plan,
        DateTime? ExpiresAt);
}
