# Off the Grid - Hackathon Platform (Frontend)

## Overview
Angular 17+ frontend application for the Blackbaud Hackathon Platform using SKY UX components for a consistent Blackbaud experience.

## Prerequisites
- Node.js 18+ and npm 9+
- Angular CLI 17+

## Quick Start

### 1. Install Dependencies
```bash
cd frontend
npm install
```

### 2. Configure Environment
Update `src/environments/environment.ts` with your settings:
```typescript
export const environment = {
  production: false,
  apiUrl: 'http://localhost:5000/api',
  bbidClientId: 'YOUR_BBID_CLIENT_ID',
  bbidAuthUrl: 'https://app.blackbaud.com/oauth/authorize',
  redirectUri: 'http://localhost:4200/auth/callback'
};
```

### 3. Run Development Server
```bash
npm start
```

Navigate to `http://localhost:4200/`

## Project Structure
```
frontend/
├── src/
│   ├── app/
│   │   ├── pages/                    # Page components
│   │   │   ├── home/
│   │   │   ├── login/
│   │   │   ├── auth-callback/
│   │   │   ├── ideas/
│   │   │   ├── teams/
│   │   │   ├── judging/
│   │   │   └── admin/
│   │   ├── services/                 # Angular services
│   │   │   ├── auth.service.ts
│   │   │   ├── hackathon.service.ts
│   │   │   └── idea.service.ts
│   │   ├── guards/                   # Route guards
│   │   │   └── auth.guard.ts
│   │   ├── interceptors/             # HTTP interceptors
│   │   │   └── auth.interceptor.ts
│   │   ├── models/                   # TypeScript interfaces
│   │   │   └── models.ts
│   │   ├── app.component.ts
│   │   ├── app.config.ts
│   │   └── app.routes.ts
│   ├── environments/
│   │   ├── environment.ts
│   │   └── environment.prod.ts
│   ├── index.html
│   ├── main.ts
│   └── styles.scss
├── angular.json
├── package.json
└── tsconfig.json
```

## Key Features

### Authentication
- Blackbaud ID OAuth 2.0 integration
- JWT token management
- Role-based access control (Participant, Judge, Admin)
- Auth guards for protected routes

### Services
- **AuthService**: Manages authentication and user session
- **HackathonService**: CRUD operations for hackathons
- **IdeaService**: Manage hackathon ideas/projects

### Components
- **Home**: Landing page with hackathon overview
- **Login**: Blackbaud OAuth login
- **Ideas**: Browse and filter submitted ideas
- **Teams**: View and manage teams
- **Judging**: Judge dashboard for evaluating submissions
- **Admin**: Admin panel for hackathon management

### Routing
- Lazy-loaded components for performance
- Protected routes with AuthGuard and RoleGuard
- OAuth callback handling

## Development

### Run Tests
```bash
npm test
```

### Build for Production
```bash
npm run build
```

Output will be in `dist/` directory.

### Lint Code
```bash
npm run lint
```

## SKY UX Integration

This application uses Blackbaud's SKY UX component library:
- `@skyux/core` - Core utilities
- `@skyux/theme` - Modern theme styling
- `@skyux/forms` - Form components
- `@skyux/layout` - Layout components
- `@skyux/modals` - Modal dialogs
- `@skyux/indicators` - Loading indicators, status badges
- `@skyux/lists` - Data grids and lists
- `@skyux/tabs` - Tab navigation
- `@skyux/popovers` - Tooltips and popovers

### Using SKY UX Components
```typescript
import { SkyModalModule } from '@skyux/modals';
import { SkyAlertModule } from '@skyux/indicators';
```

See [SKY UX Documentation](https://developer.blackbaud.com/skyux/) for component usage.

## API Integration

The frontend communicates with the .NET backend API:
- Base URL configured in environment files
- JWT tokens sent in Authorization header
- HTTP interceptor handles authentication errors

### Example API Call
```typescript
this.hackathonService.getCurrentHackathon().subscribe({
  next: (hackathon) => {
    console.log('Current hackathon:', hackathon);
  },
  error: (error) => {
    console.error('Failed to load hackathon:', error);
  }
});
```

## Authentication Flow

1. User clicks "Login" → redirected to BBID OAuth page
2. User authenticates with Blackbaud credentials
3. BBID redirects to `/auth/callback` with authorization code
4. Frontend sends code to backend `/api/auth/callback`
5. Backend exchanges code for access token and user info
6. Backend returns JWT token and user data
7. Frontend stores token and navigates to dashboard

## Environment Configuration

### Development
- API URL: `http://localhost:5000/api`
- Frontend URL: `http://localhost:4200`

### Production
Update `environment.prod.ts`:
```typescript
export const environment = {
  production: true,
  apiUrl: 'https://your-api.azurewebsites.net/api',
  bbidClientId: 'YOUR_PROD_CLIENT_ID',
  bbidAuthUrl: 'https://app.blackbaud.com/oauth/authorize',
  redirectUri: 'https://your-app.azurewebsites.net/auth/callback'
};
```

## Troubleshooting

### CORS Errors
Ensure backend has frontend origin in CORS allowed origins:
```json
"Cors": {
  "AllowedOrigins": ["http://localhost:4200"]
}
```

### OAuth Redirect Issues
- Verify redirect URI matches exactly in BBID portal
- Check that callback route is `/auth/callback`

### HTTP 401 Errors
- Token may be expired - logout and login again
- Verify token is being sent in Authorization header

## Next Steps

1. ✅ Complete authentication flow
2. ⏳ Implement Ideas list page with filtering
3. ⏳ Create Idea detail page with comments
4. ⏳ Build Team management pages
5. ⏳ Develop Judging interface with rating system
6. ⏳ Create Admin dashboard with analytics
7. ⏳ Add real-time updates with SignalR
8. ⏳ Implement comprehensive unit tests

## Resources

- [Angular Documentation](https://angular.io/docs)
- [SKY UX Components](https://developer.blackbaud.com/skyux/)
- [Blackbaud OAuth Guide](https://developer.blackbaud.com/skyapi/docs/authorization)
- [RxJS Documentation](https://rxjs.dev/)
