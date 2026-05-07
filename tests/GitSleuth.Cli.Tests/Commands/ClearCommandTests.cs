using System.CommandLine;
using GitSleuth.Cli.Commands;
using GitSleuth.Cli.Services;
using Xunit;

namespace GitSleuth.Cli.Tests.Commands;

[Collection("CommandTests")]
public class ClearCommandTests : IDisposable
{
    private readonly string _sessionFilePath;
    private readonly SessionService _sessionService;

    public ClearCommandTests()
    {
        _sessionFilePath = Path.Combine(Path.GetTempPath(), $"git-sleuth-clear-test-{Guid.NewGuid():N}.json");
        _sessionService = new SessionService(_sessionFilePath);
    }

    public void Dispose()
    {
        if (File.Exists(_sessionFilePath))
        {
            File.Delete(_sessionFilePath);
        }
    }

    [Fact]
    public async Task Clear_RemovesAllVisits()
    {
        _sessionService.RecordVisit("main");
        _sessionService.RecordVisit("feature/one");

        var command = ClearCommand.Build(_sessionService);
        await command.InvokeAsync([]);

        Assert.Empty(_sessionService.GetVisits());
    }

    [Fact]
    public async Task Clear_PrintsConfirmationMessage()
    {
        var output = await CaptureOutputAsync(() => ClearCommand.Build(_sessionService).InvokeAsync([]));

        Assert.Contains("Session history cleared.", output);
    }

    [Fact]
    public async Task Clear_WhenAlreadyEmpty_DoesNotThrow()
    {
        var exception = await Record.ExceptionAsync(() =>
            ClearCommand.Build(_sessionService).InvokeAsync([]));

        Assert.Null(exception);
    }

    [Fact]
    public async Task Clear_WhenAlreadyEmpty_StillPrintsConfirmation()
    {
        var output = await CaptureOutputAsync(() => ClearCommand.Build(_sessionService).InvokeAsync([]));

        Assert.Contains("Session history cleared.", output);
    }

    private static async Task<string> CaptureOutputAsync(Func<Task<int>> action)
    {
        var original = Console.Out;
        using var writer = new StringWriter();
        Console.SetOut(writer);
        try
        {
            await action();
            return writer.ToString();
        }
        finally
        {
            Console.SetOut(original);
        }
    }
}
