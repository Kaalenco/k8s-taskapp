FROM node:20-alpine AS build
# Pin npm to the same major version used to generate package-lock.json (v11).
# Mismatched npm versions between local and Docker cause 'npm ci' to fail with
# "Missing: X from lock file" even when the lock file is otherwise correct.
RUN npm install -g npm@11
WORKDIR /app

# Install dependencies (separate layer for caching)
COPY package.json package-lock.json ./
RUN npm ci

# Copy source and build (CRA outputs to /app/build)
COPY . .
RUN npm run build

FROM nginx:1.25-alpine
COPY --from=build /app/build /usr/share/nginx/html
COPY nginx.conf /etc/nginx/conf.d/default.conf
