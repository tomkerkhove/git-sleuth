# Git Navigator

Git Navigator is your **travel buddy** CLI that keeps track of all Git branches you visit during a CLI session.

Never lose track of where you've been — every branch switch is recorded so you can always look back at your journey.

## Installation

Install as a [.NET global tool](https://learn.microsoft.com/en-us/dotnet/core/tools/global-tools) (requires [.NET 9 SDK](https://dotnet.microsoft.com/download)):

```bash
dotnet tool install --global GitNavigator.Cli
```

After installation, the `git-navigator` command is available anywhere in your terminal.

## Usage

### Record a branch visit

```bash
# Automatically detect and record the current git branch
git-navigator visit

# Or specify a branch name explicitly
git-navigator visit <branch-name>
```

### View your branch visit log

```bash
# Full chronological log with timestamps and working directories
git-navigator log

# Unique branches visited (deduplicated)
git-navigator list
```

### Clear the session history

```bash
git-navigator clear
```

## Session Tracking

Branch visits are stored in a temporary JSON file keyed to your current shell session (by parent process ID), so:

- Every terminal window/tab has its own independent history.
- History is automatically scoped to the lifetime of your shell session.
- You can pin a session ID explicitly by setting the `GIT_NAVIGATOR_SESSION` environment variable — useful for shell integrations or scripting.

### Shell integration (optional)

For automatic tracking every time you run `git checkout` or `git switch`, add a `post-checkout` hook to your repository:

```bash
# .git/hooks/post-checkout
#!/bin/sh
git-navigator visit "$1"
```

Or set up a shell alias that records the visit:

```bash
# In your ~/.bashrc or ~/.zshrc
gco() {
  git checkout "$@" && git-navigator visit
}
```

## License Information

This is licensed under The MIT License (MIT). Which means that you can use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the web application. But you always need to state that Tom Kerkhove is the original author of this application.

Read the full license [here](LICENSE).

