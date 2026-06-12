# ── Stage 1: Build frontend ────────────────────────────────────────────────────
FROM node:22-alpine AS frontend-build
WORKDIR /app
COPY frontend/package*.json ./
RUN npm ci
COPY frontend/ .
# Empty base URL — nginx proxies /api to the backend
ENV VITE_API_BASE_URL=
RUN npm run build

# ── Stage 2: Build backend ─────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS backend-build
WORKDIR /src
# Copy NuGet config and project file first so the restore layer is cached independently
COPY backend/NuGet.Config overtime/NuGet.Config
COPY backend/overtime/overtime.csproj overtime/
# Copy offline packages as a local feed source
COPY backend/nuget-packages /nuget-packages
RUN dotnet restore overtime/overtime.csproj \
      --source /nuget-packages \
      --source https://api.nuget.org/v3/index.json \
      --packages /root/.nuget/packages
COPY backend/overtime/ overtime/
RUN dotnet publish overtime/overtime.csproj -c Release -o /publish --no-restore

# ── Stage 3: Runtime image ─────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:8.0

# Install PostgreSQL (Debian bookworm ships 15), nginx, wget
RUN apt-get update && apt-get install -y --no-install-recommends \
        postgresql \
        nginx \
        wget \
        xz-utils \
    && rm -rf /var/lib/apt/lists/*

# Install s6-overlay v3 from pre-downloaded tarballs in build context
COPY s6-overlay/dist/ /tmp/s6-dist/
RUN tar -C / -Jxp < /tmp/s6-dist/s6-overlay-noarch.tar.xz \
    && tar -C / -Jxp < /tmp/s6-dist/s6-overlay-arch.tar.xz \
    && rm -rf /tmp/s6-dist

# Create postgres user data dir
RUN mkdir -p /var/lib/postgresql/data \
    && chown -R postgres:postgres /var/lib/postgresql

# Copy built frontend
COPY --from=frontend-build /app/dist /usr/share/nginx/html

# Copy published backend
COPY --from=backend-build /publish /app

# Copy nginx config (replaces default)
COPY nginx.conf /etc/nginx/nginx.conf

# Copy s6-overlay service definitions
COPY s6-overlay/s6-rc.d/ /etc/s6-overlay/s6-rc.d/

# Make run scripts executable
RUN find /etc/s6-overlay/s6-rc.d -name "run" -exec chmod +x {} \;

EXPOSE 8080

ENTRYPOINT ["/init"]
