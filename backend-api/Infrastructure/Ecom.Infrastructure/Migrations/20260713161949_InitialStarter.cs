using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Ecom.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialStarter : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Tbl_Permission",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RoleId = table.Column<Guid>(type: "uuid", nullable: true),
                    ApiRouteId = table.Column<Guid>(type: "uuid", nullable: true),
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
                    table.PrimaryKey("PK_Tbl_Permission", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Tbl_Policy",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Module = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    IsSystemPolicy = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
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
                    table.PrimaryKey("PK_Tbl_Policy", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Tbl_Role",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    Priority = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    IsSystemRole = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
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
                    table.PrimaryKey("PK_Tbl_Role", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Tbl_RolePolicy",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RoleId = table.Column<Guid>(type: "uuid", nullable: false),
                    PolicyId = table.Column<Guid>(type: "uuid", nullable: false),
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
                    table.PrimaryKey("PK_Tbl_RolePolicy", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Tbl_RolePolicy_Tbl_Policy_PolicyId",
                        column: x => x.PolicyId,
                        principalTable: "Tbl_Policy",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Tbl_RolePolicy_Tbl_Role_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Tbl_Role",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Tbl_User",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PhoneNumber = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "boolean", nullable: false),
                    EmailConfirmed = table.Column<bool>(type: "boolean", nullable: false),
                    FullName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    AvatarId = table.Column<Guid>(type: "uuid", nullable: true),
                    Address = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ZoneIds = table.Column<Guid[]>(type: "uuid[]", nullable: false),
                    FirstLoginAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsProfileCompleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeletionReason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    FailedLoginAttempts = table.Column<int>(type: "integer", nullable: false),
                    LockoutEnd = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    LastLoginAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastLoginIp = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: true),
                    RoleId = table.Column<Guid>(type: "uuid", nullable: true),
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
                    table.PrimaryKey("PK_Tbl_User", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Tbl_User_Tbl_Role_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Tbl_Role",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Tbl_JwtRefreshToken",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Token = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedByIp = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: true),
                    CreatedByUserAgent = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    RevokedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RevokedByIp = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: true),
                    RevokedReason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ReplacedByToken = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("PK_Tbl_JwtRefreshToken", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Tbl_JwtRefreshToken_Tbl_User_UserId",
                        column: x => x.UserId,
                        principalTable: "Tbl_User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Tbl_OtpToken",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    OtpTokenType = table.Column<int>(type: "integer", nullable: false),
                    ExpiredAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsUsed = table.Column<bool>(type: "boolean", nullable: false),
                    UsedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    PhoneNumber = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    FailedAttempts = table.Column<int>(type: "integer", nullable: false),
                    MaxAttempts = table.Column<int>(type: "integer", nullable: false),
                    CreatedByIp = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: true),
                    VerifiedByIp = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: true),
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
                    table.PrimaryKey("PK_Tbl_OtpToken", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Tbl_OtpToken_Tbl_User_UserId",
                        column: x => x.UserId,
                        principalTable: "Tbl_User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Tbl_UserDeviceToken",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    FcmToken = table.Column<string>(type: "character varying(4096)", maxLength: 4096, nullable: false),
                    Platform = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    LastUsedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
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
                    table.PrimaryKey("PK_Tbl_UserDeviceToken", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Tbl_UserDeviceToken_Tbl_User_UserId",
                        column: x => x.UserId,
                        principalTable: "Tbl_User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Tbl_UserPolicy",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    PolicyId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsGranted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    Reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
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
                    table.PrimaryKey("PK_Tbl_UserPolicy", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Tbl_UserPolicy_Tbl_Policy_PolicyId",
                        column: x => x.PolicyId,
                        principalTable: "Tbl_Policy",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Tbl_UserPolicy_Tbl_User_UserId",
                        column: x => x.UserId,
                        principalTable: "Tbl_User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_JwtRefreshToken_CreatedAt",
                table: "Tbl_JwtRefreshToken",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_JwtRefreshToken_ExpiresAt",
                table: "Tbl_JwtRefreshToken",
                column: "ExpiresAt");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_JwtRefreshToken_IsDeleted",
                table: "Tbl_JwtRefreshToken",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_JwtRefreshToken_No",
                table: "Tbl_JwtRefreshToken",
                column: "No");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_JwtRefreshToken_Status",
                table: "Tbl_JwtRefreshToken",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_JwtRefreshToken_Token",
                table: "Tbl_JwtRefreshToken",
                column: "Token",
                unique: true,
                filter: "\"IsDeleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_JwtRefreshToken_UserId",
                table: "Tbl_JwtRefreshToken",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_OtpToken_CreatedAt",
                table: "Tbl_OtpToken",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_OtpToken_ExpiredAt",
                table: "Tbl_OtpToken",
                column: "ExpiredAt");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_OtpToken_IsDeleted",
                table: "Tbl_OtpToken",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_OtpToken_No",
                table: "Tbl_OtpToken",
                column: "No");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_OtpToken_OtpTokenType",
                table: "Tbl_OtpToken",
                column: "OtpTokenType");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_OtpToken_UserId",
                table: "Tbl_OtpToken",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_OtpToken_UserId_OtpTokenType_IsUsed",
                table: "Tbl_OtpToken",
                columns: new[] { "UserId", "OtpTokenType", "IsUsed" });

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_Permission_CreatedAt",
                table: "Tbl_Permission",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_Permission_IsDeleted",
                table: "Tbl_Permission",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_Permission_No",
                table: "Tbl_Permission",
                column: "No");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_Policy_Code",
                table: "Tbl_Policy",
                column: "Code",
                unique: true,
                filter: "\"IsDeleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_Policy_CreatedAt",
                table: "Tbl_Policy",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_Policy_IsDeleted",
                table: "Tbl_Policy",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_Policy_Module",
                table: "Tbl_Policy",
                column: "Module");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_Policy_No",
                table: "Tbl_Policy",
                column: "No");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_Role_Code",
                table: "Tbl_Role",
                column: "Code",
                unique: true,
                filter: "\"IsDeleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_Role_CreatedAt",
                table: "Tbl_Role",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_Role_IsDeleted",
                table: "Tbl_Role",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_Role_No",
                table: "Tbl_Role",
                column: "No");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_RolePolicy_CreatedAt",
                table: "Tbl_RolePolicy",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_RolePolicy_IsDeleted",
                table: "Tbl_RolePolicy",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_RolePolicy_No",
                table: "Tbl_RolePolicy",
                column: "No");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_RolePolicy_PolicyId",
                table: "Tbl_RolePolicy",
                column: "PolicyId");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_RolePolicy_RoleId_PolicyId",
                table: "Tbl_RolePolicy",
                columns: new[] { "RoleId", "PolicyId" },
                unique: true,
                filter: "\"IsDeleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_User_CreatedAt",
                table: "Tbl_User",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_User_Email",
                table: "Tbl_User",
                column: "Email",
                unique: true,
                filter: "\"Email\" IS NOT NULL AND \"IsDeleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_User_IsDeleted",
                table: "Tbl_User",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_User_No",
                table: "Tbl_User",
                column: "No");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_User_PhoneNumber",
                table: "Tbl_User",
                column: "PhoneNumber",
                unique: true,
                filter: "\"IsDeleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_User_RoleId",
                table: "Tbl_User",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_User_Status",
                table: "Tbl_User",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_UserDeviceToken_CreatedAt",
                table: "Tbl_UserDeviceToken",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_UserDeviceToken_FcmToken",
                table: "Tbl_UserDeviceToken",
                column: "FcmToken",
                filter: "\"IsDeleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_UserDeviceToken_IsDeleted",
                table: "Tbl_UserDeviceToken",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_UserDeviceToken_No",
                table: "Tbl_UserDeviceToken",
                column: "No");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_UserDeviceToken_UserId",
                table: "Tbl_UserDeviceToken",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_UserPolicy_CreatedAt",
                table: "Tbl_UserPolicy",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_UserPolicy_IsDeleted",
                table: "Tbl_UserPolicy",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_UserPolicy_No",
                table: "Tbl_UserPolicy",
                column: "No");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_UserPolicy_PolicyId",
                table: "Tbl_UserPolicy",
                column: "PolicyId");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_UserPolicy_UserId_PolicyId",
                table: "Tbl_UserPolicy",
                columns: new[] { "UserId", "PolicyId" },
                unique: true,
                filter: "\"IsDeleted\" = false");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Tbl_JwtRefreshToken");

            migrationBuilder.DropTable(
                name: "Tbl_OtpToken");

            migrationBuilder.DropTable(
                name: "Tbl_Permission");

            migrationBuilder.DropTable(
                name: "Tbl_RolePolicy");

            migrationBuilder.DropTable(
                name: "Tbl_UserDeviceToken");

            migrationBuilder.DropTable(
                name: "Tbl_UserPolicy");

            migrationBuilder.DropTable(
                name: "Tbl_Policy");

            migrationBuilder.DropTable(
                name: "Tbl_User");

            migrationBuilder.DropTable(
                name: "Tbl_Role");
        }
    }
}

