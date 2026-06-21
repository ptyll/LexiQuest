using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using LexiQuest.Shared.DTOs.Premium;
using Microsoft.Playwright;
using static Microsoft.Playwright.Assertions;

namespace LexiQuest.E2E.Tests;

[Trait("Category", "E2E")]
[Trait("Category", "Full")]
[Trait("Category", "Visual")]
[Collection(E2ECollection.Name)]
public class PremiumE2ETests : E2ETestBase
{
    public PremiumE2ETests(E2EEnvironmentFixture fixture) : base(fixture)
    {
    }

    [Fact]
    public async Task Premium_Page_ShowsPlansBestValueAndLockedFeaturesForFreeUser()
    {
        await RunScenarioAsync("premium", "overview-free-locked-features", async page =>
        {
            var user = await Fixture.RegisterUniqueUserAsync("premiumfree");
            await Fixture.LoginAsAsync(page, user);
            await Fixture.GoToAndWaitForAppReadyAsync(page, "/premium");

            await Expect(page.GetByTestId(Selectors.Premium.Page)).ToBeVisibleAsync(new() { Timeout = 10_000 });
            await Expect(page.GetByTestId(Selectors.Premium.MonthlyCard)).ToBeVisibleAsync();
            await Expect(page.GetByTestId(Selectors.Premium.YearlyCard)).ToBeVisibleAsync();
            await Expect(page.GetByTestId(Selectors.Premium.LifetimeCard)).ToBeVisibleAsync();
            await Expect(page.GetByTestId(Selectors.Premium.BestValueBadge)).ToContainTextAsync("NEJLEPŠÍ HODNOTA");
            await Expect(page.GetByTestId(Selectors.Premium.FeatureAvailability)).ToBeVisibleAsync();
            await Expect(page.GetByTestId(Selectors.Premium.LockedFeature).First).ToBeVisibleAsync();
            await Expect(page.GetByTestId(Selectors.Premium.LockedFeature).First).ToHaveAttributeAsync(
                "title",
                "Tato funkce je dostupná s Premium.");
            await Expect(page.GetByText("Premium_")).Not.ToBeVisibleAsync();

            await Fixture.TakeCheckpointScreenshotAsync(
                page,
                area: "premium",
                scenario: "overview-free-locked-features",
                state: "free",
                viewport: "1366x900",
                theme: "light",
                persona: "freeUser");
        });
    }

    [Fact]
    public async Task Premium_FakeCheckoutSuccess_ActivatesPremiumAndShowsActiveBadge()
    {
        await RunScenarioAsync("premium", "fake-checkout-success-activates", async page =>
        {
            var user = await Fixture.RegisterUniqueUserAsync("premiumcheckout");
            await Fixture.LoginAsAsync(page, user);
            await Fixture.GoToAndWaitForAppReadyAsync(page, "/premium");

            await ClickButtonInAsync(page.GetByTestId(Selectors.Premium.SubscribeYearly));
            await page.WaitForURLAsync("**/premium/success**", new() { Timeout = 15_000 });

            await Expect(page.GetByTestId(Selectors.Premium.CheckoutSuccess)).ToBeVisibleAsync();
            await Expect(page.GetByRole(AriaRole.Heading, new() { Name = "Platba úspěšná!" })).ToBeVisibleAsync();

            await Fixture.TakeCheckpointScreenshotAsync(
                page,
                area: "premium",
                scenario: "fake-checkout-success-activates",
                state: "success-page",
                viewport: "1366x900",
                theme: "light",
                persona: "premiumUser",
                fullPage: false);

            var status = await GetPremiumStatusAsync(user);
            status.IsActive.Should().BeTrue();
            status.Plan.Should().Be(SubscriptionPlan.Yearly);

            await Fixture.GoToAndWaitForAppReadyAsync(page, "/premium");
            await Expect(page.GetByTestId(Selectors.Premium.ActiveBadge)).ToContainTextAsync("Roční");

            await Fixture.GoToAndWaitForAppReadyAsync(page, "/profile");
            await Expect(page.GetByTestId(Selectors.Profile.PremiumBadge)).ToContainTextAsync("Roční");

            await Fixture.GoToAndWaitForAppReadyAsync(page, "/premium");

            await Fixture.TakeCheckpointScreenshotAsync(
                page,
                area: "premium",
                scenario: "fake-checkout-success-activates",
                state: "active",
                viewport: "1366x900",
                theme: "light",
                persona: "premiumUser");
        }, assertNoFailedRequests: false);
    }

