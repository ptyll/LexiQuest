namespace LexiQuest.E2E.Tests.Infrastructure;

internal static class RepositoryPaths
{
    public static readonly string Root = FindRepositoryRoot();

    public static string ApiProject => Path.Combine(Root, "src", "LexiQuest.Api", "LexiQuest.Api.csproj");

    public static string WebProject => Path.Combine(Root, "src", "LexiQuest.Web", "LexiQuest.Web.csproj");

    public static string Artifacts => Path.Combine(Root, "artifacts", "e2e");

    public static string E2ELogs => Path.Combine(Artifacts, "logs");

    public static string E2EScreenshots => Path.Combine(Artifacts, "screenshots");

    public static string E2EApprovedScreenshots => Path.Combine(Root, "tests", "LexiQuest.E2E.Tests", "Screenshots");

    public static string E2EScreenshotDiffs => Path.Combine(Artifacts, "screenshot-diffs");

    public static string E2ETraces => Path.Combine(Artifacts, "traces");

    public static string E2EVideos => Path.Combine(Artifacts, "videos");

    private static string FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);

        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "LexiQuest.slnx")))
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        throw new DirectoryNotFoundException("Could not locate repository root containing LexiQuest.slnx.");
    }
}
