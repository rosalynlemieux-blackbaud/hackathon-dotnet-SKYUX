# Multi-stage Dockerfile for Hackathon Platform (.NET 8.0)
# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy only source projects (skip test projects for production build)
COPY ["backend/src/", "backend/src/"]

# Restore dependencies for production projects only
RUN dotnet restore "backend/src/Blackbaud.Hackathon.Platform.Service/Blackbaud.Hackathon.Platform.Service.csproj"
RUN dotnet restore "backend/src/Blackbaud.Hackathon.Platform.Shared/Blackbaud.Hackathon.Platform.Shared.csproj"
RUN dotnet restore "backend/src/Blackbaud.Hackathon.Platform.Extensions/Blackbaud.Hackathon.Platform.Extensions.csproj"

# Build production projects
RUN dotnet build "backend/src/Blackbaud.Hackathon.Platform.Service/Blackbaud.Hackathon.Platform.Service.csproj" -c Release -o /app/build --no-restore

# Publish stage - publish just the Service project
FROM build AS publish
RUN dotnet publish "backend/src/Blackbaud.Hackathon.Platform.Service/Blackbaud.Hackathon.Platform.Service.csproj" -c Release -o /app/publish --no-build

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Install curl for health checks
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

# Copy published application from publish stage
COPY --from=publish /app/publish .

# Create uploads directory for file uploads
RUN mkdir -p /app/wwwroot/uploads && chmod 755 /app/wwwroot/uploads

# Expose port 5000 (HTTP) for Render
EXPOSE 5000

# Set environment variables
ENV ASPNETCORE_URLS=http://+:5000
ENV ASPNETCORE_ENVIRONMENT=Production

# Health check
HEALTHCHECK --interval=30s --timeout=10s --start-period=40s --retries=3 \
  CMD curl -f http://localhost:5000/health || exit 1

# Run the application
ENTRYPOINT ["dotnet", "Blackbaud.Hackathon.Platform.Service.dll"]
