# Testing

## Contents

- [Overview](#overview)
- [Backend — unit tests](#backend--unit-tests)
- [Backend — integration tests](#backend--integration-tests)
- [Frontend — unit tests](#frontend--unit-tests)

---

## Overview

| | Framework | Location | Needs Docker |
|---|---|---|---|
| Backend unit | xUnit | `backend/backend.Tests/Unit/` | No |
| Backend integration | xUnit + WebApplicationFactory | `backend/backend.Tests/Integration/` | Yes (PostgreSQL) |
| Frontend unit | Vitest + React Testing Library | `frontend/tests/unit/` | No |

**Running all tests in Docker:**

```bash
./scripts/run_tests.sh
```

**Running a specific suite:**

```bash
./scripts/run_tests.sh frontend-test
./scripts/run_tests.sh backend-unit-test
./scripts/run_tests.sh backend-integration-test
```

This runs frontend, backend unit, and backend integration tests sequentially and exits with a non-zero code if any suite fails. Do not use `docker compose up --abort-on-container-exit` — it stops all containers as soon as the first one finishes, which kills still-running suites.

**Tab completion for the script:**

*zsh* — add to `~/.zshrc` after `compinit`:

```zsh
run-tests() { ~/code/uni/SE2/HabitHub/scripts/run_tests.sh "$@" }
_run-tests() { compadd -- frontend-test backend-unit-test backend-integration-test }
compdef _run-tests run-tests
```

Then use `run-tests <TAB>` instead of calling the script directly.

*bash* — add to `~/.bashrc`:

```bash
run-tests() { ~/code/uni/SE2/HabitHub/scripts/run_tests.sh "$@" }
_run-tests() {
  local services="frontend-test backend-unit-test backend-integration-test"
  COMPREPLY=($(compgen -W "$services" -- "${COMP_WORDS[COMP_CWORD]}"))
}
complete -F _run-tests run-tests
```

Both approaches wrap the script in a shell function because tab completion for scripts called via relative paths (`./scripts/...`) is unreliable in both shells.

---

## Backend — unit tests

Unit tests test a single class in isolation. All external dependencies (database, services) are replaced with fakes written inline in the test file. They do not need Docker and run instantly.

### Running

```bash
# Locally
cd backend && dotnet test backend.Tests/backend.Tests.csproj --filter "Category=Unit"

# Docker
docker compose -f docker-compose.test.yml run --rm backend-unit-test
```

### Writing

Every unit test class must have the `[Trait("Category", "Unit")]` attribute so it can be filtered independently from integration tests.

**Pattern:**

```csharp
// backend/backend.Tests/Unit/Services/MyServiceTests.cs
namespace backend.Tests.Unit.Services;

[Trait("Category", "Unit")]
public class MyServiceTests
{
    private readonly FakeMyRepository _repo = new();
    private readonly MyService _sut;

    public MyServiceTests()
    {
        _sut = new MyService(_repo);
    }

    [Fact]
    public async Task DoSomething_WhenCondition_ReturnsExpected()
    {
        // Arrange
        _repo.Seed(/* test data */);

        // Act
        var result = await _sut.DoSomething();

        // Assert
        Assert.Equal("expected", result);
    }

    // Fake replaces the real repository — no database involved
    private class FakeMyRepository : IMyRepository
    {
        private readonly List<MyEntity> _data = new();

        public void Seed(MyEntity entity) => _data.Add(entity);

        public Task<MyEntity?> GetByIdAsync(Guid id) =>
            Task.FromResult(_data.FirstOrDefault(e => e.Id == id));
    }
}
```

**Rules:**
- Place fakes as private nested classes inside the test file — they are only used by that one test class.
- Name tests as `MethodName_WhenCondition_ExpectedOutcome`.
- Never touch the database, never make HTTP calls.

**Real example:** `backend/backend.Tests/Unit/Controllers/UserControllerTests.cs`

---

## Backend — integration tests

Integration tests boot the full ASP.NET Core application with a real PostgreSQL database and make actual HTTP requests against it. They always run in Docker because they require the `postgres-test` container.

`TestWebAppFactory` (in `backend/backend.Tests/Fixtures/`) handles booting the app, swapping in the test database connection, and running migrations before any test runs.

### Running

```bash
# Docker only — requires postgres-test
docker compose -f docker-compose.test.yml run --rm backend-integration-test
```

### Writing

Every integration test class must:
1. Have `[Trait("Category", "Integration")]`
2. Implement `IClassFixture<TestWebAppFactory>` — this boots the app once and shares it across all tests in the class

**Pattern:**

```csharp
// backend/backend.Tests/Integration/MyFeature/MyEndpointTests.cs
using System.Net;
using System.Net.Http.Json;
using backend.Tests.Fixtures;

namespace backend.Tests.Integration.MyFeature;

[Trait("Category", "Integration")]
public class MyEndpointTests : IClassFixture<TestWebAppFactory>
{
    private readonly HttpClient _client;

    public MyEndpointTests(TestWebAppFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task MyEndpoint_WithValidRequest_Returns200()
    {
        var response = await _client.PostAsJsonAsync("/my-route", new
        {
            field = "value"
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
```

**Rules:**
- No mocks — the full request pipeline runs against a real database.
- Each test run starts with a fresh database (the `postgres-test` container uses `tmpfs`).
- If tests share state through the database, make sure each test uses unique data (e.g. unique emails) to avoid conflicts.

**Real example:** `backend/backend.Tests/Integration/Auth/AuthEndpointsTests.cs`

---

## Frontend — unit tests

Frontend tests render React components in a simulated browser environment (jsdom) and assert on what is visible to the user. They do not make real API calls.

### Running

```bash
# Locally
cd frontend && npm test

# Docker
docker compose -f docker-compose.test.yml run --rm frontend-test
```

### Writing

Test files go in `frontend/tests/unit/` mirroring the structure of `frontend/src/`. A test for `src/pages/Home.tsx` goes in `tests/unit/pages/Home.test.tsx`.

**Pattern:**

```tsx
// frontend/tests/unit/pages/MyPage.test.tsx
import { render, screen } from "@testing-library/react";
import { MemoryRouter } from "react-router-dom";
import { describe, it, expect } from "vitest";
import MyPage from "../../../src/pages/MyPage";

describe("MyPage", () => {
  it("renders the main heading", () => {
    render(
      <MemoryRouter>
        <MyPage />
      </MemoryRouter>
    );

    expect(screen.getByRole("heading", { name: "My Page" })).toBeInTheDocument();
  });
});
```

**Wrap with `MemoryRouter`** whenever the component uses `<Link>` or `useNavigate` — otherwise the test will crash because there is no router context.

**Mocking API calls:**

```tsx
import { vi } from "vitest";

vi.mock("../../../src/api/auth", () => ({
  login: vi.fn().mockResolvedValue({ token: "fake-token" }),
}));
```

**Rules:**
- Query elements the way a user would: prefer `getByRole`, `getByLabelText`, `getByText` over `getByTestId`.
- Do not test implementation details (internal state, function calls) — test what is rendered.

**Real example:** `frontend/tests/unit/pages/Home.test.tsx`
