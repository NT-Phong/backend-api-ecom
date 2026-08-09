using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Ecom.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSePayHostedCheckoutIpnAudit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Tbl_PaymentGatewayAttempt",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PaymentId = table.Column<Guid>(type: "uuid", nullable: false),
                    Provider = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    InvoiceNumber = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    ExpectedAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    CurrencyCode = table.Column<string>(type: "character(3)", fixedLength: true, maxLength: 3, nullable: false),
                    Status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CheckoutIssuedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    PaidAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastNotificationAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ExternalOrderId = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    ExternalTransactionId = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    ExternalTransactionReference = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    ProviderOrderStatus = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    ProviderTransactionStatus = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
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
                    table.PrimaryKey("PK_Tbl_PaymentGatewayAttempt", x => x.Id);
                    table.CheckConstraint("CK_PaymentGatewayAttempt_ExpectedAmount", "\"ExpectedAmount\" > 0");
                    table.ForeignKey(
                        name: "FK_Tbl_PaymentGatewayAttempt_Tbl_Payment_PaymentId",
                        column: x => x.PaymentId,
                        principalTable: "Tbl_Payment",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Tbl_PaymentGatewayNotification",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PaymentGatewayAttemptId = table.Column<Guid>(type: "uuid", nullable: true),
                    Provider = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    NotificationType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Disposition = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    InvoiceNumber = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    OrderAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    TransactionAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    CurrencyCode = table.Column<string>(type: "character(3)", fixedLength: true, maxLength: 3, nullable: true),
                    ExternalOrderId = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    ExternalTransactionId = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    ExternalTransactionReference = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    ProviderOrderStatus = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    ProviderTransactionStatus = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    FailureReasonCode = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ReceivedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    OccurredAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
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
                    table.PrimaryKey("PK_Tbl_PaymentGatewayNotification", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Tbl_PaymentGatewayNotification_Tbl_PaymentGatewayAttempt_Pa~",
                        column: x => x.PaymentGatewayAttemptId,
                        principalTable: "Tbl_PaymentGatewayAttempt",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PaymentGatewayAttempt_Payment_Status_ExpiresAt",
                table: "Tbl_PaymentGatewayAttempt",
                columns: new[] { "PaymentId", "Status", "ExpiresAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_PaymentGatewayAttempt_CreatedAt",
                table: "Tbl_PaymentGatewayAttempt",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_PaymentGatewayAttempt_IsDeleted",
                table: "Tbl_PaymentGatewayAttempt",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_PaymentGatewayAttempt_No",
                table: "Tbl_PaymentGatewayAttempt",
                column: "No");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_PaymentGatewayAttempt_PaymentId_Provider",
                table: "Tbl_PaymentGatewayAttempt",
                columns: new[] { "PaymentId", "Provider" },
                unique: true,
                filter: "\"IsDeleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_PaymentGatewayAttempt_Provider_ExternalTransactionId",
                table: "Tbl_PaymentGatewayAttempt",
                columns: new[] { "Provider", "ExternalTransactionId" },
                unique: true,
                filter: "\"IsDeleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_PaymentGatewayAttempt_Provider_InvoiceNumber",
                table: "Tbl_PaymentGatewayAttempt",
                columns: new[] { "Provider", "InvoiceNumber" },
                unique: true,
                filter: "\"IsDeleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentGatewayNotification_Provider_Disposition_ReceivedAt",
                table: "Tbl_PaymentGatewayNotification",
                columns: new[] { "Provider", "Disposition", "ReceivedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_PaymentGatewayNotification_CreatedAt",
                table: "Tbl_PaymentGatewayNotification",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_PaymentGatewayNotification_IsDeleted",
                table: "Tbl_PaymentGatewayNotification",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_PaymentGatewayNotification_No",
                table: "Tbl_PaymentGatewayNotification",
                column: "No");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_PaymentGatewayNotification_PaymentGatewayAttemptId",
                table: "Tbl_PaymentGatewayNotification",
                column: "PaymentGatewayAttemptId");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_PaymentGatewayNotification_Provider_NotificationType_Ex~",
                table: "Tbl_PaymentGatewayNotification",
                columns: new[] { "Provider", "NotificationType", "ExternalTransactionId" },
                unique: true,
                filter: "\"IsDeleted\" = false AND \"ExternalTransactionId\" IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Tbl_PaymentGatewayNotification");

            migrationBuilder.DropTable(
                name: "Tbl_PaymentGatewayAttempt");
        }
    }
}
