# 📦 PurchaseOrderApp

A small internal module for reserving stock against purchase orders. I focused on the reservation rules, concurrency, audit trail, and finance view, because those are the parts where correctness matters most.

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

| Requirement                                                                   | Implementation                                                                                                                                    |
| ----------------------------------------------------------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------- |
| Operators can view approved purchase orders with outstanding stock needs      | Summary and detail pages show purchase orders, with warehouse and ready-to-reserve filters.                                                       |
| Operators can reserve stock fully or partially against a purchase order line  | Reservations are only available after approval and are checked against line remaining quantity and warehouse availability.                        |
| Available stock must be on-hand minus active reservations                     | Available quantity is calculated from warehouse on-hand stock minus active reservations.                                                          |
| Reserved stock must never exceed on-hand stock, including concurrent requests | Reservations lock the matching `warehouse_stock` rows before availability is recalculated.                                                        |
| Operators can release reservations fully or partially                         | Reservation management supports partial release and updates the purchase order line reserved and remaining quantities.                            |
| Every successful reservation and release must be audited                      | Reservation and release domain events create append-only audit entries.                                                                           |
| Finance can view committed reserved stock value per warehouse                 | The finance report uses the unit cost captured on each reservation, not the item's current cost.                                                  |
| Unit and weight tracked items must behave differently                         | Unit items require whole quantities. Weight items support up to 3 decimal places.                                                                 |
| The app should be runnable and reviewable locally                             | Docker Compose starts the database, API, frontend, and seeder with realistic warehouses, stock, purchase orders, reservations, and audit history. |

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

I enforce this with pessimistic row-level locking in PostgreSQL:

1. Start a transaction.
2. Lock the relevant `warehouse_stock` row with `SELECT ... FOR UPDATE`.
3. Recalculate active reserved quantity while the lock is held.
4. Reject the reservation if the requested quantity exceeds available stock.
5. Persist the reservation, update the purchase order line, write audit entries through domain events, and commit.

I chose pessimistic locking because available stock is calculated from active reservations, not from a stored `ReservedQuantity` column on `WarehouseStock`. Locking the matching `warehouse_stock` rows means only one reservation attempt can calculate and commit availability for that stock item at a time. The next request waits, then reads the latest committed state. The main advantages over optimistic concurrency for this workflow are:

- Less wasted work. Requests wait before doing business logic against stock availability that is likely to become stale.
- Fewer retries. The system avoids repeated read, attempt, conflict, reload cycles.
- More predictable behaviour. Access becomes effectively linear: one transaction completes, then the next sees the latest committed state.

This fits this project because contention is narrow. The contested resource is the matching `warehouse_stock` rows for one warehouse/item combination, not the whole purchase order process. The lock is also held for a short period of time: load the rows, recalculate active reservations, validate availability, persist the reservation changes, and commit. An optimistic approach could work, but it would need extra retry logic. A request that loses the race would have to reload and try again, and another competing request could still beat it during the retry.

<a id="assumptions"></a>

## 📝 Assumptions

- Authentication and authorization are out of scope. The app uses a trusted user value, currently `Franco Diaz`, for audit metadata.
- Warehouses, inventory items, and warehouse stock are seeded reference/admin data. In a real system these would likely be maintained through admin screens or an upstream inventory system.
- Purchase orders start as pending, with lines added before approval. Reservations are only allowed after approval because the spec says operators reserve against approved purchase order lines.
- Backend filtering and pagination are out of scope. The current frontend uses simple client-side filtering so the workflow is visible without adding more API complexity.
- Over-requested reservations are rejected instead of silently partially fulfilled. That keeps the operator in control of the quantity being reserved.
- Failed reservation or release attempts are not written to the business audit log. The audit log records successful business actions, not validation failures.
- Audit entries are not editable or deletable through the application.
- Weight-tracked items support up to 3 decimal places. Unit-tracked items require whole-number quantities.
- Purchase order line totals use the current inventory item cost when the PO is read. Reservation records are where historical cost is captured, because finance needs the value at reservation time.
- Finance reporting only includes active reservations, because released reservations are no longer committed stock.

<a id="self-review"></a>

## 🔍 Self Review

What I am confident in:

- The reservation workflow is protected by the database transaction and row lock, not only by an application-layer check.
- Integration tests exercise the locking behavior against a real PostgreSQL database, including competing requests for the same `warehouse_stock` rows.
- The domain model owns the important mutations: purchase order line reservation totals, reservation release behavior, and audit domain events.
- Audit log entries are created from domain events and saved in the same transaction as the reservation/release.
- Finance reporting uses the reservation-time cost snapshot, and tests cover the standard cost change scenario.
- The code is intentionally boring: explicit services, repositories, DTOs, models, and mapper functions rather than clever abstractions. I wanted the implementation to be easy to explain in a walkthrough.

What I would flag in review:

- The frontend has no authentication or real user context because the exercise excludes it.
- Purchase order filtering is client-side. In a production version I would move this to the API and scope it by the operator's warehouse.
- The finance report is useful for the exercise, but a real version would need date filters, exports, and reconciliation views.
- The audit log is protected by the application/persistence layer. With more time I would harden this at the database level by giving the app user only `SELECT` and `INSERT` permissions on the audit table, and optionally adding a trigger that rejects `UPDATE` and `DELETE`.
- The UI is functional and clean, but not a full production operations console.

Riskiest part:

The riskiest part is reservation lifecycle correctness. A reservation affects available stock, purchase order line totals, finance value, and audit history. These changes need to stay ACID compliant so the system never commits only part of the workflow. The concurrency case is the dangerous one. That is why I used PostgreSQL row-level locking, explicit transactions, and real database tests. Reservation attempts for the same `warehouse_stock` rows are now linear, so the second request recalculates availability after the first transaction commits.

<a id="ai-usage-note"></a>

## 🤖 AI Usage Note

I used AI tooling mainly as a planning and review assistant. I used it to compare options, draft repetitive boilerplate, and refactor parts of the frontend/backend structure. I reviewed and adjusted the output throughout, especially around domain boundaries, concurrency, audit events, EF Core mapping, and keeping the code explainable.

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
