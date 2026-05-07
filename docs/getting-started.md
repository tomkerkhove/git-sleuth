# Getting Started with Git Sleuth

Git Sleuth is a .NET global tool that automatically tracks every Git branch you visit during a CLI session — no manual tagging, no discipline required.

---

## Prerequisites

- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0) or later
- Git installed and available on your `PATH`

---

## Installation

Install git-sleuth as a [.NET global tool](https://learn.microsoft.com/en-us/dotnet/core/tools/global-tools):

```bash
dotnet tool install --global GitSleuth.Cli
```

Verify the installation worked:

```bash
git-sleuth --version
```

To upgrade to a newer version later:

```bash
dotnet tool update --global GitSleuth.Cli
```

To uninstall:

```bash
dotnet tool uninstall --global GitSleuth.Cli
```

---

## Your first session

### 1. Start the sleuth

Open a terminal, navigate to any Git repository, and start the automatic watcher:

```bash
cd ~/projects/my-repo
git-sleuth watch
```

You'll see:

```
🔍 Git Sleuth sleuth is watching '/Users/you/projects/my-repo' (every 2s). Press Ctrl+C to stop.
```

The sleuth is now running in the background of that terminal pane, silently watching for branch changes.

### 2. Switch branches normally

In another terminal pane (or a GUI like VS Code, GitHub Desktop, etc.), switch branches as you normally would:

```bash
git checkout feature/login
# work, work, work...
git checkout main
# review, review...
git switch hotfix/typo
```

Back in the sleuth pane you'll see each detection logged in real time:

```
[09:14:02] Switched to branch 'feature/login'
[09:31:17] Switched to branch 'main'
[09:33:05] Switched to branch 'hotfix/typo'
```

### 3. Review your journey

In any terminal pane, at any point:

```bash
# Full log with timestamps
git-sleuth log

# Just the unique branches (deduplicated)
git-sleuth list
```

Example `log` output:

```
Branch visit log (3 visits):

    1. [2026-05-07 09:14:02]  feature/login
       /Users/you/projects/my-repo
    2. [2026-05-07 09:31:17]  main
       /Users/you/projects/my-repo
    3. [2026-05-07 09:33:05]  hotfix/typo
       /Users/you/projects/my-repo
```

### 4. Stop the sleuth

Press **Ctrl+C** in the sleuth pane. It prints a quick summary and exits:

```
Sleuth stopped. Recorded 3 visits across 3 unique branches this session.
```

---

## What's next?

- [Commands reference](./commands.md) — all options and flags for every command
- [Automatic tracking in depth](./automatic-tracking.md) — how the sleuth works, session scoping, and tips
