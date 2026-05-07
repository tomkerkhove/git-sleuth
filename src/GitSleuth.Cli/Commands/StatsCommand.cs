using System.CommandLine;
using GitSleuth.Cli.Services;

namespace GitSleuth.Cli.Commands;

public static class StatsCommand
{
    public static Command Build(SessionService sessionService)
    {
        var command = new Command("stats", "Show statistics about the branches visited in the current session.");

        command.SetHandler(() =>
        {
            var visits = sessionService.GetVisits();
            if (visits.Count == 0)
            {
                Console.WriteLine("No branches have been visited in this session yet.");
                Console.WriteLine("Use 'git-sleuth visit' to record a branch visit.");
                return;
            }

            var branchCounts = sessionService.GetBranchVisitCounts();
            var uniqueCount = branchCounts.Count;
            var totalVisits = visits.Count;

            Console.WriteLine($"Session statistics ({totalVisits} visit{(totalVisits == 1 ? "" : "s")}, {uniqueCount} unique branch{(uniqueCount == 1 ? "" : "es")}):");
            Console.WriteLine();

            var sessionStart = visits[0].VisitedAt.ToLocalTime();
            var sessionEnd = visits[^1].VisitedAt.ToLocalTime();
            var duration = sessionEnd - sessionStart;

            Console.WriteLine($"  Session started:  {sessionStart:yyyy-MM-dd HH:mm:ss}");
            Console.WriteLine($"  Session ended:    {sessionEnd:yyyy-MM-dd HH:mm:ss}");
            Console.WriteLine($"  Duration:         {FormatDuration(duration)}");
            Console.WriteLine();

            Console.WriteLine("  Branch visit counts (most visited first):");
            Console.WriteLine();

            var labelWidth = branchCounts.Max(x => x.BranchName.Length);
            foreach (var (branch, count) in branchCounts)
            {
                Console.WriteLine($"    {branch.PadRight(labelWidth)}  {count} {(count == 1 ? "visit" : "visits")}");
            }
        });

        return command;
    }

    private static string FormatDuration(TimeSpan duration)
    {
        if (duration.TotalHours >= 1)
        {
            return $"{(int)duration.TotalHours}h {duration.Minutes}m {duration.Seconds}s";
        }

        return $"{(int)duration.TotalMinutes}m {duration.Seconds}s";
    }
}
