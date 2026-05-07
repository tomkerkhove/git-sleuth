using System.CommandLine;
using GitNavigator.Cli.Services;

namespace GitNavigator.Cli.Commands;

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
