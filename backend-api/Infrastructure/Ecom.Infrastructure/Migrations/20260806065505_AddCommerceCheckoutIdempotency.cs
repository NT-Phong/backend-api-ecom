using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Ecom.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCommerceCheckoutIdempotency : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Tbl_IdempotencyRecord",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Operation = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    OwnerScope = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    KeyHash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    RequestFingerprint = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    OrderId = table.Column<Guid>(type: "uuid", nullable: true),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    No = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ConcurrencyStamp = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tbl_IdempotencyRecord", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_IdempotencyRecord_CreatedAt",
                table: "Tbl_IdempotencyRecord",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_IdempotencyRecord_ExpiresAt",
                table: "Tbl_IdempotencyRecord",
                column: "ExpiresAt");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_IdempotencyRecord_IsDeleted",
                table: "Tbl_IdempotencyRecord",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_IdempotencyRecord_No",
                table: "Tbl_IdempotencyRecord",
                column: "No");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_IdempotencyRecord_Operation_OwnerScope_KeyHash",
                table: "Tbl_IdempotencyRecord",
                columns: new[] { "Operation", "OwnerScope", "KeyHash" },
                unique: true,
                filter: "\"IsDeleted\" = false");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Tbl_IdempotencyRecord");
        }
    }
}
