# Build: from backend/, `podman build -t myyt-api .` (or `gcloud run deploy --source .`)
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Restore first so package downloads are cached until a project file changes.
COPY src/MyYt.Domain/MyYt.Domain.csproj src/MyYt.Domain/
COPY src/MyYt.Application/MyYt.Application.csproj src/MyYt.Application/
COPY src/MyYt.Infrastructure/MyYt.Infrastructure.csproj src/MyYt.Infrastructure/
COPY src/MyYt.Api/MyYt.Api.csproj src/MyYt.Api/
RUN dotnet restore src/MyYt.Api/MyYt.Api.csproj

COPY src/ src/
RUN dotnet publish src/MyYt.Api/MyYt.Api.csproj -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app .

# Cloud Run routes traffic to $PORT (8080); ASP.NET Core images listen on 8080 by default.
# Runs as the image's non-root "app" user. Configure via env vars: MYSQL_*, FIREBASE_PROJECT_ID, Cors__Origins__0.
USER app
EXPOSE 8080
ENTRYPOINT ["dotnet", "MyYt.Api.dll"]
