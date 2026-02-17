# Configuration Guide

## 🔑 Required Configuration

Before running the application, you need to configure authentication and database settings.

## 1️⃣ Get Blackbaud Developer Credentials

### Step 1: Create/Access Your BBID Application
1. Go to https://developer.blackbaud.com/
2. Sign in with your Blackbaud account
3. Navigate to **My Applications**
4. Select your application (or create a new one)

### Step 2: ⚠️ CRITICAL - Regenerate Application Secret
**This is the #1 cause of OAuth failures!**

1. In your application settings, find **Application secrets**
2. Click **"Regenerate secret"**
3. **Copy the new secret immediately** (you won't see it again)
4. Store it securely

### Step 3: Configure Redirect URIs
Add these redirect URIs to your BBID application:

**For Local Development:**
- `http://localhost:5000/api/auth/callback` (Backend)
- `http://localhost:4200/auth/callback` (Frontend)

**For Production:**
- `https://your-api-domain.com/api/auth/callback` (Backend)
- `https://your-app-domain.com/auth/callback` (Frontend)

### Step 4: Copy Your Credentials
You'll need:
- **Client ID**: Visible in your application settings
- **Client Secret**: The secret you just regenerated

---

## 2️⃣ Configure Backend

### File: `backend/src/Blackbaud.Hackathon.Platform.Service/appsettings.json`

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=HackathonPlatform;Trusted_Connection=true;MultipleActiveResultSets=true"
  },
  "BlackbaudAuth": {
    "ClientId": "YOUR_CLIENT_ID_HERE",
    "ClientSecret": "YOUR_REGENERATED_SECRET_HERE",
    "AuthorizationEndpoint": "https://app.blackbaud.com/oauth/authorize",
    "TokenEndpoint": "https://oauth2.sky.blackbaud.com/token",
    "RedirectUri": "http://localhost:5000/api/auth/callback",
    "SasApiKey": "YOUR_SAS_API_KEY_HERE"
  },
  "Cors": {
    "AllowedOrigins": [
      "http://localhost:4200",
      "https://localhost:4200"
    ]
  },
  "Jwt": {
    "SecretKey": "YOUR_JWT_SECRET_KEY_MINIMUM_32_CHARACTERS",
    "Issuer": "HackathonPlatform",
    "Audience": "HackathonPlatformClient",
    "ExpirationMinutes": 60
  }
}
```

### Replace These Values:

1. **ClientId**: Your BBID Client ID
2. **ClientSecret**: Your regenerated BBID secret
3. **SecretKey**: Generate a random 32+ character string for JWT signing
   ```bash
   # Generate on macOS/Linux:
   openssl rand -base64 32
   
   # Generate in PowerShell:
   [Convert]::ToBase64String((1..32 | ForEach-Object { Get-Random -Maximum 256 }))
   ```
4. **ConnectionStrings:DefaultConnection**: 
   - Keep as-is for LocalDB
   - Or update for your SQL Server instance

---

## 3️⃣ Configure Frontend

### File: `frontend/src/environments/environment.ts`

```typescript
export const environment = {
  production: false,
  apiUrl: 'http://localhost:5000/api',
  bbidClientId: 'YOUR_CLIENT_ID_HERE',
  bbidAuthUrl: 'https://app.blackbaud.com/oauth/authorize',
  redirectUri: 'http://localhost:4200/auth/callback'
};
```

### Replace These Values:

1. **bbidClientId**: Same Client ID as backend
2. **apiUrl**: Keep as-is for local development
3. **redirectUri**: Keep as-is for local development

---

## 4️⃣ Database Setup Options

### Option A: SQL Server LocalDB (Easiest)
**Default configuration** - works out of the box on Windows.

Connection string:
```
Server=(localdb)\\mssqllocaldb;Database=HackathonPlatform;Trusted_Connection=true;MultipleActiveResultSets=true
```

### Option B: SQL Server Express
If you have SQL Server Express installed:

Connection string:
```
Server=localhost\\SQLEXPRESS;Database=HackathonPlatform;Trusted_Connection=true;MultipleActiveResultSets=true
```

### Option C: SQL Server with Authentication
Connection string:
```
Server=localhost;Database=HackathonPlatform;User Id=sa;Password=YourPassword;TrustServerCertificate=true;
```

### Option D: Azure SQL Database
Connection string:
```
Server=your-server.database.windows.net;Database=HackathonPlatform;User Id=your-user;Password=your-password;Encrypt=true;
```

---

## 5️⃣ Verify Configuration

### Backend Checklist
- [ ] BlackbaudAuth:ClientId is set
- [ ] BlackbaudAuth:ClientSecret is set (and regenerated!)
- [ ] ConnectionStrings:DefaultConnection is valid
- [ ] Jwt:SecretKey is at least 32 characters
- [ ] Cors:AllowedOrigins includes http://localhost:4200

### Frontend Checklist
- [ ] bbidClientId matches backend ClientId
- [ ] apiUrl points to backend (http://localhost:5000/api)
- [ ] redirectUri matches BBID portal configuration

### BBID Portal Checklist
- [ ] Application secret was regenerated
- [ ] Redirect URI http://localhost:5000/api/auth/callback is added
- [ ] Redirect URI http://localhost:4200/auth/callback is added

---

## 6️⃣ Common Issues

### OAuth 401 Unauthorized
**Cause**: Old application secret
**Solution**: Regenerate secret in BBID portal and update appsettings.json

### OAuth Redirect Mismatch
**Cause**: Redirect URI doesn't match exactly
**Solution**: Verify URIs match exactly in:
- Backend appsettings.json
- Frontend environment.ts
- BBID portal configuration

### Database Connection Failed
**Cause**: SQL Server not running or wrong connection string
**Solution**: 
- Verify SQL Server is running
- Check connection string
- Try: `dotnet ef database update` to create database

### CORS Error
**Cause**: Frontend origin not allowed
**Solution**: Add http://localhost:4200 to Cors:AllowedOrigins in appsettings.json

### JWT Invalid Signature
**Cause**: SecretKey changed or too short
**Solution**: Use consistent 32+ character SecretKey across all environments

---

## 7️⃣ Security Best Practices

### Development
✅ Use LocalDB or local SQL Server
✅ Store secrets in appsettings.Development.json (gitignored)
✅ Use User Secrets for sensitive data:
```bash
dotnet user-secrets set "BlackbaudAuth:ClientSecret" "your-secret"
```

### Production
✅ Use Azure Key Vault or environment variables
✅ Never commit secrets to Git
✅ Use different BBID application for production
✅ Enable HTTPS only
✅ Use strong JWT secret (64+ characters)
✅ Restrict CORS to specific domains
✅ Use managed identities for database access

---

## 8️⃣ Environment Variables (Alternative to appsettings.json)

You can also set these as environment variables:

```bash
# macOS/Linux
export BlackbaudAuth__ClientId="your-client-id"
export BlackbaudAuth__ClientSecret="your-secret"
export ConnectionStrings__DefaultConnection="your-connection-string"
export Jwt__SecretKey="your-jwt-secret"

