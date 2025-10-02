# Use the official .NET 8 runtime as base image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

# Use the official .NET 8 SDK for building
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project files and restore dependencies
COPY ["apps/scanner/src/Scanner.csproj", "apps/scanner/src/"]
COPY ["apps/scanner/tests/", "apps/scanner/tests/"]
RUN dotnet restore "apps/scanner/src/Scanner.csproj"

# Copy source code and build
COPY . .
WORKDIR "/src/apps/scanner/src"
RUN dotnet build "Scanner.csproj" -c Release -o /app/build

# Publish the application
FROM build AS publish
RUN dotnet publish "Scanner.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Create final runtime image
FROM base AS final
WORKDIR /app

# Install additional tools
RUN apt-get update && apt-get install -y \
    curl \
    wget \
    git \
    && rm -rf /var/lib/apt/lists/*

# Create non-root user
RUN groupadd -r scanner && useradd -r -g scanner scanner

# Create directories and set permissions
RUN mkdir -p /app/logs /app/data /app/config && \
    chown -R scanner:scanner /app

# Copy published application
COPY --from=publish /app/publish .

# Copy configuration files
COPY --from=build /src/apps/scanner/config/ /app/config/

# Set environment variables
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:80
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=1

# Health check
HEALTHCHECK --interval=30s --timeout=10s --start-period=5s --retries=3 \
    CMD curl -f http://localhost/health || exit 1

# Switch to non-root user
USER scanner

# Set working directory
WORKDIR /app

# Default command
ENTRYPOINT ["dotnet", "Scanner.dll"]
