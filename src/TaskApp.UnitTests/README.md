# TaskApp.UnitTests

Test project for the TaskApp solution using [NUnit](https://nunit.org/).

## Test categories

Tests are split into two categories using the NUnit `[Category]` attribute.

| Category | Annotation | Requires MySQL |
|---|---|---|
| Unit | _(none)_ | No |
| Integration | `[Category("Integration")]` | Yes |

```csharp
// Unit test — no annotation needed
[Test]
public void MyMethod_GivenInput_ReturnsExpected() { ... }

// Integration test — runs against a real MySQL database
[Test]
[Category("Integration")]
public async Task MyRepository_SavesRecord_ToDatabase() { ... }
```

Integration tests read the connection string from the `ConnectionStrings__DefaultConnection`
environment variable (or `ConnectionStrings:DefaultConnection` in `appsettings.json`).

---

## Running tests locally

### Unit tests only

```bash
dotnet test --filter "TestCategory!=Integration"
```

### Integration tests (requires MySQL)

Start a local MySQL instance first, then:

```bash
ConnectionStrings__DefaultConnection="Server=localhost;Database=taskapp_test;User=testuser;Password=testpass;" \
dotnet test --filter "TestCategory=Integration"
```

Or use Docker Compose to spin up MySQL automatically (see below).

### All tests via Docker Compose

From the repository root:

```bash
# Run integration tests (MySQL starts, tests run, MySQL is removed)
docker compose -f src/docker-compose.integration.yml up \
  --build \
  --exit-code-from integration-tests \
  --abort-on-container-exit

# Remove the database volume
docker compose -f src/docker-compose.integration.yml down --volumes
```

### Unit tests via Docker

```bash
# Build context must be src/ to include sibling projects
docker build -f src/TaskApp.UnitTests/Dockerfile src/
```

The build fails if any unit test fails.

---

## CI (GitHub Actions)

The `test.yml` workflow runs on every push and pull request.
Both jobs run in parallel:

| Job | What runs | How |
|---|---|---|
| `unit-tests` | `TestCategory!=Integration` | `dotnet test` on the runner |
| `integration-tests` | `TestCategory=Integration` | Docker Compose with ephemeral MySQL |

Unit test results are uploaded as a workflow artifact (`.trx` file) and are visible
in the **Actions → Summary** tab even when tests fail.
