using System.CommandLine;
using GitSleuth.Cli.Commands;
using GitSleuth.Cli.Services;

var sessionService = new SessionService();

var rootCommand = new RootCommand("Git Sleuth - your travel buddy for tracking Git branch visits in a CLI session.");

rootCommand.AddCommand(VisitCommand.Build(sessionService));
rootCommand.AddCommand(WatchCommand.Build(sessionService));
rootCommand.AddCommand(LogCommand.Build(sessionService));
rootCommand.AddCommand(ListCommand.Build(sessionService));
rootCommand.AddCommand(StatsCommand.Build(sessionService));
rootCommand.AddCommand(ClearCommand.Build(sessionService));

return await rootCommand.InvokeAsync(args);
