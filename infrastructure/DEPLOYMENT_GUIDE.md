# Azure Deployment Guide

## Overview

This guide covers deploying the Hackathon Platform to Microsoft Azure using Infrastructure as Code (IaC) with Bicep and automated CI/CD with GitHub Actions.

## Architecture

### Azure Resources

The deployment creates the following resources:

| Resource | Purpose | SKU |
|----------|---------|-----|
| **App Service Plan** | Hosts backend API | S1 (Staging), P1v3 (Production) |
| **App Service** | .NET 8.0 API with staging slot | Linux |
| **Azure SQL Database** | Primary data store | Basic (Staging), S1 (Production) |
| **Azure SQL Server** | Database host | - |
| **Storage Account** | File uploads | Standard_LRS (Staging), Standard_GRS (Prod) |
| **Application Insights** | Monitoring & telemetry | - |
| **Key Vault** | Secrets management | Standard |
| **Static Web App** | Angular frontend | Free |

### Environments

- **Staging**: Cost-optimized for testing
- **Production**: High availability with scaling

## Prerequisites

### Required Tools

1. **Azure CLI** (v2.50+)
   ```bash
   # Install on macOS
   brew install azure-cli
   
   # Install on Windows
   winget install Microsoft.AzureCLI
   
   # Verify installation
   az --version
   ```

2. **Azure Subscription**
   - Active Azure subscription with Owner/Contributor access
   - Sufficient quota for resources

3. **GitHub Repository**
   - Admin access to configure secrets
   - Actions enabled

### Azure Login

```bash
# Login to Azure
az login

# List subscriptions
az account list --output table

# Set active subscription
az account set --subscription "Your-Subscription-Name"
```

## Initial Setup

### Step 1: Create Service Principal for GitHub Actions

```bash
# Create service principal with Contributor role
SUBSCRIPTION_ID=$(az account show --query id -o tsv)

az ad sp create-for-rbac \
  --name "hackathon-platform-github-actions" \
  --role contributor \
  --scopes /subscriptions/$SUBSCRIPTION_ID \
  --sdk-auth

# Output (save this JSON for GitHub secrets):
{
  "clientId": "xxx",
  "clientSecret": "xxx",
  "subscriptionId": "xxx",
  "tenantId": "xxx",
  "activeDirectoryEndpointUrl": "https://login.microsoftonline.com",
  "resourceManagerEndpointUrl": "https://management.azure.com/",
  ...
}
```

### Step 2: Configure GitHub Secrets

Navigate to **Settings → Secrets and variables → Actions** and add:

| Secret Name | Value | Description |
|-------------|-------|-------------|
| `AZURE_CREDENTIALS` | JSON from Step 1 | Service principal credentials |
| `AZURE_SQL_CONNECTION_STRING` | `Server=tcp:...` | Staging SQL connection string |
| `AZURE_SQL_CONNECTION_STRING_PROD` | `Server=tcp:...` | Production SQL connection string |
| `AZURE_STATIC_WEB_APPS_API_TOKEN` | From Azure Portal | Static Web App deployment token (staging) |
| `AZURE_STATIC_WEB_APPS_API_TOKEN_PROD` | From Azure Portal | Static Web App deployment token (prod) |

### Step 3: Store SQL Admin Password in Key Vault

```bash
# Create a temporary Key Vault for parameter storage
az keyvault create \
  --name "hackathon-kv-params" \
  --resource-group "rg-hackathon-shared" \
  --location "eastus"

# Store SQL admin password
az keyvault secret set \
  --vault-name "hackathon-kv-params" \
  --name "SqlAdminPassword" \
  --value "YourSecurePassword123!"
```

## Deployment Methods

### Method 1: Manual Deployment (One-Time Setup)

#### Using Bash Script (Linux/macOS)

```bash
# Navigate to infrastructure directory
cd hackathon-platform/infrastructure/azure

# Make script executable
chmod +x deploy.sh

# Deploy to staging
./deploy.sh staging

# Deploy to production
./deploy.sh production
```

#### Using PowerShell Script (Windows)

```powershell
# Navigate to infrastructure directory
cd hackathon-platform\infrastructure\azure

# Deploy to staging
.\deploy.ps1 -Environment staging

# Deploy to production
.\deploy.ps1 -Environment production
```

