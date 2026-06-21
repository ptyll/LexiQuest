using System.Diagnostics;
using System.Net;

namespace LexiQuest.E2E.Tests.Infrastructure;

internal sealed class AppProcessRunner : IAsyncDisposable
{
    private readonly string _name;
    private readonly IReadOnlyList<Uri> _healthUris;
    private readonly List<string> _output = [];
    private readonly List<string> _error = [];
    private Process? _process;

    public AppProcessRunner(string name, string projectPath, int port, IReadOnlyDictionary<string, string> environment, params Uri[] healthUris)
    {
        _name = name;
        _healthUris = healthUris.Length == 0
            ? throw new ArgumentException("At least one health URI is required.", nameof(healthUris))
            : healthUris;
        ProjectPath = projectPath;
        Port = port;
        ProcessEnvironment = environment;
    }

    public string ProjectPath { get; }

    public int Port { get; }

    public IReadOnlyDictionary<string, string> ProcessEnvironment { get; }

    public string BaseUrl => $"http://127.0.0.1:{Port}";

    public IReadOnlyList<string> Output => _output;

    public IReadOnlyList<string> Error => _error;

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var startInfo = new ProcessStartInfo("dotnet", $"run --project \"{ProjectPath}\" --no-launch-profile")
        {
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            WorkingDirectory = RepositoryPaths.Root
        };

        foreach (var (key, value) in ProcessEnvironment)
        {
            startInfo.Environment[key] = value;
        }

        _process = new Process { StartInfo = startInfo, EnableRaisingEvents = true };
        _process.OutputDataReceived += (_, args) =>
        {
            if (args.Data is not null)
            {
                lock (_output)
                {
                    _output.Add(args.Data);
                }
            }
        };
        _process.ErrorDataReceived += (_, args) =>
        {
            if (args.Data is not null)
            {
                lock (_error)
                {
                    _error.Add(args.Data);
                }
            }
        };

        if (!_process.Start())
        {
            throw new InvalidOperationException($"Failed to start {_name} process.");
        }

        _process.BeginOutputReadLine();
        _process.BeginErrorReadLine();

        await WaitUntilHealthyAsync(cancellationToken);
    }

    public async ValueTask DisposeAsync()
    {
        if (_process is null)
        {
            return;
        }

        try
        {
            if (!_process.HasExited)
            {
                _process.Kill(entireProcessTree: true);
                await _process.WaitForExitAsync();
            }
        }
        finally
        {
            _process.Dispose();
        }
    }

    public async Task WriteLogsAsync(string suffix, CancellationToken cancellationToken = default)
    {
        Directory.CreateDirectory(RepositoryPaths.E2ELogs);

        var safeSuffix = string.Concat(suffix.Select(ch =>
            Path.GetInvalidFileNameChars().Contains(ch) || char.IsWhiteSpace(ch) ? '-' : char.ToLowerInvariant(ch)));

        List<string> output;
        List<string> error;

        lock (_output)
        {
            output = _output.ToList();
        }

        lock (_error)
        {
            error = _error.ToList();
        }

        await File.WriteAllLinesAsync(
            Path.Combine(RepositoryPaths.E2ELogs, $"{_name}-{safeSuffix}-stdout.log"),
            output,
            cancellationToken);
        await File.WriteAllLinesAsync(
            Path.Combine(RepositoryPaths.E2ELogs, $"{_name}-{safeSuffix}-stderr.log"),
            error,
            cancellationToken);
    }

    private async Task WaitUntilHealthyAsync(CancellationToken cancellationToken)
    {
        using var httpClient = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(2)
        };

        var deadline = DateTimeOffset.UtcNow.AddSeconds(120);
        Exception? lastException = null;

        while (DateTimeOffset.UtcNow < deadline)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (_process is { HasExited: true })
            {
                throw new InvalidOperationException($"{_name} process exited before becoming healthy.{System.Environment.NewLine}{GetDiagnostics()}");
            }

            try
            {
                var healthy = true;
                foreach (var healthUri in _healthUris)
                {
                    using var response = await httpClient.GetAsync(healthUri, cancellationToken);
                    healthy &= response.StatusCode is HttpStatusCode.OK;
                }

                if (healthy)
                {
                    return;
                }
            }
            catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
            {
                lastException = ex;
            }

            await Task.Delay(500, cancellationToken);
        }

        throw new TimeoutException($"{_name} did not become healthy at {string.Join(", ", _healthUris)}. Last error: {lastException?.Message}{System.Environment.NewLine}{GetDiagnostics()}");
    }

    private string GetDiagnostics()
    {
        List<string> output;
        List<string> error;

        lock (_output)
        {
            output = _output.TakeLast(80).ToList();
        }

        lock (_error)
        {
            error = _error.TakeLast(80).ToList();
        }

        return $"""
                --- {_name} stdout ---
                {string.Join(System.Environment.NewLine, output)}
                --- {_name} stderr ---
                {string.Join(System.Environment.NewLine, error)}
                """;
    }
}
