# Azure Infrastructure

This folder provisions the Azure resources used by the current production-shaped app:

- Azure Container Registry for the API image.
- Azure Container Apps environment and API Container App.
- Azure Database for PostgreSQL Flexible Server plus the `purchase_order` database.
- Azure Static Web Apps for the frontend.
- Log Analytics for Container Apps logs.
- Key Vault containing the generated API database connection string.
- A user-assigned managed identity with `AcrPull` on the registry.

Regional resources are configured for `australiaeast`. Static Web Apps stays on `eastus2` because Azure Static Web Apps Free is not currently available in Australia regions.

Lowest-cost settings are used:

- Container Apps Consumption with `0` minimum replicas, `1` maximum replica, `0.25` CPU, and `0.5Gi` memory.
- Azure Container Registry `Basic`.
- Static Web Apps `Free`.
- PostgreSQL Flexible Server `Burstable` `Standard_B1ms` with `32` GiB storage.
- Key Vault `standard`.
- Log Analytics pay-as-you-go with `30` day retention.

The template is additive infrastructure. It does not redirect Docker, GitHub Actions, or runtime paths to `backend/layered`.

`main.bicep` is intentionally thin. It owns parameters, naming, and wiring between modules. Product-specific resources live under `modules/`.

## Deploy

Review `infra/.env`:

```dotenv
AZURE_SUBSCRIPTION=<subscription-id-or-name>
AZURE_RESOURCE_GROUP_NAME=rg-purchase-order-prod
AZURE_RESOURCE_GROUP_LOCATION=australiaeast
AZURE_DEPLOYMENT_NAME=purchase-order-infra
AZURE_TEMPLATE_FILE=infra/main.bicep
AZURE_PARAMETERS_FILE=infra/main.parameters.prod.json
POSTGRES_ADMIN_PASSWORD=<strong-postgresql-admin-password>
AZURE_SKIP_WHAT_IF=false
AZURE_WHAT_IF_ONLY=false
```

Run the provisioning script from the repository root:

```powershell
./scripts/provision-infra.ps1
```

To target a specific subscription:

```powershell
./scripts/provision-infra.ps1 -Subscription "<subscription-id-or-name>"
```

To preview without provisioning:

```powershell
./scripts/provision-infra.ps1 -WhatIfOnly
```

To skip the preview:

```powershell
./scripts/provision-infra.ps1 -SkipWhatIf
```

The script creates the resource group, merges the committed parameters with the PostgreSQL password from `infra/.env`, deploys `main.bicep`, and prints the GitHub secrets needed by the existing workflows.

To add your workstation to PostgreSQL firewall rules, add your public IP range to `postgresFirewallRules` in `main.parameters.prod.json` before running the script.

Use `infra/.env` for normal value changes. Command-line arguments are now reserved for the common runtime controls:

```powershell
./scripts/provision-infra.ps1 `
  -Subscription "<subscription-id-or-name>" `
  -SkipWhatIf
```

## Manual Deploy

The script is just a wrapper around these Azure CLI steps.

Log in and choose the subscription:

```powershell
az login
az account set --subscription "<subscription-id-or-name>"
```

Create or update the resource group:

```powershell
az group create `
  --name rg-purchase-order-prod `
  --location australiaeast
```

Read the PostgreSQL password without committing it:

```powershell
$postgresPassword = Read-Host "PostgreSQL admin password" -AsSecureString
$postgresPasswordPlain = ConvertFrom-SecureString $postgresPassword -AsPlainText
```

Preview the deployment:

```powershell
az deployment group what-if `
  --name purchase-order-infra `
  --resource-group rg-purchase-order-prod `
  --template-file infra/main.bicep `
  --parameters "@infra/main.parameters.prod.json" `
    postgresAdminPassword=$postgresPasswordPlain
```

Provision the resources:

```powershell
az deployment group create `
  --name purchase-order-infra `
  --resource-group rg-purchase-order-prod `
  --template-file infra/main.bicep `
  --parameters "@infra/main.parameters.prod.json" `
    postgresAdminPassword=$postgresPasswordPlain
```

Read the workflow outputs:

```powershell
az deployment group show `
  --resource-group rg-purchase-order-prod `
  --name purchase-order-infra `
  --query properties.outputs.githubSecrets.value
```

## Parameters And Secrets

Normal environment values live in `main.parameters.prod.json`:

```json
{
    "parameters": {
        "environmentName": {
            "value": "prod"
        },
        "location": {
            "value": "australiaeast"
        }
    }
}
```

Secret values should not be committed. `infra/.env` is ignored by Git, so set `POSTGRES_ADMIN_PASSWORD` locally:

```dotenv
POSTGRES_ADMIN_PASSWORD=replace-with-a-strong-local-secret
```

The script injects that password into a temporary parameters file that is deleted after deployment. If `POSTGRES_ADMIN_PASSWORD` is empty, the script fails and tells you which key is missing.

In GitHub Actions, the same script can read the password from a GitHub Secret mapped to an environment variable:

```powershell
$env:POSTGRES_ADMIN_PASSWORD = "${{ secrets.POSTGRES_ADMIN_PASSWORD }}"
./scripts/provision-infra.ps1 -SkipWhatIf
```

If you need to override normal values temporarily, create another env file and pass it to the script:

```powershell
./scripts/provision-infra.ps1 `
  -EnvFile infra/.env.prod
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
