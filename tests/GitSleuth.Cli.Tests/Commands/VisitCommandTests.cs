using System.CommandLine;
using GitSleuth.Cli.Commands;
using GitSleuth.Cli.Services;
using Xunit;

namespace GitSleuth.Cli.Tests.Commands;

[Collection("CommandTests")]
public class VisitCommandTests : IDisposable
{
    private readonly string _sessionFilePath;
    private readonly SessionService _sessionService;

    public VisitCommandTests()
    {
        _sessionFilePath = Path.Combine(Path.GetTempPath(), $"git-sleuth-visit-test-{Guid.NewGuid():N}.json");
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
    public async Task Visit_WithExplicitBranch_RecordsVisit()
    {
        var command = VisitCommand.Build(_sessionService);
        await command.InvokeAsync(["feature/payments"]);

        var visits = _sessionService.GetVisits();
        Assert.Single(visits);
        Assert.Equal("feature/payments", visits[0].BranchName);
    }

    [Fact]
    public async Task Visit_WithExplicitBranch_PrintsConfirmation()
    {
        var output = await CaptureOutputAsync(
            () => VisitCommand.Build(_sessionService).InvokeAsync(["main"]));

        Assert.Contains("Visited branch 'main'.", output);
    }

    [Fact]
    public async Task Visit_WithExplicitBranch_StoresWorkingDirectory()
    {
        var command = VisitCommand.Build(_sessionService);
        await command.InvokeAsync(["main"]);

        var visits = _sessionService.GetVisits();
        Assert.Single(visits);
        Assert.False(string.IsNullOrWhiteSpace(visits[0].WorkingDirectory));
    }

    [Fact]
    public async Task Visit_CalledMultipleTimes_RecordsAllVisits()
    {
        var command = VisitCommand.Build(_sessionService);
        await command.InvokeAsync(["main"]);
        await command.InvokeAsync(["feature/one"]);
        await command.InvokeAsync(["main"]);

        var visits = _sessionService.GetVisits();
        Assert.Equal(3, visits.Count);
        Assert.Equal("main", visits[0].BranchName);
        Assert.Equal("feature/one", visits[1].BranchName);
        Assert.Equal("main", visits[2].BranchName);
    }

    [Fact]
    public async Task Visit_WithExplicitBranch_SetsVisitedAtTimestamp()
    {
        var before = DateTimeOffset.UtcNow.AddSeconds(-1);
        var command = VisitCommand.Build(_sessionService);
        await command.InvokeAsync(["main"]);
        var after = DateTimeOffset.UtcNow.AddSeconds(1);

        var visits = _sessionService.GetVisits();
        Assert.Single(visits);
        Assert.InRange(visits[0].VisitedAt, before, after);
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
