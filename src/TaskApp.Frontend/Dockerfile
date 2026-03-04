# Build a frontend image directly from a GitHub repository (React/Vite example)
# Usage:
#   docker build -f Dockerfile \
#     --build-arg GIT_REPO=https://github.com/<your-org>/<your-react-repo> \
#     --build-arg GIT_REF=main \
#     -t taskapp-frontend:github .

FROM node:18-alpine AS build
ARG GIT_REPO=https://github.com/Kaalenco/k8s-taskapp.git
ARG GIT_REF=trunk

RUN apk add --no-cache git
WORKDIR /app

# Clone the repo at a specific ref
RUN git clone --depth 1 --branch ${GIT_REF} ${GIT_REPO} .

# Install dependencies and build
RUN npm install
RUN npm run build

FROM nginx:1.25-alpine
COPY --from=build /app/dist /usr/share/nginx/html
COPY nginx.conf /etc/nginx/conf.d/default.conf
