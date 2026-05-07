# Git Navigator

Git Navigator is your **travel buddy** CLI — a .NET global tool that automatically tracks every Git branch you visit during a CLI session.

Run it as a background sleuth and it silently watches your repository for branch changes, recording your complete navigation history. Never lose track of where you've been.

```
🔍 Git Navigator sleuth is watching '/projects/my-repo' (every 2s). Press Ctrl+C to stop.

[09:14:02] Switched to branch 'feature/login'
[09:31:17] Switched to branch 'main'
[09:33:05] Switched to branch 'hotfix/typo'

Sleuth stopped. Recorded 3 visits across 3 unique branches this session.
```

---

## Installation

Requires [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0) or later.

```bash
dotnet tool install --global GitNavigator.Cli
```

---

## Quick start

```bash
# 1. Start the sleuth in your repo
cd ~/projects/my-repo
git-navigator watch

# 2. Switch branches however you like — terminal, IDE, GUI — the sleuth catches them all

# 3. Review your journey at any time
git-navigator log     # full history with timestamps
git-navigator list    # unique branches visited
```

---

## Commands

| Command | Description |
|---|---|
| `git-navigator watch` | 🔍 Start the sleuth — auto-detect and record every branch change |
| `git-navigator log` | Show the full chronological visit history |
| `git-navigator list` | Show the unique branches visited this session |
| `git-navigator visit [branch]` | Manually record a branch visit |
| `git-navigator clear` | Reset the session history |

---

## Documentation

| Guide | Description |
|---|---|
| [Getting Started](docs/getting-started.md) | Installation, first session walkthrough |
| [Commands Reference](docs/commands.md) | All commands, options, and examples |
| [Automatic Tracking](docs/automatic-tracking.md) | How the sleuth works, session scoping, workflows, and limitations |

---

## License Information

This is licensed under The MIT License (MIT). Which means that you can use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the web application. But you always need to state that Tom Kerkhove is the original author of this application.

Read the full license [here](LICENSE).

