# HabitHub

A habit-tracking app with team support. Build and track habits solo or with a group.

## Stack

| Layer | Technology |
|---|---|
| Frontend | React 19 + TypeScript + Vite |
| Backend | ASP.NET Core 8 (C#) |
| Database | PostgreSQL 16 |
| Runtime | Docker Compose |

## Quick start

```bash
git clone git@github.com:vushauv/HabitHub.git && cd HabitHub
cp .env.example .env
docker compose up --build
```

| Service | URL |
|---|---|
| Frontend | http://localhost:3000 |
| Backend | http://localhost:5000 |
| Swagger | http://localhost:5000/swagger |

## Docs

- [Development setup](docs/development-setup.md) — prerequisites, hot-reload, migrations, env vars
- [Testing](docs/testing.md) — running and writing backend and frontend tests
- [Scripts](docs/scripts.md) — utility scripts

## License

GNU General Public License v3.0 — see [LICENSE](LICENSE).