# Windows PowerShell
$env:BlackbaudAuth__ClientId="your-client-id"
$env:BlackbaudAuth__ClientSecret="your-secret"
$env:ConnectionStrings__DefaultConnection="your-connection-string"
$env:Jwt__SecretKey="your-jwt-secret"
```

Note: Use double underscores `__` to represent nested JSON properties.

---

## 9️⃣ Quick Start Commands

After configuration is complete:

```bash
# Backend
cd backend
dotnet restore
dotnet ef migrations add InitialCreate --project src/Blackbaud.Hackathon.Platform.Shared --startup-project src/Blackbaud.Hackathon.Platform.Service
dotnet ef database update --project src/Blackbaud.Hackathon.Platform.Shared --startup-project src/Blackbaud.Hackathon.Platform.Service
cd src/Blackbaud.Hackathon.Platform.Service
dotnet run

# Frontend (new terminal)
cd frontend
npm install
npm start
```

Application will be available at:
- Frontend: http://localhost:4200
- Backend API: https://localhost:5001
- Swagger: https://localhost:5001/swagger

---

## 🆘 Need Help?

1. Check [IMPLEMENTATION_STATUS.md](IMPLEMENTATION_STATUS.md) for current status
2. Review [Blackbaud OAuth Documentation](https://developer.blackbaud.com/skyapi/docs/authorization)
3. Verify all checklist items above
4. Check console/terminal for error messages
