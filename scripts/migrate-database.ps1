#!/usr/bin/env pwsh

<#
.SYNOPSIS
Applies EF Core migrations to the PurchaseOrder database.

.EXAMPLE
./scripts/migrate-database.ps1
#>

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$apiProject = Join-Path $PSScriptRoot "../backend/PurchaseOrderApp.Api/PurchaseOrderApp.Api.csproj"
$infrastructureProject = Join-Path $PSScriptRoot "../backend/PurchaseOrderApp.Infrastructure/PurchaseOrderApp.Infrastructure.csproj"
$env:Database__PurchaseOrderDb = "Host=localhost;Port=56433;Database=purchase_order;Username=local-dev;Password=local-dev"

dotnet ef database update --project $infrastructureProject --startup-project $apiProject
if ($LASTEXITCODE -ne 0) {
    exit $LASTEXITCODE
}

Write-Host "Database migration complete." -ForegroundColor Green
