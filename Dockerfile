# Multi-stage Dockerfile for Hackathon Platform (.NET 8.0)
# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution and project files
COPY ["backend/Blackbaud.Hackathon.Platform.sln", "backend/"]
COPY ["backend/src/", "backend/src/"]
COPY ["backend/test/", "backend/test/"]

# Restore dependencies
RUN dotnet restore "backend/Blackbaud.Hackathon.Platform.sln"

# Build the solution
RUN dotnet build "backend/Blackbaud.Hackathon.Platform.sln" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "backend/src/Blackbaud.Hackathon.Platform.Service/Blackbaud.Hackathon.Platform.Service.csproj" -c Release -o /app/publish

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
