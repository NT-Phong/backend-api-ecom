using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ecom.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddOutboxProcessingLease : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_OutboxMessage_Pending",
                table: "Tbl_OutboxMessage");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "DeadLetteredAt",
                table: "Tbl_OutboxMessage",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "LeaseExpiresAt",
                table: "Tbl_OutboxMessage",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "LeaseToken",
                table: "Tbl_OutboxMessage",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_OutboxMessage_Pending",
                table: "Tbl_OutboxMessage",
                columns: new[] { "ProcessedAt", "DeadLetteredAt", "NextAttemptAt", "LeaseExpiresAt", "OccurredOn" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_OutboxMessage_Pending",
                table: "Tbl_OutboxMessage");

            migrationBuilder.DropColumn(
                name: "DeadLetteredAt",
                table: "Tbl_OutboxMessage");

            migrationBuilder.DropColumn(
                name: "LeaseExpiresAt",
                table: "Tbl_OutboxMessage");

            migrationBuilder.DropColumn(
                name: "LeaseToken",
                table: "Tbl_OutboxMessage");

            migrationBuilder.CreateIndex(
                name: "IX_OutboxMessage_Pending",
                table: "Tbl_OutboxMessage",
                columns: new[] { "ProcessedAt", "NextAttemptAt", "OccurredOn" });
        }
    }
}
