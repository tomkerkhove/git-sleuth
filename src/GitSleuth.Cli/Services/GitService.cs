using System.Diagnostics;

namespace GitSleuth.Cli.Services;

public static class GitService
{
    /// <summary>
    /// Returns the name of the currently checked-out branch in the given working directory,
    /// or null if the directory is not a Git repository or is in a detached HEAD state.
    /// </summary>
    public static string? GetCurrentBranch(string workingDirectory)
    {
        try
        {
            var psi = new ProcessStartInfo("git", "branch --show-current")
            {
                WorkingDirectory = workingDirectory,
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
