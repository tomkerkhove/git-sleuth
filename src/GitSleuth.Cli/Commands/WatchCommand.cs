using System.CommandLine;
using System.CommandLine.Invocation;
using GitSleuth.Cli.Services;

namespace GitSleuth.Cli.Commands;

public static class WatchCommand
{
    public static Command Build(SessionService sessionService)
    {
        var directoryOption = new Option<string>(
            name: "--directory",
            description: "Path to the Git repository to watch. Defaults to the current directory.",
            getDefaultValue: Directory.GetCurrentDirectory);

        var intervalOption = new Option<int>(
            name: "--interval",
            description: "Polling interval in seconds.",
            getDefaultValue: () => 2);

        var quietOption = new Option<bool>(
            name: "--quiet",
            description: "Suppress output when a branch change is detected.");

        var command = new Command("watch", "Run as a background sleuth, automatically tracking every branch change.")
        {
            directoryOption,
            intervalOption,
            quietOption
        };

        command.SetHandler(async (InvocationContext context) =>
        {
            var directory = context.ParseResult.GetValueForOption(directoryOption)!;
            var intervalSeconds = context.ParseResult.GetValueForOption(intervalOption);
            var quiet = context.ParseResult.GetValueForOption(quietOption);
            var cancellationToken = context.GetCancellationToken();

            var interval = TimeSpan.FromSeconds(Math.Max(1, intervalSeconds));
            var watchService = new WatchService(sessionService);

            if (!quiet)
            {
                Console.WriteLine($"🔍 Git Sleuth sleuth is watching '{directory}' (every {interval.TotalSeconds}s). Press Ctrl+C to stop.");
                Console.WriteLine();
            }

            await watchService.WatchAsync(
                directory,
                interval,
                visit =>
                {
                    if (!quiet)
                    {
                        Console.WriteLine($"[{visit.VisitedAt.ToLocalTime():HH:mm:ss}] Switched to branch '{visit.BranchName}'");
                    }
                },
                cancellationToken);

            if (!quiet)
            {
                Console.WriteLine();
                var visits = sessionService.GetVisits();
                var uniqueCount = sessionService.GetUniqueBranches().Count;
                Console.WriteLine($"Sleuth stopped. Recorded {visits.Count} visit{(visits.Count == 1 ? "" : "s")} across {uniqueCount} unique branch{(uniqueCount == 1 ? "" : "es")} this session.");
            }
        });

        return command;
    }
}
