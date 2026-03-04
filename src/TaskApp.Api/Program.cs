using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using TaskApp.Api.Data;
using Scalar.AspNetCore;
using TaskApp.Api;

var builder = WebApplication.CreateEmptyBuilder(new WebApplicationOptions { Args = args });

// HTTP server — Kestrel is the default cross-platform web server
// CreateEmptyBuilder does not wire up ASPNETCORE_URLS automatically, so the
// listen address must be configured explicitly. ListenAnyIP binds to 0.0.0.0
// so Kubernetes liveness/readiness probes can reach the pod IP.
builder.WebHost.UseKestrelCore();
builder.WebHost.ConfigureKestrel(options =>
{
    var port = int.Parse(Environment.GetEnvironmentVariable("ASPNETCORE_HTTP_PORT") ?? "5073");
    options.ListenAnyIP(port);
});

// Configuration sources, in ascending priority order
builder.Configuration
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables()
    .AddUserSecrets<Program>()
    .AddCommandLine(args);

// Logging providers
builder.Logging
    .AddConfiguration(builder.Configuration.GetSection("Logging"))
    .AddConsole()
    .AddDebug()
    .AddEventSourceLogger();

// Routing infrastructure — required for MapControllers, MapHealthChecks, etc.
builder.Services.AddRouting();

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();

// Configure database connection
builder.Services.AddDatabase(builder.Configuration);

// Configure CORS for frontend access
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend",
        policy => policy
            .AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader());
});

// Add health checks
// /health  — app-alive check (startup + liveness probe): always returns 200 if the
//             ASP.NET Core process is running. Does NOT require database connectivity.
// /ready   — readiness check: verifies the database is reachable before the pod
//             receives traffic. Returns 503 if the database is unavailable.
builder.Services
    .AddHealthChecks()
    .AddDbContextCheck<TaskDbContext>(tags: ["ready"]);

var app = builder.Build();

// Initialize database
DatabaseInitializer.Initialize(app.Services);

// Configure the HTTP request pipeline
app.MapOpenApi();
app.MapScalarApiReference(options =>
{
    options.WithTitle("Task Management API")
           .WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient);
});

app.UseCors("AllowFrontend");
app.UseAuthorization();
app.MapControllers();

// Health check endpoints
// /health: startup and liveness probe — returns 200 if the ASP.NET Core process is alive.
//   Only "self" checks run here (no database query), so this passes even before the
//   database is reachable, allowing the pod to start up cleanly.
app.MapHealthChecks("/health", new HealthCheckOptions
{
    Predicate = _ => false   // skip all registered checks, just return Healthy
});

// /ready: readiness probe — returns 200 only when the database is reachable.
//   Kubernetes removes the pod from service endpoints while this returns non-2xx,
//   so no traffic is sent until the database connection is established.
app.MapHealthChecks("/ready", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready")
});

app.Run();
