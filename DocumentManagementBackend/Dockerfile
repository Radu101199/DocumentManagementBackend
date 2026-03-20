# ─── Stage 1: Build ───────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copiază solution și project files (pentru cache layer)
COPY DocumentManagementBackend.sln .
COPY src/DocumentManagementBackend.Domain/DocumentManagementBackend.Domain.csproj                         src/DocumentManagementBackend.Domain/
COPY src/DocumentManagementBackend.Application/DocumentManagementBackend.Application.csproj               src/DocumentManagementBackend.Application/
COPY src/DocumentManagementBackend.Infrastructure/DocumentManagementBackend.Infrastructure.csproj         src/DocumentManagementBackend.Infrastructure/
COPY src/DocumentManagementBackend.API/DocumentManagementBackend.API.csproj                               src/DocumentManagementBackend.API/

# Restore — separat ca să fie cached dacă .csproj nu s-a schimbat
RUN dotnet restore src/DocumentManagementBackend.API/DocumentManagementBackend.API.csproj

# Copiază tot codul
COPY src/ src/

# Build și publish
RUN dotnet publish src/DocumentManagementBackend.API/DocumentManagementBackend.API.csproj \
    -c Release \
    -o /app/publish \
    --no-restore

# ─── Stage 2: Runtime ─────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

# Non-root user pentru securitate
RUN addgroup --system appgroup && adduser --system --ingroup appgroup appuser

# Copiază doar artefactele publicate
COPY --from=build /app/publish .

# Setează ownership
RUN chown -R appuser:appgroup /app
USER appuser

# Port-ul pe care ascultă API-ul
EXPOSE 8080

# Health check
HEALTHCHECK --interval=30s --timeout=10s --start-period=40s --retries=3 \
    CMD curl -f http://localhost:8080/health || exit 1

ENTRYPOINT ["dotnet", "DocumentManagementBackend.API.dll"]
