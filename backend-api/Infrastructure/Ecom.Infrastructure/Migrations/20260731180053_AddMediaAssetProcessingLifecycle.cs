using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ecom.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddMediaAssetProcessingLifecycle : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "NextScanAttemptAt",
                table: "Tbl_MediaAsset",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ScanAttemptCount",
                table: "Tbl_MediaAsset",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "ScanLeaseExpiresAt",
                table: "Tbl_MediaAsset",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Sha256",
                table: "Tbl_MediaAsset",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ThumbnailStorageKey",
                table: "Tbl_MediaAsset",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NextScanAttemptAt",
                table: "Tbl_MediaAsset");

            migrationBuilder.DropColumn(
                name: "ScanAttemptCount",
                table: "Tbl_MediaAsset");

            migrationBuilder.DropColumn(
                name: "ScanLeaseExpiresAt",
                table: "Tbl_MediaAsset");

            migrationBuilder.DropColumn(
                name: "Sha256",
                table: "Tbl_MediaAsset");

            migrationBuilder.DropColumn(
                name: "ThumbnailStorageKey",
                table: "Tbl_MediaAsset");
        }
    }
}
