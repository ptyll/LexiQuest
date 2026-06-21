using System.Net.Http.Json;
using System.Net;
using System.Text.Json;

namespace LexiQuest.E2E.Tests.Infrastructure;

public sealed class Smtp4DevClient
{
    private readonly HttpClient _httpClient;

    public Smtp4DevClient(string baseUrl)
    {
        BaseUrl = baseUrl.TrimEnd('/');
        _httpClient = new HttpClient { BaseAddress = new Uri($"{BaseUrl}/") };
    }

    public string BaseUrl { get; }

    public async Task<JsonElement> GetMessagesAsync(CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.GetAsync("api/Messages", cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<JsonElement>(cancellationToken: cancellationToken);
    }

    public async Task<JsonElement> WaitForMessageAsync(
        Func<JsonElement, bool> predicate,
        TimeSpan? timeout = null,
        CancellationToken cancellationToken = default)
    {
        var deadline = DateTimeOffset.UtcNow + (timeout ?? TimeSpan.FromSeconds(15));
        var lastPayload = string.Empty;

        while (DateTimeOffset.UtcNow < deadline)
        {
            var messages = await GetMessagesAsync(cancellationToken);
            lastPayload = messages.GetRawText();

            foreach (var summary in EnumerateMessages(messages))
            {
                var message = await GetMessageDetailsAsync(summary, cancellationToken);
                if (predicate(message))
                {
                    return message;
                }
            }

            await Task.Delay(500, cancellationToken);
        }

        throw new TimeoutException($"smtp4dev message was not found. Last payload: {lastPayload}");
    }

    public async Task<string> WaitForMessageTextAsync(
        Func<string, bool> predicate,
        TimeSpan? timeout = null,
        CancellationToken cancellationToken = default)
    {
        var deadline = DateTimeOffset.UtcNow + (timeout ?? TimeSpan.FromSeconds(15));
        var lastPayload = string.Empty;

        while (DateTimeOffset.UtcNow < deadline)
        {
            var messages = await GetMessagesAsync(cancellationToken);
            lastPayload = messages.GetRawText();

            foreach (var summary in EnumerateMessages(messages))
            {
                var messageText = await GetMessageTextAsync(summary, cancellationToken);
                lastPayload = messageText;

                if (predicate(messageText))
                {
                    return messageText;
                }
            }

            await Task.Delay(500, cancellationToken);
        }

        throw new TimeoutException($"smtp4dev message text was not found. Last payload: {lastPayload}");
    }

    public async Task ClearMessagesAsync(CancellationToken cancellationToken = default)
    {
        foreach (var endpoint in new[] { "api/Messages/*", "api/Messages" })
        {
            try
            {
                using var response = await _httpClient.DeleteAsync(endpoint, cancellationToken);
                if (response.IsSuccessStatusCode)
                {
                    return;
                }
            }
            catch (HttpRequestException)
            {
                return;
            }
        }
    }

    private async Task<JsonElement> GetMessageDetailsAsync(JsonElement summary, CancellationToken cancellationToken)
    {
        var id = TryGetStringProperty(summary, "id", "Id", "messageId", "MessageId");
        if (string.IsNullOrWhiteSpace(id))
        {
            return summary;
        }

        foreach (var endpoint in new[] { $"api/Messages/{id}", $"api/messages/{id}" })
        {
            try
            {
                var response = await _httpClient.GetAsync(endpoint, cancellationToken);
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<JsonElement>(cancellationToken: cancellationToken);
                }
            }
            catch (HttpRequestException)
            {
                return summary;
            }
        }

        return summary;
    }

    private async Task<string> GetMessageTextAsync(JsonElement summary, CancellationToken cancellationToken)
    {
        var parts = new List<string> { summary.GetRawText() };
        var id = TryGetStringProperty(summary, "id", "Id", "messageId", "MessageId");

        if (string.IsNullOrWhiteSpace(id))
        {
            return string.Join(Environment.NewLine, parts);
        }

        foreach (var endpoint in new[]
        {
            $"api/Messages/{id}",
            $"api/messages/{id}",
            $"api/Messages/{id}/html",
            $"api/messages/{id}/html",
            $"api/Messages/{id}/text",
            $"api/messages/{id}/text",
            $"api/Messages/{id}/source",
            $"api/messages/{id}/source",
            $"api/Messages/{id}/eml",
            $"api/messages/{id}/eml"
        })
        {
            try
            {
                using var response = await _httpClient.GetAsync(endpoint, cancellationToken);
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync(cancellationToken);
                    parts.Add(content);

                    var decodedContent = WebUtility.HtmlDecode(content);
                    if (!string.Equals(decodedContent, content, StringComparison.Ordinal))
                    {
                        parts.Add(decodedContent);
                    }
                }
            }
            catch (HttpRequestException)
            {
                // smtp4dev exposes different message body endpoints across versions.
            }
        }

        return string.Join(Environment.NewLine, parts);
    }

    private static IEnumerable<JsonElement> EnumerateMessages(JsonElement root)
    {
        if (root.ValueKind == JsonValueKind.Array)
        {
            foreach (var item in root.EnumerateArray())
            {
                yield return item;
            }
        }
        else if (root.ValueKind == JsonValueKind.Object)
        {
            foreach (var propertyName in new[] { "messages", "Messages", "items", "Items", "results", "Results" })
            {
                if (root.TryGetProperty(propertyName, out var items) && items.ValueKind == JsonValueKind.Array)
                {
                    foreach (var item in items.EnumerateArray())
                    {
                        yield return item;
                    }

                    yield break;
                }
            }
        }
    }

    private static string? TryGetStringProperty(JsonElement element, params string[] propertyNames)
    {
        if (element.ValueKind != JsonValueKind.Object)
        {
            return null;
        }

        foreach (var propertyName in propertyNames)
        {
            if (element.TryGetProperty(propertyName, out var property))
            {
                return property.ValueKind switch
                {
                    JsonValueKind.String => property.GetString(),
                    JsonValueKind.Number => property.GetRawText(),
                    _ => null
                };
            }
        }

        return null;
    }
}
