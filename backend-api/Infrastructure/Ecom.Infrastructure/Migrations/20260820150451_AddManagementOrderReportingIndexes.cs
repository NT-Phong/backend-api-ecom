using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ecom.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddManagementOrderReportingIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_PaymentTransaction_Type_Status_OccurredAt_PaymentId_Active",
                table: "Tbl_PaymentTransaction",
                columns: new[] { "TransactionType", "Status", "OccurredAt", "PaymentId" },
                filter: "\"IsDeleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_Payment_Status_OrderId_Active",
                table: "Tbl_Payment",
                columns: new[] { "Status", "OrderId" },
                filter: "\"IsDeleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_OrderStatusHistory_ToStatus_ChangedAt_OrderId_Active",
                table: "Tbl_OrderStatusHistory",
                columns: new[] { "ToStatus", "ChangedAt", "OrderId" },
                filter: "\"IsDeleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_Order_PlacedAt_Active",
                table: "Tbl_Order",
                column: "PlacedAt",
                filter: "\"IsDeleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_Order_Status_PlacedAt_Active",
                table: "Tbl_Order",
                columns: new[] { "Status", "PlacedAt" },
                filter: "\"IsDeleted\" = false");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PaymentTransaction_Type_Status_OccurredAt_PaymentId_Active",
                table: "Tbl_PaymentTransaction");

            migrationBuilder.DropIndex(
                name: "IX_Payment_Status_OrderId_Active",
                table: "Tbl_Payment");

            migrationBuilder.DropIndex(
                name: "IX_OrderStatusHistory_ToStatus_ChangedAt_OrderId_Active",
                table: "Tbl_OrderStatusHistory");

            migrationBuilder.DropIndex(
                name: "IX_Order_PlacedAt_Active",
                table: "Tbl_Order");

            migrationBuilder.DropIndex(
                name: "IX_Order_Status_PlacedAt_Active",
                table: "Tbl_Order");
        }
    }
}
