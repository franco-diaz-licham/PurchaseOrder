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
az group create --name rg-purchase-order-prod --location eastus2
```

Deploy the template:

```powershell
$postgresPassword = Read-Host "PostgreSQL admin password" -AsSecureString
$postgresPasswordPlain = ConvertFrom-SecureString $postgresPassword -AsPlainText

az deployment group create `
  --name purchase-order-infra `
  --resource-group rg-purchase-order-prod `
  --template-file infra/main.bicep `
  --parameters "@infra/main.parameters.prod.json" `
    postgresAdminPassword=$postgresPasswordPlain
```

To add your workstation to PostgreSQL firewall rules, add your public IP range to `postgresFirewallRules` in `main.parameters.prod.json`.

## Parameters And Secrets

Normal environment values live in `main.parameters.prod.json`:

```json
{
  "parameters": {
    "environmentName": {
      "value": "prod"
    },
    "location": {
      "value": "eastus2"
    }
  }
}
```

Secret values should not be committed. Pass them after the parameters file:

```powershell
az deployment group create `
  --name purchase-order-infra `
  --resource-group rg-purchase-order-prod `
  --template-file infra/main.bicep `
  --parameters "@infra/main.parameters.prod.json" `
    postgresAdminPassword=$postgresPasswordPlain
```

In GitHub Actions, the same pattern uses GitHub Secrets:

```powershell
az deployment group create `
  --name purchase-order-infra `
  --resource-group $env:RESOURCE_GROUP `
  --template-file infra/main.bicep `
  --parameters "@infra/main.parameters.prod.json" `
    postgresAdminPassword="$env:POSTGRES_ADMIN_PASSWORD"
```

If you need to override a normal value temporarily, pass it after the file:

```powershell
az deployment group create `
  --name purchase-order-infra `
  --resource-group rg-purchase-order-prod `
  --template-file infra/main.bicep `
  --parameters "@infra/main.parameters.prod.json" `
    postgresAdminPassword=$postgresPasswordPlain `
    location=australiaeast
```

## GitHub Secrets

After deployment, read the outputs:

```powershell
az deployment group show `
  --resource-group rg-purchase-order-prod `
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
