# Build a Frontend Image From a GitHub Repository

This folder shows how to build a frontend container image directly from a GitHub repository. The example assumes a React app built with Vite, where `npm run build` produces a `dist/` folder.

If your app outputs `build/` (for example Create React App), update the Dockerfile to copy from `/app/build` instead of `/app/dist`.

## 1) Choose a repository

Use your own React repository or any GitHub repo that builds with `npm run build`.

You can create a simple React app with Vite and push it to GitHub:

```bash
npm create vite@latest my-taskapp-ui -- --template react
cd my-taskapp-ui
npm install
npm run build
```

After pushing to GitHub, use that repository URL in the build arguments below.

## 2) Build the image from GitHub

From this folder, run:

```bash
docker build -f Dockerfile.github \
  --build-arg GIT_REPO=https://github.com/<your-org>/<your-react-repo> \
  --build-arg GIT_REF=main \
  -t taskapp-frontend:github .
```

## 3) Run locally (optional)

```bash
docker run --rm -p 8080:80 taskapp-frontend:github
```

Open `http://localhost:8080` to verify the build.

## Notes

- The Dockerfile copies `nginx.conf` from this folder to enable SPA routing.
- If your repo uses a different build command, update `npm run build` accordingly.
- For a vanilla HTML/JS app, you can skip Node entirely and just copy static files into the Nginx image.
