# 📦 PurchaseOrderApp

![.NET](https://img.shields.io/badge/.NET-8-512BD4?logo=dotnet&logoColor=white)
![React](https://img.shields.io/badge/React-19-61DAFB?logo=react&logoColor=111111)
![TypeScript](https://img.shields.io/badge/TypeScript-6-3178C6?logo=typescript&logoColor=white)
![Vite](https://img.shields.io/badge/Vite-8-646CFF?logo=vite&logoColor=white)
![PostgreSQL](https://img.shields.io/badge/PostgreSQL-17-4169E1?logo=postgresql&logoColor=white)
![Docker](https://img.shields.io/badge/Docker-Compose-2496ED?logo=docker&logoColor=white)
![Tailwind CSS](https://img.shields.io/badge/Tailwind_CSS-4-06B6D4?logo=tailwindcss&logoColor=white)
![Azure](https://img.shields.io/badge/Azure-Bicep-0078D4?logo=microsoftazure&logoColor=white)

PurchaseOrderApp is a full-stack inventory reservation system for purchase order operations. It helps operators review purchase orders, reserve warehouse stock against approved purchase order lines, release reservations, inspect audit history, and view committed stock value for finance reporting.

The backend is built around the reservation lifecycle: purchase order line quantities, warehouse availability, reservation records, audit entries, and finance values are updated together so the system does not commit half of a stock operation.

## 🧭 System Design

PurchaseOrderApp is split into a React frontend, an ASP.NET Core API, and a PostgreSQL database. Docker Compose runs the local environment with the API, frontend, database, and a database seeder. Azure infrastructure is defined with Bicep and provisioned through a PowerShell script.

The primary backend implementation uses Clean Architecture. A separate layered architecture rewrite is kept under `backend/layered` for architecture study and comparison, but the Docker setup, workflows, and production-shaped paths continue to use `backend/clean`.

## ✨ Features

- Purchase order summary and detail views
- Purchase order approval, closure, and cancellation
- Purchase order line creation and removal
- Full and partial stock reservations against approved purchase order lines
- Full and partial reservation release
- Warehouse stock availability checks
- PostgreSQL row-level locking for concurrent reservation safety
- Reservation and release audit history
- Warehouse committed-stock finance report
- Seeded local data for warehouses, stock, purchase orders, reservations, and audit entries
- Azure Bicep infrastructure for low-cost cloud deployment

## 🧰 Technology Stack

### ⚙️ Backend

- .NET 8
- ASP.NET Core Web API
- Entity Framework Core
- PostgreSQL with Npgsql
- Clean Architecture project split
- Domain-driven design style entities and value objects
- Application use-case services
- Repository and unit-of-work style persistence boundaries
- Domain events for audit entry creation
- Swagger / OpenAPI
- NUnit and Testcontainers

### 🖥️ Frontend

- React 19
- Vite 8
- TypeScript 6
- Tailwind CSS 4
- TanStack Query
- React Router
- Zustand
- React Hook Form
- Zod
- Base UI
- Vitest and React Testing Library

### 🐳 Local Infrastructure

- Docker Compose
- PostgreSQL 17
- API container
- Frontend container
- Database seeder container
- PowerShell helper scripts

### ☁️ Azure Infrastructure

- Azure Container Apps for the API
- Azure Static Web Apps for the frontend
- Azure Container Registry for the API image
- Azure Database for PostgreSQL Flexible Server
- Azure Key Vault for secrets
- Log Analytics for Container Apps logs
- User-assigned managed identity for ACR pull access
- Bicep modules for repeatable provisioning

## 🏗️ Project Architecture

### Backend

```text
backend/
├── clean/       # Main Clean Architecture backend
├── layered/     # Layered architecture rewrite for study and comparison
└── PurchaseOrderApp.slnx
```

### Clean Architecture Backend

```text
backend/clean/
├── PurchaseOrderApp.Api/             # Controllers, API models, startup, Swagger, HTTP concerns
├── PurchaseOrderApp.Application/     # Use cases, ports, DTOs, transaction coordination
├── PurchaseOrderApp.Domain/          # Entities, value objects, enums, domain events, business rules
├── PurchaseOrderApp.Infrastructure/  # EF Core DbContext, configurations, migrations, repositories
└── PurchaseOrderApp.Tests/           # Domain, API, and PostgreSQL-backed integration tests
```

### Layered Backend

```text
backend/layered/
├── PurchaseOrderApp.Services/  # Controllers, validation filters, API models, startup
├── PurchaseOrderApp.BL/        # Commands, queries, handlers, policies, ports, business models
└── PurchaseOrderApp.DAL/       # EF Core DbContext, migrations, repositories, infrastructure implementations
```

### Frontend

```text
frontend/
├── src/          # App source, feature modules, shared UI, services, and state
├── public/       # Static public assets
├── package.json
└── vite.config.ts
```

### Docker

```text
docker/
├── api.dockerfile
├── db-seeder.dockerfile
├── docker-compose.yml
└── frontend.dockerfile
```

### Infrastructure

```text
infra/
├── main.bicep
├── main.parameters.prod.json
└── modules/
```

## 🔁 Reservation Workflow

```text
Operator
  -> selects an approved purchase order line
  -> chooses a warehouse and reservation quantity
  -> PurchaseOrderApp.Api reservation endpoint
  -> Application reservation use case
  -> PostgreSQL transaction
  -> lock matching warehouse_stock row
  -> recalculate active reserved quantity
  -> create stock reservation
  -> update purchase order line reserved quantity
  -> append audit entry
  -> commit transaction
```

The critical invariant is:

```text
Active reserved quantity for a warehouse/item must never exceed on-hand quantity.
```

The reservation flow uses pessimistic row-level locking so competing requests for the same warehouse/item combination are processed one at a time. The next request waits, reads the latest committed reservation state, and only succeeds if stock is still available.

## 🚀 Getting Started

### Prerequisites

- .NET 8 SDK
- Node.js 20+
- Docker Desktop
- PowerShell 7+
- PostgreSQL client tools

## 🐳 Run With Docker

From the repository root:

```powershell
./scripts/compose.ps1
```

This stops the current compose environment, rebuilds images, and starts the local stack using:

```text
docker/docker-compose.yml
```

Main local services:

```text
Frontend:    http://localhost:5173
API:         http://localhost:5180
Swagger:     http://localhost:5180/swagger
PostgreSQL:  localhost:56433
```

Docker database credentials:

```text
Database: purchase_order
Username: local-dev
Password: local-dev
```

The API applies EF Core migrations on startup. The `db-seeder` container waits for the schema and then runs `scripts/Seeder.sql`.

## ⚙️ Backend Setup

Restore and build the backend solution:

```powershell
dotnet restore backend/PurchaseOrderApp.slnx
dotnet build backend/PurchaseOrderApp.slnx
```

Run the API locally:

```powershell
dotnet run --project backend/clean/PurchaseOrderApp.Api/PurchaseOrderApp.Api.csproj
```

Apply migrations manually:

```powershell
./scripts/migrate-database.ps1
```

Seed local data manually:

```powershell
./scripts/initialize-database.ps1
```

The local API expects PostgreSQL at `localhost:56433`, which is the host port exposed by Docker Compose.

## 🖥️ Frontend Setup

From the frontend directory:

```powershell
cd frontend
npm install
npm run dev
```

Useful commands:

```powershell
npm run build
npm run lint
npm run test
npm run preview
```

## 🧪 Tests

Run backend tests:

```powershell
dotnet test backend/clean/PurchaseOrderApp.Tests/PurchaseOrderApp.Tests.csproj
```

Run frontend checks from the frontend directory:

```powershell
npm run build
npm run test
```

The most important tests are the PostgreSQL-backed reservation tests because they verify that competing requests cannot over-reserve stock for the same warehouse/item.

## ☁️ Azure Deployment

PurchaseOrderApp can be provisioned to Azure with Bicep. The infrastructure lives under `infra/`, and deployment is wrapped by `scripts/provision-infra.ps1`.

Review `infra/.env` and set the required values:

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

Provision the resources:

```powershell
./scripts/provision-infra.ps1
```

Preview the deployment without provisioning:

```powershell
./scripts/provision-infra.ps1 -WhatIfOnly
```

See `infra/README.md` for the full Azure deployment notes and manual Azure CLI commands.
