# Off the Grid - Hackathon Platform

A full-stack hackathon management platform built with .NET 8.0 and Angular 17+, featuring Blackbaud ID authentication and SKY UX components.

## 🚀 Project Overview

This platform enables organizations to run hackathons with features for:
- **Participants**: Submit ideas, form teams, collaborate
- **Judges**: Evaluate submissions with weighted criteria
- **Admins**: Configure hackathons, manage users, view analytics

## 🏗️ Architecture

### Backend (.NET 8.0)
- **Web API**: RESTful API with Entity Framework Core
- **Database**: SQL Server with comprehensive schema
- **Authentication**: Blackbaud ID OAuth 2.0 + JWT
- **Architecture**: Clean separation with Service, BusinessLogic, DataAccess layers

### Frontend (Angular 17+)
- **UI Framework**: Angular with SKY UX components
- **State Management**: RxJS Observables
- **Routing**: Lazy-loaded modules with route guards
- **Styling**: SKY UX Modern Theme + custom SCSS

## 📁 Project Structure

```
hackathon-platform/
├── backend/                    # .NET 8.0 Web API
│   ├── src/
│   │   ├── Blackbaud.Hackathon.Platform.Service/      # API controllers
│   │   ├── Blackbaud.Hackathon.Platform.Shared/       # Business logic & data access
│   │   └── Blackbaud.Hackathon.Platform.Extensions/   # Utilities
│   ├── test/                   # Unit and integration tests
│   └── README.md
├── frontend/                   # Angular 17+ application
│   ├── src/
│   │   ├── app/
│   │   │   ├── pages/         # Page components
│   │   │   ├── services/      # API services
│   │   │   ├── guards/        # Route guards
│   │   │   └── models/        # TypeScript interfaces
│   │   └── environments/      # Environment configs
│   └── README.md
├── database/                   # SQL scripts and migrations
└── docs/                       # Additional documentation
```

## 🔧 Prerequisites

### Backend
- .NET 8.0 SDK
- SQL Server or LocalDB
- Visual Studio 2022 or VS Code with C# extension

### Frontend
- Node.js 18+
- Angular CLI 17+
- npm 9+

### Authentication
- Blackbaud Developer Account
- BBID Application credentials

## 🚦 Quick Start

### 1. Clone Repository
```bash
git clone <repository-url>
cd hackathon-platform
```

### 2. Setup Backend

#### Configure BBID Authentication
**⚠️ CRITICAL**: Regenerate your application secret in the [Blackbaud Developer Portal](https://developer.blackbaud.com/) before proceeding. This prevents 90% of OAuth failures.

Edit `backend/src/Blackbaud.Hackathon.Platform.Service/appsettings.json`:
```json
{
  "BlackbaudAuth": {
    "ClientId": "YOUR_CLIENT_ID",
    "ClientSecret": "YOUR_REGENERATED_SECRET"
  },
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=HackathonPlatform;..."
  }
}
```

#### Create Database
```bash
cd backend
dotnet ef migrations add InitialCreate --project src/Blackbaud.Hackathon.Platform.Shared --startup-project src/Blackbaud.Hackathon.Platform.Service
dotnet ef database update --project src/Blackbaud.Hackathon.Platform.Shared --startup-project src/Blackbaud.Hackathon.Platform.Service
```

#### Run Backend
```bash
cd src/Blackbaud.Hackathon.Platform.Service
dotnet run
```

API available at: https://localhost:5001

### 3. Setup Frontend

Edit `frontend/src/environments/environment.ts`:
```typescript
export const environment = {
  apiUrl: 'http://localhost:5000/api',
  bbidClientId: 'YOUR_CLIENT_ID'
};
```

#### Install and Run
```bash
cd frontend
npm install
npm start
```

Application available at: http://localhost:4200

## 🔑 Authentication Setup

### Blackbaud Developer Portal Configuration

1. Create application at https://developer.blackbaud.com/apps
2. Set redirect URI: `http://localhost:5000/api/auth/callback`
3. Set frontend callback: `http://localhost:4200/auth/callback`
4. **Regenerate application secret** (critical step)
5. Copy Client ID and Secret to configuration files

### OAuth Endpoints
- **Authorization**: `https://app.blackbaud.com/oauth/authorize`
- **Token**: `https://oauth2.sky.blackbaud.com/token`
- **Grant Type**: Authorization Code

## 📊 Database Schema

### Core Tables
- **Users** - User accounts linked to Blackbaud IDs
- **UserRoles** - Role assignments per hackathon
- **Hackathons** - Hackathon events
- **Tracks** - Hackathon categories
- **Awards** - Prizes and recognitions
- **JudgingCriteria** - Evaluation criteria with weights
- **Milestones** - Timeline events
- **Teams** - Participant teams
- **Ideas** - Project submissions
- **Ratings** - Judge evaluations
- **Comments** - Idea discussions

## 🎯 Key Features

### For Participants
- ✅ Create and join teams
- ✅ Submit project ideas
- ✅ Track submission status
- ✅ View hackathon timeline
- ⏳ Real-time collaboration

### For Judges
- ✅ View submitted ideas
- ✅ Rate based on weighted criteria
- ✅ Provide feedback
- ⏳ Export evaluation reports

### For Admins
- ✅ Create and configure hackathons
- ✅ Manage tracks and awards
- ✅ Assign user roles
- ⏳ View analytics dashboard
- ⏳ Announce winners

## 🧪 Testing

### Backend
```bash
cd backend
dotnet test
```

### Frontend
```bash
cd frontend
npm test
```

## 📦 Deployment

### Backend (Azure App Service)
1. Create Azure App Service
2. Configure Azure SQL Database
3. Set application settings (connection strings, BBID credentials)
4. Deploy via Azure DevOps or GitHub Actions

### Frontend (Azure Static Web Apps)
1. Create Static Web App
2. Configure build settings
3. Set environment variables
4. Deploy via GitHub Actions

## 🔒 Security

- JWT tokens for API authentication
- Role-based authorization
- HTTPS required in production
- SQL injection protection via EF Core
- XSS prevention with input sanitization
- CORS configured for specific origins

## 🐛 Troubleshooting

### OAuth 401 Errors
- Regenerate BBID application secret
- Verify ClientId and ClientSecret are correct
- Check redirect URI matches exactly

### CORS Errors
- Add frontend origin to backend `appsettings.json`
- Verify CORS middleware order in `Program.cs`

### Database Connection Issues
- Check SQL Server is running
- Verify connection string
- Ensure database exists

## 📚 Documentation

- [Backend README](backend/README.md) - API documentation
- [Frontend README](frontend/README.md) - Angular app details
- [Implementation Plan](hackathon-platform-dotnet-angular-plan.md) - Full 8-week plan

## 🤝 Contributing

1. Create feature branch
2. Make changes
3. Run tests
4. Submit pull request

## 📝 License

Copyright © 2025 Blackbaud, Inc.

## 🆘 Support

For issues or questions:
- Review troubleshooting sections
- Check [Blackbaud Developer Docs](https://developer.blackbaud.com/)
- Contact development team

---

**Built with ❤️ for the Blackbaud community**
