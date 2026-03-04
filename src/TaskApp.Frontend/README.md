# Sample React Frontend

This is a minimal React app for testing Docker builds in Kubernetes-101.

## Structure
- `package.json` — React dependencies and scripts
- `src/` — React source files
- `public/` — Static HTML

## Local Development

The app reads `window.env.API_URL` at runtime to determine the backend address. When
running locally with `serve`, you need to point it at the API port (5000).

**Build only** (outputs to `build/`, with `API_URL` set to `http://localhost:5000`):

```bash
npm run build:local
```

**Build and serve** (builds, injects the env config, then serves on `http://localhost:3000`):

```bash
npm run serve:local
```

The `build:local` script runs `react-scripts build` and then patches `build/index.html`
to inject `window.env = { "API_URL": "http://localhost:5000" }` so all API calls go to
the correct port. The helper that does the patching lives in `scripts/inject-env.js`.

## Build (Docker)

To build with Docker:

```
docker build -f ../Dockerfile.frontend -t taskapp-frontend:custom ..
```

Or follow the instructions in the course documentation.
