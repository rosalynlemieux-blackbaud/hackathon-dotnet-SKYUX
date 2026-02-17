# Infrastructure as Code

This directory contains Azure infrastructure definitions and deployment automation for the Hackathon Platform.

## Structure

```
infrastructure/
├── azure/
│   ├── main.bicep                    # Main infrastructure template
│   ├── parameters.staging.json       # Staging environment parameters
│   ├── parameters.production.json    # Production environment parameters
│   ├── deploy.sh                     # Bash deployment script
│   └── deploy.ps1                    # PowerShell deployment script
├── DEPLOYMENT_GUIDE.md               # Comprehensive deployment documentation
└── QUICK_START.md                    # 5-minute quick start guide
```

## Quick Links

- **[Quick Start Guide](QUICK_START.md)** - Get started in 5 minutes
- **[Full Deployment Guide](DEPLOYMENT_GUIDE.md)** - Detailed instructions, troubleshooting, and advanced topics

## Technologies

- **Bicep**: Infrastructure as Code (IaC) for Azure
- **GitHub Actions**: CI/CD automation
- **Azure App Service**: Backend API hosting (.NET 8.0)
- **Azure Static Web Apps**: Frontend hosting (Angular)
- **Azure SQL Database**: Relational data store
- **Azure Storage**: File uploads
- **Application Insights**: Monitoring and diagnostics
- **Key Vault**: Secrets management

## Environments

### Staging
- **Purpose**: Testing and QA
- **Cost**: ~$50-75/month
- **SKU**: Basic/Standard
- **Seeding**: Enabled (demo data)

### Production
- **Purpose**: Live production workloads
- **Cost**: ~$150-200/month
- **SKU**: Standard/Premium
- **Seeding**: Disabled
- **Features**: Deployment slots, autoscaling, geo-redundancy

## Deployment Methods

### 1. Manual Deployment (Initial Setup)

**Linux/macOS:**
```bash
cd infrastructure/azure
chmod +x deploy.sh
./deploy.sh staging
```

**Windows:**
```powershell
cd infrastructure\azure
.\deploy.ps1 -Environment staging
```

### 2. Automated CI/CD (Continuous)

GitHub Actions workflow automatically deploys on push to `main` branch.

Workflow: `.github/workflows/azure-deploy.yml`

**Stages:**
1. Build backend (.NET)
2. Build frontend (Angular)
3. Run tests
4. Deploy to staging
5. Deploy to production (manual approval)

## Prerequisites

- Azure CLI 2.50+
- Azure subscription
- GitHub repository admin access
- Contributor role on Azure subscription

## Configuration

### Required GitHub Secrets

| Secret | Purpose |
|--------|---------|
| `AZURE_CREDENTIALS` | Service principal JSON |
| `AZURE_SQL_CONNECTION_STRING` | Staging SQL connection |
| `AZURE_SQL_CONNECTION_STRING_PROD` | Production SQL connection |
| `AZURE_STATIC_WEB_APPS_API_TOKEN` | Staging frontend deploy token |
| `AZURE_STATIC_WEB_APPS_API_TOKEN_PROD` | Production frontend deploy token |

### App Service Settings

Set via Azure CLI or Portal:

```bash
az webapp config appsettings set \
  --resource-group rg-hackathon-platform-staging \
  --name hackathon-platform-api-staging \
  --settings \
    BlackbaudAuth__ClientId="YOUR_ID" \
    BlackbaudAuth__ClientSecret="YOUR_SECRET" \
    Jwt__SecretKey="YOUR_KEY" \
    Email__SmtpServer="smtp.gmail.com" \
    Email__SmtpUsername="YOUR_EMAIL"
```

## Resource Naming Convention

All resources follow this pattern:

```
{resource-type}-{base-name}-{component}-{environment}
```

Examples:
- `rg-hackathon-platform-staging` (Resource Group)
- `hackathon-platform-api-staging` (App Service)
- `hackathon-platform-sql-staging` (SQL Server)

