# Commands Reference

Git Sleuth exposes five commands. Run any command with `--help` to see its options inline.

```
git-sleuth [command] [options]
```

---

## `watch` — automatic sleuth mode ⭐

> Run as a background agent that automatically detects and records every branch change.

```bash
git-sleuth watch [options]
```

| Option | Default | Description |
|---|---|---|
| `--directory <path>` | current directory | Path to the Git repository to watch |
| `--interval <seconds>` | `2` | How often (in seconds) to poll for a branch change |
| `--quiet` | false | Suppress output; track silently with no console noise |

### Examples

Watch the current directory with default settings:

```bash
git-sleuth watch
```

Watch a specific repository every second:

```bash
git-sleuth watch --directory ~/projects/my-repo --interval 1
```

Watch silently (no output) — useful when running in the background:

```bash
git-sleuth watch --quiet &
```

---

## `log` — full visit history

> Show the complete chronological list of branch visits recorded this session.

```bash
git-sleuth log
```

Each entry shows:
- A sequential index
- The local timestamp of the visit
- The branch name
- The working directory at the time

### Example output

```
Branch visit log (4 visits):

    1. [2026-05-07 09:14:02]  feature/login
       /Users/you/projects/my-repo
    2. [2026-05-07 09:31:17]  main
       /Users/you/projects/my-repo
    3. [2026-05-07 09:33:05]  hotfix/typo
       /Users/you/projects/my-repo
    4. [2026-05-07 09:40:01]  main
       /Users/you/projects/my-repo
```

---

## `list` — unique branches visited

> Show the distinct set of branches visited this session, in the order they were first visited.

```bash
git-sleuth list
```

Visits to the same branch more than once are deduplicated — only the first visit is counted for ordering.

### Example output

```
Branches visited this session (3 unique):

  - feature/login
  - main
  - hotfix/typo
```

---

## `visit` — manual record

> Manually record a visit to a branch. Useful when you don't want to run the sleuth continuously, or for scripting.

```bash
git-sleuth visit [<branch>]
```

| Argument | Description |
|---|---|
| `branch` (optional) | Branch name to record. If omitted, auto-detects the current branch via `git branch --show-current`. |

### Examples

Record the current branch automatically:

```bash
git-sleuth visit
```

Record a specific branch by name:

```bash
git-sleuth visit feature/payments
```

---

## `clear` — reset session history

> Remove all visits recorded in the current session.

```bash
git-sleuth clear
```

This is useful when you want to start a fresh travel log without opening a new terminal window.

### Example

```bash
$ git-sleuth list
Branches visited this session (3 unique):

  - main
  - feature/login
  - hotfix/typo

$ git-sleuth clear
Session history cleared.

$ git-sleuth list
No branches have been visited in this session yet.
```
