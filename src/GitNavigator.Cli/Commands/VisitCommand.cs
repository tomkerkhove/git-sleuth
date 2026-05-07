using System.CommandLine;
using System.Diagnostics;
using GitNavigator.Cli.Services;

namespace GitNavigator.Cli.Commands;

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
            var branchName = branch ?? GetCurrentGitBranch();
            if (string.IsNullOrWhiteSpace(branchName))
            {
                Console.Error.WriteLine("Could not determine the current branch. Please specify a branch name explicitly.");
                return;
            }

            var workingDirectory = Directory.GetCurrentDirectory();
            sessionService.RecordVisit(branchName, workingDirectory);
            Console.WriteLine($"Visited branch '{branchName}'.");
        }, branchArgument);

        return command;
    }

    private static string? GetCurrentGitBranch()
    {
        try
        {
            var psi = new ProcessStartInfo("git", "branch --show-current")
            {
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false
            };

            using var process = Process.Start(psi);
            if (process is null)
            {
                return null;
            }

            var output = process.StandardOutput.ReadToEnd().Trim();
            process.WaitForExit();

            return process.ExitCode == 0 && !string.IsNullOrWhiteSpace(output) ? output : null;
        }
        catch
        {
            return null;
        }
    }
}
