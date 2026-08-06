using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ecom.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class EnforceOrderOwnershipAndFulfillmentCardinality : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DO $$
                BEGIN
                    IF EXISTS (
                        SELECT 1
                        FROM "Tbl_Order"
                        WHERE ("UserId" IS NULL) = (NULLIF("GuestTokenHashSnapshot", '') IS NULL)) THEN
                        RAISE EXCEPTION 'Cannot enforce order ownership: an order must have exactly one owner.';
                    END IF;

                    IF EXISTS (
                        SELECT "OrderId"
                        FROM "Tbl_Payment"
                        WHERE "IsDeleted" = false
                        GROUP BY "OrderId"
                        HAVING COUNT(*) > 1) THEN
                        RAISE EXCEPTION 'Cannot enforce payment cardinality: active duplicate payments exist.';
                    END IF;

                    IF EXISTS (
                        SELECT "OrderId"
                        FROM "Tbl_Shipment"
                        WHERE "IsDeleted" = false
                        GROUP BY "OrderId"
                        HAVING COUNT(*) > 1) THEN
                        RAISE EXCEPTION 'Cannot enforce shipment cardinality: active duplicate shipments exist.';
                    END IF;
                END $$;
                """);

            migrationBuilder.DropIndex(
                name: "IX_Tbl_Shipment_OrderId",
                table: "Tbl_Shipment");

            migrationBuilder.DropIndex(
                name: "IX_Tbl_Payment_OrderId",
                table: "Tbl_Payment");

            migrationBuilder.DropIndex(
                name: "IX_Tbl_Order_UserId",
                table: "Tbl_Order");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_Shipment_OrderId",
                table: "Tbl_Shipment",
                column: "OrderId",
                unique: true,
                filter: "\"IsDeleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_Payment_OrderId",
                table: "Tbl_Payment",
                column: "OrderId",
                unique: true,
                filter: "\"IsDeleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_Order_GuestTokenHashSnapshot_PlacedAt",
                table: "Tbl_Order",
                columns: new[] { "GuestTokenHashSnapshot", "PlacedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Order_UserId_PlacedAt",
                table: "Tbl_Order",
                columns: new[] { "UserId", "PlacedAt" });

            migrationBuilder.AddCheckConstraint(
                name: "CK_Order_Owner",
                table: "Tbl_Order",
                sql: "(\"UserId\" IS NULL) <> (NULLIF(\"GuestTokenHashSnapshot\", '') IS NULL)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Tbl_Shipment_OrderId",
                table: "Tbl_Shipment");

            migrationBuilder.DropIndex(
                name: "IX_Tbl_Payment_OrderId",
                table: "Tbl_Payment");

            migrationBuilder.DropIndex(
                name: "IX_Order_GuestTokenHashSnapshot_PlacedAt",
                table: "Tbl_Order");

            migrationBuilder.DropIndex(
                name: "IX_Order_UserId_PlacedAt",
                table: "Tbl_Order");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Order_Owner",
                table: "Tbl_Order");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_Shipment_OrderId",
                table: "Tbl_Shipment",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_Payment_OrderId",
                table: "Tbl_Payment",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_Order_UserId",
                table: "Tbl_Order",
                column: "UserId");
        }
    }
}
