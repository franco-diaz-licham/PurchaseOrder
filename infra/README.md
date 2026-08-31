# Azure Infrastructure

This folder provisions the Azure resources used by the current production-shaped app:

- Azure Container Registry for the API image.
- Azure Container Apps environment and API Container App.
- Azure Database for PostgreSQL Flexible Server plus the `purchase_order` database.
- Azure Static Web Apps for the frontend.
- Log Analytics for Container Apps logs.
- Key Vault containing the generated API database connection string.
- A user-assigned managed identity with `AcrPull` on the registry.

The template is additive infrastructure. It does not redirect Docker, GitHub Actions, or runtime paths to `backend/layered`.

`main.bicep` is intentionally thin. It owns parameters, naming, and wiring between modules. Product-specific resources live under `modules/`.

## Deploy

Create or choose a resource group:

```powershell
az group create --name rg-purchase-order-dev --location eastus2
```

Deploy the template:

```powershell
$postgresPassword = Read-Host "PostgreSQL admin password" -AsSecureString
$postgresPasswordPlain = ConvertFrom-SecureString $postgresPassword -AsPlainText

az deployment group create `
  --name purchase-order-infra `
  --resource-group rg-purchase-order-dev `
  --template-file infra/main.bicep `
  --parameters `
    environmentName=dev `
    location=eastus2 `
    staticWebAppLocation=eastus2 `
    postgresAdminPassword=$postgresPasswordPlain
```

To add your workstation to PostgreSQL firewall rules, pass `postgresFirewallRules` with your public IP range. The example parameters file shows the shape.

## GitHub Secrets

After deployment, read the outputs:

```powershell
az deployment group show `
  --resource-group rg-purchase-order-dev `
  --name purchase-order-infra `
  --query properties.outputs
```

Map the `githubSecrets` output to the existing workflow secrets:

- `ACR_NAME`
- `API_BASE_URL`
- `API_CONTAINER_APP`
- `RESOURCE_GROUP`
- `SWA_NAME`

`AZURE_CREDENTIALS` still comes from your existing Azure service principal setup.

## Notes

The PostgreSQL firewall defaults to allowing Azure-hosted services because Azure Container Apps outbound IPs are not stable enough for a simple public-firewall setup. For stricter production networking, the next step would be VNet integration with private PostgreSQL access.
