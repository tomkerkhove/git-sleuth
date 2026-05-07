using GitSleuth.Cli.Services;
using Xunit;

namespace GitSleuth.Cli.Tests.Services;

public class SessionServiceTests : IDisposable
{
    private readonly string _sessionFilePath;
    private readonly SessionService _sut;

    public SessionServiceTests()
    {
        _sessionFilePath = Path.Combine(Path.GetTempPath(), $"git-sleuth-test-{Guid.NewGuid():N}.json");
        _sut = new SessionService(_sessionFilePath);
    }

    public void Dispose()
    {
        if (File.Exists(_sessionFilePath))
        {
            File.Delete(_sessionFilePath);
        }
    }

    [Fact]
    public void GetVisits_WhenNoVisitsRecorded_ReturnsEmptyList()
    {
        var visits = _sut.GetVisits();

        Assert.Empty(visits);
    }

    [Fact]
    public void RecordVisit_SingleVisit_AppearsInLog()
    {
        _sut.RecordVisit("main");

        var visits = _sut.GetVisits();
        Assert.Single(visits);
        Assert.Equal("main", visits[0].BranchName);
    }

    [Fact]
    public void RecordVisit_StoresWorkingDirectory()
    {
        var workDir = "/some/repo/path";
        _sut.RecordVisit("feature/test", workDir);

        var visits = _sut.GetVisits();
        Assert.Single(visits);
        Assert.Equal("/some/repo/path", visits[0].WorkingDirectory);
    }

    [Fact]
    public void RecordVisit_StoresCommitSha()
    {
        _sut.RecordVisit("main", "/repo", "abc1234");

        var visits = _sut.GetVisits();
        Assert.Single(visits);
        Assert.Equal("abc1234", visits[0].CommitSha);
    }

    [Fact]
    public void RecordVisit_CommitShaIsNull_WhenNotProvided()
    {
        _sut.RecordVisit("main");

        var visits = _sut.GetVisits();
        Assert.Single(visits);
        Assert.Null(visits[0].CommitSha);
    }

    [Fact]
    public void RecordVisit_MultipleVisits_AllAppearInChronologicalOrder()
    {
        _sut.RecordVisit("main");
        _sut.RecordVisit("feature/one");
        _sut.RecordVisit("feature/two");

        var visits = _sut.GetVisits();
        Assert.Equal(3, visits.Count);
        Assert.Equal("main", visits[0].BranchName);
        Assert.Equal("feature/one", visits[1].BranchName);
        Assert.Equal("feature/two", visits[2].BranchName);
    }

    [Fact]
    public void RecordVisit_SameBranchMultipleTimes_AllVisitsAreRecorded()
    {
        _sut.RecordVisit("main");
        _sut.RecordVisit("feature/one");
        _sut.RecordVisit("main");

        var visits = _sut.GetVisits();
        Assert.Equal(3, visits.Count);
    }

    [Fact]
    public void GetUniqueBranches_ReturnsEachBranchOnce()
    {
        _sut.RecordVisit("main");
        _sut.RecordVisit("feature/one");
        _sut.RecordVisit("main");
        _sut.RecordVisit("feature/two");
        _sut.RecordVisit("main");

        var unique = _sut.GetUniqueBranches();
        Assert.Equal(3, unique.Count);
        Assert.Contains("main", unique);
        Assert.Contains("feature/one", unique);
        Assert.Contains("feature/two", unique);
    }

    [Fact]
    public void GetUniqueBranches_PreservesOrderOfFirstVisit()
    {
        _sut.RecordVisit("feature/two");
        _sut.RecordVisit("main");
        _sut.RecordVisit("feature/two");

        var unique = _sut.GetUniqueBranches();
        Assert.Equal(2, unique.Count);
        Assert.Equal("feature/two", unique[0]);
        Assert.Equal("main", unique[1]);
    }

    [Fact]
    public void GetBranchVisitCounts_WhenNoVisits_ReturnsEmptyList()
    {
        var counts = _sut.GetBranchVisitCounts();

        Assert.Empty(counts);
    }

    [Fact]
    public void GetBranchVisitCounts_SingleBranch_ReturnsCountOfOne()
    {
        _sut.RecordVisit("main");

        var counts = _sut.GetBranchVisitCounts();

        Assert.Single(counts);
        Assert.Equal("main", counts[0].BranchName);
        Assert.Equal(1, counts[0].Count);
    }

