# 📦 PurchaseOrderApp

Internal purchase order stock reservation module focused on safe reservations, releases, audit history, and finance committed-value reporting.

## 🧭 Table of Contents

- [✨ Requirements](#requirements)
- [🧰 Tech Stack](#tech-stack)
- [🚀 Running Locally](#running-locally)
- [⚙️ Running Without Docker](#running-without-docker)
- [🧪 Tests](#tests)
- [🔒 Concurrency Approach](#concurrency-approach)
- [📝 Assumptions](#assumptions)
- [🔍 Self Review](#self-review)
- [🤖 AI Usage Note](#ai-usage-note)
- [⏭️ What I Would Do With More Time](#what-i-would-do-with-more-time)

<a id="requirements"></a>

## ✨ Requirements

| Requirement                                                                   | Implementation                                                                                                                                                      |
| ----------------------------------------------------------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| Operators can view approved purchase orders with outstanding stock needs      | Summary and detail views support warehouse and ready-to-reserve filtering.                                                                                          |
| Operators can reserve stock fully or partially against a purchase order line  | Reservation workflow is available after approval and validates against both line remaining quantity and warehouse availability.                                     |
| Available stock must be on-hand minus active reservations                     | Warehouse availability is calculated from `WarehouseStock` on-hand quantity minus active `StockReservation` records.                                                |
| Reserved stock must never exceed on-hand stock, including concurrent requests | Reservation uses PostgreSQL pessimistic row-level locking on the warehouse/item stock row inside an explicit transaction.                                           |
| Operators can release reservations fully or partially                         | Reservation management supports partial release and updates the purchase order line reserved/remaining quantities.                                                  |
| Every successful reservation and release must be audited                      | Domain events create append-only audit log entries with timestamp, user, item, warehouse, quantity, and resulting available balance.                                |
| Finance can view committed reserved stock value per warehouse                 | Finance report summarizes active reservation value by warehouse using the unit cost captured when each reservation was created.                                     |
| Unit and weight tracked items must behave differently                         | Unit-tracked items require whole quantities; weight-tracked items support decimal quantities up to 3 decimal places.                                                |
| The app should be runnable and reviewable locally                             | Docker Compose starts PostgreSQL, API, frontend, and automatic database seeding with realistic warehouses, stock, purchase orders, reservations, and audit entries. |

<a id="tech-stack"></a>

## 🧰 Tech Stack

![.NET](https://img.shields.io/badge/.NET-8-512BD4?logo=dotnet&logoColor=white)
![React](https://img.shields.io/badge/React-19-61DAFB?logo=react&logoColor=111111)
![TypeScript](https://img.shields.io/badge/TypeScript-5-3178C6?logo=typescript&logoColor=white)
![PostgreSQL](https://img.shields.io/badge/PostgreSQL-17-4169E1?logo=postgresql&logoColor=white)
![Docker](https://img.shields.io/badge/Docker-Compose-2496ED?logo=docker&logoColor=white)
![Tailwind CSS](https://img.shields.io/badge/Tailwind_CSS-4-06B6D4?logo=tailwindcss&logoColor=white)

- Backend: .NET 8, EF Core, PostgreSQL
- Frontend: React, TypeScript
- Tests: NUnit, Testcontainers
- Local setup: Docker Compose

<a id="running-locally"></a>

## 🚀 Running Locally

Start PostgreSQL, the API, the frontend, and the automatic database seeder:

```powershell
./scripts/compose.ps1
```

The compose setup exposes:

- Frontend: `http://localhost:5173`
- API: `http://localhost:5180`
- Swagger: `http://localhost:5180/swagger`
- PostgreSQL from host: `localhost:56433`

Database connection from the host:

```text
Host=localhost;Port=56433;Database=purchase_order;Username=local-dev;Password=local-dev
```

The API runs EF Core migrations on startup. The `db-seeder` container waits for the API-migrated schema and then runs `scripts/Seeder.sql`.

Manual database commands:

```powershell
./scripts/migrate-database.ps1
./scripts/initialize-database.ps1
```

<a id="running-without-docker"></a>

## ⚙️ Running Without Docker

Backend:

```powershell
dotnet build backend/PurchaseOrderApp.Api/PurchaseOrderApp.Api.csproj
dotnet run --project backend/PurchaseOrderApp.Api/PurchaseOrderApp.Api.csproj
```

Frontend:

```powershell
npm --prefix frontend install
npm --prefix frontend run dev
```

The local API expects PostgreSQL at `localhost:56433`, which is the host port exposed by Docker Compose.

<a id="tests"></a>

## 🧪 Tests

Run all backend tests:

```powershell
dotnet test backend/PurchaseOrderApp.Tests/PurchaseOrderApp.Tests.csproj
```

Run frontend checks:

```powershell
npm --prefix frontend run build
npm --prefix frontend run test
```

The most important tests are the PostgreSQL-backed reservation and row-locking tests. They verify that competing requests cannot over-reserve stock for the same warehouse/item.

<a id="concurrency-approach"></a>

## 🔒 Concurrency Approach

The critical invariant is:

```text
Active reserved quantity for a warehouse/item must never exceed on-hand quantity.
```

This is enforced with pessimistic row-level locking in PostgreSQL:

1. Start a transaction.
2. Lock the relevant `warehouse_stock` row with `SELECT ... FOR UPDATE`.
3. Recalculate active reserved quantity while the lock is held.
4. Reject the reservation if the requested quantity exceeds available stock.
5. Persist the reservation, update the purchase order line, write audit entries through domain events, and commit.

I chose pessimistic locking because available stock is calculated from active reservations, not from a stored `ReservedQuantity` column on `WarehouseStock`. Locking the warehouse/item stock row ensures only one reservation attempt can calculate and commit availability for that stock item at a time. The advantage over optimistic concurrency is that competing requests wait before doing business logic against stale stock availability. This avoids retry loops and makes the behaviour predictable: one transaction completes, then the next sees the latest committed state. This is a good fit because the contention is narrow. The lock is only taken on the single warehouse/item stock row and is held briefly while availability is recalculated and the reservation is saved.

<a id="assumptions"></a>

## 📝 Assumptions

- Authentication and authorization are out of scope. The app uses a trusted user value, currently `Franco Diaz`, for audit metadata.
- Warehouses, inventory items, and warehouse stock are seeded reference/admin data for this exercise.
- Operators create purchase orders as drafts, add line items, and then approve the purchase order before reservations are allowed.
- Backend filtering, pagination, and operator-specific warehouse assignment are intentionally deferred. The current frontend performs simple client-side filtering.
- Over-requested reservations are rejected rather than silently partially fulfilled.
- Failed reservation and release attempts are not written to the business audit log.
- Audit entries are permanent records and are not editable or deletable through the application.
- Weight-tracked items support up to 3 decimal places. Unit-tracked items require whole-number quantities.
- Purchase order line unit costs and totals are calculated from the current inventory item standard cost when the PO is read. They are not snapshot values; reservation records are the point where unit cost is captured historically.
- Finance reports only include active reservations because released reservations are no longer committed stock.

<a id="self-review"></a>

## 🔍 Self Review

What I am confident in:

- The reservation workflow is protected at the database level, not only by an application-layer check.
- Integration tests exercise the row-level locking behavior against a real PostgreSQL database.
- The domain model owns the important mutations: purchase order line reservation totals, reservation release behavior, and audit domain events.
- Audit log entries are created from domain events and saved in the same transaction as the reservation/release.
- Finance reporting uses the reservation-time cost snapshot, and tests cover the standard cost change scenario.
- The code is intentionally boring: explicit services, repositories, DTOs, models, and mapper functions rather than clever abstractions.

What I would flag in review:

- The frontend has no authentication or real user context because the exercise excludes it.
- Purchase order list filtering is client-side. A production system should add backend filtering, pagination, and authorization-aware warehouse scoping.
- The finance report needs more work, a real system would likely need date filters, export support, and reconciliation views.
- The audit log is protected by the application/persistence layer. With more time I would also harden this at the database level by giving the app user only `SELECT` and `INSERT` permissions on the audit table, and optionally adding a trigger that rejects `UPDATE` and `DELETE`.
- The UI is functional and clean, but not a full production operations console.

Riskiest part:

The riskiest part is reservation lifecycle correctness. A reservation affects available stock, purchase order line reserved/remaining quantities, finance value, and audit history. These changes need to remain ACID compliant so the system never commits only part of the workflow. The concurrency issue is the most dangerous version of that risk. That is why the solution uses PostgreSQL row-level locking, explicit transactions, and real database tests. The implementation deliberately serializes reservation attempts for the same warehouse/item row so the second request recalculates availability after the first transaction commits.

<a id="ai-usage-note"></a>

## 🤖 AI Usage Note

I used AI tooling mainly as a planning and review assistant: to explore implementation options, compare tradeoffs, and check whether the solution still matched the project requirements. I also used it to help with repetitive boilerplate and refactoring, but I reviewed and adjusted the output throughout, especially around domain boundaries, concurrency, audit events, EF Core mapping, and keeping the code explainable for an interview.

<a id="what-i-would-do-with-more-time"></a>

## ⏭️ What I Would Do With More Time

- Add backend pagination and server-side filters for purchase order, audit, and finance screens.
- Add a real user context and warehouse assignment model.
- Add database-level audit immutability protections.
- Add an outbox if reservation/release events need to be published to other systems through a message broker.
- Optimize TanStack Query mutations with targeted cache updates, optimistic updates where useful, and smaller refetch payloads.
- Add browser-level end-to-end tests for the main operator workflows.
- Add richer finance reporting, such as date filters, export support, and reconciliation views.
- Add server-sent events for live stock availability updates, so operators see changes from competing reservations without manually refreshing the page.
