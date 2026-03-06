# Unit Test Dockerfile
#
# Runs unit tests (Category != Integration) at Docker build time.
# The build fails if any test fails — no separate run step needed.
#
# Build from the src/ directory (needs all sibling projects):
#   docker build -f TaskApp.UnitTests/Dockerfile -t taskapp-unit-tests .
#
# To mark a test as integration-only (excluded here), add:
#   [Category("Integration")]
# to the test class or method.

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS test
WORKDIR /src

# Copy solution and restore once (layer-cached until .csproj files change)
COPY TaskApp.sln .
COPY TaskApp.Api.Data/TaskApp.Api.Data.csproj       TaskApp.Api.Data/
COPY TaskApp.Application/TaskApp.Application.csproj TaskApp.Application/
COPY TaskApp.UnitTests/TaskApp.UnitTests.csproj     TaskApp.UnitTests/
COPY TaskApp.Api/TaskApp.Api.csproj                 TaskApp.Api/
COPY TaskApp.Db/TaskApp.Db.csproj                   TaskApp.Db/
RUN dotnet restore TaskApp.UnitTests/TaskApp.UnitTests.csproj

# Copy source and run unit tests — fails the build if any test fails
COPY . .
RUN dotnet test TaskApp.UnitTests/TaskApp.UnitTests.csproj \
    --no-restore \
    --filter "TestCategory!=Integration" \
    --logger "console;verbosity=normal"