    [Fact]
    public void GetBranchVisitCounts_MultipleBranches_SortedByCountDescending()
    {
        _sut.RecordVisit("main");
        _sut.RecordVisit("feature/one");
        _sut.RecordVisit("main");
        _sut.RecordVisit("main");
        _sut.RecordVisit("feature/one");

        var counts = _sut.GetBranchVisitCounts();

        Assert.Equal(2, counts.Count);
        Assert.Equal("main", counts[0].BranchName);
        Assert.Equal(3, counts[0].Count);
        Assert.Equal("feature/one", counts[1].BranchName);
        Assert.Equal(2, counts[1].Count);
    }

    [Fact]
    public void GetBranchVisitCounts_TiesAreResolvedAlphabetically()
    {
        _sut.RecordVisit("zebra");
        _sut.RecordVisit("alpha");

        var counts = _sut.GetBranchVisitCounts();

        Assert.Equal(2, counts.Count);
        Assert.Equal("alpha", counts[0].BranchName);
        Assert.Equal("zebra", counts[1].BranchName);
    }

    [Fact]
    public void GetBranchVisitCounts_AllCountsAreAccurate()
    {
        _sut.RecordVisit("main");
        _sut.RecordVisit("feature/one");
        _sut.RecordVisit("main");
        _sut.RecordVisit("feature/two");

        var counts = _sut.GetBranchVisitCounts();
        var dict = counts.ToDictionary(x => x.BranchName, x => x.Count);

        Assert.Equal(2, dict["main"]);
        Assert.Equal(1, dict["feature/one"]);
        Assert.Equal(1, dict["feature/two"]);
    }

    [Fact]
    public void ClearSession_RemovesAllVisits()
    {
        _sut.RecordVisit("main");
        _sut.RecordVisit("feature/one");
        _sut.ClearSession();

        var visits = _sut.GetVisits();
        Assert.Empty(visits);
    }

    [Fact]
    public void ClearSession_WhenAlreadyEmpty_DoesNotThrow()
    {
        var exception = Record.Exception(() => _sut.ClearSession());
        Assert.Null(exception);
    }

    [Fact]
    public void RecordVisit_VisitedAtIsPopulated()
    {
        var before = DateTimeOffset.UtcNow.AddSeconds(-1);
        _sut.RecordVisit("main");
        var after = DateTimeOffset.UtcNow.AddSeconds(1);

        var visits = _sut.GetVisits();
        Assert.Single(visits);
        Assert.InRange(visits[0].VisitedAt, before, after);
    }

    [Fact]
    public void GetVisits_WhenSessionFileIsCorrupted_ReturnsEmptyList()
    {
        File.WriteAllText(_sessionFilePath, "this is not valid json {{{");

        var visits = _sut.GetVisits();
        Assert.Empty(visits);
    }

    [Fact]
    public void SessionService_CanBeUsedWithNewInstance_PersistedDataIsLoaded()
    {
        _sut.RecordVisit("main");
        _sut.RecordVisit("develop");

        // Create a new instance pointing to the same file to simulate separate invocations
        var secondInstance = new SessionService(_sessionFilePath);
        var visits = secondInstance.GetVisits();

        Assert.Equal(2, visits.Count);
        Assert.Equal("main", visits[0].BranchName);
        Assert.Equal("develop", visits[1].BranchName);
    }

    [Fact]
    public void GetShellSessionId_WhenEnvVarIsSet_ReturnsEnvVarValue()
    {
        var originalValue = Environment.GetEnvironmentVariable("GIT_SLEUTH_SESSION");
        try
        {
            Environment.SetEnvironmentVariable("GIT_SLEUTH_SESSION", "test-session-123");
            var id = SessionService.GetShellSessionId();
            Assert.Equal("test-session-123", id);
        }
        finally
        {
            Environment.SetEnvironmentVariable("GIT_SLEUTH_SESSION", originalValue);
        }
    }

    [Fact]
    public void GetShellSessionId_WhenEnvVarIsNotSet_ReturnsNonEmptyString()
    {
        var originalValue = Environment.GetEnvironmentVariable("GIT_SLEUTH_SESSION");
        try
        {
            Environment.SetEnvironmentVariable("GIT_SLEUTH_SESSION", null);
            var id = SessionService.GetShellSessionId();
            Assert.False(string.IsNullOrWhiteSpace(id));
        }
        finally
        {
            Environment.SetEnvironmentVariable("GIT_SLEUTH_SESSION", originalValue);
        }
    }
}
