using GitNavigator.Cli.Models;

namespace GitNavigator.Cli.Services;

/// <summary>
/// Watches a directory for Git branch changes and records visits automatically.
/// </summary>
public class WatchService
{
    private readonly SessionService _sessionService;
    private readonly Func<string, string?> _getCurrentBranch;

    public WatchService(SessionService sessionService)
        : this(sessionService, GitService.GetCurrentBranch)
    {
    }

    public WatchService(SessionService sessionService, Func<string, string?> getCurrentBranch)
    {
        _sessionService = sessionService;
        _getCurrentBranch = getCurrentBranch;
    }

    /// <summary>
    /// Monitors the specified directory for branch changes, recording each switch,
    /// until the cancellation token is triggered (e.g. Ctrl+C).
    /// </summary>
    /// <param name="directory">The working directory of the Git repository to watch.</param>
    /// <param name="interval">How often to poll for a branch change.</param>
    /// <param name="onBranchChanged">
    /// Optional callback invoked each time a branch change is detected.
    /// Receives the new <see cref="BranchVisit"/> that was recorded.
    /// </param>
    /// <param name="cancellationToken">Token used to stop the watch loop.</param>
    public async Task WatchAsync(
        string directory,
        TimeSpan interval,
        Action<BranchVisit>? onBranchChanged,
        CancellationToken cancellationToken)
    {
        string? lastBranch = null;

        while (!cancellationToken.IsCancellationRequested)
        {
            var currentBranch = _getCurrentBranch(directory);

            if (!string.IsNullOrWhiteSpace(currentBranch) && currentBranch != lastBranch)
            {
                var visit = new BranchVisit
                {
                    BranchName = currentBranch,
                    VisitedAt = DateTimeOffset.UtcNow,
                    WorkingDirectory = directory
                };

                _sessionService.RecordVisit(currentBranch, directory);
                onBranchChanged?.Invoke(visit);

                lastBranch = currentBranch;
            }

            try
            {
                await Task.Delay(interval, cancellationToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }
    }
}
