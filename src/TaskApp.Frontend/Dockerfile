FROM node:20-alpine AS build
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
