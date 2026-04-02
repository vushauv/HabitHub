# Scripts

Utility scripts for development and maintenance tasks. All scripts live in the `scripts/` directory at the repo root.

## Contents

- [clean_docker](#clean_docker) — stop and remove Docker containers, volumes, and networks

---

## clean_docker

**Files:** `scripts/clean_docker.sh` (bash) / `scripts/clean_docker.ps1` (PowerShell)

Stops all project containers and removes associated volumes and networks created by Docker Compose. Optionally removes locally built images.

### When to use

- After finishing a dev session to free up disk space and stop running containers
- When you want a clean slate before starting fresh (e.g. to re-run migrations from scratch)
- When switching branches that have DB schema differences — wipe `postgres_data` volume so EF Core applies migrations cleanly

### When NOT to remove images

You do **not** need to remove images on a normal cleanup. Docker will reuse cached layers and only rebuild what changed. Only remove images if you changed a `Dockerfile` and want a guaranteed clean rebuild.

### Usage

**Linux / macOS (bash):**
```bash
./scripts/clean_docker.sh            # stop containers, remove volumes and networks
./scripts/clean_docker.sh --images   # also remove locally built images
```

**Windows (PowerShell):**
```powershell
./scripts/clean_docker.ps1           # stop containers, remove volumes and networks
./scripts/clean_docker.ps1 --images  # also remove locally built images
```

### What it removes

| Resource | Always | With `--images` |
|---|---|---|
| Containers (`habithub-postgres`, `habithub-backend`, `habithub-frontend`) | yes | yes |
| Volumes (`postgres_data`, `frontend_node_modules`) | yes | yes |
| Network (`habithub-network`) | yes | yes |
| Locally built images (backend, frontend) | no | yes |
| Pulled images (`postgres:16-alpine`) | no | no |

> **Note:** Removing `postgres_data` deletes all local database data. The next `docker compose up` will start with an empty database and re-run migrations automatically.
