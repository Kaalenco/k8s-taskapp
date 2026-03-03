# Sample React Frontend

This is a minimal React app for testing Docker builds in Kubernetes-101.

## Structure
- `package.json` — React dependencies and scripts
- `src/` — React source files
- `public/` — Static HTML

## Build

To build with Docker:

```
docker build -f ../Dockerfile.frontend -t taskapp-frontend:custom ..
```

Or follow the instructions in the course documentation.
