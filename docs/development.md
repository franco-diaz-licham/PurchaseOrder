# Development Notes

This document captures implementation decisions, assumptions, and tradeoffs made while building the Purchase Order Stock Reservation module. The final README can summarize these notes once the solution is complete.

## Assumptions

- Authentication and authorization are out of scope. The API will accept a trusted user identifier/name for commands that need audit metadata.
- Warehouses are reference data, not compile-time enum values. They will be modeled as entities and seeded for local/demo use.
- Inventory item categories are stable enough for this exercise and will be modeled as an enum.
- Purchase orders belong to a single warehouse. Warehouse operators view approved purchase orders for that warehouse.
- Reservations are made against a specific purchase order line.
- Releases are made against a specific stock reservation, not directly against a purchase order line.
- Over-requested reservations are rejected rather than silently partially fulfilled.
- Failed reservation or release attempts are not written to the business audit log.
- Audit log entries are append-only business records and will not be edited or deleted through application workflows.
- Quantity values use `decimal`, not floating-point types.
- Weight-tracked items support up to 3 decimal places.
- Unit-tracked items must use whole-number quantities.
- Available stock is derived from on-hand stock minus active reservations.
- Released reservations are retained as historical records instead of being deleted.
- Committed stock value uses the reservation-time cost snapshot, not the current inventory item cost.

## Domain Decisions

- The domain is implemented in an EF Core-friendly style rather than a purely isolated DDD model.
- Entities have private parameterless constructors for EF hydration.
- Entity state uses private setters and is mutated through aggregate methods.
- Strongly typed IDs are used to avoid primitive obsession while keeping persistence mapping straightforward.
- Lifecycle metadata such as `CreatedAt`, `CreatedBy`, `UpdatedAt`, and `UpdatedBy` lives on the base entity.
- `PurchaseOrder` owns `PurchaseOrderLine` because line reservation totals are part of the purchase order consistency boundary.
- `StockReservation` is its own entity because it has its own lifecycle, cost snapshot, and release behavior.
- `WarehouseStock` owns on-hand quantity for a warehouse/item pair, but active reserved quantity remains derived from reservations.
- Audit entries are immutable records created by successful reservation and release workflows.

## Concurrency Decision

The critical invariant is that active reservations for a warehouse/item must never exceed on-hand stock.

The intended implementation is to enforce this in the database-backed application workflow, not by a simple application-level check. The chosen approach is pessimistic row-level locking:

1. Start a transaction.
2. Lock the relevant warehouse stock row.
3. Calculate active reserved quantity for the same warehouse and item inside the transaction.
4. Reject the command if the requested reservation exceeds available stock.
5. Persist the reservation, update the purchase order line, append the audit entry, and commit atomically.

For PostgreSQL, this can be implemented with a targeted `SELECT ... FOR UPDATE` lock on the warehouse stock row.

Optimistic concurrency was considered but not chosen for the first implementation. It would require every reservation attempt to update a concurrency-controlled stock row, which usually means adding a denormalized value such as `ReservedQuantity` or touching a version column only to detect conflicts. That adds retry handling and extra state that must stay consistent with `StockReservation` records.

Pessimistic locking is easier to explain and test for this module: competing reservations for the same warehouse/item are serialized at the database row level, availability is recalculated while the lock is held, and only then is the reservation written.

## Things To Revisit With More Time

- Add a richer warehouse administration model if warehouses need CRUD, addresses, regions, or operator assignment.
- Add a category reference table if inventory categories need user management, hierarchy, or reporting metadata.
- Consider a stock ledger if the broader system needs full inventory movement history beyond reservation/release audit entries.
- Add an outbox pattern if reservation events need to be published to other services.
- Add stronger audit protection at the database level, such as permissions or triggers preventing update/delete on audit records.
- Add more realistic user context once authentication and authorization are introduced.
- Add broader integration tests around transaction isolation and PostgreSQL locking behavior.
- Add performance-focused read models if finance reporting grows beyond simple warehouse-level summaries.
