# GitHub Repository Setup — HabbitHub

Documentation of the GitHub configuration for branch management, protection rules, and deployment environments.

## Remark

Sure, many things here are to consider. That's just a skeleton.

## Branch Model

| Branch | Purpose | Deploys to |
|--------|---------|------------|
| `main` | Production — represents what's currently deployed | Production server |
| `staging` | Pre-production verification environment | Staging server |
| `dev` | Integration branch for active development | None (CI only) |

`main` is the default branch.

**Promotion flow:** `feature/*` → `dev` → `staging` → `main` (always via pull request, never direct push).

## Branch Protection (Rulesets)

### `main` (strictest)

- Restrict deletions: **on**
- Require pull request before merging: **on**
  - Required approvals: **2**
  - Dismiss stale approvals on new commits: **on**
  - Require approval of the most recent reviewable push: **on**
- Require status checks to pass: **on**
  - Require branch to be up to date: **on**
- Block force pushes: **on**
- Require linear history: **on** (enforces squash merge)

### `staging`

- Restrict deletions: **on**
- Require pull request before merging: **on**
  - Required approvals: **1**
  - Dismiss stale approvals on new commits: **on**
- Require status checks to pass: **on**
  - Require branch to be up to date: **on**
- Block force pushes: **on**

### `dev` (lightest)

- Restrict deletions: **on**
- Require pull request before merging: **on**
  - Required approvals: **1**
- Require status checks to pass: **on** (basic checks only — lint, build)
- Block force pushes: **on**

## Merge Strategy

Configured under Settings → General → Pull Requests:

- Allow merge commits: **off**
- Allow squash merging: **on** (primary and only method)
- Allow rebase merging: **off**
- Always suggest updating pull request branches: **on**
- Automatically delete head branches: **on**

### Why squash merging only?

When a PR is squash-merged, all of its commits are compressed into a single commit on the target branch. This means the branch history stays clean — each commit in `dev`, `staging`, or `main` represents exactly one complete unit of work (one feature, one bugfix, one refactor). Without this, the history fills up with intermediate commits like "wip", "fix typo", "actually fix it", "oops forgot a file" from every contributor, making it hard to understand what changed and when. The individual commit history is still preserved inside the closed PR on GitHub if anyone needs to dig into it.

Only one merge strategy is enabled to avoid inconsistency. If all three are available, different developers pick different ones and the git history becomes a mix of styles. A single option removes the decision entirely — there's just one "Merge" button.

## Branch Naming & Development Workflow

### Branch naming conventions

| Pattern | Used for | Example |
|---------|----------|---------|
| `feature/<short-description>` | New functionality | `feature/user-auth` |
| `bugfix/<short-description>` | Bug fixes | `bugfix/login-redirect-loop` |
| `hotfix/<short-description>` | Urgent production fixes | `hotfix/crash-on-empty-profile` |
| `chore/<short-description>` | Non-functional work (CI, deps, docs) | `chore/update-docker-base-image` |
| `refactor/<short-description>` | Code restructuring | `refactor/extract-notification-service` |

Use lowercase, hyphens as separators, keep names short but descriptive.

### Commit message convention

We follow [Conventional Commits](https://www.conventionalcommits.org/). Every commit message uses this format:

```
<type>(<optional scope>): <short description>
```

**Types:**

| Type | When to use |
|------|-------------|
| `feat` | A new feature or user-facing functionality |
| `fix` | A bug fix |
| `docs` | Documentation-only changes |
| `style` | Formatting, missing semicolons, etc. — no logic change |
| `refactor` | Code restructuring without changing behavior |
| `test` | Adding or updating tests |
| `chore` | Build process, CI, dependencies, tooling |
| `perf` | Performance improvements |
| `ci` | Changes to CI/CD configuration files and scripts |

**Scope** is optional and indicates the area of the codebase: `api`, `frontend`, `db`, `auth`, `docker`, etc.

**Examples:**

```
feat(api): add endpoint for habit streak calculation
fix(frontend): prevent crash when habit list is empty
chore(docker): upgrade postgres image to 16.3
docs: add commit convention to github-setup
refactor(api): extract notification logic into separate service
test(api): add integration tests for auth middleware
ci: add bearer security scan to PR workflow
```

**Breaking changes** are indicated by a `!` after the type/scope or by a `BREAKING CHANGE:` footer:

```
feat(api)!: change habit response schema to v2 format
```

Since we use squash merging, the squash commit message on `dev`/`staging`/`main` should follow this convention. GitHub auto-fills the squash message from the PR title, so **name your PRs using the conventional commit format** (e.g. `feat(api): add habit reminders endpoint`). Individual commits on feature branches don't need to be as strict — they get squashed away — but following the convention everywhere builds good habits.

### Typical feature workflow

1. Developer pulls latest `dev` and creates a branch: `git checkout -b feature/habit-reminders dev`
2. Work happens, commits are pushed to the feature branch.
3. Developer opens a PR from `feature/habit-reminders` → `dev`.
4. CI runs, a teammate reviews, PR is squash-merged into `dev`.
5. The feature branch is automatically deleted (GitHub setting).

### Multiple people working on a large feature

If a feature is too big for one person or one PR, create a shared integration branch for that feature and branch off of it:

```
dev
 └── feature/notifications           ← shared integration branch
      ├── feature/notifications-email  ← Alice's work
      └── feature/notifications-push   ← Bob's work
```

Alice and Bob each PR into `feature/notifications`. Once the full feature is complete and tested, `feature/notifications` is PR'd into `dev` as a single unit.

### What to do when you have a PR open but want to start new work

Do not wait for your PR to be merged. Create a new branch from `dev` and start working:

```
dev
 ├── feature/habit-reminders    ← PR open, waiting for review
 └── feature/streak-tracking    ← new work, branched from dev independently
```

Each feature branch is independent. If your first PR gets changes requested, you switch back to it, push fixes, and continue on the second branch when done. The two branches don't depend on each other.

If the new feature genuinely depends on the code in your open PR (it needs functions or types you just wrote), branch off your open PR instead of `dev`:

```
dev
 └── feature/habit-reminders         ← PR open, waiting for review
      └── feature/habit-statistics    ← depends on habit-reminders code
```

Once `feature/habit-reminders` merges into `dev`, rebase `feature/habit-statistics` onto `dev` to pick up the merged changes, then PR it into `dev` as usual. This is the only case where you branch off something other than `dev`.

### Hotfix workflow

Hotfixes bypass the normal promotion flow because they need to reach production quickly:

1. Branch from `main`: `git checkout -b hotfix/crash-on-empty-profile main`
2. Fix the issue, PR into `main` directly.
3. After merging to `main`, immediately open a PR to merge `main` back into `dev` (and `staging` if needed) so the fix propagates everywhere.

## Environments

Configured under Settings → Environments.

### `production`

- Required reviewers: **yes** (deployment must be manually approved)
- Deployment branches: **`main` only**
- Secrets: production webhook URL and token stored here

### `staging`

- Required reviewers: **no** (auto-deploys on merge)
- Deployment branches: **`staging` only**
- Secrets: staging webhook URL and token stored here

## Notes

- CI status check names will be added to rulesets once workflows are configured.
- A `CODEOWNERS` file should be added to `.github/CODEOWNERS` to require specific reviewers for infrastructure files (Dockerfiles, compose files, CI workflows, migration folders).
- Repository secrets for webhooks go in environment-scoped secrets, not repository-level secrets.
