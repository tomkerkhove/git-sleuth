# Automatic Branch Tracking

Git Sleuth's `watch` command acts as a silent sleuth — a background agent that continuously monitors your Git repository and records every branch switch automatically, without any manual intervention.

---

## How it works

When you run `git-sleuth watch`, it starts a polling loop that:

1. Calls `git branch --show-current` in the watched directory at a regular interval (default: every 2 seconds)
2. Compares the result to the last recorded branch
3. If the branch has changed, records a new visit (branch name, timestamp, directory) to the session log
4. Repeats until you press **Ctrl+C**

Because the detection is based on polling the repository state rather than intercepting shell commands, it catches branch switches made by **any tool** — the terminal, VS Code's Source Control panel, GitHub Desktop, GitKraken, or any other Git client.

---

## Session scoping

Every terminal window (or tab) has its own independent session history. Git Sleuth identifies a session by the **parent process ID** (the PID of your shell), so:

- All `git-sleuth` invocations within the same terminal share the same session file
- Opening a new terminal starts a fresh session
- History is stored in your system's temp folder and is cleaned up automatically by the OS

### Pinning a session ID

If you need multiple terminal windows to share the same history — for example, in a CI pipeline or a scripted workflow — set the `GIT_NAVIGATOR_SESSION` environment variable to any string:

```bash
export GIT_NAVIGATOR_SESSION=my-feature-sprint
git-sleuth watch &
```

Any other terminal with the same `GIT_NAVIGATOR_SESSION` value will read from and write to the same session file.

---

## Recommended workflows

### Terminal split-pane setup

The simplest way to use the sleuth is to dedicate one pane to it and work in other panes:

```
┌────────────────────────┬────────────────────────┐
│  git-sleuth watch   │  your normal work here │
│                        │                        │
│ [09:14] feature/login  │  $ git checkout main   │
│ [09:31] main           │  $ vim README.md       │
│ [09:33] hotfix/typo    │  $ git switch hotfix   │
└────────────────────────┴────────────────────────┘
```

### Silent background mode

If you don't want to dedicate a pane, run the sleuth silently in the background:

```bash
# Start silently in the background
git-sleuth watch --quiet &

# Do your work normally...
git checkout feature/x
git checkout main

# Review when ready
git-sleuth log
```

> **Tip:** Add `git-sleuth watch --quiet &` to your shell's per-directory hook (e.g. a `.envrc` file with [direnv](https://direnv.net/)) to auto-start tracking whenever you `cd` into a repository.

### Watching multiple repositories at once

Start a watcher for each repository you're actively working in:

```bash
git-sleuth watch --directory ~/projects/frontend --quiet &
git-sleuth watch --directory ~/projects/backend --quiet &
```

Both watchers share the same session, so `git-sleuth log` will show branches from all repositories interleaved chronologically.

---

## Tuning the polling interval

The default interval of 2 seconds is a balance between responsiveness and resource use. You can adjust it:

```bash
# More responsive — detect switches within 1 second
git-sleuth watch --interval 1

# Lower resource use — check every 5 seconds
git-sleuth watch --interval 5
```

The minimum effective interval is 1 second (values lower than 1 are clamped to 1).

---

## Limitations

| Situation | Behaviour |
|---|---|
| **Detached HEAD** | The sleuth cannot record a branch name when HEAD is detached (e.g. after `git checkout <commit-sha>`). That state is silently skipped. |
| **Outside a Git repo** | If the watched directory is not inside a Git repository, `git` returns an error. The sleuth ignores it and keeps polling. |
| **Repository deleted** | If the directory disappears while the sleuth is running, it continues to poll silently. No error is raised. |
| **Multiple branches from one repo** | Each poll captures the single checked-out branch. Worktrees each need their own watcher pointed at the worktree directory. |