#### Using Azure CLI Directly

```bash
# Set variables
ENVIRONMENT="staging"
RESOURCE_GROUP="rg-hackathon-platform-${ENVIRONMENT}"
LOCATION="eastus"

# Create resource group
az group create \
  --name $RESOURCE_GROUP \
  --location $LOCATION

# Deploy Bicep template
az deployment group create \
  --resource-group $RESOURCE_GROUP \
  --template-file main.bicep \
  --parameters parameters.${ENVIRONMENT}.json \
  --name "hackathon-$(date +%Y%m%d-%H%M%S)"

# Get deployment outputs
az deployment group show \
  --resource-group $RESOURCE_GROUP \
  --name "hackathon-YYYYMMDD-HHMMSS" \
  --query properties.outputs
```

### Method 2: Automated CI/CD with GitHub Actions

The `.github/workflows/azure-deploy.yml` workflow automatically:

1. ✅ Builds .NET backend
2. ✅ Runs unit tests
3. ✅ Builds Angular frontend
4. ✅ Deploys to Azure staging
5. ✅ Runs database migrations
6. ⏸️ Waits for approval
7. ✅ Deploys to production
8. ✅ Runs health checks

#### Trigger Deployment

**Automatic:** Push to `main` branch
```bash
git push origin main
```

**Manual:** GitHub Actions → Run workflow
1. Go to **Actions** tab
2. Select **Azure Deployment CI/CD**
3. Click **Run workflow**
4. Choose environment (staging/production)

## Post-Deployment Configuration

### Step 1: Verify Deployment

```bash
# Get API URL
API_URL=$(az deployment group show \
  --resource-group rg-hackathon-platform-staging \
  --name YOUR_DEPLOYMENT_NAME \
  --query properties.outputs.apiAppServiceUrl.value -o tsv)

# Test health endpoint
curl $API_URL/health

# Expected: HTTP 200 OK
```

### Step 2: Run Database Migrations

```bash
# Connect to SQL Database
SQL_CONNECTION="Server=tcp:hackathon-platform-sql-staging.database.windows.net,1433;Initial Catalog=hackathon-platform-db-staging;..."

# Run migrations
cd hackathon-platform/backend
dotnet ef database update --connection "$SQL_CONNECTION"

# Verify seeding (if enabled)
# Check logs in Application Insights
```

### Step 3: Configure Application Settings

```bash
# Set Blackbaud OAuth credentials
az webapp config appsettings set \
  --resource-group rg-hackathon-platform-staging \
  --name hackathon-platform-api-staging \
  --settings \
    BlackbaudAuth__ClientId="YOUR_CLIENT_ID" \
    BlackbaudAuth__ClientSecret="YOUR_CLIENT_SECRET" \
    BlackbaudAuth__RedirectUri="https://hackathon-platform-api-staging.azurewebsites.net/api/auth/callback"

# Set JWT secret (use strong random string)
az webapp config appsettings set \
  --resource-group rg-hackathon-platform-staging \
  --name hackathon-platform-api-staging \
  --settings \
    Jwt__SecretKey="YOUR_SECURE_JWT_SECRET_KEY_MINIMUM_32_CHARS"

# Configure SMTP for emails
az webapp config appsettings set \
  --resource-group rg-hackathon-platform-staging \
  --name hackathon-platform-api-staging \
  --settings \
    Email__SmtpServer="smtp.gmail.com" \
    Email__SmtpPort="587" \
    Email__SmtpUsername="YOUR_EMAIL" \
    Email__SmtpPassword="YOUR_APP_PASSWORD"
```

### Step 4: Configure CORS

Frontend domain must be allowed:

```bash
# Get Static Web App URL
FRONTEND_URL=$(az staticwebapp show \
  --name hackathon-platform-web-staging \
  --query "defaultHostname" -o tsv)

# Update CORS in App Service
az webapp cors add \
  --resource-group rg-hackathon-platform-staging \
  --name hackathon-platform-api-staging \
  --allowed-origins "https://$FRONTEND_URL"
```

### Step 5: Enable Continuous Deployment

