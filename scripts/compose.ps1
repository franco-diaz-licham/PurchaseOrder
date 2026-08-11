#!/usr/bin/env pwsh

<#
.SYNOPSIS
Stops and rebuilds the local Docker environment.

.EXAMPLE
./scripts/compose.ps1
#>

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$composeFile = Join-Path $PSScriptRoot "../docker/docker-compose.yml"

Write-Host "Stopping Docker environment..." -ForegroundColor Yellow
docker compose -f $composeFile down --remove-orphans
if ($LASTEXITCODE -ne 0) {
    exit $LASTEXITCODE
}

Write-Host "Starting Docker environment..." -ForegroundColor Cyan
docker compose -f $composeFile up -d --build
if ($LASTEXITCODE -ne 0) {
    exit $LASTEXITCODE
}

Write-Host "Docker environment is ready." -ForegroundColor Green
