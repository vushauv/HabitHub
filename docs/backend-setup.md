# Backend — Developer Guide

Everything you need to start developing, testing, and debugging the backend.

## Prerequisites

- [Docker](https://docs.docker.com/get-docker/) and Docker Compose
- [.NET 10 SDK](https://dotnet.microsoft.com/download) — needed for running tests and EF migrations locally

## Quick Start

```bash
# 1. Clone and enter the repo
git clone git@github.com:vushauv/HabitHub.git && cd HabitHub

# or via https

git clone https://github.com/vushauv/HabitHub.git && cd HabitHub


# 2. Create your .env (defaults work out of the box)
cp .env.example .env

# 3. Start everything
docker compose up --build
```

That's it. The backend is at `http://localhost:5000`, Swagger UI at `http://localhost:5000/swagger`.

Hot-reload is enabled — edit any `.cs` file and the app recompiles automatically inside the container.

### Stopping

```bash
docker compose down          # stop containers, keep database
docker compose down -v       # stop containers AND wipe the database volume
```

## Running Tests

Tests run locally (not in Docker), using xUnit:

```bash
dotnet test
```

This runs from the repo root using the solution file. The test project lives in `tests/backend/`.

### SDK version mismatch

The Docker container and your local machine may have slightly different .NET SDK patch versions (e.g. 10.0.4 inside Docker vs 10.0.5 locally). Since both write to the same `backend/obj/` directory via the bind mount, a `dotnet restore` from one can confuse the other.

If you see errors like `Package X, version Y was not found` after running the app in Docker, fix it with:

```bash
rm -rf backend/obj
dotnet restore
```

This is harmless — it just forces a clean restore with your local SDK.

## Database

### Connecting from the host

The PostgreSQL container exposes a port on your host machine, configured by `POSTGRES_HOST_PORT` in `.env` (default: `54320`).

```bash
# psql
psql -h localhost -p 54320 -U postgres -d habithub

# or any GUI tool (DBeaver, DataGrip, etc.)
# Host: localhost
# Port: 54320
# User: postgres (or whatever BACKEND__POSTGRESUSER is set to)
# Password: supersecret (or whatever BACKEND__POSTGRESPASSWORD is set to)
# Database: habithub
```

> **Note:** the backend connects to PostgreSQL on port `5432` (the internal Docker network port), not `54320`. The host port mapping is only for your local tools.

### Migrations

Migrations run automatically on app startup (`db.Database.Migrate()` in `Program.cs`), so you don't need to apply them manually.

**When to create a new migration:**

- You changed a model class in `Models/` (added/removed/renamed a property)
- You changed the `OnModelCreating` configuration in `AppDbContext`

**How to create a migration:**

```bash
# Make sure Docker is running (you need the database)
docker compose up -d postgres

# From the repo root
dotnet ef migrations add <MigrationName> --project backend
```

This generates files in `backend/Migrations/`. Commit them — they're part of the codebase.

**If you need to undo the last migration** (only if it hasn't been applied yet):

```bash
dotnet ef migrations remove --project backend
```

If it was already applied, create a new migration that reverts the changes instead.

## Project Structure

```
backend/
├── Configuration/     # Strongly-typed settings (AppSettings)
├── Controllers/       # API endpoints
├── Data/              # DbContext and design-time factory
├── Dtos/              # Request/response DTOs
├── Migrations/        # EF Core migrations (auto-generated)
├── Models/            # Entity classes
├── Repositories/      # Data access layer
├── Dockerfile
└── Program.cs         # App entry point and DI setup

tests/
└── backend/           # xUnit test project
```

## Docker Compose Files

| File | Purpose | When to use |
|------|---------|-------------|
| `docker-compose.yml` | **Development** — bind mounts, hot-reload, dev Dockerfile stage | `docker compose up` (default) |
| `docker-compose.prod.yml` | **Production** overrides — final Dockerfile stage, no bind mounts | `docker compose -f docker-compose.yml -f docker-compose.prod.yml up` |

The dev setup uses a custom entrypoint (`docker-entrypoint.dev.sh`) that automatically matches the container's user to your host UID/GID, so files created by the container are owned by you (no root-owned `bin/`/`obj/` problems).

## Environment Variables

All backend variables use the `BACKEND__` prefix (double underscore maps to ASP.NET's configuration hierarchy).

| Variable | Description | Default |
|----------|-------------|---------|
| `BACKEND__POSTGRESHOST` | PostgreSQL host | `localhost` |
| `BACKEND__POSTGRESPORT` | PostgreSQL port | `5432` |
| `BACKEND__POSTGRESDB` | Database name | — |
| `BACKEND__POSTGRESUSER` | Database user | — |
| `BACKEND__POSTGRESPASSWORD` | Database password | — |
| `BACKEND__APPPORT` | Host port the backend maps to | `5000` |
| `BACKEND__CORSORIGINS` | Allowed CORS origins | `http://localhost:3000` |
| `POSTGRES_HOST_PORT` | Host port for direct DB access | `54320` |
| `ASPNETCORE_ENVIRONMENT` | ASP.NET environment | `Development` |

## Troubleshooting

### `Permission denied` when running `dotnet test` or `dotnet build`

The `backend/obj/` or `backend/bin/` directories were created by an older Docker setup running as root. Fix:

```bash
docker compose run --rm backend chown -R $(id -u):$(id -g) /src/bin /src/obj
```

Or if containers aren't running, delete and recreate them:

```bash
rm -rf backend/bin backend/obj
dotnet restore
```

### Container fails to start with `ASPNETCORE_URLS` warning

This is just a warning, not an error. The app binds to `http://+:5000` as configured.

### `Package X was not found` during local build

See [SDK version mismatch](#sdk-version-mismatch) above.
