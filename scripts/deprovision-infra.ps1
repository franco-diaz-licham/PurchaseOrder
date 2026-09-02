#!/usr/bin/env pwsh

<#
.SYNOPSIS
Deletes the PurchaseOrder Azure infrastructure resource group.

.DESCRIPTION
Reads the target subscription and resource group from infra/.env, confirms the
resource group contents, and deletes the group through Azure CLI.

.EXAMPLE
./scripts/deprovision-infra.ps1

.EXAMPLE
./scripts/deprovision-infra.ps1 -Subscription "00000000-0000-0000-0000-000000000000"

.EXAMPLE
./scripts/deprovision-infra.ps1 -Force

.EXAMPLE
./scripts/deprovision-infra.ps1 -Force -NoWait
#>

[CmdletBinding()]
param(
    [string]$Subscription,
    [string]$EnvFile = (Join-Path $PSScriptRoot "../infra/.env"),
    [switch]$Force,
    [switch]$NoWait
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

# Load reusable helper functions from the local script module.
Import-Module (Join-Path $PSScriptRoot "InfraHelpers.psm1") -Force

# Validate required local tooling before reading configuration.
if (-not (Get-Command az -ErrorAction SilentlyContinue)) {
    throw "Azure CLI was not found. Install it from https://learn.microsoft.com/cli/azure/install-azure-cli, then run this script again."
}

# Load environment-driven Azure configuration.
$repoRoot = (Resolve-Path (Join-Path $PSScriptRoot "..")).Path
$envFilePath = if ([IO.Path]::IsPathRooted($EnvFile)) { $EnvFile } else { Join-Path $repoRoot $EnvFile }
Import-DotEnv -Path $envFilePath

# Resolve the subscription and resource group that will be deleted.
$subscriptionName = if ([string]::IsNullOrWhiteSpace($Subscription)) { Get-Config -Name "AZURE_SUBSCRIPTION" } else { $Subscription }
$resourceGroupName = Get-Config -Name "AZURE_RESOURCE_GROUP_NAME"

# Ensure Azure CLI is authenticated before attempting deletion.
$account = $null
try {
    $account = Invoke-AzJson -Arguments @("account", "show", "--output", "json")
} catch {
    Write-Host "Azure CLI is not logged in. Starting az login..." -ForegroundColor Yellow
    Invoke-Az -Arguments @("login", "--output", "none")
    $account = Invoke-AzJson -Arguments @("account", "show", "--output", "json")
}

# Select the target subscription from .env or the explicit script parameter.
Write-Host "Using Azure subscription: $subscriptionName" -ForegroundColor Cyan
Invoke-Az -Arguments @("account", "set", "--subscription", $subscriptionName)
$account = Invoke-AzJson -Arguments @("account", "show", "--output", "json")

Write-Host "Active subscription: $($account.name) [$($account.id)]" -ForegroundColor Cyan
Write-Host "Target resource group: $resourceGroupName" -ForegroundColor Yellow

# Exit cleanly when the resource group has already been removed.
$resourceGroupExists = Invoke-AzJson -Arguments @(
    "group", "exists",
    "--name", $resourceGroupName,
    "--output", "json"
)

if (-not $resourceGroupExists) {
    Write-Host "Resource group does not exist. Nothing to delete." -ForegroundColor Green
    return
}

# Show the resources that will be removed with the resource group.
Write-Host ""
Write-Host "Resources that will be deleted:" -ForegroundColor Yellow
Invoke-Az -Arguments @(
    "resource", "list",
    "--resource-group", $resourceGroupName,
    "--query", "[].{Name:name, Type:type, Location:location}",
    "--output", "table"
)

# Require an explicit confirmation unless the script is intentionally forced.
if (-not $Force.IsPresent) {
    Write-Host ""
    Write-Host "This will delete the entire resource group and every resource inside it." -ForegroundColor Red
    $confirmation = Read-Host "Type '$resourceGroupName' to confirm"

    if ($confirmation -ne $resourceGroupName) {
        Write-Host "Confirmation did not match. No resources were deleted." -ForegroundColor Yellow
        return
    }
}

# Delete the resource group. This is the deprovisioning boundary for the environment.
$deleteArguments = @(
    "group", "delete",
    "--name", $resourceGroupName,
    "--yes"
)

if ($NoWait.IsPresent) {
    $deleteArguments += "--no-wait"
}

Write-Host ""
Write-Host "Deleting resource group: $resourceGroupName" -ForegroundColor Cyan
Invoke-Az -Arguments $deleteArguments

if ($NoWait.IsPresent) {
    Write-Host "Deletion started. Azure will continue deleting the resource group in the background." -ForegroundColor Green
} else {
    Write-Host "Resource group deleted." -ForegroundColor Green
}
