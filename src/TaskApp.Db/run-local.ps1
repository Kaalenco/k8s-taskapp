# =============================================================================
# run-local.ps1 — Build and run the TaskApp.Db migration runner locally
#
# Builds the taskapp-csharp-db Docker image and runs it against a local MySQL
# instance. Reads the connection string from the MYSQL_TASKAPP_CONNECTION
# environment variable — set it as a user or machine environment variable
# before running this script.
#
# Usage:
#   .\run-local.ps1
#
# To set the connection string for the current session only:
#   $env:MYSQL_TASKAPP_CONNECTION = "Server=localhost;Port=3306;Database=taskapp;User=root;Password=yourpassword;"
# =============================================================================

$ErrorActionPreference = 'Stop'

$ImageName    = 'taskapp-csharp-db:latest'
$Dockerfile   = Join-Path $PSScriptRoot 'Dockerfile'
$BuildContext = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path

# ---------------------------------------------------------------------------
# Validate connection string
# ---------------------------------------------------------------------------
$ConnectionString = $env:MYSQL_TASKAPP_CONNECTION
if ([string]::IsNullOrWhiteSpace($ConnectionString)) {
    Write-Error @"
MYSQL_TASKAPP_CONNECTION is not set.
Set it as an environment variable and retry, e.g.:
  `$env:MYSQL_TASKAPP_CONNECTION = "Server=localhost;Port=3306;Database=taskapp;User=root;Password=yourpassword;"
"@
    exit 1
}

# ---------------------------------------------------------------------------
# Build
# ---------------------------------------------------------------------------
Write-Host ""
Write-Host "Building $ImageName ..." -ForegroundColor Cyan
Write-Host "  Dockerfile : $Dockerfile"
Write-Host "  Context    : $BuildContext"
Write-Host ""

docker build -f $Dockerfile -t $ImageName $BuildContext

if ($LASTEXITCODE -ne 0) {
    Write-Error "Docker build failed (exit code $LASTEXITCODE)."
    exit $LASTEXITCODE
}

# ---------------------------------------------------------------------------
# Rewrite the Server= value for Docker
# Inside the container, localhost/127.0.0.1 refers to the container itself.
# host.docker.internal is the Docker Desktop alias for the host machine.
# ---------------------------------------------------------------------------
$DockerConnectionString = $ConnectionString -replace 'Server=[^;]+', 'Server=host.docker.internal'

Write-Host ""
Write-Host "Build succeeded. Running migrations against local MySQL ..." -ForegroundColor Green
$MaskedOriginal = $ConnectionString       -replace '(Password=)[^;]+', '$1********'
$MaskedDocker   = $DockerConnectionString -replace '(Password=)[^;]+', '$1********'
Write-Host "  Original connection : $MaskedOriginal"
Write-Host "  Docker connection   : $MaskedDocker"
Write-Host ""

docker run --rm `
    -e "MYSQL_TASKAPP_CONNECTION=$DockerConnectionString" `
    $ImageName `
    --database mysql

if ($LASTEXITCODE -ne 0) {
    Write-Error "Migration runner exited with code $LASTEXITCODE."
    exit $LASTEXITCODE
}

Write-Host ""
Write-Host "Migrations completed successfully." -ForegroundColor Green
