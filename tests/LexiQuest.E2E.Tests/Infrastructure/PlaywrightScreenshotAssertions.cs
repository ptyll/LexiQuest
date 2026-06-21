using System.Text.Json;
using Microsoft.Playwright;
using Xunit.Sdk;

namespace LexiQuest.E2E.Tests.Infrastructure;

internal sealed record E2EScreenshotApprovalManifest(IReadOnlyList<string> Approved);

internal static class ScreenshotApprovalManifest
{
    private const string ManifestFileName = "approved-screenshots.json";

    public static string ManifestPath => Path.Combine(RepositoryPaths.E2EApprovedScreenshots, ManifestFileName);

    public static IReadOnlySet<string> GetApprovedRelativePaths()
    {
        if (!File.Exists(ManifestPath))
        {
            return new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        }

        var manifest = JsonSerializer.Deserialize<E2EScreenshotApprovalManifest>(
            File.ReadAllText(ManifestPath),
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        return manifest?.Approved
            .Select(Normalize)
            .ToHashSet(StringComparer.OrdinalIgnoreCase)
            ?? new HashSet<string>(StringComparer.OrdinalIgnoreCase);
    }

    public static bool IsApproved(string relativePath) =>
        GetApprovedRelativePaths().Contains(Normalize(relativePath));

    public static string Normalize(string relativePath) =>
        relativePath.Replace('\\', '/').TrimStart('/');
}

internal static class PlaywrightScreenshotAssertions
{
    public static async Task ToHaveScreenshotAsync(
        this IPage page,
        string approvedRelativePath,
        bool fullPage = true,
        int maxDifferentBytes = 0)
    {
        var normalizedRelativePath = ScreenshotApprovalManifest.Normalize(approvedRelativePath);
        var expectedPath = Path.Combine(
            RepositoryPaths.E2EApprovedScreenshots,
            normalizedRelativePath.Replace('/', Path.DirectorySeparatorChar));

        if (!File.Exists(expectedPath))
        {
            throw new XunitException(
                $"Approved screenshot baseline is listed but missing: {normalizedRelativePath}");
        }

        var actual = await page.ScreenshotAsync(new PageScreenshotOptions
        {
            FullPage = fullPage,
            Animations = ScreenshotAnimations.Disabled
        });
        var expected = await File.ReadAllBytesAsync(expectedPath);

        var differentBytes = CountDifferentBytes(expected, actual);
        if (differentBytes <= maxDifferentBytes)
        {
            return;
        }

        var actualPath = Path.Combine(
            RepositoryPaths.E2EScreenshotDiffs,
            normalizedRelativePath.Replace('/', Path.DirectorySeparatorChar));
        Directory.CreateDirectory(Path.GetDirectoryName(actualPath)!);
        await File.WriteAllBytesAsync(actualPath, actual);

        var metadataPath = Path.ChangeExtension(actualPath, ".json");
        var metadata = new
        {
            approved = normalizedRelativePath,
            expectedPath,
            actualPath,
            expectedBytes = expected.Length,
            actualBytes = actual.Length,
            differentBytes,
            maxDifferentBytes
        };
        await File.WriteAllTextAsync(
            metadataPath,
            JsonSerializer.Serialize(metadata, new JsonSerializerOptions { WriteIndented = true }));

        throw new XunitException(
            $"Screenshot does not match approved baseline '{normalizedRelativePath}'. "
            + $"Different bytes: {differentBytes}, allowed: {maxDifferentBytes}. Actual saved to: {actualPath}");
    }

    private static int CountDifferentBytes(byte[] expected, byte[] actual)
    {
        var comparedLength = Math.Min(expected.Length, actual.Length);
        var different = Math.Abs(expected.Length - actual.Length);

        for (var i = 0; i < comparedLength; i++)
        {
            if (expected[i] != actual[i])
            {
                different++;
            }
        }

        return different;
    }
}