    [Theory]
    [InlineData(SubscriptionPlan.Monthly)]
    [InlineData(SubscriptionPlan.Yearly)]
    [InlineData(SubscriptionPlan.Lifetime)]
    public async Task Premium_CheckoutPlans_ReturnLocalFakeRedirect(SubscriptionPlan plan)
    {
        await RunScenarioAsync("premium", $"checkout-{plan.ToString().ToLowerInvariant()}-fake-redirect", async _ =>
        {
            var user = await Fixture.RegisterUniqueUserAsync($"premium{plan.ToString().ToLowerInvariant()}");
            using var apiClient = await Fixture.CreateAuthenticatedApiClientAsync(user);
            using var response = await apiClient.PostAsJsonAsync(
                "api/v1/premium/checkout",
                new CreateCheckoutRequest(plan));

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var checkout = await response.Content.ReadFromJsonAsync<CheckoutResponse>();
            checkout.Should().NotBeNull();
            checkout!.StripeCheckoutUrl.Should().StartWith(Fixture.WebBaseUrl);
            checkout.StripeCheckoutUrl.Should().Contain("/premium/success");
            checkout.StripeCheckoutUrl.Should().Contain("session_id=cs_test_");
            checkout.StripeCheckoutUrl.Should().Contain($"plan={plan}");
            checkout.StripeCheckoutUrl.Should().Contain("e2e=true");
        }, assertNoConsoleErrors: false, assertNoFailedRequests: false);
    }

    [Fact]
    public async Task Premium_CheckoutCancel_DoesNotActivatePremium()
    {
        await RunScenarioAsync("premium", "checkout-cancel-no-activation", async page =>
        {
            var user = await Fixture.RegisterUniqueUserAsync("premiumcancel");
            await Fixture.LoginAsAsync(page, user);
            await Fixture.GoToAndWaitForAppReadyAsync(page, "/premium/cancel");

            await Expect(page.GetByTestId(Selectors.Premium.CheckoutCancel)).ToBeVisibleAsync();
            await Expect(page.GetByRole(AriaRole.Heading, new() { Name = "Platba zrušena" })).ToBeVisibleAsync();

            var status = await GetPremiumStatusAsync(user);
            status.IsActive.Should().BeFalse();

            await Fixture.TakeCheckpointScreenshotAsync(
                page,
                area: "premium",
                scenario: "checkout-cancel-no-activation",
                state: "cancel",
                viewport: "1366x900",
                theme: "light",
                persona: "freeUser",
                fullPage: false);
        });
    }

    [Fact]
    public async Task Premium_CancelAndExpiredSubscription_UpdateDisplayedStatus()
    {
        await RunScenarioAsync("premium", "cancel-and-expired-status", async page =>
        {
            var user = await Fixture.RegisterUniqueUserAsync("premiumstatus");
            await Fixture.ForceUserPremiumAsync(user.Email);
            await Fixture.LoginAsAsync(page, user);
            await Fixture.GoToAndWaitForAppReadyAsync(page, "/premium");

            await Expect(page.GetByTestId(Selectors.Premium.ActiveBadge)).ToBeVisibleAsync();
            await ClickButtonInAsync(page.GetByTestId(Selectors.Premium.CancelSubscription));
            await Expect(page.GetByText("Premium bylo zrušeno.")).ToBeVisibleAsync(new() { Timeout = 10_000 });

            var status = await GetPremiumStatusAsync(user);
            status.IsActive.Should().BeFalse();
            status.Status.Should().Be(SubscriptionStatus.Cancelled);

            var expiredUser = await Fixture.RegisterUniqueUserAsync("premiumexpired");
            await Fixture.ForceUserPremiumAsync(expiredUser.Email, expiresAtUtc: DateTime.UtcNow.AddDays(-1));
            using var expiredClient = await Fixture.CreateAuthenticatedApiClientAsync(expiredUser);
            using var response = await expiredClient.GetAsync("api/v1/premium/status");
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var expiredStatus = await response.Content.ReadFromJsonAsync<SubscriptionStatusDto>();
            expiredStatus.Should().NotBeNull();
            expiredStatus!.IsActive.Should().BeFalse();
            expiredStatus.Status.Should().Be(SubscriptionStatus.Expired);
        }, assertNoFailedRequests: false);
    }

