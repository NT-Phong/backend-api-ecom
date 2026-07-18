using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Ecom.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddIdentitySessionFoundation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "PhoneNumber",
                table: "Tbl_User",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(20)",
                oldMaxLength: 20);

            migrationBuilder.AddColumn<DateTime>(
                name: "EmailVerifiedAt",
                table: "Tbl_User",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NormalizedEmail",
                table: "Tbl_User",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NormalizedPhoneNumber",
                table: "Tbl_User",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "PhoneVerifiedAt",
                table: "Tbl_User",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SecurityStamp",
                table: "Tbl_User",
                type: "character varying(64)",
                maxLength: 64,
                nullable: false,
                defaultValue: "");

            // Expand/backfill only. Legacy raw indexes and tables intentionally remain during compatibility.
            migrationBuilder.Sql("""
                UPDATE "Tbl_User"
                SET "NormalizedEmail" = CASE WHEN "Email" IS NULL THEN NULL ELSE UPPER(BTRIM("Email")) END,
                    "NormalizedPhoneNumber" = CASE WHEN "PhoneNumber" IS NULL THEN NULL ELSE REGEXP_REPLACE("PhoneNumber", '[^0-9]', '', 'g') END,
                    "SecurityStamp" = REPLACE(gen_random_uuid()::text, '-', ''),
                    "EmailVerifiedAt" = CASE WHEN "EmailConfirmed" THEN COALESCE("UpdatedAt", "CreatedAt", NOW()) ELSE NULL END,
                    "PhoneVerifiedAt" = CASE WHEN "PhoneNumberConfirmed" THEN COALESCE("UpdatedAt", "CreatedAt", NOW()) ELSE NULL END
                WHERE "SecurityStamp" = '' OR "NormalizedEmail" IS NULL OR "NormalizedPhoneNumber" IS NULL;
                """);

            migrationBuilder.CreateTable(
                name: "Tbl_UserSession",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    ClientType = table.Column<int>(type: "integer", nullable: false),
                    AuthenticationMethod = table.Column<int>(type: "integer", nullable: false),
                    AuthenticationStrength = table.Column<int>(type: "integer", nullable: false),
                    SecurityStamp = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    DeviceId = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    LastSeenAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IdleExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    AbsoluteExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    RevokedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RevocationReason = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
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
                    table.PrimaryKey("PK_Tbl_UserSession", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Tbl_UserSession_Tbl_User_UserId",
                        column: x => x.UserId,
                        principalTable: "Tbl_User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Tbl_VerificationChallenge",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: true),
                    Purpose = table.Column<int>(type: "integer", nullable: false),
                    DestinationHash = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    SecretHash = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    FailedAttempts = table.Column<int>(type: "integer", nullable: false),
                    MaxAttempts = table.Column<int>(type: "integer", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ConsumedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    SupersededAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedByIpHash = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
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
                    table.PrimaryKey("PK_Tbl_VerificationChallenge", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Tbl_VerificationChallenge_Tbl_User_UserId",
                        column: x => x.UserId,
                        principalTable: "Tbl_User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Tbl_SecurityEvent",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: true),
                    SessionId = table.Column<Guid>(type: "uuid", nullable: true),
                    EventType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    RiskLevel = table.Column<int>(type: "integer", nullable: false),
                    Success = table.Column<bool>(type: "boolean", nullable: false),
                    IpFingerprint = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    UserAgentSummary = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    Metadata = table.Column<string>(type: "jsonb", nullable: true),
                    OccurredAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
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
                    table.PrimaryKey("PK_Tbl_SecurityEvent", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Tbl_SecurityEvent_Tbl_UserSession_SessionId",
                        column: x => x.SessionId,
                        principalTable: "Tbl_UserSession",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Tbl_SecurityEvent_Tbl_User_UserId",
                        column: x => x.UserId,
                        principalTable: "Tbl_User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Tbl_SessionRefreshToken",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SessionId = table.Column<Guid>(type: "uuid", nullable: false),
                    FamilyId = table.Column<Guid>(type: "uuid", nullable: false),
                    TokenHash = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UsedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RevokedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ReplacedByTokenId = table.Column<Guid>(type: "uuid", nullable: true),
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
                    table.PrimaryKey("PK_Tbl_SessionRefreshToken", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Tbl_SessionRefreshToken_Tbl_SessionRefreshToken_ReplacedByT~",
                        column: x => x.ReplacedByTokenId,
                        principalTable: "Tbl_SessionRefreshToken",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Tbl_SessionRefreshToken_Tbl_UserSession_SessionId",
                        column: x => x.SessionId,
                        principalTable: "Tbl_UserSession",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_User_NormalizedEmail",
                table: "Tbl_User",
                column: "NormalizedEmail",
                unique: true,
                filter: "\"NormalizedEmail\" IS NOT NULL AND \"IsDeleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_User_NormalizedPhoneNumber",
                table: "Tbl_User",
                column: "NormalizedPhoneNumber",
                unique: true,
                filter: "\"NormalizedPhoneNumber\" IS NOT NULL AND \"IsDeleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_SecurityEvent_CreatedAt",
                table: "Tbl_SecurityEvent",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_SecurityEvent_EventType",
                table: "Tbl_SecurityEvent",
                column: "EventType");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_SecurityEvent_IsDeleted",
                table: "Tbl_SecurityEvent",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_SecurityEvent_No",
                table: "Tbl_SecurityEvent",
                column: "No");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_SecurityEvent_SessionId",
                table: "Tbl_SecurityEvent",
                column: "SessionId");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_SecurityEvent_UserId_OccurredAt",
                table: "Tbl_SecurityEvent",
                columns: new[] { "UserId", "OccurredAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_SessionRefreshToken_CreatedAt",
                table: "Tbl_SessionRefreshToken",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_SessionRefreshToken_FamilyId",
                table: "Tbl_SessionRefreshToken",
                column: "FamilyId");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_SessionRefreshToken_IsDeleted",
                table: "Tbl_SessionRefreshToken",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_SessionRefreshToken_No",
                table: "Tbl_SessionRefreshToken",
                column: "No");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_SessionRefreshToken_ReplacedByTokenId",
                table: "Tbl_SessionRefreshToken",
                column: "ReplacedByTokenId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_SessionRefreshToken_SessionId",
                table: "Tbl_SessionRefreshToken",
                column: "SessionId");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_SessionRefreshToken_TokenHash",
                table: "Tbl_SessionRefreshToken",
                column: "TokenHash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_UserSession_AbsoluteExpiresAt",
                table: "Tbl_UserSession",
                column: "AbsoluteExpiresAt");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_UserSession_CreatedAt",
                table: "Tbl_UserSession",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_UserSession_IsDeleted",
                table: "Tbl_UserSession",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_UserSession_No",
                table: "Tbl_UserSession",
                column: "No");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_UserSession_UserId_RevokedAt",
                table: "Tbl_UserSession",
                columns: new[] { "UserId", "RevokedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_VerificationChallenge_CreatedAt",
                table: "Tbl_VerificationChallenge",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_VerificationChallenge_DestinationHash_Purpose_Status",
                table: "Tbl_VerificationChallenge",
                columns: new[] { "DestinationHash", "Purpose", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_VerificationChallenge_ExpiresAt",
                table: "Tbl_VerificationChallenge",
                column: "ExpiresAt");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_VerificationChallenge_IsDeleted",
                table: "Tbl_VerificationChallenge",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_VerificationChallenge_No",
                table: "Tbl_VerificationChallenge",
                column: "No");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_VerificationChallenge_UserId",
                table: "Tbl_VerificationChallenge",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Tbl_SecurityEvent");

            migrationBuilder.DropTable(
                name: "Tbl_SessionRefreshToken");

            migrationBuilder.DropTable(
                name: "Tbl_VerificationChallenge");

            migrationBuilder.DropTable(
                name: "Tbl_UserSession");

            migrationBuilder.DropIndex(
                name: "IX_Tbl_User_NormalizedEmail",
                table: "Tbl_User");

            migrationBuilder.DropIndex(
                name: "IX_Tbl_User_NormalizedPhoneNumber",
                table: "Tbl_User");

            migrationBuilder.DropColumn(
                name: "EmailVerifiedAt",
                table: "Tbl_User");

            migrationBuilder.DropColumn(
                name: "NormalizedEmail",
                table: "Tbl_User");

            migrationBuilder.DropColumn(
                name: "NormalizedPhoneNumber",
                table: "Tbl_User");

            migrationBuilder.DropColumn(
                name: "PhoneVerifiedAt",
                table: "Tbl_User");

            migrationBuilder.DropColumn(
                name: "SecurityStamp",
                table: "Tbl_User");

            migrationBuilder.AlterColumn<string>(
                name: "PhoneNumber",
                table: "Tbl_User",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(20)",
                oldMaxLength: 20,
                oldNullable: true);

        }
    }
}
