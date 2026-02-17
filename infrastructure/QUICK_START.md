# Azure Deployment - Quick Start

This is a condensed version of the full [DEPLOYMENT_GUIDE.md](DEPLOYMENT_GUIDE.md). For detailed instructions, see the full guide.

## Prerequisites

- Azure CLI installed: `az --version`
- Azure subscription with Owner/Contributor access
- GitHub repository admin access

## 5-Minute Setup

### 1. Azure Login

```bash
az login
az account set --subscription "Your-Subscription-Name"
```

### 2. Create Service Principal

```bash
SUBSCRIPTION_ID=$(az account show --query id -o tsv)

az ad sp create-for-rbac \
  --name "hackathon-platform-github-actions" \
  --role contributor \
  --scopes /subscriptions/$SUBSCRIPTION_ID \
  --sdk-auth
```

**Save the JSON output!**

### 3. Configure GitHub Secrets

Go to **Settings → Secrets and variables → Actions**, add:

- `AZURE_CREDENTIALS`: JSON from step 2
- `AZURE_SQL_CONNECTION_STRING`: Your staging SQL connection string
- `AZURE_SQL_CONNECTION_STRING_PROD`: Your production SQL connection string

### 4. Deploy Infrastructure

```bash
cd hackathon-platform/infrastructure/azure

# Make executable (Linux/macOS)
chmod +x deploy.sh

# Deploy to staging
./deploy.sh staging

# Or on Windows PowerShell:
.\deploy.ps1 -Environment staging
```

### 5. Configure App Settings

```bash
# Set OAuth credentials
az webapp config appsettings set \
  --resource-group rg-hackathon-platform-staging \
  --name hackathon-platform-api-staging \
  --settings \
    BlackbaudAuth__ClientId="YOUR_CLIENT_ID" \
    BlackbaudAuth__ClientSecret="YOUR_CLIENT_SECRET" \
    Jwt__SecretKey="YOUR_SECURE_JWT_SECRET_32_CHARS_MINIMUM"
```

### 6. Push Code

```bash
git push origin main
```

GitHub Actions will automatically build and deploy!

## Verify Deployment

```bash
# Get API URL
API_URL=$(az deployment group show \
  --resource-group rg-hackathon-platform-staging \
  --name YOUR_DEPLOYMENT_NAME \
  --query properties.outputs.apiAppServiceUrl.value -o tsv)

# Test
curl $API_URL/health
```

## Common Commands

```bash
# View logs
az webapp log tail \
  --resource-group rg-hackathon-platform-staging \
  --name hackathon-platform-api-staging

# Restart app
az webapp restart \
  --resource-group rg-hackathon-platform-staging \
  --name hackathon-platform-api-staging

# Scale up
az appservice plan update \
  --resource-group rg-hackathon-platform-staging \
  --name hackathon-platform-plan-staging \
  --sku P1v3

# Delete environment
az group delete --name rg-hackathon-platform-staging --yes
```

## Resources Created

- App Service (API)
- Static Web App (Frontend)
- Azure SQL Database
- Storage Account (file uploads)
- Application Insights (monitoring)
- Key Vault (secrets)

## Cost Estimate

**Staging**: ~$50-75/month  
**Production**: ~$150-200/month

Enable autoscaling for cost optimization.

## Troubleshooting

| Issue | Solution |
|-------|----------|
| 500 error | Check logs: `az webapp log tail ...` |
| Database timeout | Add IP to firewall: `az sql server firewall-rule create ...` |
| Deployment fails | Verify service principal: `az ad sp list --display-name ...` |

## Next Steps

1. ✅ Deploy infrastructure
2. ✅ Configure app settings
3. ✅ Push code
4. ✅ Run database migrations
5. Configure custom domain (optional)
6. Set up monitoring alerts
7. Configure backup policies

---

**Need help?** See full [DEPLOYMENT_GUIDE.md](DEPLOYMENT_GUIDE.md)
