@echo off
echo ==========================================
echo Off the Grid - Hackathon Platform
echo Quick Start Setup Script
echo ==========================================
echo.

echo Checking prerequisites...

where dotnet >nul 2>nul
if %ERRORLEVEL% NEQ 0 (
    echo ERROR: .NET SDK not found. Please install .NET 8.0 SDK from https://dotnet.microsoft.com/
    exit /b 1
)
echo .NET SDK found

where node >nul 2>nul
if %ERRORLEVEL% NEQ 0 (
    echo ERROR: Node.js not found. Please install Node.js 18+ from https://nodejs.org/
    exit /b 1
)
echo Node.js found

where npm >nul 2>nul
if %ERRORLEVEL% NEQ 0 (
    echo ERROR: npm not found. Please install npm.
    exit /b 1
)
echo npm found

echo.
echo ==========================================
echo Configuration Required
echo ==========================================
echo.
echo Before continuing, you must:
echo 1. REGENERATE your BBID application secret
echo    Go to: https://developer.blackbaud.com/
echo.
echo 2. Update backend\src\Blackbaud.Hackathon.Platform.Service\appsettings.json:
echo    - BlackbaudAuth:ClientId
echo    - BlackbaudAuth:ClientSecret
echo    - ConnectionStrings:DefaultConnection
echo    - Jwt:SecretKey
echo.
echo 3. Update frontend\src\environments\environment.ts:
echo    - apiUrl
echo    - bbidClientId
echo    - redirectUri
echo.
set /p CONFIGURED="Have you completed the configuration? (y/n): "
if /i not "%CONFIGURED%"=="y" (
    echo Please complete configuration and run this script again.
    exit /b 1
)

echo.
echo ==========================================
echo Installing Backend Dependencies
echo ==========================================
cd backend
call dotnet restore
if %ERRORLEVEL% NEQ 0 (
    echo ERROR: Failed to restore backend dependencies
    exit /b 1
)
echo Backend dependencies restored

echo.
echo ==========================================
echo Installing Frontend Dependencies
echo ==========================================
cd ..\frontend
call npm install
if %ERRORLEVEL% NEQ 0 (
    echo ERROR: Failed to install frontend dependencies
    exit /b 1
)
echo Frontend dependencies installed

echo.
echo ==========================================
echo Setting up Database
echo ==========================================
cd ..\backend
echo Creating migration...
call dotnet ef migrations add InitialCreate --project src\Blackbaud.Hackathon.Platform.Shared --startup-project src\Blackbaud.Hackathon.Platform.Service

echo Applying migration...
call dotnet ef database update --project src\Blackbaud.Hackathon.Platform.Shared --startup-project src\Blackbaud.Hackathon.Platform.Service
if %ERRORLEVEL% NEQ 0 (
    echo ERROR: Failed to apply migrations
    exit /b 1
)
echo Database created and migrations applied

echo.
echo ==========================================
echo Setup Complete!
echo ==========================================
echo.
echo To start the application:
echo.
echo 1. Start Backend (Terminal 1):
echo    cd backend\src\Blackbaud.Hackathon.Platform.Service
echo    dotnet run
echo    API will be at: https://localhost:5001
echo.
echo 2. Start Frontend (Terminal 2):
echo    cd frontend
echo    npm start
echo    App will be at: http://localhost:4200
echo.
echo Happy Hacking!
pause
