#!/usr/bin/env pwsh

<#
.SYNOPSIS
Provisions the PurchaseOrder Azure infrastructure with Bicep.

.EXAMPLE
./scripts/provision-infra.ps1

.EXAMPLE
./scripts/provision-infra.ps1 -Subscription "00000000-0000-0000-0000-000000000000" -SkipWhatIf

.EXAMPLE
$password = Read-Host "PostgreSQL admin password" -AsSecureString
./scripts/provision-infra.ps1 -PostgresAdminPassword $password
#>

[CmdletBinding()]
param(
    [string]$Subscription,
    [string]$EnvFile = (Join-Path $PSScriptRoot "../infra/.env"),
    [securestring]$PostgresAdminPassword,
    [switch]$SkipWhatIf,
    [switch]$WhatIfOnly
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

Import-Module (Join-Path $PSScriptRoot "ProvisionInfra.psm1") -Force

if (-not (Get-Command az -ErrorAction SilentlyContinue)) {
    throw "Azure CLI was not found. Install it from https://learn.microsoft.com/cli/azure/install-azure-cli, then run this script again."
}

$repoRoot = (Resolve-Path (Join-Path $PSScriptRoot "..")).Path
$envFilePath = if ([IO.Path]::IsPathRooted($EnvFile)) { $EnvFile } else { Join-Path $repoRoot $EnvFile }
Import-DotEnv -Path $envFilePath

$subscriptionName = if ([string]::IsNullOrWhiteSpace($Subscription)) { Get-Config -Name "AZURE_SUBSCRIPTION" } else { $Subscription }
$resourceGroupName = Get-Config -Name "AZURE_RESOURCE_GROUP_NAME"
$resourceGroupLocation = Get-Config -Name "AZURE_RESOURCE_GROUP_LOCATION"
$deploymentName = Get-Config -Name "AZURE_DEPLOYMENT_NAME"
$templatePath = Resolve-RepoPath -Path (Get-Config -Name "AZURE_TEMPLATE_FILE") -RepoRoot $repoRoot
$parametersPath = Resolve-RepoPath -Path (Get-Config -Name "AZURE_PARAMETERS_FILE") -RepoRoot $repoRoot
$skipWhatIfEnabled = $SkipWhatIf.IsPresent -or (Get-BoolConfig -Name "AZURE_SKIP_WHAT_IF")
$whatIfOnlyEnabled = $WhatIfOnly.IsPresent -or (Get-BoolConfig -Name "AZURE_WHAT_IF_ONLY")

if ($skipWhatIfEnabled -and $whatIfOnlyEnabled) {
    throw "Use either -SkipWhatIf or -WhatIfOnly, not both."
}

$account = $null
try {
    $account = Invoke-AzJson -Arguments @("account", "show", "--output", "json")
} catch {
    Write-Host "Azure CLI is not logged in. Starting az login..." -ForegroundColor Yellow
    Invoke-Az -Arguments @("login", "--output", "none")
    $account = Invoke-AzJson -Arguments @("account", "show", "--output", "json")
}

if (-not [string]::IsNullOrWhiteSpace($subscriptionName)) {
    Write-Host "Using Azure subscription: $subscriptionName" -ForegroundColor Cyan
    Invoke-Az -Arguments @("account", "set", "--subscription", $subscriptionName)
    $account = Invoke-AzJson -Arguments @("account", "show", "--output", "json")
}

Write-Host "Active subscription: $($account.name) [$($account.id)]" -ForegroundColor Cyan

if ($null -eq $PostgresAdminPassword) {
    $passwordFromEnv = Get-Config -Name "POSTGRES_ADMIN_PASSWORD"
    $PostgresAdminPassword = ConvertTo-SecureString $passwordFromEnv -AsPlainText -Force
}

$postgresPasswordPlain = ConvertFrom-SecureStringToPlainText $PostgresAdminPassword
if ([string]::IsNullOrWhiteSpace($postgresPasswordPlain)) {
    throw "PostgreSQL admin password is required."
}

$temporaryParametersFile = Join-Path ([IO.Path]::GetTempPath()) "purchase-order.$([guid]::NewGuid()).parameters.json"

try {
    $parameters = Get-Content $parametersPath -Raw | ConvertFrom-Json -AsHashtable
    $parameters.parameters["postgresAdminPassword"] = @{
        value = $postgresPasswordPlain
    }

    $parameters |
        ConvertTo-Json -Depth 20 |
        Set-Content -Path $temporaryParametersFile -Encoding utf8NoBOM

    Write-Host "Ensuring resource group exists: $resourceGroupName" -ForegroundColor Cyan
    Invoke-Az -Arguments @(
        "group", "create",
        "--name", $resourceGroupName,
        "--location", $resourceGroupLocation,
        "--output", "none"
    )

    if (-not $skipWhatIfEnabled) {
        Write-Host "Running deployment preview..." -ForegroundColor Cyan
        Invoke-Az -Arguments @(
            "deployment", "group", "what-if",
            "--name", $deploymentName,
            "--resource-group", $resourceGroupName,
            "--template-file", $templatePath,
            "--parameters", "@$temporaryParametersFile"
        )
    }

    if ($whatIfOnlyEnabled) {
        Write-Host "What-if complete. No resources were provisioned." -ForegroundColor Green
        return
    }

    Write-Host "Provisioning Azure resources..." -ForegroundColor Cyan
    $deployment = Invoke-AzJson -Arguments @(
        "deployment", "group", "create",
        "--name", $deploymentName,
        "--resource-group", $resourceGroupName,
        "--template-file", $templatePath,
        "--parameters", "@$temporaryParametersFile",
        "--output", "json"
    )

    Write-Host "Provisioning complete." -ForegroundColor Green
    Write-Host ""
    Write-Host "GitHub repository secrets to set:" -ForegroundColor Cyan
    $deployment.properties.outputs.githubSecrets.value | Format-List
} finally {
    if (Test-Path $temporaryParametersFile) {
        Remove-Item -Force $temporaryParametersFile
    }
}
