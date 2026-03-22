# Backend Setup

## Prerequisites

- [Docker](https://docs.docker.com/get-docker/) and Docker Compose
- [.NET 10 SDK](https://dotnet.microsoft.com/download) (only for running EF migrations locally)

## Getting Started

### 1. Create the `.env` file

```bash
cp .env.example .env
```

Edit `.env` and set your values. The defaults from `.env.example` work for local development.

### 2. Start everything with Docker Compose

For **development** (with hot-reload):

```bash
docker compose -f docker-compose.yml -f docker-compose.override.yml up --build
```

For **production-like** mode:

```bash
docker compose up --build
```

The backend will be available at `http://localhost:5000`.
Swagger UI is at `http://localhost:5000/swagger` (development mode only).

### 3. Stop the containers

```bash
docker compose down
```

To also wipe the database volume:

```bash
docker compose down -v
```

## Database Migrations

Migrations run automatically on startup (`db.Database.Migrate()` in `Program.cs`).

To add a new migration locally:

```bash
cd backend
dotnet ef migrations add <MigrationName>
```

This requires the `.env` file in the project root with valid `BACKEND__*` variables, and a running PostgreSQL instance (either via Docker or locally).

## Environment Variables

All backend variables use the `BACKEND__` prefix.

| Variable                   | Description              | Default               |
| -------------------------- | ------------------------ | --------------------- |
| `BACKEND__POSTGRESHOST`    | PostgreSQL host          | `localhost`           |
| `BACKEND__POSTGRESPORT`    | PostgreSQL port          | `5432`                |
| `BACKEND__POSTGRESDB`      | Database name            | —                     |
| `BACKEND__POSTGRESUSER`    | Database user            | —                     |
| `BACKEND__POSTGRESPASSWORD`| Database password        | —                     |
| `BACKEND__APPPORT`         | Port the backend maps to | `5000`                |
| `BACKEND__CORSORIGINS`     | Allowed CORS origins     | `http://localhost:3000`|
| `POSTGRES_HOST_PORT`       | Host port for PostgreSQL | —                     |
| `ASPNETCORE_ENVIRONMENT`   | ASP.NET environment      | `Production`          |
