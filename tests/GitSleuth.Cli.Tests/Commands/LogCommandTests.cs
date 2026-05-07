using System.CommandLine;
using GitSleuth.Cli.Commands;
using GitSleuth.Cli.Services;
using Xunit;

namespace GitSleuth.Cli.Tests.Commands;

[Collection("CommandTests")]
public class LogCommandTests : IDisposable
{
    private readonly string _sessionFilePath;
    private readonly SessionService _sessionService;

    public LogCommandTests()
    {
        _sessionFilePath = Path.Combine(Path.GetTempPath(), $"git-sleuth-log-test-{Guid.NewGuid():N}.json");
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
    public async Task Log_WhenNoVisits_PrintsEmptyMessage()
    {
        var output = await CaptureOutputAsync(() => LogCommand.Build(_sessionService).InvokeAsync([]));

        Assert.Contains("No branches have been visited in this session yet.", output);
    }

    [Fact]
    public async Task Log_WhenNoVisits_IncludesUsageHint()
    {
        var output = await CaptureOutputAsync(() => LogCommand.Build(_sessionService).InvokeAsync([]));

        Assert.Contains("git-sleuth visit", output);
    }

    [Fact]
    public async Task Log_ShowsAllVisitsIncludingDuplicates()
    {
        _sessionService.RecordVisit("main");
        _sessionService.RecordVisit("feature/one");
        _sessionService.RecordVisit("main");

        var output = await CaptureOutputAsync(() => LogCommand.Build(_sessionService).InvokeAsync([]));

        Assert.Contains("3 visits", output);
    }

    [Fact]
    public async Task Log_ShowsBranchNamesInOutput()
    {
        _sessionService.RecordVisit("main");
        _sessionService.RecordVisit("feature/one");

        var output = await CaptureOutputAsync(() => LogCommand.Build(_sessionService).InvokeAsync([]));

        Assert.Contains("main", output);
        Assert.Contains("feature/one", output);
    }

    [Fact]
    public async Task Log_ShowsWorkingDirectory_WhenPresent()
    {
        _sessionService.RecordVisit("main", "/my/repo");

        var output = await CaptureOutputAsync(() => LogCommand.Build(_sessionService).InvokeAsync([]));

        Assert.Contains("/my/repo", output);
    }

    [Fact]
    public async Task Log_ShowsSequentialIndexes()
    {
        _sessionService.RecordVisit("main");
        _sessionService.RecordVisit("feature/one");
        _sessionService.RecordVisit("feature/two");

        var output = await CaptureOutputAsync(() => LogCommand.Build(_sessionService).InvokeAsync([]));

        Assert.Contains("1.", output);
        Assert.Contains("2.", output);
        Assert.Contains("3.", output);
    }

    [Fact]
    public async Task Log_UsesSingularVisit_WhenOnlyOne()
    {
        _sessionService.RecordVisit("main");

        var output = await CaptureOutputAsync(() => LogCommand.Build(_sessionService).InvokeAsync([]));

        // Should say "1 visit" not "1 visits"
        Assert.Contains("1 visit", output);
        Assert.DoesNotContain("1 visits", output);
    }

    [Fact]
    public async Task Log_ShowsCommitSha_WhenPresent()
    {
        _sessionService.RecordVisit("main", "/my/repo", "abc1234");

        var output = await CaptureOutputAsync(() => LogCommand.Build(_sessionService).InvokeAsync([]));

        Assert.Contains("abc1234", output);
    }

    [Fact]
    public async Task Log_DoesNotShowCommitSuffix_WhenCommitShaIsNull()
    {
        _sessionService.RecordVisit("main", "/my/repo");

        var output = await CaptureOutputAsync(() => LogCommand.Build(_sessionService).InvokeAsync([]));

        Assert.DoesNotContain("()", output);
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
