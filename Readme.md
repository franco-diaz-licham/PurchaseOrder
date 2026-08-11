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
