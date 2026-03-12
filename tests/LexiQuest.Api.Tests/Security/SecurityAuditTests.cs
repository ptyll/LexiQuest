using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;
using FluentAssertions;
using Microsoft.IdentityModel.Tokens;

namespace LexiQuest.Api.Tests.Security;

[Trait("Category", "Security")]
public class SecurityAuditTests : IClassFixture<SecurityAuditTests.SecurityTestFactory>
{
    private readonly HttpClient _client;

    public class SecurityTestFactory : CustomWebApplicationFactory
    {
        public SecurityTestFactory() : base($"SecurityAuditDb_{Guid.NewGuid():N}") { }
    }

    public SecurityAuditTests(SecurityTestFactory factory)
    {
        _client = factory.CreateClient();
    }

    // ──────────────────────────────────────────────────────────────────
    // Authentication Tests
    // ──────────────────────────────────────────────────────────────────

    [Fact]
    public async Task ProtectedEndpoint_WithoutToken_Returns401()
    {
        var response = await _client.GetAsync("/api/v1/users/me");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task ProtectedEndpoint_WithInvalidToken_Returns401()
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, "/api/v1/users/me");
        request.Headers.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "invalid.token.here");

        var response = await _client.SendAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task ProtectedEndpoint_WithExpiredToken_Returns401()
    {
        var token = GenerateJwtToken(
            userId: Guid.NewGuid(),
            role: "User",
            expiresAt: DateTime.UtcNow.AddHours(-1));

        using var request = new HttpRequestMessage(HttpMethod.Get, "/api/v1/users/me");
        request.Headers.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var response = await _client.SendAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task ProtectedEndpoint_WithWrongSigningKey_Returns401()
    {
        var wrongKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes("Wrong-Key-That-Is-Long-Enough-For-HS256-Algorithm-!!!!!!!!"));
        var credentials = new SigningCredentials(wrongKey, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: "TestIssuer",
            audience: "TestAudience",
            claims: new[] { new Claim("sub", Guid.NewGuid().ToString()) },
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: credentials);

        var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

        using var request = new HttpRequestMessage(HttpMethod.Get, "/api/v1/users/me");
        request.Headers.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", tokenString);

        var response = await _client.SendAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task ProtectedEndpoint_WithMalformedBearerToken_Returns401()
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, "/api/v1/users/me");
        request.Headers.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "not-a-jwt");

        var response = await _client.SendAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // ──────────────────────────────────────────────────────────────────
    // Authorization Tests
    // ──────────────────────────────────────────────────────────────────

    [Fact]
    public async Task AdminEndpoint_WithoutToken_Returns401()
    {
        var response = await _client.GetAsync("/api/v1/admin/users");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task AdminEndpoint_WithNonAdminToken_Returns403()
    {
        var token = GenerateJwtToken(
            userId: Guid.NewGuid(),
            role: "User",
            expiresAt: DateTime.UtcNow.AddHours(1));

        using var request = new HttpRequestMessage(HttpMethod.Get, "/api/v1/admin/users");
        request.Headers.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var response = await _client.SendAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task ShopEndpoint_WithoutToken_Returns401()
    {
        var response = await _client.GetAsync("/api/v1/shop/items");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task AchievementsEndpoint_WithoutToken_Returns401()
    {
        var response = await _client.GetAsync("/api/v1/achievements");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task LeaguesEndpoint_WithoutToken_Returns401()
    {
        var response = await _client.GetAsync("/api/v1/leagues/current");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // ──────────────────────────────────────────────────────────────────
    // Input Validation / Injection Tests
    // ──────────────────────────────────────────────────────────────────

    [Theory]
    [InlineData("'; DROP TABLE Users; --")]
    [InlineData("' OR '1'='1")]
    [InlineData("<script>alert('xss')</script>")]
    [InlineData("<img onerror=alert(1) src=x>")]
    [InlineData("{{7*7}}")]
    [InlineData("${7*7}")]
    [InlineData("../../../etc/passwd")]
    public async Task Register_WithInjectionString_DoesNotExploit(string maliciousInput)
    {
        var response = await _client.PostAsJsonAsync("/api/v1/users/register", new
        {
            Email = $"test{Guid.NewGuid():N}@test.com",
            Username = maliciousInput,
            Password = "ValidPass123!",
            ConfirmPassword = "ValidPass123!",
            AcceptTerms = true
        });

        // Should either reject (400) or safely store (201/409) - never 500
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.BadRequest,
            HttpStatusCode.OK,
            HttpStatusCode.Created,
            HttpStatusCode.Conflict);
        response.StatusCode.Should().NotBe(HttpStatusCode.InternalServerError);
    }

    [Theory]
    [InlineData("'; DROP TABLE Users; --")]
    [InlineData("' OR '1'='1")]
    [InlineData("<script>alert('xss')</script>")]
    public async Task Login_WithInjectionString_DoesNotExploit(string maliciousInput)
    {
        var response = await _client.PostAsJsonAsync("/api/v1/users/login", new
        {
            Email = maliciousInput,
            Password = maliciousInput,
            RememberMe = false
        });

        // Should reject or return unauthorized - never 500
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.BadRequest,
            HttpStatusCode.Unauthorized);
        response.StatusCode.Should().NotBe(HttpStatusCode.InternalServerError);
    }

    // ──────────────────────────────────────────────────────────────────
    // CORS Tests
    // ──────────────────────────────────────────────────────────────────

    [Fact]
    public async Task CorsHeaders_OnlyAllowConfiguredOrigins()
    {
        var request = new HttpRequestMessage(HttpMethod.Options, "/api/v1/users/me");
        request.Headers.Add("Origin", "https://evil-site.com");
        request.Headers.Add("Access-Control-Request-Method", "GET");

        var response = await _client.SendAsync(request);

        // Should NOT include the evil origin in Access-Control-Allow-Origin
        if (response.Headers.TryGetValues("Access-Control-Allow-Origin", out var origins))
        {
            origins.Should().NotContain("https://evil-site.com");
            origins.Should().NotContain("*");
        }
    }

    [Fact]
    public async Task CorsHeaders_DoNotAllowWildcardOrigin()
    {
        var request = new HttpRequestMessage(HttpMethod.Options, "/api/v1/users/register");
        request.Headers.Add("Origin", "https://evil-site.com");
        request.Headers.Add("Access-Control-Request-Method", "POST");

        var response = await _client.SendAsync(request);

        if (response.Headers.TryGetValues("Access-Control-Allow-Origin", out var origins))
        {
            origins.Should().NotContain("*",
                "CORS should not allow wildcard origin when credentials are enabled");
        }
    }

    // ──────────────────────────────────────────────────────────────────
    // Security Headers Tests
    // ──────────────────────────────────────────────────────────────────

    [Fact]
    public async Task Response_ContainsXContentTypeOptionsHeader()
    {
        var response = await _client.GetAsync("/health");

        response.Headers.TryGetValues("X-Content-Type-Options", out var values).Should().BeTrue(
            "X-Content-Type-Options header should be present to prevent MIME sniffing");
        values.Should().Contain("nosniff");
    }

    [Fact]
    public async Task Response_ContainsXFrameOptionsHeader()
    {
        var response = await _client.GetAsync("/health");

        response.Headers.TryGetValues("X-Frame-Options", out var values).Should().BeTrue(
            "X-Frame-Options header should be present to prevent clickjacking");
        values.Should().Contain("DENY");
    }

    [Fact]
    public async Task Response_ContainsReferrerPolicyHeader()
    {
        var response = await _client.GetAsync("/health");

        response.Headers.TryGetValues("Referrer-Policy", out var values).Should().BeTrue(
            "Referrer-Policy header should be present");
        values.Should().Contain("strict-origin-when-cross-origin");
    }

    [Fact]
    public async Task Response_ContainsPermissionsPolicyHeader()
    {
        var response = await _client.GetAsync("/health");

        response.Headers.TryGetValues("Permissions-Policy", out var values).Should().BeTrue(
            "Permissions-Policy header should restrict browser features");
    }

    [Fact]
    public async Task Response_DisablesXssProtectionHeader()
    {
        // Modern best practice: X-XSS-Protection should be "0" to avoid
        // browser XSS auditor vulnerabilities in legacy browsers.
        var response = await _client.GetAsync("/health");

        response.Headers.TryGetValues("X-XSS-Protection", out var values).Should().BeTrue(
            "X-XSS-Protection header should be present and set to 0");
        values.Should().Contain("0");
    }

    // ──────────────────────────────────────────────────────────────────
    // Rate Limiting Tests
    // ──────────────────────────────────────────────────────────────────

    [Fact]
    public async Task Login_MultipleFailedAttempts_Returns401OrRateLimited()
    {
        for (int i = 0; i < 10; i++)
        {
            await _client.PostAsJsonAsync("/api/v1/users/login", new
            {
                Email = "nonexistent@test.com",
                Password = "WrongPassword123!",
                RememberMe = false
            });
        }

        var response = await _client.PostAsJsonAsync("/api/v1/users/login", new
        {
            Email = "nonexistent@test.com",
            Password = "WrongPassword123!",
            RememberMe = false
        });

        // Should eventually return 429 (rate limited) or 401/423 (locked out)
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.TooManyRequests,
            HttpStatusCode.Unauthorized,
            (HttpStatusCode)423); // Locked
    }

    // ──────────────────────────────────────────────────────────────────
    // No Secrets in Code Tests
    // ──────────────────────────────────────────────────────────────────

    [Fact]
    public void AppSettings_DoNotContainLiveStripeKeys()
    {
        var baseDir = AppDomain.CurrentDomain.BaseDirectory;
        var settingsFiles = new[]
        {
            Path.Combine(baseDir, "appsettings.json"),
            Path.Combine(baseDir, "appsettings.Development.json"),
            Path.Combine(baseDir, "appsettings.Test.json")
        };

        foreach (var file in settingsFiles)
        {
            if (!File.Exists(file)) continue;

            var content = File.ReadAllText(file);
            content.Should().NotContain("sk_live_",
                $"File {Path.GetFileName(file)} should not contain live Stripe secret keys");
            content.Should().NotContain("pk_live_",
                $"File {Path.GetFileName(file)} should not contain live Stripe publishable keys");
            content.Should().NotContain("whsec_live_",
                $"File {Path.GetFileName(file)} should not contain live webhook secrets");
        }
    }

    [Fact]
    public void AppSettings_DoNotContainProductionSecrets()
    {
        var baseDir = AppDomain.CurrentDomain.BaseDirectory;
        var settingsFiles = new[]
        {
            Path.Combine(baseDir, "appsettings.json"),
            Path.Combine(baseDir, "appsettings.Development.json"),
            Path.Combine(baseDir, "appsettings.Test.json")
        };

        foreach (var file in settingsFiles)
        {
            if (!File.Exists(file)) continue;

            var content = File.ReadAllText(file);
            content.Should().NotContain("REAL_SECRET",
                $"File {Path.GetFileName(file)} should not contain real secrets");
            content.Should().NotContain("PRODUCTION",
                $"File {Path.GetFileName(file)} should not contain production markers");
        }
    }

    [Fact]
    public void JwtSecretKey_IsNotInMainAppSettings()
    {
        var baseDir = AppDomain.CurrentDomain.BaseDirectory;
        var mainSettings = Path.Combine(baseDir, "appsettings.json");

        if (!File.Exists(mainSettings)) return;

        var content = File.ReadAllText(mainSettings);

        // The main appsettings.json should NOT have SecretKey hardcoded;
        // it should come from environment variables or user secrets.
        content.Should().NotMatchRegex("\"SecretKey\"\\s*:\\s*\"[A-Za-z0-9+/=\\-!@#$%^&*]{16,}\"",
            "JWT SecretKey should not be hardcoded in appsettings.json - use environment variables or user secrets");
    }

    // ──────────────────────────────────────────────────────────────────
    // Webhook Security Tests
    // ──────────────────────────────────────────────────────────────────

    [Fact]
    public async Task StripeWebhook_WithoutSignature_ShouldNotCrash()
    {
        var response = await _client.PostAsJsonAsync("/api/v1/webhooks/stripe", new
        {
            type = "checkout.session.completed",
            data = new { @object = new { } }
        });

        // Should handle gracefully - never 500 (may be 200, 400, or redirect)
        response.StatusCode.Should().NotBe(HttpStatusCode.InternalServerError);
    }

    [Fact]
    public async Task StripeWebhook_WithEmptyBody_DoesNotCrash()
    {
        var response = await _client.PostAsync("/api/v1/webhooks/stripe",
            new StringContent("{}", Encoding.UTF8, "application/json"));

        // Should not return 500 - any other status is acceptable
        response.StatusCode.Should().NotBe(HttpStatusCode.InternalServerError);
    }

    // ──────────────────────────────────────────────────────────────────
    // Error Disclosure Tests
    // ──────────────────────────────────────────────────────────────────

    [Fact]
    public async Task NonExistentEndpoint_Returns404WithoutStackTrace()
    {
        var response = await _client.GetAsync("/api/v1/nonexistent");
        var body = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        body.Should().NotContain("StackTrace");
        body.Should().NotContain("at LexiQuest");
    }

    [Fact]
    public async Task Register_WithInvalidPayload_DoesNotLeakInternalDetails()
    {
        var response = await _client.PostAsJsonAsync("/api/v1/users/register", new
        {
            Email = "",
            Username = "",
            Password = "",
            ConfirmPassword = "",
            AcceptTerms = false
        });

        var body = await response.Content.ReadAsStringAsync();

        body.Should().NotContain("Exception");
        body.Should().NotContain("StackTrace");
        body.Should().NotContain("SqlException");
    }

    // ──────────────────────────────────────────────────────────────────
    // Token claim manipulation Tests
    // ──────────────────────────────────────────────────────────────────

    [Fact]
    public async Task Token_WithNoneAlgorithm_Returns401()
    {
        // The "none" algorithm attack is a classic JWT vulnerability.
        // Build a token header with "alg": "none" manually.
        var header = Convert.ToBase64String(Encoding.UTF8.GetBytes("{\"alg\":\"none\",\"typ\":\"JWT\"}"))
            .TrimEnd('=').Replace('+', '-').Replace('/', '_');
        var payload = Convert.ToBase64String(Encoding.UTF8.GetBytes(
            $"{{\"sub\":\"{Guid.NewGuid()}\",\"role\":\"Admin\",\"exp\":{DateTimeOffset.UtcNow.AddHours(1).ToUnixTimeSeconds()}}}"))
            .TrimEnd('=').Replace('+', '-').Replace('/', '_');

        var unsignedToken = $"{header}.{payload}.";

        using var request = new HttpRequestMessage(HttpMethod.Get, "/api/v1/admin/users");
        request.Headers.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", unsignedToken);

        var response = await _client.SendAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // ──────────────────────────────────────────────────────────────────
    // Helpers
    // ──────────────────────────────────────────────────────────────────

    private static string GenerateJwtToken(Guid userId, string role, DateTime expiresAt)
    {
        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes("Test-Secret-Key-That-Is-Long-Enough-For-HS256-Algorithm-!!"));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(ClaimTypes.Role, role)
        };

        var token = new JwtSecurityToken(
            issuer: "TestIssuer",
            audience: "TestAudience",
            claims: claims,
            expires: expiresAt,
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