```bash
# Enable deployment slots (production only)
az webapp deployment slot create \
  --resource-group rg-hackathon-platform-production \
  --name hackathon-platform-api-production \
  --slot staging

# Configure auto-swap
az webapp deployment slot auto-swap \
  --resource-group rg-hackathon-platform-production \
  --name hackathon-platform-api-production \
  --slot staging
```

## Monitoring & Troubleshooting

### Application Insights

View logs and telemetry:

```bash
# Get Application Insights instrumentation key
az monitor app-insights component show \
  --resource-group rg-hackathon-platform-staging \
  --app hackathon-platform-insights-staging \
  --query "instrumentationKey" -o tsv

# Access in Azure Portal:
# Application Insights → Live Metrics → Failures → Logs
```

### View Application Logs

```bash
# Stream logs from App Service
az webapp log tail \
  --resource-group rg-hackathon-platform-staging \
  --name hackathon-platform-api-staging

# Download logs
az webapp log download \
  --resource-group rg-hackathon-platform-staging \
  --name hackathon-platform-api-staging \
  --log-file app-logs.zip
```

### Common Issues

#### Issue: Database Connection Timeout

**Solution:** Add your IP to SQL Server firewall

```bash
MY_IP=$(curl -s https://api.ipify.org)

az sql server firewall-rule create \
  --resource-group rg-hackathon-platform-staging \
  --server hackathon-platform-sql-staging \
  --name "AllowMyIP" \
  --start-ip-address $MY_IP \
  --end-ip-address $MY_IP
```

#### Issue: Deployment Fails with Authentication Error

**Solution:** Verify service principal permissions

```bash
# Check service principal
az ad sp list --display-name "hackathon-platform-github-actions" --output table

# Verify role assignment
az role assignment list \
  --assignee YOUR_SERVICE_PRINCIPAL_ID \
  --output table
```

#### Issue: App Service Shows 500 Error

**Solution:** Check application logs

```bash
# Enable detailed error messages
az webapp config set \
  --resource-group rg-hackathon-platform-staging \
  --name hackathon-platform-api-staging \
  --detailed-error-logging-enabled true

# View logs
az webapp log tail \
  --resource-group rg-hackathon-platform-staging \
  --name hackathon-platform-api-staging
```

## Scaling & Performance

### Scale Up (Vertical Scaling)

```bash
# Upgrade to higher SKU
az appservice plan update \
  --resource-group rg-hackathon-platform-production \
  --name hackathon-platform-plan-production \
  --sku P2v3
```

### Scale Out (Horizontal Scaling)

```bash
# Increase instance count
az appservice plan update \
  --resource-group rg-hackathon-platform-production \
  --name hackathon-platform-plan-production \
  --number-of-workers 3

# Configure autoscaling
az monitor autoscale create \
  --resource-group rg-hackathon-platform-production \
  --resource hackathon-platform-plan-production \
  --resource-type Microsoft.Web/serverfarms \
  --name autoscale-rule \
  --min-count 2 \
  --max-count 5 \
  --count 2

# Add CPU-based scaling rule
az monitor autoscale rule create \
  --resource-group rg-hackathon-platform-production \
  --autoscale-name autoscale-rule \
  --condition "Percentage CPU > 70 avg 5m" \
  --scale out 1
```

### Database Performance

```bash
# Upgrade SQL Database tier
az sql db update \
  --resource-group rg-hackathon-platform-production \
  --server hackathon-platform-sql-production \
  --name hackathon-platform-db-production \
  --service-objective S2

# Enable Query Performance Insights
# (Available in Azure Portal → SQL Database → Intelligent Performance)
```

## Cost Optimization

### Staging Environment

- Use **Basic** SQL Database (instead of Standard)
- Use **S1** App Service Plan (instead of P1v3)
- Disable seeding in production: `Database__SeedOnStartup: false`
- Use **Standard_LRS** Storage (instead of GRS)

### Production Environment

- Enable **autoscaling** to handle variable load
- Use **reserved capacity** for significant discount
- Enable **auto-pause** for non-prod SQL databases
- Review **Application Insights** retention (default 90 days)

### Cost Monitoring

```bash
# View cost analysis
az consumption usage list \
  --start-date 2026-02-01 \
  --end-date 2026-02-28 \
  --output table

# Set budget alerts
az consumption budget create \
  --budget-name hackathon-platform-budget \
  --amount 100 \
  --time-grain Monthly \
  --start-date 2026-02-01 \
  --end-date 2026-12-31
```

