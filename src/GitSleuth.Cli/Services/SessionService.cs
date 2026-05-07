using System.Text.Json;
using GitSleuth.Cli.Models;

namespace GitSleuth.Cli.Services;

public class SessionService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true
    };

    private readonly string _sessionFilePath;

    public SessionService() : this(ResolveSessionFilePath())
    {
    }

    public SessionService(string sessionFilePath)
    {
        _sessionFilePath = sessionFilePath;
    }

    /// <summary>
    /// Records a visit to the specified branch, optionally capturing the working directory.
    /// </summary>
    public void RecordVisit(string branchName, string? workingDirectory = null)
    {
        var session = LoadSession();
        session.Visits.Add(new BranchVisit
        {
            BranchName = branchName,
            VisitedAt = DateTimeOffset.UtcNow,
            WorkingDirectory = workingDirectory
        });
        SaveSession(session);
    }

    /// <summary>
    /// Returns all branch visits recorded in the current session, in chronological order.
    /// </summary>
    public IReadOnlyList<BranchVisit> GetVisits()
    {
        return LoadSession().Visits.AsReadOnly();
    }

    /// <summary>
    /// Returns the distinct branches visited in the current session (deduplicated, order of first visit).
    /// </summary>
    public IReadOnlyList<string> GetUniqueBranches()
    {
        return LoadSession().Visits
            .Select(v => v.BranchName)
            .Distinct(StringComparer.Ordinal)
            .ToList()
            .AsReadOnly();
    }

    /// <summary>
    /// Clears all visits recorded in the current session.
    /// </summary>
    public void ClearSession()
    {
        var session = LoadSession();
        session.Visits.Clear();
        SaveSession(session);
    }

    private Session LoadSession()
    {
        if (!File.Exists(_sessionFilePath))
        {
            return new Session { SessionId = Path.GetFileNameWithoutExtension(_sessionFilePath) };
        }

        try
        {
            var json = File.ReadAllText(_sessionFilePath);
            return JsonSerializer.Deserialize<Session>(json, JsonOptions)
                   ?? new Session { SessionId = Path.GetFileNameWithoutExtension(_sessionFilePath) };
        }
        catch (JsonException)
        {
            return new Session { SessionId = Path.GetFileNameWithoutExtension(_sessionFilePath) };
        }
    }

    private void SaveSession(Session session)
    {
        var json = JsonSerializer.Serialize(session, JsonOptions);
        File.WriteAllText(_sessionFilePath, json);
    }

    /// <summary>
    /// Resolves the path to the session file for the current shell session.
    /// The session is identified by the parent process ID so that all invocations
    /// within the same terminal share the same history file.
    /// </summary>
    private static string ResolveSessionFilePath()
    {
        var sessionId = GetShellSessionId();
        var fileName = $"git-sleuth-session-{sessionId}.json";
        return Path.Combine(Path.GetTempPath(), fileName);
    }

    /// <summary>
    /// Returns a stable identifier for the current shell session.
    /// Checks the GIT_SLEUTH_SESSION environment variable first, then falls back
    /// to the parent process ID (Unix) or Windows logon session ID.
    /// </summary>
    internal static string GetShellSessionId()
    {
        // Allow callers to pin a session ID explicitly (useful for shell integrations and tests)
        var envSessionId = Environment.GetEnvironmentVariable("GIT_SLEUTH_SESSION");
        if (!string.IsNullOrWhiteSpace(envSessionId))
        {
            return envSessionId;
        }

        try
        {
            var ppid = GetParentProcessId(Environment.ProcessId);
            return ppid > 0 ? ppid.ToString() : Environment.ProcessId.ToString();
        }
        catch
        {
            return Environment.ProcessId.ToString();
        }
    }

    private static int GetParentProcessId(int pid)
    {
        try
        {
            // On Unix-like systems, read /proc/<pid>/status for the PPid field
            var statusFile = $"/proc/{pid}/status";
            if (!File.Exists(statusFile))
            {
                return -1;
            }

            foreach (var line in File.ReadLines(statusFile))
            {
                if (line.StartsWith("PPid:", StringComparison.OrdinalIgnoreCase))
                {
                    var parts = line.Split(':', StringSplitOptions.TrimEntries);
                    if (parts.Length >= 2 && int.TryParse(parts[1], out var ppid))
                    {
                        return ppid;
                    }
                }
            }

            return -1;
        }
        catch
        {
            return -1;
        }
    }
}
