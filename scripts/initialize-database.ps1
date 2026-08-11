#!/usr/bin/env pwsh

<#
.SYNOPSIS
Runs the seed SQL script.

.EXAMPLE
./scripts/initialize-database.ps1
#>

param(
    [string]$PostgresHost = "localhost",
    [string]$PostgresPort = "55433",
    [string]$PostgresDatabase = "purchase_order",
    [string]$PostgresUser = "local-dev",
    [string]$PostgresPassword = "local-dev"
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$connection = "host=$PostgresHost port=$PostgresPort dbname=$PostgresDatabase user=$PostgresUser"
$seedScript = Join-Path $PSScriptRoot "Seeder.sql"
$env:PGPASSWORD = $PostgresPassword
$env:PGCLIENTENCODING = "UTF8"

Write-Host "Waiting for migrated schema..." -ForegroundColor Yellow
for ($attempt = 1; $attempt -le 30; $attempt++) {
    psql $connection -v ON_ERROR_STOP=1 -c "SELECT 1 FROM warehouses LIMIT 1" *> $null
    if ($LASTEXITCODE -eq 0) {
        break
    }

    Start-Sleep -Seconds 2
}

if ($LASTEXITCODE -ne 0) {
    Write-Host "Migrated schema was not found." -ForegroundColor Red
    exit $LASTEXITCODE
}

Write-Host "Running Seeder.sql..." -ForegroundColor Magenta
psql $connection -v ON_ERROR_STOP=1 -f $seedScript
if ($LASTEXITCODE -ne 0) {
    exit $LASTEXITCODE
}

Write-Host "Database initialized." -ForegroundColor Green
