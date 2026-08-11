BEGIN;

DELETE FROM audit_log_entries;
DELETE FROM stock_reservations;
DELETE FROM purchase_order_lines;
DELETE FROM purchase_orders;
DELETE FROM warehouse_stock;
DELETE FROM inventory_items;
DELETE FROM warehouses;

INSERT INTO warehouses ("Id", "Code", "Name", "CreatedAt", "CreatedBy", "UpdatedAt", "UpdatedBy")
VALUES
    ('11111111-1111-1111-1111-111111111111', 'A', 'Warehouse A', '2026-01-01T00:00:00+00:00', 'seed', NULL, NULL),
    ('22222222-2222-2222-2222-222222222222', 'B', 'Warehouse B', '2026-01-01T00:00:00+00:00', 'seed', NULL, NULL);

INSERT INTO inventory_items ("Id", "Sku", "Name", "Category", "TrackingMode", "StandardCost", "CreatedAt", "CreatedBy", "UpdatedAt", "UpdatedBy")
VALUES
    ('aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa1', 'BOLT-10', '10mm Bolt', 'Hardware', 'Unit', 2.5000, '2026-01-01T00:00:00+00:00', 'seed', NULL, NULL),
    ('aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa2', 'RICE-BULK', 'Bulk Rice', 'BulkGoods', 'Weight', 1.7500, '2026-01-01T00:00:00+00:00', 'seed', NULL, NULL),
    ('aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa3', 'CABLE-STD', 'Standard Cable', 'General', 'Unit', 8.2500, '2026-01-01T00:00:00+00:00', 'seed', NULL, NULL);

INSERT INTO warehouse_stock ("Id", "WarehouseId", "InventoryItemId", "OnHandQuantity", "CreatedAt", "CreatedBy", "UpdatedAt", "UpdatedBy")
VALUES
    ('bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb01', '11111111-1111-1111-1111-111111111111', 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa1', 100.000, '2026-01-01T00:00:00+00:00', 'seed', NULL, NULL),
    ('bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb02', '11111111-1111-1111-1111-111111111111', 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa2', 100.000, '2026-01-01T00:00:00+00:00', 'seed', NULL, NULL),
    ('bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb03', '22222222-2222-2222-2222-222222222222', 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa3', 75.000, '2026-01-01T00:00:00+00:00', 'seed', NULL, NULL);

INSERT INTO purchase_orders ("Id", "PurchaseOrderNumber", "WarehouseId", "Status", "CreatedAt", "CreatedBy", "UpdatedAt", "UpdatedBy")
VALUES
    ('cccccccc-cccc-cccc-cccc-cccccccccc01', 'PO-1001', '11111111-1111-1111-1111-111111111111', 'Approved', '2026-01-01T00:00:00+00:00', 'seed', NULL, NULL),
    ('cccccccc-cccc-cccc-cccc-cccccccccc02', 'PO-1002', '22222222-2222-2222-2222-222222222222', 'Approved', '2026-01-01T00:00:00+00:00', 'seed', NULL, NULL);

INSERT INTO purchase_order_lines ("Id", "PurchaseOrderId", "InventoryItemId", "QuantityOrdered", "QuantityReserved", "CreatedAt", "CreatedBy", "UpdatedAt", "UpdatedBy")
VALUES
    ('dddddddd-dddd-dddd-dddd-dddddddddd01', 'cccccccc-cccc-cccc-cccc-cccccccccc01', 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa1', 100.000, 0.000, '2026-01-01T00:00:00+00:00', 'seed', NULL, NULL),
    ('dddddddd-dddd-dddd-dddd-dddddddddd02', 'cccccccc-cccc-cccc-cccc-cccccccccc01', 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa2', 25.000, 0.000, '2026-01-01T00:00:00+00:00', 'seed', NULL, NULL),
    ('dddddddd-dddd-dddd-dddd-dddddddddd03', 'cccccccc-cccc-cccc-cccc-cccccccccc02', 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa3', 40.000, 0.000, '2026-01-01T00:00:00+00:00', 'seed', NULL, NULL);

COMMIT;
