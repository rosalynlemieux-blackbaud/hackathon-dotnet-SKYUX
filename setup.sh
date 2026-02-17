#!/bin/bash

echo "=========================================="
echo "Off the Grid - Hackathon Platform"
echo "Quick Start Setup Script"
echo "=========================================="
echo ""

# Check prerequisites
echo "🔍 Checking prerequisites..."

# Check .NET
if ! command -v dotnet &> /dev/null; then
    echo "❌ .NET SDK not found. Please install .NET 8.0 SDK from https://dotnet.microsoft.com/"
    exit 1
fi
echo "✅ .NET SDK found: $(dotnet --version)"

# Check Node.js
if ! command -v node &> /dev/null; then
    echo "❌ Node.js not found. Please install Node.js 18+ from https://nodejs.org/"
    exit 1
fi
echo "✅ Node.js found: $(node --version)"

# Check npm
if ! command -v npm &> /dev/null; then
    echo "❌ npm not found. Please install npm."
    exit 1
fi
echo "✅ npm found: $(npm --version)"

echo ""
echo "=========================================="
echo "⚙️  Configuration Required"
echo "=========================================="
echo ""
echo "Before continuing, you must:"
echo "1. ⚠️  REGENERATE your BBID application secret"
echo "   Go to: https://developer.blackbaud.com/"
echo ""
echo "2. 📝 Update backend/src/Blackbaud.Hackathon.Platform.Service/appsettings.json:"
echo "   - BlackbaudAuth:ClientId"
echo "   - BlackbaudAuth:ClientSecret"
echo "   - ConnectionStrings:DefaultConnection"
echo "   - Jwt:SecretKey"
echo ""
echo "3. 📝 Update frontend/src/environments/environment.ts:"
echo "   - apiUrl"
echo "   - bbidClientId"
echo "   - redirectUri"
echo ""
read -p "Have you completed the configuration? (y/n) " -n 1 -r
echo ""
if [[ ! $REPLY =~ ^[Yy]$ ]]; then
    echo "Please complete configuration and run this script again."
    exit 1
fi

echo ""
echo "=========================================="
echo "📦 Installing Backend Dependencies"
echo "=========================================="
cd backend
dotnet restore
if [ $? -ne 0 ]; then
    echo "❌ Failed to restore backend dependencies"
    exit 1
fi
echo "✅ Backend dependencies restored"

echo ""
echo "=========================================="
echo "📦 Installing Frontend Dependencies"
echo "=========================================="
cd ../frontend
npm install
if [ $? -ne 0 ]; then
    echo "❌ Failed to install frontend dependencies"
    exit 1
fi
echo "✅ Frontend dependencies installed"

echo ""
echo "=========================================="
echo "🗄️  Setting up Database"
echo "=========================================="
cd ../backend
echo "Creating migration..."
dotnet ef migrations add InitialCreate \
    --project src/Blackbaud.Hackathon.Platform.Shared \
    --startup-project src/Blackbaud.Hackathon.Platform.Service

if [ $? -ne 0 ]; then
    echo "⚠️  Migration creation failed (may already exist)"
fi

echo "Applying migration..."
dotnet ef database update \
    --project src/Blackbaud.Hackathon.Platform.Shared \
    --startup-project src/Blackbaud.Hackathon.Platform.Service

if [ $? -ne 0 ]; then
    echo "❌ Failed to apply migrations"
    exit 1
fi
echo "✅ Database created and migrations applied"

echo ""
echo "=========================================="
echo "✨ Setup Complete!"
echo "=========================================="
echo ""
echo "To start the application:"
echo ""
echo "1. Start Backend (Terminal 1):"
echo "   cd backend/src/Blackbaud.Hackathon.Platform.Service"
echo "   dotnet run"
echo "   API will be at: https://localhost:5001"
echo ""
echo "2. Start Frontend (Terminal 2):"
echo "   cd frontend"
echo "   npm start"
echo "   App will be at: http://localhost:4200"
echo ""
echo "🎉 Happy Hacking!"
