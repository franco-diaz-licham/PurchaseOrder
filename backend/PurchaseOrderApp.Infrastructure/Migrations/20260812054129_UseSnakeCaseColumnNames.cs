using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PurchaseOrderApp.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UseSnakeCaseColumnNames : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_purchase_order_lines_inventory_items_InventoryItemId",
                table: "purchase_order_lines");

            migrationBuilder.DropForeignKey(
                name: "FK_purchase_order_lines_purchase_orders_PurchaseOrderId",
                table: "purchase_order_lines");

            migrationBuilder.DropForeignKey(
                name: "FK_purchase_orders_warehouses_WarehouseId",
                table: "purchase_orders");

            migrationBuilder.DropForeignKey(
                name: "FK_stock_reservations_inventory_items_InventoryItemId",
                table: "stock_reservations");

            migrationBuilder.DropForeignKey(
                name: "FK_stock_reservations_purchase_order_lines_PurchaseOrderLineId",
                table: "stock_reservations");

            migrationBuilder.DropForeignKey(
                name: "FK_stock_reservations_warehouses_WarehouseId",
                table: "stock_reservations");

            migrationBuilder.DropForeignKey(
                name: "FK_warehouse_stock_inventory_items_InventoryItemId",
                table: "warehouse_stock");

            migrationBuilder.DropForeignKey(
                name: "FK_warehouse_stock_warehouses_WarehouseId",
                table: "warehouse_stock");

            migrationBuilder.DropPrimaryKey(
                name: "PK_warehouses",
                table: "warehouses");

            migrationBuilder.DropPrimaryKey(
                name: "PK_warehouse_stock",
                table: "warehouse_stock");

            migrationBuilder.DropPrimaryKey(
                name: "PK_stock_reservations",
                table: "stock_reservations");

            migrationBuilder.DropPrimaryKey(
                name: "PK_purchase_orders",
                table: "purchase_orders");

            migrationBuilder.DropPrimaryKey(
                name: "PK_purchase_order_lines",
                table: "purchase_order_lines");

            migrationBuilder.DropPrimaryKey(
                name: "PK_inventory_items",
                table: "inventory_items");

            migrationBuilder.DropPrimaryKey(
                name: "PK_audit_log_entries",
                table: "audit_log_entries");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "warehouses",
                newName: "name");

            migrationBuilder.RenameColumn(
                name: "Code",
                table: "warehouses",
                newName: "code");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "warehouses",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "UpdatedBy",
                table: "warehouses",
                newName: "updated_by");

            migrationBuilder.RenameColumn(
                name: "UpdatedAt",
                table: "warehouses",
                newName: "updated_at");

            migrationBuilder.RenameColumn(
                name: "CreatedBy",
                table: "warehouses",
                newName: "created_by");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "warehouses",
                newName: "created_at");

            migrationBuilder.RenameIndex(
                name: "IX_warehouses_Code",
                table: "warehouses",
                newName: "ix_warehouses_code");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "warehouse_stock",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "WarehouseId",
                table: "warehouse_stock",
                newName: "warehouse_id");

            migrationBuilder.RenameColumn(
                name: "UpdatedBy",
                table: "warehouse_stock",
                newName: "updated_by");

            migrationBuilder.RenameColumn(
                name: "UpdatedAt",
                table: "warehouse_stock",
                newName: "updated_at");

            migrationBuilder.RenameColumn(
                name: "OnHandQuantity",
                table: "warehouse_stock",
                newName: "on_hand_quantity");

            migrationBuilder.RenameColumn(
                name: "InventoryItemId",
                table: "warehouse_stock",
                newName: "inventory_item_id");

            migrationBuilder.RenameColumn(
                name: "CreatedBy",
                table: "warehouse_stock",
                newName: "created_by");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "warehouse_stock",
                newName: "created_at");

            migrationBuilder.RenameIndex(
                name: "IX_warehouse_stock_WarehouseId_InventoryItemId",
                table: "warehouse_stock",
                newName: "ix_warehouse_stock_warehouse_id_inventory_item_id");

            migrationBuilder.RenameIndex(
                name: "IX_warehouse_stock_InventoryItemId",
                table: "warehouse_stock",
                newName: "ix_warehouse_stock_inventory_item_id");

            migrationBuilder.RenameColumn(
                name: "Status",
                table: "stock_reservations",
                newName: "status");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "stock_reservations",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "WarehouseId",
                table: "stock_reservations",
                newName: "warehouse_id");

            migrationBuilder.RenameColumn(
                name: "UpdatedBy",
                table: "stock_reservations",
                newName: "updated_by");

            migrationBuilder.RenameColumn(
                name: "UpdatedAt",
                table: "stock_reservations",
                newName: "updated_at");

            migrationBuilder.RenameColumn(
                name: "UnitCostSnapshot",
                table: "stock_reservations",
                newName: "unit_cost_snapshot");

            migrationBuilder.RenameColumn(
                name: "QuantityReserved",
                table: "stock_reservations",
                newName: "quantity_reserved");

            migrationBuilder.RenameColumn(
                name: "PurchaseOrderLineId",
                table: "stock_reservations",
                newName: "purchase_order_line_id");

            migrationBuilder.RenameColumn(
                name: "InventoryItemId",
                table: "stock_reservations",
                newName: "inventory_item_id");

            migrationBuilder.RenameColumn(
                name: "CreatedBy",
                table: "stock_reservations",
                newName: "created_by");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "stock_reservations",
                newName: "created_at");

            migrationBuilder.RenameIndex(
                name: "IX_stock_reservations_WarehouseId_InventoryItemId_Status",
                table: "stock_reservations",
                newName: "ix_stock_reservations_warehouse_id_inventory_item_id_status");

            migrationBuilder.RenameIndex(
                name: "IX_stock_reservations_PurchaseOrderLineId",
                table: "stock_reservations",
                newName: "ix_stock_reservations_purchase_order_line_id");

            migrationBuilder.RenameIndex(
                name: "IX_stock_reservations_InventoryItemId",
                table: "stock_reservations",
                newName: "ix_stock_reservations_inventory_item_id");

            migrationBuilder.RenameColumn(
                name: "Status",
                table: "purchase_orders",
                newName: "status");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "purchase_orders",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "WarehouseId",
                table: "purchase_orders",
                newName: "warehouse_id");

            migrationBuilder.RenameColumn(
                name: "UpdatedBy",
                table: "purchase_orders",
                newName: "updated_by");

            migrationBuilder.RenameColumn(
                name: "UpdatedAt",
                table: "purchase_orders",
                newName: "updated_at");

            migrationBuilder.RenameColumn(
                name: "PurchaseOrderNumber",
                table: "purchase_orders",
                newName: "purchase_order_number");

            migrationBuilder.RenameColumn(
                name: "CreatedBy",
                table: "purchase_orders",
                newName: "created_by");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "purchase_orders",
                newName: "created_at");

            migrationBuilder.RenameIndex(
                name: "IX_purchase_orders_WarehouseId",
                table: "purchase_orders",
                newName: "ix_purchase_orders_warehouse_id");

            migrationBuilder.RenameIndex(
                name: "IX_purchase_orders_PurchaseOrderNumber",
                table: "purchase_orders",
                newName: "ix_purchase_orders_purchase_order_number");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "purchase_order_lines",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "UpdatedBy",
                table: "purchase_order_lines",
                newName: "updated_by");

            migrationBuilder.RenameColumn(
                name: "UpdatedAt",
                table: "purchase_order_lines",
                newName: "updated_at");

            migrationBuilder.RenameColumn(
                name: "QuantityReserved",
                table: "purchase_order_lines",
                newName: "quantity_reserved");

            migrationBuilder.RenameColumn(
                name: "QuantityOrdered",
                table: "purchase_order_lines",
                newName: "quantity_ordered");

            migrationBuilder.RenameColumn(
                name: "PurchaseOrderId",
                table: "purchase_order_lines",
                newName: "purchase_order_id");

            migrationBuilder.RenameColumn(
                name: "InventoryItemId",
                table: "purchase_order_lines",
                newName: "inventory_item_id");

            migrationBuilder.RenameColumn(
                name: "CreatedBy",
                table: "purchase_order_lines",
                newName: "created_by");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "purchase_order_lines",
                newName: "created_at");

            migrationBuilder.RenameIndex(
                name: "IX_purchase_order_lines_PurchaseOrderId",
                table: "purchase_order_lines",
                newName: "ix_purchase_order_lines_purchase_order_id");

            migrationBuilder.RenameIndex(
                name: "IX_purchase_order_lines_InventoryItemId",
                table: "purchase_order_lines",
                newName: "ix_purchase_order_lines_inventory_item_id");

            migrationBuilder.RenameColumn(
                name: "Sku",
                table: "inventory_items",
                newName: "sku");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "inventory_items",
                newName: "name");

            migrationBuilder.RenameColumn(
                name: "Category",
                table: "inventory_items",
                newName: "category");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "inventory_items",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "UpdatedBy",
                table: "inventory_items",
                newName: "updated_by");

            migrationBuilder.RenameColumn(
                name: "UpdatedAt",
                table: "inventory_items",
                newName: "updated_at");

            migrationBuilder.RenameColumn(
                name: "TrackingMode",
                table: "inventory_items",
                newName: "tracking_mode");

            migrationBuilder.RenameColumn(
                name: "StandardCost",
                table: "inventory_items",
                newName: "standard_cost");

            migrationBuilder.RenameColumn(
                name: "CreatedBy",
                table: "inventory_items",
                newName: "created_by");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "inventory_items",
                newName: "created_at");

            migrationBuilder.RenameIndex(
                name: "IX_inventory_items_Sku",
                table: "inventory_items",
                newName: "ix_inventory_items_sku");

            migrationBuilder.RenameColumn(
                name: "Quantity",
                table: "audit_log_entries",
                newName: "quantity");

            migrationBuilder.RenameColumn(
                name: "Action",
                table: "audit_log_entries",
                newName: "action");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "audit_log_entries",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "WarehouseId",
                table: "audit_log_entries",
                newName: "warehouse_id");

            migrationBuilder.RenameColumn(
                name: "UpdatedBy",
                table: "audit_log_entries",
                newName: "updated_by");

            migrationBuilder.RenameColumn(
                name: "UpdatedAt",
                table: "audit_log_entries",
                newName: "updated_at");

            migrationBuilder.RenameColumn(
                name: "StockReservationId",
                table: "audit_log_entries",
                newName: "stock_reservation_id");

            migrationBuilder.RenameColumn(
                name: "ResultingAvailableQuantity",
                table: "audit_log_entries",
                newName: "resulting_available_quantity");

            migrationBuilder.RenameColumn(
                name: "PurchaseOrderLineId",
                table: "audit_log_entries",
                newName: "purchase_order_line_id");

            migrationBuilder.RenameColumn(
                name: "InventoryItemId",
                table: "audit_log_entries",
                newName: "inventory_item_id");

            migrationBuilder.RenameColumn(
                name: "CreatedBy",
                table: "audit_log_entries",
                newName: "created_by");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "audit_log_entries",
                newName: "created_at");

            migrationBuilder.AddPrimaryKey(
                name: "pk_warehouses",
                table: "warehouses",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_warehouse_stock",
                table: "warehouse_stock",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_stock_reservations",
                table: "stock_reservations",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_purchase_orders",
                table: "purchase_orders",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_purchase_order_lines",
                table: "purchase_order_lines",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_inventory_items",
                table: "inventory_items",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_audit_log_entries",
                table: "audit_log_entries",
                column: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_purchase_order_lines_inventory_items_inventory_item_id",
                table: "purchase_order_lines",
                column: "inventory_item_id",
                principalTable: "inventory_items",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_purchase_order_lines_purchase_orders_purchase_order_id",
                table: "purchase_order_lines",
                column: "purchase_order_id",
                principalTable: "purchase_orders",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_purchase_orders_warehouses_warehouse_id",
                table: "purchase_orders",
                column: "warehouse_id",
                principalTable: "warehouses",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_stock_reservations_inventory_items_inventory_item_id",
                table: "stock_reservations",
                column: "inventory_item_id",
                principalTable: "inventory_items",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_stock_reservations_purchase_order_lines_purchase_order_line",
                table: "stock_reservations",
                column: "purchase_order_line_id",
                principalTable: "purchase_order_lines",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_stock_reservations_warehouses_warehouse_id",
                table: "stock_reservations",
                column: "warehouse_id",
                principalTable: "warehouses",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_warehouse_stock_inventory_items_inventory_item_id",
                table: "warehouse_stock",
                column: "inventory_item_id",
                principalTable: "inventory_items",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_warehouse_stock_warehouses_warehouse_id",
                table: "warehouse_stock",
                column: "warehouse_id",
                principalTable: "warehouses",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_purchase_order_lines_inventory_items_inventory_item_id",
                table: "purchase_order_lines");

            migrationBuilder.DropForeignKey(
                name: "fk_purchase_order_lines_purchase_orders_purchase_order_id",
                table: "purchase_order_lines");

            migrationBuilder.DropForeignKey(
                name: "fk_purchase_orders_warehouses_warehouse_id",
                table: "purchase_orders");

            migrationBuilder.DropForeignKey(
                name: "fk_stock_reservations_inventory_items_inventory_item_id",
                table: "stock_reservations");

            migrationBuilder.DropForeignKey(
                name: "fk_stock_reservations_purchase_order_lines_purchase_order_line",
                table: "stock_reservations");

            migrationBuilder.DropForeignKey(
                name: "fk_stock_reservations_warehouses_warehouse_id",
                table: "stock_reservations");

            migrationBuilder.DropForeignKey(
                name: "fk_warehouse_stock_inventory_items_inventory_item_id",
                table: "warehouse_stock");

            migrationBuilder.DropForeignKey(
                name: "fk_warehouse_stock_warehouses_warehouse_id",
                table: "warehouse_stock");

            migrationBuilder.DropPrimaryKey(
                name: "pk_warehouses",
                table: "warehouses");

            migrationBuilder.DropPrimaryKey(
                name: "pk_warehouse_stock",
                table: "warehouse_stock");

            migrationBuilder.DropPrimaryKey(
                name: "pk_stock_reservations",
                table: "stock_reservations");

            migrationBuilder.DropPrimaryKey(
                name: "pk_purchase_orders",
                table: "purchase_orders");

            migrationBuilder.DropPrimaryKey(
                name: "pk_purchase_order_lines",
                table: "purchase_order_lines");

            migrationBuilder.DropPrimaryKey(
                name: "pk_inventory_items",
                table: "inventory_items");

            migrationBuilder.DropPrimaryKey(
                name: "pk_audit_log_entries",
                table: "audit_log_entries");

            migrationBuilder.RenameColumn(
                name: "name",
                table: "warehouses",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "code",
                table: "warehouses",
                newName: "Code");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "warehouses",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "updated_by",
                table: "warehouses",
                newName: "UpdatedBy");

            migrationBuilder.RenameColumn(
                name: "updated_at",
                table: "warehouses",
                newName: "UpdatedAt");

            migrationBuilder.RenameColumn(
                name: "created_by",
                table: "warehouses",
                newName: "CreatedBy");

            migrationBuilder.RenameColumn(
                name: "created_at",
                table: "warehouses",
                newName: "CreatedAt");

            migrationBuilder.RenameIndex(
                name: "ix_warehouses_code",
                table: "warehouses",
                newName: "IX_warehouses_Code");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "warehouse_stock",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "warehouse_id",
                table: "warehouse_stock",
                newName: "WarehouseId");

            migrationBuilder.RenameColumn(
                name: "updated_by",
                table: "warehouse_stock",
                newName: "UpdatedBy");

            migrationBuilder.RenameColumn(
                name: "updated_at",
                table: "warehouse_stock",
                newName: "UpdatedAt");

            migrationBuilder.RenameColumn(
                name: "on_hand_quantity",
                table: "warehouse_stock",
                newName: "OnHandQuantity");

            migrationBuilder.RenameColumn(
                name: "inventory_item_id",
                table: "warehouse_stock",
                newName: "InventoryItemId");

            migrationBuilder.RenameColumn(
                name: "created_by",
                table: "warehouse_stock",
                newName: "CreatedBy");

            migrationBuilder.RenameColumn(
                name: "created_at",
                table: "warehouse_stock",
                newName: "CreatedAt");

            migrationBuilder.RenameIndex(
                name: "ix_warehouse_stock_warehouse_id_inventory_item_id",
                table: "warehouse_stock",
                newName: "IX_warehouse_stock_WarehouseId_InventoryItemId");

            migrationBuilder.RenameIndex(
                name: "ix_warehouse_stock_inventory_item_id",
                table: "warehouse_stock",
                newName: "IX_warehouse_stock_InventoryItemId");

            migrationBuilder.RenameColumn(
                name: "status",
                table: "stock_reservations",
                newName: "Status");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "stock_reservations",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "warehouse_id",
                table: "stock_reservations",
                newName: "WarehouseId");

            migrationBuilder.RenameColumn(
                name: "updated_by",
                table: "stock_reservations",
                newName: "UpdatedBy");

            migrationBuilder.RenameColumn(
                name: "updated_at",
                table: "stock_reservations",
                newName: "UpdatedAt");

            migrationBuilder.RenameColumn(
                name: "unit_cost_snapshot",
                table: "stock_reservations",
                newName: "UnitCostSnapshot");

            migrationBuilder.RenameColumn(
                name: "quantity_reserved",
                table: "stock_reservations",
                newName: "QuantityReserved");

            migrationBuilder.RenameColumn(
                name: "purchase_order_line_id",
                table: "stock_reservations",
                newName: "PurchaseOrderLineId");

            migrationBuilder.RenameColumn(
                name: "inventory_item_id",
                table: "stock_reservations",
                newName: "InventoryItemId");

            migrationBuilder.RenameColumn(
                name: "created_by",
                table: "stock_reservations",
                newName: "CreatedBy");

            migrationBuilder.RenameColumn(
                name: "created_at",
                table: "stock_reservations",
                newName: "CreatedAt");

            migrationBuilder.RenameIndex(
                name: "ix_stock_reservations_warehouse_id_inventory_item_id_status",
                table: "stock_reservations",
                newName: "IX_stock_reservations_WarehouseId_InventoryItemId_Status");

            migrationBuilder.RenameIndex(
                name: "ix_stock_reservations_purchase_order_line_id",
                table: "stock_reservations",
                newName: "IX_stock_reservations_PurchaseOrderLineId");

            migrationBuilder.RenameIndex(
                name: "ix_stock_reservations_inventory_item_id",
                table: "stock_reservations",
                newName: "IX_stock_reservations_InventoryItemId");

            migrationBuilder.RenameColumn(
                name: "status",
                table: "purchase_orders",
                newName: "Status");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "purchase_orders",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "warehouse_id",
                table: "purchase_orders",
                newName: "WarehouseId");

            migrationBuilder.RenameColumn(
                name: "updated_by",
                table: "purchase_orders",
                newName: "UpdatedBy");

            migrationBuilder.RenameColumn(
                name: "updated_at",
                table: "purchase_orders",
                newName: "UpdatedAt");

            migrationBuilder.RenameColumn(
                name: "purchase_order_number",
                table: "purchase_orders",
                newName: "PurchaseOrderNumber");

            migrationBuilder.RenameColumn(
                name: "created_by",
                table: "purchase_orders",
                newName: "CreatedBy");

            migrationBuilder.RenameColumn(
                name: "created_at",
                table: "purchase_orders",
                newName: "CreatedAt");

            migrationBuilder.RenameIndex(
                name: "ix_purchase_orders_warehouse_id",
                table: "purchase_orders",
                newName: "IX_purchase_orders_WarehouseId");

            migrationBuilder.RenameIndex(
                name: "ix_purchase_orders_purchase_order_number",
                table: "purchase_orders",
                newName: "IX_purchase_orders_PurchaseOrderNumber");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "purchase_order_lines",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "updated_by",
                table: "purchase_order_lines",
                newName: "UpdatedBy");

            migrationBuilder.RenameColumn(
                name: "updated_at",
                table: "purchase_order_lines",
                newName: "UpdatedAt");

            migrationBuilder.RenameColumn(
                name: "quantity_reserved",
                table: "purchase_order_lines",
                newName: "QuantityReserved");

            migrationBuilder.RenameColumn(
                name: "quantity_ordered",
                table: "purchase_order_lines",
                newName: "QuantityOrdered");

            migrationBuilder.RenameColumn(
                name: "purchase_order_id",
                table: "purchase_order_lines",
                newName: "PurchaseOrderId");

            migrationBuilder.RenameColumn(
                name: "inventory_item_id",
                table: "purchase_order_lines",
                newName: "InventoryItemId");

            migrationBuilder.RenameColumn(
                name: "created_by",
                table: "purchase_order_lines",
                newName: "CreatedBy");

            migrationBuilder.RenameColumn(
                name: "created_at",
                table: "purchase_order_lines",
                newName: "CreatedAt");

            migrationBuilder.RenameIndex(
                name: "ix_purchase_order_lines_purchase_order_id",
                table: "purchase_order_lines",
                newName: "IX_purchase_order_lines_PurchaseOrderId");

            migrationBuilder.RenameIndex(
                name: "ix_purchase_order_lines_inventory_item_id",
                table: "purchase_order_lines",
                newName: "IX_purchase_order_lines_InventoryItemId");

            migrationBuilder.RenameColumn(
                name: "sku",
                table: "inventory_items",
                newName: "Sku");

            migrationBuilder.RenameColumn(
                name: "name",
                table: "inventory_items",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "category",
                table: "inventory_items",
                newName: "Category");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "inventory_items",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "updated_by",
                table: "inventory_items",
                newName: "UpdatedBy");

            migrationBuilder.RenameColumn(
                name: "updated_at",
                table: "inventory_items",
                newName: "UpdatedAt");

            migrationBuilder.RenameColumn(
                name: "tracking_mode",
                table: "inventory_items",
                newName: "TrackingMode");

            migrationBuilder.RenameColumn(
                name: "standard_cost",
                table: "inventory_items",
                newName: "StandardCost");

            migrationBuilder.RenameColumn(
                name: "created_by",
                table: "inventory_items",
                newName: "CreatedBy");

            migrationBuilder.RenameColumn(
                name: "created_at",
                table: "inventory_items",
                newName: "CreatedAt");

            migrationBuilder.RenameIndex(
                name: "ix_inventory_items_sku",
                table: "inventory_items",
                newName: "IX_inventory_items_Sku");

            migrationBuilder.RenameColumn(
                name: "quantity",
                table: "audit_log_entries",
                newName: "Quantity");

            migrationBuilder.RenameColumn(
                name: "action",
                table: "audit_log_entries",
                newName: "Action");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "audit_log_entries",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "warehouse_id",
                table: "audit_log_entries",
                newName: "WarehouseId");

            migrationBuilder.RenameColumn(
                name: "updated_by",
                table: "audit_log_entries",
                newName: "UpdatedBy");

            migrationBuilder.RenameColumn(
                name: "updated_at",
                table: "audit_log_entries",
                newName: "UpdatedAt");

            migrationBuilder.RenameColumn(
                name: "stock_reservation_id",
                table: "audit_log_entries",
                newName: "StockReservationId");

            migrationBuilder.RenameColumn(
                name: "resulting_available_quantity",
                table: "audit_log_entries",
                newName: "ResultingAvailableQuantity");

            migrationBuilder.RenameColumn(
                name: "purchase_order_line_id",
                table: "audit_log_entries",
                newName: "PurchaseOrderLineId");

            migrationBuilder.RenameColumn(
                name: "inventory_item_id",
                table: "audit_log_entries",
                newName: "InventoryItemId");

            migrationBuilder.RenameColumn(
                name: "created_by",
                table: "audit_log_entries",
                newName: "CreatedBy");

            migrationBuilder.RenameColumn(
                name: "created_at",
                table: "audit_log_entries",
                newName: "CreatedAt");

            migrationBuilder.AddPrimaryKey(
                name: "PK_warehouses",
                table: "warehouses",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_warehouse_stock",
                table: "warehouse_stock",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_stock_reservations",
                table: "stock_reservations",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_purchase_orders",
                table: "purchase_orders",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_purchase_order_lines",
                table: "purchase_order_lines",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_inventory_items",
                table: "inventory_items",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_audit_log_entries",
                table: "audit_log_entries",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_purchase_order_lines_inventory_items_InventoryItemId",
                table: "purchase_order_lines",
                column: "InventoryItemId",
                principalTable: "inventory_items",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_purchase_order_lines_purchase_orders_PurchaseOrderId",
                table: "purchase_order_lines",
                column: "PurchaseOrderId",
                principalTable: "purchase_orders",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_purchase_orders_warehouses_WarehouseId",
                table: "purchase_orders",
                column: "WarehouseId",
                principalTable: "warehouses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_stock_reservations_inventory_items_InventoryItemId",
                table: "stock_reservations",
                column: "InventoryItemId",
                principalTable: "inventory_items",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_stock_reservations_purchase_order_lines_PurchaseOrderLineId",
                table: "stock_reservations",
                column: "PurchaseOrderLineId",
                principalTable: "purchase_order_lines",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_stock_reservations_warehouses_WarehouseId",
                table: "stock_reservations",
                column: "WarehouseId",
                principalTable: "warehouses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_warehouse_stock_inventory_items_InventoryItemId",
                table: "warehouse_stock",
                column: "InventoryItemId",
                principalTable: "inventory_items",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_warehouse_stock_warehouses_WarehouseId",
                table: "warehouse_stock",
                column: "WarehouseId",
                principalTable: "warehouses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