    [Fact]
    public async Task Premium_StripeWebhookCheckoutCompleted_ActivatesSubscription()
    {
        await RunScenarioAsync("premium", "stripe-webhook-checkout-completed", async _ =>
        {
            var user = await Fixture.RegisterUniqueUserAsync("premiumwebhookcompleted");
            var customerId = $"cus_e2e_{Guid.NewGuid():N}";
            var subscriptionId = $"sub_e2e_{Guid.NewGuid():N}";
            await Fixture.SetStripeCustomerIdAsync(user.Email, customerId);

            using var apiClient = new HttpClient { BaseAddress = new Uri(Fixture.ApiBaseUrl) };
            using var response = await apiClient.PostAsJsonAsync("api/v1/webhooks/stripe/e2e", new
            {
                Type = "checkout.session.completed",
                StripeCustomerId = customerId,
                StripeSubscriptionId = subscriptionId,
                Plan = "Yearly"
            });

            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var status = await GetPremiumStatusAsync(user);
            status.IsActive.Should().BeTrue();
            status.Plan.Should().Be(SubscriptionPlan.Yearly);
        }, assertNoConsoleErrors: false, assertNoFailedRequests: false);
    }

    [Fact]
    public async Task Premium_StripeWebhookInvoicePaid_ExtendsSubscription()
    {
        await RunScenarioAsync("premium", "stripe-webhook-invoice-paid", async _ =>
        {
            var user = await Fixture.RegisterUniqueUserAsync("premiumwebhookpaid");
            var subscriptionId = $"sub_e2e_{Guid.NewGuid():N}";
            var newExpiresAt = DateTime.UtcNow.AddDays(45);
            await Fixture.ForceUserPremiumAsync(
                user.Email,
                expiresAtUtc: DateTime.UtcNow.AddDays(5),
                premiumPlan: "Monthly",
                subscriptionPlan: "Monthly",
                stripeSubscriptionId: subscriptionId);

            using var apiClient = new HttpClient { BaseAddress = new Uri(Fixture.ApiBaseUrl) };
            using var response = await apiClient.PostAsJsonAsync("api/v1/webhooks/stripe/e2e", new
            {
                Type = "invoice.paid",
                StripeSubscriptionId = subscriptionId,
                ExpiresAt = newExpiresAt
            });

            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var status = await GetPremiumStatusAsync(user);
            status.IsActive.Should().BeTrue();
            status.Status.Should().Be(SubscriptionStatus.Active);
            status.ExpiresAt.Should().BeAfter(DateTime.UtcNow.AddDays(40));
        }, assertNoConsoleErrors: false, assertNoFailedRequests: false);
    }

    [Fact]
    public async Task Premium_StripeWebhookInvoiceFailed_MarksPastDue()
    {
        await RunScenarioAsync("premium", "stripe-webhook-invoice-failed", async _ =>
        {
            var user = await Fixture.RegisterUniqueUserAsync("premiumwebhookfailed");
            var subscriptionId = $"sub_e2e_{Guid.NewGuid():N}";
            await Fixture.ForceUserPremiumAsync(
                user.Email,
                premiumPlan: "Monthly",
                subscriptionPlan: "Monthly",
                stripeSubscriptionId: subscriptionId);

            using var apiClient = new HttpClient { BaseAddress = new Uri(Fixture.ApiBaseUrl) };
            using var response = await apiClient.PostAsJsonAsync("api/v1/webhooks/stripe/e2e", new
            {
                Type = "invoice.payment_failed",
                StripeSubscriptionId = subscriptionId
            });

            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var status = await GetPremiumStatusAsync(user);
            status.IsActive.Should().BeFalse();
            status.Status.Should().Be(SubscriptionStatus.PastDue);
        }, assertNoConsoleErrors: false, assertNoFailedRequests: false);
    }

