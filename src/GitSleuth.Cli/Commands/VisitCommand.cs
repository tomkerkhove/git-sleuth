using System.CommandLine;
using GitSleuth.Cli.Services;

namespace GitSleuth.Cli.Commands;

public static class VisitCommand
{
    public static Command Build(SessionService sessionService)
    {
        var branchArgument = new Argument<string?>(
            name: "branch",
            description: "Name of the branch to record a visit for. Defaults to the current git branch.",
            getDefaultValue: () => null);

        var command = new Command("visit", "Record a visit to a Git branch.")
        {
            branchArgument
        };

        command.SetHandler(branch =>
        {
            var workingDirectory = Directory.GetCurrentDirectory();
            var branchName = branch ?? GitService.GetCurrentBranch(workingDirectory);
            if (string.IsNullOrWhiteSpace(branchName))
            {
                Console.Error.WriteLine("Could not determine the current branch. Please specify a branch name explicitly.");
                return;
            }

            sessionService.RecordVisit(branchName, workingDirectory);
            Console.WriteLine($"Visited branch '{branchName}'.");
        }, branchArgument);

        return command;
    }
}
