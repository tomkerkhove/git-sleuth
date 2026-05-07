using System.CommandLine;
using GitSleuth.Cli.Services;

namespace GitSleuth.Cli.Commands;

public static class ClearCommand
{
    public static Command Build(SessionService sessionService)
    {
        var command = new Command("clear", "Clear all branch visits recorded in the current session.");

        command.SetHandler(() =>
        {
            sessionService.ClearSession();
            Console.WriteLine("Session history cleared.");
        });

        return command;
    }
}
