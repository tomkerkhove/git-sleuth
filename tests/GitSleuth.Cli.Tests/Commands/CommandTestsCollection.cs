using Xunit;

namespace GitSleuth.Cli.Tests.Commands;

/// <summary>
/// Marks command test classes as belonging to the same collection so they run
/// sequentially and don't race on Console.Out redirection.
/// </summary>
[CollectionDefinition("CommandTests")]
public class CommandTestsCollection { }
