using System.CommandLine;
using GitSleuth.Cli.Services;

namespace GitSleuth.Cli.Commands;

public static class LogCommand
{
    public static Command Build(SessionService sessionService)
    {
        var command = new Command("log", "Show the full log of branch visits in the current session.");

        command.SetHandler(() =>
        {
            var visits = sessionService.GetVisits();
            if (visits.Count == 0)
            {
                Console.WriteLine("No branches have been visited in this session yet.");
                Console.WriteLine("Use 'git-sleuth visit' to record a branch visit.");
                return;
            }

            Console.WriteLine($"Branch visit log ({visits.Count} visit{(visits.Count == 1 ? "" : "s")}):");
            Console.WriteLine();

            for (int i = 0; i < visits.Count; i++)
            {
                var visit = visits[i];
                var localTime = visit.VisitedAt.ToLocalTime();
                var commitSuffix = string.IsNullOrWhiteSpace(visit.CommitSha) ? "" : $"  ({visit.CommitSha})";
                Console.WriteLine($"  {i + 1,3}. [{localTime:yyyy-MM-dd HH:mm:ss}]  {visit.BranchName}{commitSuffix}");
                if (!string.IsNullOrWhiteSpace(visit.WorkingDirectory))
                {
                    Console.WriteLine($"       {visit.WorkingDirectory}");
                }
            }
        });

        return command;
    }
}