## Backup & Disaster Recovery

### Database Backups

```bash
# Manual backup
az sql db export \
  --resource-group rg-hackathon-platform-production \
  --server hackathon-platform-sql-production \
  --name hackathon-platform-db-production \
  --admin-user sqladmin \
  --admin-password "YOUR_PASSWORD" \
  --storage-key-type StorageAccessKey \
  --storage-key "YOUR_STORAGE_KEY" \
  --storage-uri "https://yourstorageaccount.blob.core.windows.net/backups/backup.bacpac"

# Restore from backup
az sql db import \
  --resource-group rg-hackathon-platform-production \
  --server hackathon-platform-sql-production \
  --name hackathon-platform-db-production-restored \
  --admin-user sqladmin \
  --admin-password "YOUR_PASSWORD" \
  --storage-key-type StorageAccessKey \
  --storage-key "YOUR_STORAGE_KEY" \
  --storage-uri "https://yourstorageaccount.blob.core.windows.net/backups/backup.bacpac"
```

### App Service Backups

Automatic backups (production only):

```bash
# Configure automatic backup
az webapp config backup update \
  --resource-group rg-hackathon-platform-production \
  --webapp-name hackathon-platform-api-production \
  --container-url "https://yourstorageaccount.blob.core.windows.net/backups?sv=..." \
  --frequency 1d \
  --retain-one true \
  --retention 30
```

## Security Hardening

### Enable Managed Identity

Already configured in Bicep template. Verify:

```bash
az webapp identity show \
  --resource-group rg-hackathon-platform-production \
  --name hackathon-platform-api-production
```

### Configure TLS/SSL

```bash
# Enforce HTTPS only (already in template)
az webapp update \
  --resource-group rg-hackathon-platform-production \
  --name hackathon-platform-api-production \
  --https-only true

# Set minimum TLS version
az webapp config set \
  --resource-group rg-hackathon-platform-production \
  --name hackathon-platform-api-production \
  --min-tls-version 1.2
```

### Network Security

```bash
# Restrict SQL Server access to Azure services only
az sql server firewall-rule create \
  --resource-group rg-hackathon-platform-production \
  --server hackathon-platform-sql-production \
  --name "AllowAzureServices" \
  --start-ip-address 0.0.0.0 \
  --end-ip-address 0.0.0.0

# Enable Advanced Threat Protection
az sql server threat-policy update \
  --resource-group rg-hackathon-platform-production \
  --server hackathon-platform-sql-production \
  --state Enabled
```

## Cleanup

### Delete Staging Environment

```bash
az group delete \
  --name rg-hackathon-platform-staging \
  --yes \
  --no-wait
```

### Delete Production Environment

```bash
az group delete \
  --name rg-hackathon-platform-production \
  --yes \
  --no-wait
```

### Delete Service Principal

```bash
az ad sp delete \
  --id $(az ad sp list --display-name "hackathon-platform-github-actions" --query "[0].id" -o tsv)
```

---

## Quick Reference

### Resource Naming Convention

| Resource Type | Naming Pattern |
|---------------|----------------|
| Resource Group | `rg-hackathon-platform-{env}` |
| App Service Plan | `hackathon-platform-plan-{env}` |
| App Service | `hackathon-platform-api-{env}` |
| SQL Server | `hackathon-platform-sql-{env}` |
| SQL Database | `hackathon-platform-db-{env}` |
| Storage Account | `hackathonplatstor{unique}` |
| Key Vault | `hackathon-platform-kv-{unique}` |
| App Insights | `hackathon-platform-insights-{env}` |

### Useful Commands

```bash
# List all resources in resource group
az resource list --resource-group rg-hackathon-platform-staging --output table

# Get connection string
az sql db show-connection-string \
  --client ado.net \
  --name hackathon-platform-db-staging \
  --server hackathon-platform-sql-staging

# Restart app service
az webapp restart \
  --resource-group rg-hackathon-platform-staging \
  --name hackathon-platform-api-staging

# View deployment history
az deployment group list \
  --resource-group rg-hackathon-platform-staging \
  --output table
```

---

**Last Updated**: February 17, 2026  
**Azure CLI Version**: 2.57+  
**Bicep Version**: 0.24+
