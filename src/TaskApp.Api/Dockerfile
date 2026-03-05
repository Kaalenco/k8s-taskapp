# =============================================================================
# TaskApp.Api — ASP.NET Core Web API
# Exposes REST endpoints consumed by the React frontend.
# Runs as a long-lived service (Kubernetes Deployment) on port 5073.
# =============================================================================

# Build stage

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy project files and restore (build context is ./src, covering all projects)
COPY TaskApp.Api.Data/TaskApp.Api.Data.csproj TaskApp.Api.Data/
COPY TaskApp.Api/TaskApp.Api.csproj TaskApp.Api/
RUN dotnet restore TaskApp.Api/TaskApp.Api.csproj

# Copy source code and build
COPY TaskApp.Api.Data/ TaskApp.Api.Data/
COPY TaskApp.Api/ TaskApp.Api/
RUN dotnet publish TaskApp.Api/TaskApp.Api.csproj -c Release -o /app/publish
# -------------------------
# Result: project compiled and published in the SDK image with all dependencies resolved.
# Published output is in /app/publish, ready to be copied into the final image.

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

# Install curl for the Docker-level health check below
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

# Copy published application
COPY --from=build /app/publish .

# Explicitly run as the non-root user defined by the base image (app, UID 1654).
# dotnet/aspnet:8.0+ already sets this user by default, so this line is technically
# redundant — but keeping it makes the security intent visible without relying on
# knowledge of what the base image does internally. Remove if you prefer leaner Dockerfiles.
USER $APP_UID

# Documents the port the application listens on — metadata only.
# This does NOT open or publish the port; actual binding is controlled by
# 'docker run -p' or the Kubernetes Service/ingress configuration.
EXPOSE 5073

# Overrides the default ASP.NET Core listening address.
# Without this, .NET 8+ defaults to port 8080 — this pins the app to 5073
# to match the EXPOSE above and the Kubernetes Service targetPort.
ENV ASPNETCORE_URLS=http://+:5073

# ASP.NET Core already defaults to Production when this variable is unset in a container,
# so this line is redundant. Keeping it here makes the intent explicit.
# Remove if you prefer leaner Dockerfiles.
ENV ASPNETCORE_ENVIRONMENT=Production

# Docker-level health check — only evaluated by the Docker engine (e.g. 'docker run').
# Kubernetes ignores this entirely and uses its own liveness/readiness probes
# defined in the pod spec instead.
HEALTHCHECK --interval=30s --timeout=3s --start-period=40s --retries=3 \
  CMD curl -f http://localhost:5073/health || exit 1

# Run application
ENTRYPOINT ["dotnet", "TaskApp.Api.dll"]
# -------------------------
# Result: minimal ASP.NET runtime image serving the API on port 5073, running as non-root.
# SDK tooling is excluded — only the published binaries are present.
