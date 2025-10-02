# Use the official .NET 8 runtime as base image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# Use the official .NET 8 SDK for building
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project files
COPY ["src/CatalogApi/CatalogApi.csproj", "src/CatalogApi/"]
COPY ["tests/CatalogApi.Tests/CatalogApi.Tests.csproj", "tests/CatalogApi.Tests/"]

# Restore dependencies
RUN dotnet restore "src/CatalogApi/CatalogApi.csproj"
RUN dotnet restore "tests/CatalogApi.Tests/CatalogApi.Tests.csproj"

# Copy all source code
COPY . .

# Build the application
WORKDIR "/src/src/CatalogApi"
RUN dotnet build "CatalogApi.csproj" -c Release -o /app/build

# Publish the application
FROM build AS publish
RUN dotnet publish "CatalogApi.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Final stage
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Health check
HEALTHCHECK --interval=30s --timeout=10s --start-period=60s --retries=3 \
  CMD curl -f http://localhost:8080/health || exit 1

ENTRYPOINT ["dotnet", "CatalogApi.dll"]
