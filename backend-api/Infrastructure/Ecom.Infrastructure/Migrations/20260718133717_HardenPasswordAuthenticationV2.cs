using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ecom.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class HardenPasswordAuthenticationV2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AlgorithmVersion",
                table: "Tbl_PasswordCredential",
                type: "character varying(32)",
                maxLength: 32,
                nullable: false,
                defaultValue: "bcrypt-v1");

            migrationBuilder.AddColumn<DateTime>(
                name: "LastFailedAt",
                table: "Tbl_PasswordCredential",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LockedUntil",
                table: "Tbl_PasswordCredential",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AlgorithmVersion",
                table: "Tbl_PasswordCredential");

            migrationBuilder.DropColumn(
                name: "LastFailedAt",
                table: "Tbl_PasswordCredential");

            migrationBuilder.DropColumn(
                name: "LockedUntil",
                table: "Tbl_PasswordCredential");
        }
    }
}
