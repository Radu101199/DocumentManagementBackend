FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY DocumentManagementBackend.sln .
COPY src/DocumentManagementBackend.Domain/DocumentManagementBackend.Domain.csproj                     src/DocumentManagementBackend.Domain/
COPY src/DocumentManagementBackend.Application/DocumentManagementBackend.Application.csproj           src/DocumentManagementBackend.Application/
COPY src/DocumentManagementBackend.Infrastructure/DocumentManagementBackend.Infrastructure.csproj     src/DocumentManagementBackend.Infrastructure/
COPY src/DocumentManagementBackend.API/DocumentManagementBackend.API.csproj                           src/DocumentManagementBackend.API/

RUN dotnet restore src/DocumentManagementBackend.API/DocumentManagementBackend.API.csproj

COPY src/ src/

RUN dotnet publish src/DocumentManagementBackend.API/DocumentManagementBackend.API.csproj \
    -c Release \
    -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

RUN addgroup --system appgroup && adduser --system --ingroup appgroup appuser

COPY --from=build /app/publish .

RUN chown -R appuser:appgroup /app
USER appuser

EXPOSE 8080

ENTRYPOINT ["dotnet", "DocumentManagementBackend.API.dll"]
