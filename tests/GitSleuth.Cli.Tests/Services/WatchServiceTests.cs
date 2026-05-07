using GitSleuth.Cli.Services;
using Xunit;

namespace GitSleuth.Cli.Tests.Services;

public class WatchServiceTests : IDisposable
{
    private readonly string _sessionFilePath;
    private readonly SessionService _sessionService;

    public WatchServiceTests()
    {
        _sessionFilePath = Path.Combine(Path.GetTempPath(), $"git-sleuth-watch-test-{Guid.NewGuid():N}.json");
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
    public async Task WatchAsync_RecordsVisit_WhenBranchChanges()
    {
        // Arrange: sequence of branch values returned by the mock
        var branches = new Queue<string?>(["main", "main", "feature/one", "feature/one"]);
        var sut = new WatchService(_sessionService, _ => branches.TryDequeue(out var b) ? b : null);

        using var cts = new CancellationTokenSource();

        // Act: run until queue is drained, then cancel
        var watchTask = sut.WatchAsync(
            directory: "/repo",
            interval: TimeSpan.FromMilliseconds(10),
            onBranchChanged: null,
            cancellationToken: cts.Token);

        await Task.Delay(200);
        await cts.CancelAsync();
        await watchTask;

        // Assert: saw initial "main" and switch to "feature/one"
        var visits = _sessionService.GetVisits();
        Assert.Equal(2, visits.Count);
        Assert.Equal("main", visits[0].BranchName);
        Assert.Equal("feature/one", visits[1].BranchName);
    }

    [Fact]
    public async Task WatchAsync_DoesNotRecordDuplicates_WhenBranchUnchanged()
    {
        // Same branch returned every poll — should only be recorded once
        var sut = new WatchService(_sessionService, _ => "main");

        using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(150));

        await sut.WatchAsync(
            directory: "/repo",
            interval: TimeSpan.FromMilliseconds(20),
            onBranchChanged: null,
            cancellationToken: cts.Token);

        var visits = _sessionService.GetVisits();
        Assert.Single(visits);
        Assert.Equal("main", visits[0].BranchName);
    }

    [Fact]
    public async Task WatchAsync_InvokesCallback_WhenBranchChanges()
    {
        var detectedBranches = new List<string>();
        var branches = new Queue<string?>(["main", "feature/new"]);
        var sut = new WatchService(_sessionService, _ => branches.TryDequeue(out var b) ? b : "feature/new");

        using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(200));

        await sut.WatchAsync(
            directory: "/repo",
            interval: TimeSpan.FromMilliseconds(20),
            onBranchChanged: visit => detectedBranches.Add(visit.BranchName),
            cancellationToken: cts.Token);

        Assert.Contains("main", detectedBranches);
        Assert.Contains("feature/new", detectedBranches);
    }

    [Fact]
    public async Task WatchAsync_IgnoresNullOrEmptyBranch_WhenNotInGitRepo()
    {
        // Simulate a directory that is not a git repo (returns null)
        var sut = new WatchService(_sessionService, _ => null);

        using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(100));

        await sut.WatchAsync(
            directory: "/not-a-repo",
            interval: TimeSpan.FromMilliseconds(20),
            onBranchChanged: null,
            cancellationToken: cts.Token);

        var visits = _sessionService.GetVisits();
        Assert.Empty(visits);
    }

    [Fact]
    public async Task WatchAsync_StoresWorkingDirectory_WithEachVisit()
    {
        var branches = new Queue<string?>(["main", "develop"]);
        var sut = new WatchService(_sessionService, _ => branches.TryDequeue(out var b) ? b : "develop");

        using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(200));

        await sut.WatchAsync(
            directory: "/my/repo",
            interval: TimeSpan.FromMilliseconds(20),
            onBranchChanged: null,
            cancellationToken: cts.Token);

        var visits = _sessionService.GetVisits();
        Assert.All(visits, v => Assert.Equal("/my/repo", v.WorkingDirectory));
    }

    [Fact]
    public async Task WatchAsync_RecordsReturnVisit_WhenSwitchingBackToPreviousBranch()
    {
        // main → feature → main: all three should be recorded
        var branches = new Queue<string?>(["main", "feature/x", "main"]);
        var sut = new WatchService(_sessionService, _ => branches.TryDequeue(out var b) ? b : "main");

        using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(300));

        await sut.WatchAsync(
            directory: "/repo",
            interval: TimeSpan.FromMilliseconds(10),
            onBranchChanged: null,
            cancellationToken: cts.Token);

        var visits = _sessionService.GetVisits();
        Assert.True(visits.Count >= 3);
        Assert.Equal("main", visits[0].BranchName);
        Assert.Equal("feature/x", visits[1].BranchName);
        Assert.Equal("main", visits[2].BranchName);
    }
}
