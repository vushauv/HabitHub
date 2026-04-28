---
name: commit
description: Stage and commit changes using Conventional Commits format prefixed with the ticket ID parsed from the current git branch. Trigger when the user says "commit", "commit my changes", "/commit", or asks to create a git commit.
---

# Commit skill

Create a git commit using Conventional Commits format with the ticket ID at the very beginning.

## Format

```
<TICKET> <type>(<scope>): <subject>

<optional body explaining why, not what>
```

Examples:
- `HABITHUB-61 feat(backend): add Serilog logging`
- `HABITHUB-80 refactor(tests): extract shared setup helpers`
- `HABITHUB-57 fix(api): return 404 instead of 500 on missing team`
- `HABITHUB-12 chore: bump Npgsql to 8.0.11`

Rules:
- Subject ≤ 72 chars total (including ticket).
- Subject in imperative mood, lowercase after the type, no trailing period.
- Scope optional. Use only when it clarifies (e.g., `backend`, `frontend`, `tests`, `ci`).
- Body only when the "why" is non-obvious. Skip otherwise.

## Conventional Commits types

| Type | Use for |
|------|---------|
| `feat` | new user-facing feature |
| `fix` | bug fix |
| `refactor` | code change, no behavior change |
| `perf` | performance improvement |
| `test` | tests only |
| `docs` | documentation only |
| `chore` | tooling, deps, config |
| `build` | build system / package changes |
| `ci` | CI config |
| `style` | formatting, whitespace |
| `revert` | revert a prior commit |

## Steps

1. Run these in parallel to understand the change:
   - `git status` — see staged + unstaged + untracked
   - `git diff` — unstaged content
   - `git diff --staged` — already-staged content
   - `git log --oneline -10` — recent commit style for reference
   - `git rev-parse --abbrev-ref HEAD` — current branch name

2. **Extract ticket** from branch name. Match regex `[A-Z]+-\d+` (e.g., `HABITHUB-61` from `feature/HABITHUB-61-logs-for-backend`). If no match, ask the user for the ticket ID before continuing.

3. **Pick the type** by inspecting the diff:
   - New endpoint, service, or visible feature → `feat`
   - Bug being corrected → `fix`
   - Restructure with same behavior → `refactor`
   - Only `*.Tests/` files → `test`
   - Only `.csproj` package bumps → `chore` (or `build` for build pipeline)
   - Only `.md` files → `docs`

4. **Pick scope** (optional). Common scopes in this repo: `backend`, `frontend`, `tests`, `ci`, `db`. Skip scope if change touches many areas.

5. **Write subject**: imperative, ≤ 72 chars including ticket.

6. **Decide body**: include only if reviewer would not understand the why from the diff. Reference incidents, constraints, or trade-offs. Keep ≤ 3 short lines.

7. **Stage files** by name (never `git add -A` or `git add .`). Skip:
   - `TEST_SETUP.md` (personal notes — see memory)
   - `.env`, credentials, anything matching `*secret*`, `*credentials*`
   - Large binaries unless the user named them

8. **Show the user the drafted message and the file list before committing.** Ask for confirmation. The commit step is not pre-approved — wait for the user to approve the commit tool call.

9. On approval, commit with HEREDOC:
   ```
   git commit -m "$(cat <<'EOF'
   HABITHUB-XX type(scope): subject

   Optional body line.

   Co-Authored-By: Claude Opus 4.7 <noreply@anthropic.com>
   EOF
   )"
   ```

10. After commit, run `git status` to confirm tree clean and report the new commit hash.

## Do not

- Do not amend prior commits — always create a new commit.
- Do not use `--no-verify` or skip hooks.
- Do not push. User pushes manually.
- Do not commit if working tree is empty — tell the user there's nothing to commit.
- Do not silently fall back to a generic ticket like `NONE` — ask if missing.