    [Fact]
    public async Task Premium_StripeWebhookSubscriptionCancelled_MarksCancelled()
    {
        await RunScenarioAsync("premium", "stripe-webhook-subscription-cancelled", async _ =>
        {
            var user = await Fixture.RegisterUniqueUserAsync("premiumwebhookcancelled");
            var subscriptionId = $"sub_e2e_{Guid.NewGuid():N}";
            await Fixture.ForceUserPremiumAsync(
                user.Email,
                premiumPlan: "Monthly",
                subscriptionPlan: "Monthly",
                stripeSubscriptionId: subscriptionId);

            using var apiClient = new HttpClient { BaseAddress = new Uri(Fixture.ApiBaseUrl) };
            using var response = await apiClient.PostAsJsonAsync("api/v1/webhooks/stripe/e2e", new
            {
                Type = "customer.subscription.deleted",
                StripeSubscriptionId = subscriptionId
            });

            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var status = await GetPremiumStatusAsync(user);
            status.IsActive.Should().BeFalse();
            status.Status.Should().Be(SubscriptionStatus.Cancelled);
        }, assertNoConsoleErrors: false, assertNoFailedRequests: false);
    }

    [Fact]
    public async Task Premium_ExpiryReminderEmail_IsCapturedBySmtp4Dev()
    {
        await RunScenarioAsync("premium", "expiry-reminder-email-smtp4dev", async page =>
        {
            var user = await Fixture.RegisterUniqueUserAsync("premiumexpiryemail");
            await Fixture.Smtp4Dev.ClearMessagesAsync();

            await Fixture.ForceUserPremiumAsync(
                user.Email,
                expiresAtUtc: DateTime.UtcNow.AddDays(2).AddHours(12),
                premiumPlan: "Monthly",
                subscriptionPlan: "Monthly");

            await RunPremiumExpiryReminderJobAsync();

            var emailText = await Fixture.Smtp4Dev.WaitForMessageTextAsync(
                text => text.Contains(user.Email, StringComparison.OrdinalIgnoreCase)
                    && text.Contains("Premium brzy vyprší", StringComparison.Ordinal)
                    && text.Contains("prémiové funkce", StringComparison.Ordinal),
                TimeSpan.FromSeconds(10));
            emailText.Should().Contain(user.Email);
            emailText.Should().Contain("Premium brzy vyprší");

            await Fixture.LoginAsAsync(page, user);
            await Fixture.GoToAndWaitForAppReadyAsync(page, "/premium");
            await Expect(page.GetByTestId(Selectors.Premium.ActiveBadge)).ToBeVisibleAsync(new() { Timeout = 10_000 });
            await Expect(page.GetByTestId(Selectors.Premium.ActiveBadge)).ToContainTextAsync("Měsíční");

            await Fixture.TakeCheckpointScreenshotAsync(
                page,
                area: "premium",
                scenario: "expiry-reminder-email-smtp4dev",
                state: "active-expiring-soon",
                viewport: "1366x900",
                theme: "light",
                persona: "expiringPremiumUser");
        }, assertNoConsoleErrors: false, assertNoFailedRequests: false);
    }

    [Fact]
    public async Task Premium_StripeWebhookInvalidSignature_IsRejected()
    {
        await RunScenarioAsync("premium", "stripe-webhook-invalid-signature", async _ =>
        {
            using var apiClient = new HttpClient { BaseAddress = new Uri(Fixture.ApiBaseUrl) };
            using var request = new HttpRequestMessage(HttpMethod.Post, "api/v1/webhooks/stripe")
            {
                Content = JsonContent.Create(new
                {
                    id = "evt_e2e_invalid_signature",
                    @object = "event",
                    type = "checkout.session.completed",
                    data = new { @object = new { } }
                })
            };
            request.Headers.Add("Stripe-Signature", "t=1,v1=invalid");

            using var response = await apiClient.SendAsync(request);

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }, assertNoConsoleErrors: false, assertNoFailedRequests: false);
    }

    private async Task<SubscriptionStatusDto> GetPremiumStatusAsync(TestUser user)
    {
        using var apiClient = await Fixture.CreateAuthenticatedApiClientAsync(user);
        using var response = await apiClient.GetAsync("api/v1/premium/status");
        response.EnsureSuccessStatusCode();

        var status = await response.Content.ReadFromJsonAsync<SubscriptionStatusDto>();
        status.Should().NotBeNull();
        return status!;
    }

    private async Task RunPremiumExpiryReminderJobAsync()
    {
        using var httpClient = new HttpClient { BaseAddress = new Uri(Fixture.ApiBaseUrl) };
        using var response = await httpClient.PostAsync("api/v1/e2e/premium/run-expiry-reminders", null);
        response.EnsureSuccessStatusCode();
    }

    private static async Task ClickButtonInAsync(ILocator locator)
    {
        await locator.GetByRole(AriaRole.Button).ClickAsync();
    }
}
