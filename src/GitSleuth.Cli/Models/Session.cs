namespace GitNavigator.Cli.Models;

public class Session
{
    public string SessionId { get; set; } = string.Empty;
    public List<BranchVisit> Visits { get; set; } = [];
}
