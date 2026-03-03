# Building the C# Task API Docker Image

This directory contains a complete ASP.NET Core Web API application that implements the Task Management backend for the Kubernetes-101 course.

## What This API Does

- Provides RESTful endpoints for CRUD operations on tasks
- Connects to Azure SQL Database (or SQL Server)
- Implements health checks (`/health` and `/ready`)
- Uses Entity Framework Core for database access
- Supports CORS for frontend integration

## Prerequisites

- .NET 8.0 SDK (optional, only needed for local development)
- Docker Desktop or Docker CLI

## Quick Start: Build the Image

### For Docker Desktop Kubernetes (Windows/Mac)

```bash
cd scripts/session14a/csharp-api-src/
docker build -t taskapp-csharp-api:latest .
```

Docker Desktop Kubernetes uses the same Docker daemon as your local Docker, so images built locally are automatically available to Kubernetes.

### For Minikube

```bash
# 1. Build the image
cd scripts/session14a/csharp-api-src/
docker build -t taskapp-csharp-api:latest .

# 2. Load into minikube's Docker daemon
minikube image load taskapp-csharp-api:latest
```

Minikube runs in a VM with its own Docker daemon, so you must explicitly load images into it.

### For AKS or Other Cloud Kubernetes

You need to push the image to a container registry (Docker Hub, Azure Container Registry, etc.):

```bash
# Build and tag for your registry
docker build -t <your-registry>/taskapp-csharp-api:latest .

# Push to registry
docker push <your-registry>/taskapp-csharp-api:latest
```

Then update the deployment YAML to reference your registry image.

## Alternative: Clone from Git Repository

If this code were _hosted in a Git repository_, you would:

```bash
# Clone the repository
git clone https://github.com/your-org/kubernetes-101-csharp-api.git
cd kubernetes-101-csharp-api

# Build the Docker image
docker build -t taskapp-csharp-api:latest .

# For Docker Desktop - ready to use
# For Minikube - load the image
minikube image load taskapp-csharp-api:latest
```

This approach mirrors the Node.js backend workflow from Session 14.

## Project Structure

```
csharp-api-src/
├── Program.cs                    # Application entry point and configuration
├── TaskApp.Api.csproj           # Project file with dependencies
├── appsettings.json             # Default configuration (overridden by env vars)
├── Dockerfile                    # Multi-stage Docker build
├── .dockerignore                # Files to exclude from Docker build
├── Models/
│   └── TaskItem.cs              # Task entity model
├── Data/
│   └── TaskDbContext.cs         # Entity Framework database context
└── Controllers/
    └── TasksController.cs       # REST API endpoints
```

## API Endpoints

- `GET /health` - Health check (returns healthy if app is running)
- `GET /ready` - Readiness check (verifies database connectivity)
- `GET /api/tasks` - List all tasks
- `GET /api/tasks/{id}` - Get specific task
- `POST /api/tasks` - Create new task
- `PUT /api/tasks/{id}` - Update existing task
- `DELETE /api/tasks/{id}` - Delete task

## Environment Variables

The API expects these environment variables (set by Kubernetes):

- `ConnectionStrings__DefaultConnection` - SQL Server connection string
- `ASPNETCORE_ENVIRONMENT` - Environment (Development/Production)
- `ASPNETCORE_URLS` - URLs to listen on (default: http://+:5073)

The double underscore `__` in environment variables maps to nested JSON configuration:
- `ConnectionStrings__DefaultConnection` → `ConnectionStrings:DefaultConnection`

## Database Schema

The API expects a table named `tasks` with these columns:

```sql
CREATE TABLE tasks (
    id INT IDENTITY(1,1) PRIMARY KEY,
    title NVARCHAR(255) NOT NULL,
    description NVARCHAR(MAX),
    status NVARCHAR(50) DEFAULT 'pending',
    created_at DATETIME2 DEFAULT GETUTCDATE(),
    updated_at DATETIME2
);
```

This schema is created by the Azure SQL initialization job in Session 13a.

## Local Development

To run locally without Docker:

```bash
# Restore dependencies
dotnet restore

# Run the application
dotnet run
```

Access the API at http://localhost:5073/api/tasks and Scalar API documentation at http://localhost:5073/scalar/v1

## Troubleshooting

### Image build fails

Ensure you're in the correct directory and have Docker running:

```bash
cd scripts/session14a/csharp-api-src/
docker info  # Verify Docker is running
```

### Container starts but crashes

Check logs for connection string errors:

```bash
docker logs <container-id>
```

Ensure the connection string is correctly formatted.

### Health check fails

The health check requires the API to respond on port 5073. Verify the port is correctly exposed and the application is listening.

## Next Steps

After building the image, return to [Session 14a](../../docs/Session_14a-CSharp_API_Backend.md) to deploy it to Kubernetes.
