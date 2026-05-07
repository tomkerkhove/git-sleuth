namespace GitNavigator.Cli.Models;

public class BranchVisit
{
    public string BranchName { get; set; } = string.Empty;
    public DateTimeOffset VisitedAt { get; set; }
    public string? WorkingDirectory { get; set; }
}
