using System.CommandLine;
using GitSleuth.Cli.Services;

namespace GitSleuth.Cli.Commands;

public static class ListCommand
{
    public static Command Build(SessionService sessionService)
    {
        var command = new Command("list", "Show the unique branches visited in the current session.");

        command.SetHandler(() =>
        {
            var branches = sessionService.GetUniqueBranches();
            if (branches.Count == 0)
            {
                Console.WriteLine("No branches have been visited in this session yet.");
                Console.WriteLine("Use 'git-sleuth visit' to record a branch visit.");
                return;
            }

            Console.WriteLine($"Branches visited this session ({branches.Count} unique):");
            Console.WriteLine();

            foreach (var branch in branches)
            {
                Console.WriteLine($"  - {branch}");
            }
        });

        return command;
    }
}
