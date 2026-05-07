using System.CommandLine;
using GitSleuth.Cli.Commands;
using GitSleuth.Cli.Services;
using Xunit;

namespace GitSleuth.Cli.Tests.Commands;

[Collection("CommandTests")]
public class ListCommandTests : IDisposable
{
    private readonly string _sessionFilePath;
    private readonly SessionService _sessionService;

    public ListCommandTests()
    {
        _sessionFilePath = Path.Combine(Path.GetTempPath(), $"git-sleuth-list-test-{Guid.NewGuid():N}.json");
        _sessionService = new SessionService(_sessionFilePath);
    }

    public void Dispose()
    {
        if (File.Exists(_sessionFilePath))
        {
            File.Delete(_sessionFilePath);
        }
    }

    [Fact]
    public async Task List_WhenNoVisits_PrintsEmptyMessage()
    {
        var output = await CaptureOutputAsync(() => ListCommand.Build(_sessionService).InvokeAsync([]));

        Assert.Contains("No branches have been visited in this session yet.", output);
    }

    [Fact]
    public async Task List_WhenNoVisits_IncludesUsageHint()
    {
        var output = await CaptureOutputAsync(() => ListCommand.Build(_sessionService).InvokeAsync([]));

        Assert.Contains("git-sleuth visit", output);
    }

    [Fact]
    public async Task List_ShowsAllUniqueBranches()
    {
        _sessionService.RecordVisit("main");
        _sessionService.RecordVisit("feature/one");
        _sessionService.RecordVisit("main"); // duplicate — should appear only once

        var output = await CaptureOutputAsync(() => ListCommand.Build(_sessionService).InvokeAsync([]));

        Assert.Contains("main", output);
        Assert.Contains("feature/one", output);
    }

    [Fact]
    public async Task List_ShowsCorrectUniqueCount()
    {
        _sessionService.RecordVisit("main");
        _sessionService.RecordVisit("feature/one");
        _sessionService.RecordVisit("main");

        var output = await CaptureOutputAsync(() => ListCommand.Build(_sessionService).InvokeAsync([]));

        Assert.Contains("2 unique", output);
    }

    [Fact]
    public async Task List_DeduplicatesBranches_OnlyCountsUniqueOnce()
    {
        _sessionService.RecordVisit("main");
        _sessionService.RecordVisit("main");
        _sessionService.RecordVisit("main");

        var output = await CaptureOutputAsync(() => ListCommand.Build(_sessionService).InvokeAsync([]));

        Assert.Contains("1 unique", output);
        // "main" should appear exactly once in the list (plus any count lines)
        var listItems = output.Split('\n')
            .Count(l => l.TrimStart().StartsWith("- main", StringComparison.Ordinal));
        Assert.Equal(1, listItems);
    }

    [Fact]
    public async Task List_PreservesOrderOfFirstVisit()
    {
        _sessionService.RecordVisit("feature/z");
        _sessionService.RecordVisit("main");

        var output = await CaptureOutputAsync(() => ListCommand.Build(_sessionService).InvokeAsync([]));

        var featurePos = output.IndexOf("feature/z", StringComparison.Ordinal);
        var mainPos = output.IndexOf("main", StringComparison.Ordinal);
        Assert.True(featurePos < mainPos);
    }

    private static async Task<string> CaptureOutputAsync(Func<Task<int>> action)
    {
        var original = Console.Out;
        using var writer = new StringWriter();
        Console.SetOut(writer);
        try
        {
            await action();
            return writer.ToString();
        }
        finally
        {
            Console.SetOut(original);
        }
    }
}
