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
    private readonly ILogger<WebhookController> _logger;

    public WebhookController(
        StripeSubscriptionService subscriptionService,
        IOptions<StripeSettings> stripeSettings,
        ILogger<WebhookController> logger)
    {
        _subscriptionService = subscriptionService;
        _stripeSettings = stripeSettings.Value;
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

        var subscriptionId = invoice.SubscriptionId;
        
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

        var subscriptionId = invoice.SubscriptionId;
        
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
}
