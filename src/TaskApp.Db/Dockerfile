# =============================================================================
# TaskApp.Db — EF Core Migration Runner
# Console application that applies Entity Framework migrations to the database.
# Runs once and exits (Kubernetes init container or Job), not a long-lived service.
# =============================================================================

# Build stage

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["TaskApp.Db/TaskApp.Db.csproj", "TaskApp.Db/"]
RUN dotnet restore "./TaskApp.Db/TaskApp.Db.csproj"
COPY . .
WORKDIR "/src/TaskApp.Db"
# 'dotnet publish' (in the next stage) already compiles implicitly, making this build step
# redundant. The /app/build output is not used by any subsequent stage.
# It is kept here as a fast-fail: if compilation breaks, this stage fails before publish runs.
RUN dotnet build "./TaskApp.Db.csproj" -c $BUILD_CONFIGURATION -o /app/build
# -------------------------
# Result: project compiled in the SDK image with all dependencies restored and binaries in /app/build.

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish

# ARGs do not persist across stages and must be redeclared in each stage that needs them.
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./TaskApp.Db.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false
# -------------------------
# Result: self-contained publish output in /app/publish, ready to be copied into the final image.

# Runtime stage
FROM mcr.microsoft.com/dotnet/runtime:10.0 AS runtime
WORKDIR /app
USER $APP_UID
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TaskApp.Db.dll"]
CMD ["--database", "mysql"]
# -------------------------
# Result: minimal runtime image with the published app. Defaults to MySQL migrations.
# Override CMD in the Kubernetes Job/initContainer spec to target a different database,
# e.g. args: ["--database", "sqlserver"]