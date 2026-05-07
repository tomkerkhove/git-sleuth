using System.CommandLine;
using GitSleuth.Cli.Commands;
using GitSleuth.Cli.Services;
using Xunit;

namespace GitSleuth.Cli.Tests.Commands;

[Collection("CommandTests")]
public class StatsCommandTests : IDisposable
{
    private readonly string _sessionFilePath;
    private readonly SessionService _sessionService;

    public StatsCommandTests()
    {
        _sessionFilePath = Path.Combine(Path.GetTempPath(), $"git-sleuth-stats-test-{Guid.NewGuid():N}.json");
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
    public async Task Stats_WhenNoVisits_PrintsEmptyMessage()
    {
        var output = await CaptureOutputAsync(() => StatsCommand.Build(_sessionService).InvokeAsync([]));

        Assert.Contains("No branches have been visited in this session yet.", output);
    }

    [Fact]
    public async Task Stats_WhenNoVisits_IncludesUsageHint()
    {
        var output = await CaptureOutputAsync(() => StatsCommand.Build(_sessionService).InvokeAsync([]));

        Assert.Contains("git-sleuth visit", output);
    }

    [Fact]
    public async Task Stats_ShowsTotalVisitCount()
    {
        _sessionService.RecordVisit("main");
        _sessionService.RecordVisit("feature/one");
        _sessionService.RecordVisit("main");

        var output = await CaptureOutputAsync(() => StatsCommand.Build(_sessionService).InvokeAsync([]));

        Assert.Contains("3 visits", output);
    }

    [Fact]
    public async Task Stats_UsesSingularVisit_WhenOnlyOne()
    {
        _sessionService.RecordVisit("main");

        var output = await CaptureOutputAsync(() => StatsCommand.Build(_sessionService).InvokeAsync([]));

        Assert.Contains("1 visit", output);
        Assert.DoesNotContain("1 visits", output);
    }

    [Fact]
    public async Task Stats_ShowsUniqueBranchCount()
    {
        _sessionService.RecordVisit("main");
        _sessionService.RecordVisit("feature/one");
        _sessionService.RecordVisit("main");

        var output = await CaptureOutputAsync(() => StatsCommand.Build(_sessionService).InvokeAsync([]));

        Assert.Contains("2 unique branches", output);
    }

    [Fact]
    public async Task Stats_UsesSingularBranch_WhenOnlyOne()
    {
        _sessionService.RecordVisit("main");

        var output = await CaptureOutputAsync(() => StatsCommand.Build(_sessionService).InvokeAsync([]));

        Assert.Contains("1 unique branch", output);
        Assert.DoesNotContain("1 unique branches", output);
    }

    [Fact]
    public async Task Stats_ShowsSessionStartAndEnd()
    {
        _sessionService.RecordVisit("main");
        _sessionService.RecordVisit("feature/one");

        var output = await CaptureOutputAsync(() => StatsCommand.Build(_sessionService).InvokeAsync([]));

        Assert.Contains("Session started:", output);
        Assert.Contains("Session ended:", output);
    }

    [Fact]
    public async Task Stats_ShowsDuration()
    {
        _sessionService.RecordVisit("main");
        _sessionService.RecordVisit("feature/one");

        var output = await CaptureOutputAsync(() => StatsCommand.Build(_sessionService).InvokeAsync([]));

        Assert.Contains("Duration:", output);
    }

    [Fact]
    public async Task Stats_ShowsBranchVisitCounts()
    {
        _sessionService.RecordVisit("main");
        _sessionService.RecordVisit("feature/one");
        _sessionService.RecordVisit("main");

        var output = await CaptureOutputAsync(() => StatsCommand.Build(_sessionService).InvokeAsync([]));

        Assert.Contains("main", output);
        Assert.Contains("feature/one", output);
    }

    [Fact]
    public async Task Stats_ShowsMostVisitedBranchFirst()
    {
        _sessionService.RecordVisit("feature/one");
        _sessionService.RecordVisit("main");
        _sessionService.RecordVisit("main");
        _sessionService.RecordVisit("main");

        var output = await CaptureOutputAsync(() => StatsCommand.Build(_sessionService).InvokeAsync([]));

        var mainPos = output.IndexOf("main", StringComparison.Ordinal);
        var featurePos = output.IndexOf("feature/one", StringComparison.Ordinal);

        // "main" should appear before "feature/one" in the branch counts section
        // (both appear in the header too, so find counts section via "most visited")
        var countsHeaderPos = output.IndexOf("most visited", StringComparison.OrdinalIgnoreCase);
        Assert.True(countsHeaderPos >= 0);

        var mainCountPos = output.IndexOf("main", countsHeaderPos, StringComparison.Ordinal);
        var featureCountPos = output.IndexOf("feature/one", countsHeaderPos, StringComparison.Ordinal);
        Assert.True(mainCountPos < featureCountPos);
    }

    [Fact]
    public async Task Stats_PerBranchCount_ShowsCorrectNumbers()
    {
        _sessionService.RecordVisit("main");
        _sessionService.RecordVisit("main");
        _sessionService.RecordVisit("feature/one");

        var output = await CaptureOutputAsync(() => StatsCommand.Build(_sessionService).InvokeAsync([]));

        Assert.Contains("2 visits", output);
        Assert.Contains("1 visit", output);
    }

    [Fact]
    public async Task Stats_PerBranchCount_UsesSingularVisit_ForCountOfOne()
    {
        _sessionService.RecordVisit("main");
        _sessionService.RecordVisit("feature/one");

        var output = await CaptureOutputAsync(() => StatsCommand.Build(_sessionService).InvokeAsync([]));

        // Each branch has 1 visit — both should show "1 visit" not "1 visits"
        Assert.DoesNotContain("1 visits", output);
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
