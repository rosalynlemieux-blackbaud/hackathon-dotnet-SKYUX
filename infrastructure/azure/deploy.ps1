# Azure Deployment Script for Hackathon Platform
# Usage: .\deploy.ps1 -Environment [staging|production]

param(
    [Parameter(Mandatory=$false)]
    [ValidateSet('staging', 'production')]
    [string]$Environment = 'staging'
)

$ErrorActionPreference = "Stop"

# Configuration
$ResourceGroup = "rg-hackathon-platform-$Environment"
$Location = "eastus"
$TemplateFile = ".\main.bicep"
$ParametersFile = ".\parameters.$Environment.json"

Write-Host "========================================" -ForegroundColor Green
Write-Host "Hackathon Platform - Azure Deployment" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Green
Write-Host ""
Write-Host "Environment: $Environment" -ForegroundColor Yellow
Write-Host "Resource Group: $ResourceGroup" -ForegroundColor Yellow
Write-Host "Location: $Location" -ForegroundColor Yellow
Write-Host ""

# Check if Azure CLI is installed
try {
    az --version | Out-Null
} catch {
    Write-Host "Error: Azure CLI is not installed" -ForegroundColor Red
    Write-Host "Please install from: https://docs.microsoft.com/en-us/cli/azure/install-azure-cli" -ForegroundColor Yellow
    exit 1
}

# Check if logged in to Azure
Write-Host "Checking Azure login status..." -ForegroundColor Yellow
try {
    az account show | Out-Null
} catch {
    Write-Host "Not logged in to Azure. Please login:" -ForegroundColor Red
    az login
}

# Display current subscription
$Subscription = az account show --query name -o tsv
Write-Host "Current subscription: $Subscription" -ForegroundColor Green
Write-Host ""

# Confirm deployment
$Confirm = Read-Host "Do you want to continue with deployment? (yes/no)"
if ($Confirm -ne "yes") {
    Write-Host "Deployment cancelled" -ForegroundColor Red
    exit 0
}

# Create resource group if it doesn't exist
Write-Host "Creating resource group..." -ForegroundColor Yellow
az group create `
    --name $ResourceGroup `
    --location $Location `
    --output none

Write-Host "✓ Resource group ready" -ForegroundColor Green

# Validate Bicep template
Write-Host "Validating Bicep template..." -ForegroundColor Yellow
az deployment group validate `
    --resource-group $ResourceGroup `
    --template-file $TemplateFile `
    --parameters $ParametersFile `
    --output none

Write-Host "✓ Template validation passed" -ForegroundColor Green

# Deploy infrastructure
Write-Host "Deploying infrastructure..." -ForegroundColor Yellow
Write-Host "This may take 5-10 minutes..." -ForegroundColor Yellow

$DeploymentName = "hackathon-platform-$(Get-Date -Format 'yyyyMMdd-HHmmss')"

az deployment group create `
    --resource-group $ResourceGroup `
    --template-file $TemplateFile `
    --parameters $ParametersFile `
    --name $DeploymentName `
    --output table

Write-Host "✓ Infrastructure deployed successfully" -ForegroundColor Green

# Get outputs
Write-Host "Retrieving deployment outputs..." -ForegroundColor Yellow
$ApiUrl = az deployment group show `
    --resource-group $ResourceGroup `
    --name $DeploymentName `
    --query properties.outputs.apiAppServiceUrl.value `
    -o tsv

$SqlServer = az deployment group show `
    --resource-group $ResourceGroup `
    --name $DeploymentName `
    --query properties.outputs.sqlServerFqdn.value `
    -o tsv

Write-Host ""
Write-Host "========================================" -ForegroundColor Green
Write-Host "Deployment Complete!" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Green
Write-Host ""
Write-Host "API URL: $ApiUrl" -ForegroundColor Green
Write-Host "SQL Server: $SqlServer" -ForegroundColor Green
Write-Host ""
Write-Host "Next Steps:" -ForegroundColor Yellow
Write-Host "1. Configure GitHub Actions secrets with Azure credentials"
Write-Host "2. Push code to trigger CI/CD pipeline"
Write-Host "3. Run database migrations"
Write-Host "4. Configure custom domain (optional)"
Write-Host ""
Write-Host "Deployment script completed successfully!" -ForegroundColor Green
