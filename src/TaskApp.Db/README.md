# TaskApp.Db — EF Core Migration Runner

This project owns the EF Core migration history and applies it to the target database.
It is a self-contained console application: it connects, applies all pending migrations,
and exits with code `0` on success or `1` on failure.

It is designed to run as a **Kubernetes init container** or a **cron job** and is
safe to execute repeatedly — `MigrateAsync` is idempotent.

---

## Project layout

```
TaskApp.Db/
├── Migrations/
│   ├── 20260303000001_InitialCreate.cs   ← migration files (auto-generated)
│   └── TaskDbContextModelSnapshot.cs     ← current model snapshot (auto-generated)
├── DatabaseProvider.cs                   ← enum + connection-key helper
├── TaskDbContextDesignTimeFactory.cs     ← lets dotnet ef create the DbContext
├── Program.cs                            ← connects, migrates, exits
└── appsettings.json                      ← local dev connection strings
```

The **entity model and DbContext** (`TaskItem`, `TaskDbContext`) live in the
`TaskApp.Api.Data` class library, which this project references. Migrations are
stored here because `dotnet ef` must be run against an **executable** project.

---

## Required startup argument

`--database` is required and selects the database engine:

| Value | Provider | Notes |
|---|---|---|
| `mysql` | Oracle MySQL (`UseMySQL`) | Used in Kubernetes deployments |
| `sqlserver` | SQL Server (`UseSqlServer`) | Used in local / Azure dev |

```powershell
dotnet run -- --database mysql
dotnet run -- --database sqlserver
```

The program exits immediately with a non-zero code if `--database` is missing or invalid.

---

## Connection string resolution

The connection string is resolved in this order for each provider:

| Provider | Environment variable | appsettings.json key |
|---|---|---|
| `mysql` | `MYSQL_TASKAPP_CONNECTION` | `ConnectionStrings:MySqlConnection` |
| `sqlserver` | `SQLSERVER_TASKAPP_CONNECTION` | `ConnectionStrings:SqlServerConnection` |

Environment variables take priority over `appsettings.json`.
Update `appsettings.json` for local development; use environment variables in containers.

---

## Prerequisites

Install the EF Core global tool once:

```powershell
dotnet tool install --global dotnet-ef
# or update
dotnet tool update --global dotnet-ef
```

Verify:

```powershell
dotnet ef --version
```

---

## Adding a migration after changing TaskApp.Api.Data

Run from the **repository root**. Pass `-- --database <provider>` after the EF options
to tell the design-time factory which provider to scaffold for (defaults to `mysql`):

```powershell
dotnet ef migrations add <MigrationName> --project src/TaskApp.Db --startup-project src/TaskApp.Db --context TaskDbContext -- --database mysql
```

```powershell
dotnet ef migrations add <MigrationName> --project src/TaskApp.Db --startup-project src/TaskApp.Db --context TaskDbContext -- --database sqlserver
```

This creates two files in `Migrations/`:

- `<Timestamp>_<MigrationName>.cs` — the `Up` / `Down` SQL operations
- `TaskDbContextModelSnapshot.cs` — updated automatically; **do not edit by hand**

Commit both files.

> **Note:** MySQL and SQL Server generate different SQL in migration files
> (column types, identity annotations, etc.). If you target both engines,
> maintain a separate migrations folder per provider, or use the Helm chart's
> init-container to run the correct image against the correct database.

---

## Removing the most recent migration

Only do this if the migration has **not yet been applied** to any shared database.

Use `--force` to skip the live database check (without it, EF connects to verify
the migration is absent from `__EFMigrationsHistory`):

```powershell
dotnet ef migrations remove --force --project src/TaskApp.Db --startup-project src/TaskApp.Db --context TaskDbContext
```

---

## Applying migrations locally

```powershell
# MySQL — via appsettings.json (edit MySqlConnection first)
dotnet run --project src/TaskApp.Db -- --database mysql

# MySQL — via environment variable
$env:MYSQL_TASKAPP_CONNECTION = "Server=127.0.0.1;Port=3306;Database=taskapp;User=root;Password=changeme;"
dotnet run --project src/TaskApp.Db -- --database mysql

# SQL Server — via environment variable
$env:SQLSERVER_TASKAPP_CONNECTION = "Server=.;Database=taskapp;Trusted_Connection=True;TrustServerCertificate=True;"
dotnet run --project src/TaskApp.Db -- --database sqlserver
```

---

## Docker / Kubernetes

The `Dockerfile` builds a minimal image that runs the migration runner once and exits.
In the Helm chart it is used as an init container, passing `--database mysql` so the
API pod only starts after MySQL migrations succeed.
