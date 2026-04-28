---
name: log
description: Add structured Serilog/ILogger logs to a C# file or directory of files. Trigger when the user says "/log <path>", "add logs to <file|dir>", "instrument logging in <path>", or asks to add logs/observability to code.
---

# Log skill

Add structured logs to one C# file or every C# file in a directory. Use the project's existing logging stack (Serilog via `ILogger<T>`).

## Inputs

- One file path → instrument just that file.
- A directory path → instrument every `*.cs` file under it (skip `*.Tests/`, `bin/`, `obj/`, generated files).
- No path → ask the user which file or directory to target.

## Steps

1. Resolve the target paths. If a directory, list its `.cs` files first and confirm the set with the user before editing many files.
2. Read each target file in full before editing.
3. Apply the rules below, then run `dotnet build backend/backend.csproj` (or the relevant project) to verify nothing broke.
4. Report a per-file summary of logs added.

## Logger injection

This project uses C# 12 primary constructors. Add `ILogger<T> logger` as a constructor parameter:

```csharp
public class TeamService(
    ITeamRepository repo,
    ILogger<TeamService> logger) : ITeamService
{
    // ... use `logger.LogXxx(...)` directly
}
```

For top-level statements in `Program.cs`, use `app.Logger`.

DI registration is automatic — `ILogger<T>` is always resolvable. No registration needed.

## What to log, where

| Layer | Level | When |
|-------|-------|------|
| Repository — mutations (`Create*`, `Update*`, `Delete*`, `Invalidate*`, `Expire*`) | `Information` | On success, after `SaveChangesAsync` |
| Repository — null/skip branches in mutations | `Warning` | Before `return` |
| Repository — read-only `Get*` | none | Skip — too noisy. EF logs cover SQL |
| Service / business logic — domain events (login, signup, team created, invite accepted, etc.) | `Information` | On success |
| Service — validation failures, expected business rejects | `Warning` | Before throw / return |
| Service / middleware — caught exceptions | `Error` | Inside `catch`, with `ex` as first arg |
| Middleware — auth failures | `Warning` | When token invalid / missing |
| Middleware — auth success | `Debug` | Optional, dev-only |
| Background services — start, finish, items processed | `Information` | At cycle boundaries |
| Hot loops, internal branches | `Debug` | Only when actively diagnosing |

Avoid logging in trivial pass-through methods or every `Get*`.

## Template rules

- Use **structured templates**, never `$"..."` interpolation, never `string.Format`, never `+`.
  - Yes: `logger.LogInformation("Created team {TeamId} for {UserId}", teamId, userId);`
  - No: `logger.LogInformation($"Created team {teamId} for {userId}");`
- Property names use PascalCase: `{TeamId}`, `{UserId}`, `{Count}`, `{Status}`.
- Exception is **always the first argument** when present:
  ```csharp
  logger.LogError(ex, "Failed creating team {TeamName}", name);
  ```
- Imperative-style messages, no trailing period: `"Created session"`, not `"A session was created."`.
- Keep messages short. Push details into structured properties, not prose.

## Secrets — MUST hash, never raw

**Critical rule.** Secrets must never appear in logs as raw values. This includes session tokens, API keys, JWTs, password hashes, refresh tokens, email verification tokens, OAuth codes.

**SessionId IS a secret in this project.** It is the bearer token used for authentication. Even though it shows up at API boundaries and is convenient to log, the raw value must never reach logs.

When you genuinely need to correlate log lines for the same session/token (e.g., session lifecycle in `SessionRepository`, auth middleware), log a stable hash instead. Use the shared helper:

```csharp
using backend.Logging;
// ...
logger.LogInformation("Created session {SessionFingerprint} for user {UserId} ({UserType})",
    LogRedaction.Fingerprint(session.SessionId), session.UserId, session.UserType);
```

`LogRedaction.Fingerprint(token)` lives at `backend/Logging/LogRedaction.cs`. Returns the first 12 hex chars of `SHA-256(token)` — irreversible, stable across log lines, collision-safe at app scale.

**Property naming:** when the value is a fingerprint, name it `{*Fingerprint}` (e.g., `{SessionFingerprint}`, `{TokenFingerprint}`) so log readers know it is redacted, not the real value.

**Whenever you reach for a secret in a log call, stop and ask: do I need correlation? If yes → fingerprint it. If no → drop it and log only the user / entity ID.**

## Other sensitive data

- **Passwords / password hashes:** never log, not even as length. Log only the entity ID, e.g., `"Updated password for member {MemberId}"`.
- **Emails:** PII. Prefer `{UserId}` over `{Email}`. If you must log an email (e.g., signup attempted with an invalid address that failed validation), prefer the ID and a category like `"email validation failed for {UserId}"`. Avoid logging full email addresses in `Information` flows.
- **PII generally:** names, addresses, phone numbers, payment details — out.
- **Connection strings, env config:** never log values. Log only that config was loaded.

## Process per file

For each target `.cs` file:

1. Identify the public-facing methods.
2. Decide level per the table above.
3. Modify the constructor (or primary ctor) to accept `ILogger<T> logger` if not already present.
4. Insert log calls per the rules. Add `using backend.Logging;` if calling `LogRedaction.Fingerprint`.
5. Re-read the file to verify the edit, then move to the next.

## Do not

- Do not change behavior — only add logs.
- Do not add try/catch wrappers solely to log; only insert `LogError` inside an existing catch.
- Do not log inside hot loops without `Debug` level + a clear reason.
- Do not log raw secrets, even truncated. Use `LogRedaction.Fingerprint`.
- Do not log emails, passwords, or password hashes.
- Do not introduce new dependencies — Serilog and `LogRedaction` already exist.
- Do not commit. End by suggesting `/commit` if user wants to save.
