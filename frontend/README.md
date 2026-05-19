# HabitHub Frontend

React 19 + TypeScript + Vite. Runs inside Docker — no local Node required for development.

## Development

Start via Docker Compose from the repo root:

```bash
docker compose up --build
```

Frontend is available at `http://localhost:3000`. Vite HMR is enabled — edits to `.tsx`/`.ts` files reflect instantly.

API calls use relative paths (`/api/...`), proxied to the backend by Vite's dev server.

## Adding npm packages

```bash
docker compose exec frontend npm install <package-name>
```

Or install locally and rebuild:

```bash
cd frontend && npm install <package-name>
docker compose up -d --build frontend
```

## Tests

See [docs/testing.md](../docs/testing.md) for full details.

```bash
# Unit tests
npm run test:unit

# Integration tests
npm run test:integration
```

Test files live in `tests/unit/` and `tests/integration/`, mirroring `src/`.