## Monitoring

### Application Insights

```bash
# View in Portal
az monitor app-insights component show \
  --resource-group rg-hackathon-platform-staging \
  --app hackathon-platform-insights-staging
```

**Key Metrics:**
- Request rate
- Response time
- Failure rate
- Dependency calls
- Custom events

### Log Streaming

```bash
az webapp log tail \
  --resource-group rg-hackathon-platform-staging \
  --name hackathon-platform-api-staging
```

## Scaling

### Vertical (Scale Up)

```bash
az appservice plan update \
  --resource-group rg-hackathon-platform-production \
  --name hackathon-platform-plan-production \
  --sku P2v3
```

### Horizontal (Scale Out)

```bash
az appservice plan update \
  --resource-group rg-hackathon-platform-production \
  --name hackathon-platform-plan-production \
  --number-of-workers 3
```

### Autoscaling

Configured in Bicep for production environment. Scales based on:
- CPU utilization (>70% = scale out)
- Memory pressure
- Request queue length

## Backup & Recovery

### Database Backups

- **Automatic**: Daily backups (7-day retention)
- **Manual**: Export to .bacpac file

```bash
az sql db export \
  --resource-group rg-hackathon-platform-production \
  --server hackathon-platform-sql-production \
  --name hackathon-platform-db-production \
  --admin-user sqladmin \
  --storage-uri "https://storage.blob.core.windows.net/backups/backup.bacpac"
```

### App Service Backups

Automatic backups configured for production:
- Frequency: Daily
- Retention: 30 days
- Includes: Code, configuration, database connection strings

## Security

- ✅ HTTPS enforced
- ✅ TLS 1.2 minimum
- ✅ Managed Identity for secrets
- ✅ SQL firewall rules
- ✅ Storage private access
- ✅ Key Vault for sensitive data
- ✅ Application Insights for audit logs

## Cost Optimization

### Staging Tips
- Use Basic SQL Database tier
- Use S1 App Service Plan
- Stop resources when not in use
- Enable seeding for instant demo data

### Production Tips
- Enable autoscaling (pay for what you use)
- Use reserved instances (up to 72% savings)
- Set budget alerts
- Review Application Insights retention
- Use Azure Advisor recommendations

### Monthly Cost Breakdown

**Staging:**
- App Service Plan (S1): ~$70
- SQL Database (Basic): ~$5
- Storage: ~$1
- Application Insights: ~$0 (free tier)
- **Total: ~$75/month**

**Production:**
- App Service Plan (P1v3): ~$140
- SQL Database (S1): ~$30
- Storage (GRS): ~$5
- Application Insights: ~$10
- **Total: ~$185/month**

## Troubleshooting

Common issues and solutions in [DEPLOYMENT_GUIDE.md](DEPLOYMENT_GUIDE.md#monitoring--troubleshooting).

Quick checks:

```bash
# Check deployment status
az deployment group list \
  --resource-group rg-hackathon-platform-staging \
  --output table

# Check app status
az webapp show \
  --resource-group rg-hackathon-platform-staging \
  --name hackathon-platform-api-staging \
  --query state -o tsv

# Check SQL connectivity
az sql db show-connection-string \
  --client ado.net \
  --name hackathon-platform-db-staging \
  --server hackathon-platform-sql-staging
```

## Cleanup

Delete entire environment:

```bash
# Staging
az group delete --name rg-hackathon-platform-staging --yes --no-wait

# Production
az group delete --name rg-hackathon-platform-production --yes --no-wait
```

## Support

- **Documentation**: See [DEPLOYMENT_GUIDE.md](DEPLOYMENT_GUIDE.md)
- **Azure Issues**: Check Application Insights logs
- **GitHub Actions**: View workflow run logs
- **Database**: Check SQL Server firewall rules

---

**Getting Started**: Read [QUICK_START.md](QUICK_START.md) for a 5-minute deployment guide.
