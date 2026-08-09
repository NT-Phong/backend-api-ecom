using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Ecom.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSePayBankVietQr : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Tbl_PaymentBankQrAttempt",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PaymentId = table.Column<Guid>(type: "uuid", nullable: false),
                    Provider = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    PaymentCode = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ExpectedAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    CurrencyCode = table.Column<string>(type: "character(3)", fixedLength: true, maxLength: 3, nullable: false),
                    VirtualAccountFingerprint = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    QrIssuedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    PaidAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastNotificationAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ExternalTransactionId = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    ExternalTransactionReference = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
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
                    table.PrimaryKey("PK_Tbl_PaymentBankQrAttempt", x => x.Id);
                    table.CheckConstraint("CK_PaymentBankQrAttempt_ExpectedAmount", "\"ExpectedAmount\" > 0");
                    table.ForeignKey(
                        name: "FK_Tbl_PaymentBankQrAttempt_Tbl_Payment_PaymentId",
                        column: x => x.PaymentId,
                        principalTable: "Tbl_Payment",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Tbl_PaymentBankQrWebhookNotification",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PaymentBankQrAttemptId = table.Column<Guid>(type: "uuid", nullable: true),
                    Provider = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    NotificationType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Disposition = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    PaymentCode = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    TransactionAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    CurrencyCode = table.Column<string>(type: "character(3)", fixedLength: true, maxLength: 3, nullable: true),
                    ExternalTransactionId = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    ExternalTransactionReference = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
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
                    table.PrimaryKey("PK_Tbl_PaymentBankQrWebhookNotification", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Tbl_PaymentBankQrWebhookNotification_Tbl_PaymentBankQrAttem~",
                        column: x => x.PaymentBankQrAttemptId,
                        principalTable: "Tbl_PaymentBankQrAttempt",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PaymentBankQrAttempt_Payment_Status_ExpiresAt",
                table: "Tbl_PaymentBankQrAttempt",
                columns: new[] { "PaymentId", "Status", "ExpiresAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_PaymentBankQrAttempt_CreatedAt",
                table: "Tbl_PaymentBankQrAttempt",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_PaymentBankQrAttempt_IsDeleted",
                table: "Tbl_PaymentBankQrAttempt",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_PaymentBankQrAttempt_No",
                table: "Tbl_PaymentBankQrAttempt",
                column: "No");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_PaymentBankQrAttempt_PaymentId_Provider",
                table: "Tbl_PaymentBankQrAttempt",
                columns: new[] { "PaymentId", "Provider" },
                unique: true,
                filter: "\"IsDeleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_PaymentBankQrAttempt_Provider_ExternalTransactionId",
                table: "Tbl_PaymentBankQrAttempt",
                columns: new[] { "Provider", "ExternalTransactionId" },
                unique: true,
                filter: "\"IsDeleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_PaymentBankQrAttempt_Provider_PaymentCode",
                table: "Tbl_PaymentBankQrAttempt",
                columns: new[] { "Provider", "PaymentCode" },
                unique: true,
                filter: "\"IsDeleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentBankQrNotification_Provider_Disposition_ReceivedAt",
                table: "Tbl_PaymentBankQrWebhookNotification",
                columns: new[] { "Provider", "Disposition", "ReceivedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_PaymentBankQrWebhookNotification_CreatedAt",
                table: "Tbl_PaymentBankQrWebhookNotification",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_PaymentBankQrWebhookNotification_IsDeleted",
                table: "Tbl_PaymentBankQrWebhookNotification",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_PaymentBankQrWebhookNotification_No",
                table: "Tbl_PaymentBankQrWebhookNotification",
                column: "No");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_PaymentBankQrWebhookNotification_PaymentBankQrAttemptId",
                table: "Tbl_PaymentBankQrWebhookNotification",
                column: "PaymentBankQrAttemptId");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_PaymentBankQrWebhookNotification_Provider_NotificationT~",
                table: "Tbl_PaymentBankQrWebhookNotification",
                columns: new[] { "Provider", "NotificationType", "ExternalTransactionId" },
                unique: true,
                filter: "\"IsDeleted\" = false AND \"ExternalTransactionId\" IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Tbl_PaymentBankQrWebhookNotification");

            migrationBuilder.DropTable(
                name: "Tbl_PaymentBankQrAttempt");
        }
    }
}
