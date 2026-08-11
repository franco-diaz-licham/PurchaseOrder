# Purchase Order System

Internal purchase order stock reservation module built with .NET 8, ASP.NET Core Web API, EF Core 8, and PostgreSQL.

## Current Backend Surface

- Purchase order lifecycle: list, get, submit, add line, approve, close, and cancel.
- Reservation workflow: list, get, reserve, and release.
- Audit log: list permanent reservation/release audit entries.
- Finance report: committed stock value per warehouse using reservation-time standard cost snapshots.
- Inventory item standard cost update: included only to demonstrate that finance reporting keeps using the reservation-time cost snapshot after the current standard cost changes.

## Assumptions

- Warehouse, inventory item, and warehouse stock records are seeded setup data for this exercise.
- The API does not expose full warehouse or inventory CRUD because the take-home focuses on purchase order reservation behavior, concurrency safety, audit logging, and finance reporting.
- Authentication and authorization are out of scope. Requests pass a trusted user value for lifecycle and audit metadata.
- Cancelling a purchase order is used instead of hard delete so reservation and audit history remain intact.

More detailed implementation notes are in `docs/development.md`.

## Local Docker Setup

Start PostgreSQL, run API migrations, seed the database, and start the API:

```powershell
./scripts/compose.ps1
```

The API runs EF Core migrations during startup. The seeder container runs automatically during compose startup and then runs `scripts/Seeder.sql`.

To apply EF Core migrations manually:

```powershell
./scripts/migrate-database.ps1
```

To reseed manually:

```powershell
./scripts/initialize-database.ps1
```

The API runs at `http://localhost:5180` and Swagger is available at `http://localhost:5180/swagger`.

The Docker database is available from the host at:

```text
Host=localhost;Port=55433;Database=purchase_order;Username=local-dev;Password=local-dev
```

The seed script uses Docker Compose, so it does not require PostgreSQL client tools on your machine.
