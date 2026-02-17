# Hackathon Platform - .NET Backend

## Overview
This is the backend API for the Off the Grid Hackathon Platform, built with .NET 8.0 and Entity Framework Core. It provides RESTful APIs for managing hackathons, teams, ideas, judging, and user authentication via Blackbaud ID (BBID).

## Prerequisites
- .NET 8.0 SDK or later
- SQL Server or SQL Server LocalDB
- Visual Studio 2022 or VS Code with C# extension
- Blackbaud Developer Account with BBID application credentials

## Project Structure
```
backend/
├── src/
│   ├── Blackbaud.Hackathon.Platform.Service/       # Web API project
│   │   ├── Controllers/                            # API controllers
│   │   ├── Program.cs                              # Application startup
│   │   └── appsettings.json                        # Configuration
│   ├── Blackbaud.Hackathon.Platform.Shared/        # Shared libraries
│   │   ├── BusinessLogic/                          # Service layer
│   │   ├── DataAccess/                             # EF Core DbContext
│   │   ├── Models/                                 # Entity models
│   │   │   └── DTOs/                               # Data transfer objects
│   │   └── Extensions/                             # Extension methods
│   └── Blackbaud.Hackathon.Platform.Extensions/    # Utility extensions
└── test/
    ├── Blackbaud.Hackathon.Platform.Service.Tests/
    └── Blackbaud.Hackathon.Platform.Shared.Tests/
```

## Getting Started

### 1. Configure Blackbaud Authentication

⚠️ **CRITICAL**: Before starting, you MUST regenerate your application secret in the Blackbaud Developer Portal. This prevents 90% of OAuth failures.

1. Go to [Blackbaud Developer Portal](https://developer.blackbaud.com/)
2. Select your application
3. Navigate to "Application secrets"
4. Click "Regenerate secret" and save the new value
5. Update `appsettings.json` with your credentials:

```json
{
  "BlackbaudAuth": {
    "ClientId": "YOUR_CLIENT_ID",
    "ClientSecret": "YOUR_REGENERATED_SECRET",
    "RedirectUri": "http://localhost:5000/api/auth/callback"
  },
  "Jwt": {
    "SecretKey": "YourSecureSecretKeyMinimum32Characters!",
    "Issuer": "HackathonPlatform",
    "Audience": "HackathonPlatformClient",
    "ExpirationMinutes": 60
  }
}
```

### 2. Setup Database

Update the connection string in `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=HackathonPlatform;Trusted_Connection=true;MultipleActiveResultSets=true"
  }
}
```

### 3. Create Database with EF Core Migrations

Navigate to the backend directory:
```bash
cd backend
```

Install EF Core tools (if not already installed):
```bash
dotnet tool install --global dotnet-ef
```

Create initial migration:
```bash
dotnet ef migrations add InitialCreate --project src/Blackbaud.Hackathon.Platform.Shared --startup-project src/Blackbaud.Hackathon.Platform.Service
```

Apply migration to create database:
```bash
dotnet ef database update --project src/Blackbaud.Hackathon.Platform.Shared --startup-project src/Blackbaud.Hackathon.Platform.Service
```

### 4. Restore Dependencies and Build

```bash
dotnet restore
dotnet build
```

### 5. Run the Application

```bash
cd src/Blackbaud.Hackathon.Platform.Service
dotnet run
```

The API will be available at:
- HTTP: http://localhost:5000
- HTTPS: https://localhost:5001
- Swagger UI: https://localhost:5001/swagger

## API Endpoints

### Authentication
- `GET /api/auth/login` - Initiates BBID OAuth flow
- `POST /api/auth/callback` - Handles OAuth callback
- `GET /api/auth/me` - Gets current user info

### Hackathons
- `GET /api/hackathons` - Get all hackathons
- `GET /api/hackathons/{id}` - Get specific hackathon
- `GET /api/hackathons/current` - Get current active hackathon
- `POST /api/hackathons` - Create hackathon (Admin only)
- `PUT /api/hackathons/{id}` - Update hackathon (Admin only)
- `DELETE /api/hackathons/{id}` - Delete hackathon (Admin only)

### Ideas
- `GET /api/ideas` - Get all ideas
- `GET /api/ideas/{id}` - Get specific idea
- `POST /api/ideas` - Create new idea (Participant only)
- `PUT /api/ideas/{id}` - Update idea (Owner only)
- `POST /api/ideas/{id}/submit` - Submit idea for judging
- `DELETE /api/ideas/{id}` - Delete idea (Owner only)

## Authentication Flow

1. Frontend calls `GET /api/auth/login` to get BBID authorization URL
2. User is redirected to Blackbaud OAuth page
3. After login, user is redirected back with authorization code
4. Frontend sends code to `POST /api/auth/callback`
5. Backend exchanges code for access token with BBID
6. Backend extracts user info from token
7. Backend creates/updates user in database
8. Backend generates JWT token for subsequent API calls
9. Frontend stores JWT token and uses it in Authorization header

## Authorization Policies

- **ParticipantOnly**: Requires role "participant", "judge", or "admin"
- **JudgeOnly**: Requires role "judge" or "admin"
- **AdminOnly**: Requires role "admin"

## Database Schema

See the entity models in `src/Blackbaud.Hackathon.Platform.Shared/Models/`:
- **User** - User accounts linked to Blackbaud IDs
- **UserRole** - User roles per hackathon
- **Hackathon** - Hackathon events
- **Track** - Hackathon tracks/categories
- **Award** - Awards for winners
- **JudgingCriterion** - Criteria for judging (with weights)
- **Milestone** - Hackathon milestones
- **Team** - Participant teams
- **TeamMember** - Team membership
- **Idea** - Project submissions
- **IdeaAward** - Awards given to ideas
- **Rating** - Judge ratings for ideas
- **Comment** - Comments on ideas

## Development

### Running Tests
```bash
dotnet test
```

### Adding a New Migration
```bash
dotnet ef migrations add MigrationName --project src/Blackbaud.Hackathon.Platform.Shared --startup-project src/Blackbaud.Hackathon.Platform.Service
```

### Reverting a Migration
```bash
dotnet ef migrations remove --project src/Blackbaud.Hackathon.Platform.Shared --startup-project src/Blackbaud.Hackathon.Platform.Service
```

## Troubleshooting

### OAuth 401 Unauthorized
- Verify you regenerated the application secret
- Check that ClientId and ClientSecret match your BBID app
- Ensure redirect URI matches exactly in BBID portal

### Database Connection Issues
- Verify SQL Server is running
- Check connection string in appsettings.json
- Try using SQL Server Express instead of LocalDB

### CORS Errors
- Verify frontend origin is in `appsettings.json` Cors.AllowedOrigins
- Check that CORS middleware is registered before authentication

## Next Steps

1. Seed database with Off the Grid 2025 data
2. Create additional controllers (Teams, Ratings, Comments)
3. Implement real-time updates with SignalR
4. Add comprehensive unit and integration tests
5. Set up CI/CD pipeline in Azure DevOps
6. Deploy to Azure App Service

## Resources

- [.NET Documentation](https://docs.microsoft.com/en-us/dotnet/)
- [Entity Framework Core](https://docs.microsoft.com/en-us/ef/core/)
- [Blackbaud OAuth Guide](https://developer.blackbaud.com/skyapi/docs/authorization)
- [Azure App Service](https://docs.microsoft.com/en-us/azure/app-service/)
