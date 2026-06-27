# PptPowerKeys.Api container image (S01-011 — part A).
#
# Build context MUST be the repository root, because PptPowerKeys.Api references
# PptPowerKeys.Core via a project reference. Build with:
#   docker build -t pptpowerkeys-api .
#   docker run -p 8080:8080 pptpowerkeys-api
#
# The image listens on http://+:8080 by default. Container hosts that inject a
# port via the PORT env (Render / Fly / Railway) are honoured by Program.cs.

# ── Build / publish ──────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Restore against the csproj files first so layers cache when only code changes.
COPY src/PptPowerKeys.Core/PptPowerKeys.Core.csproj src/PptPowerKeys.Core/
COPY src/PptPowerKeys.Api/PptPowerKeys.Api.csproj src/PptPowerKeys.Api/
RUN dotnet restore src/PptPowerKeys.Api/PptPowerKeys.Api.csproj

# Copy the rest of the sources needed to publish the API (+ its Core reference).
COPY src/PptPowerKeys.Core/ src/PptPowerKeys.Core/
COPY src/PptPowerKeys.Api/ src/PptPowerKeys.Api/

RUN dotnet publish src/PptPowerKeys.Api/PptPowerKeys.Api.csproj \
    -c Release -o /app/publish /p:UseAppHost=false

# ── Runtime ──────────────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

# Container hosts route HTTP to this port; PORT env (if set) overrides at runtime.
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "PptPowerKeys.Api.dll"]
