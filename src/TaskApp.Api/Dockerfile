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

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

# Install SQL Server tools for health checks (optional)
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

# Copy published application
COPY --from=build /app/publish .

# Expose port
EXPOSE 5073

# Set environment variables
ENV ASPNETCORE_URLS=http://+:5073
ENV ASPNETCORE_ENVIRONMENT=Production

# Health check
HEALTHCHECK --interval=30s --timeout=3s --start-period=40s --retries=3 \
  CMD curl -f http://localhost:5073/health || exit 1

# Run application
ENTRYPOINT ["dotnet", "TaskApp.Api.dll"]
