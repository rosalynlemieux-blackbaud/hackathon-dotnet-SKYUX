#!/bin/bash

# Azure Deployment Script for Hackathon Platform
# Usage: ./deploy.sh [staging|production]

set -e

# Configuration
ENVIRONMENT=${1:-staging}
RESOURCE_GROUP="rg-hackathon-platform-${ENVIRONMENT}"
LOCATION="eastus"
TEMPLATE_FILE="./main.bicep"
PARAMETERS_FILE="./parameters.${ENVIRONMENT}.json"

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

echo -e "${GREEN}========================================${NC}"
echo -e "${GREEN}Hackathon Platform - Azure Deployment${NC}"
echo -e "${GREEN}========================================${NC}"
echo ""
echo -e "Environment: ${YELLOW}${ENVIRONMENT}${NC}"
echo -e "Resource Group: ${YELLOW}${RESOURCE_GROUP}${NC}"
echo -e "Location: ${YELLOW}${LOCATION}${NC}"
echo ""

# Check if Azure CLI is installed
if ! command -v az &> /dev/null; then
    echo -e "${RED}Error: Azure CLI is not installed${NC}"
    echo "Please install from: https://docs.microsoft.com/en-us/cli/azure/install-azure-cli"
    exit 1
fi

# Check if logged in to Azure
echo -e "${YELLOW}Checking Azure login status...${NC}"
az account show &> /dev/null || {
    echo -e "${RED}Not logged in to Azure. Please login:${NC}"
    az login
}

# Display current subscription
SUBSCRIPTION=$(az account show --query name -o tsv)
echo -e "Current subscription: ${GREEN}${SUBSCRIPTION}${NC}"
echo ""

# Confirm deployment
read -p "Do you want to continue with deployment? (yes/no): " CONFIRM
if [ "$CONFIRM" != "yes" ]; then
    echo -e "${RED}Deployment cancelled${NC}"
    exit 0
fi

# Create resource group if it doesn't exist
echo -e "${YELLOW}Creating resource group...${NC}"
az group create \
    --name "${RESOURCE_GROUP}" \
    --location "${LOCATION}" \
    --output none

echo -e "${GREEN}✓ Resource group ready${NC}"

# Validate Bicep template
echo -e "${YELLOW}Validating Bicep template...${NC}"
az deployment group validate \
    --resource-group "${RESOURCE_GROUP}" \
    --template-file "${TEMPLATE_FILE}" \
    --parameters "${PARAMETERS_FILE}" \
    --output none

echo -e "${GREEN}✓ Template validation passed${NC}"

# Deploy infrastructure
echo -e "${YELLOW}Deploying infrastructure...${NC}"
echo -e "${YELLOW}This may take 5-10 minutes...${NC}"

DEPLOYMENT_NAME="hackathon-platform-$(date +%Y%m%d-%H%M%S)"

az deployment group create \
    --resource-group "${RESOURCE_GROUP}" \
    --template-file "${TEMPLATE_FILE}" \
    --parameters "${PARAMETERS_FILE}" \
    --name "${DEPLOYMENT_NAME}" \
    --output table

echo -e "${GREEN}✓ Infrastructure deployed successfully${NC}"

# Get outputs
echo -e "${YELLOW}Retrieving deployment outputs...${NC}"
API_URL=$(az deployment group show \
    --resource-group "${RESOURCE_GROUP}" \
    --name "${DEPLOYMENT_NAME}" \
    --query properties.outputs.apiAppServiceUrl.value \
    -o tsv)

SQL_SERVER=$(az deployment group show \
    --resource-group "${RESOURCE_GROUP}" \
    --name "${DEPLOYMENT_NAME}" \
    --query properties.outputs.sqlServerFqdn.value \
    -o tsv)

echo ""
echo -e "${GREEN}========================================${NC}"
echo -e "${GREEN}Deployment Complete!${NC}"
echo -e "${GREEN}========================================${NC}"
echo ""
echo -e "API URL: ${GREEN}${API_URL}${NC}"
echo -e "SQL Server: ${GREEN}${SQL_SERVER}${NC}"
echo ""
echo -e "${YELLOW}Next Steps:${NC}"
echo "1. Configure GitHub Actions secrets with Azure credentials"
echo "2. Push code to trigger CI/CD pipeline"
echo "3. Run database migrations"
echo "4. Configure custom domain (optional)"
echo ""
echo -e "${GREEN}Deployment script completed successfully!${NC}"
