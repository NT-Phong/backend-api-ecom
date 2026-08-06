using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ecom.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddMediaAssetUploadIntent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "TargetVisibility",
                table: "Tbl_MediaAsset",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "Public");

            migrationBuilder.AddColumn<string>(
                name: "UploadIntent",
                table: "Tbl_MediaAsset",
                type: "character varying(40)",
                maxLength: 40,
                nullable: false,
                defaultValue: "ProductImage");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TargetVisibility",
                table: "Tbl_MediaAsset");

            migrationBuilder.DropColumn(
                name: "UploadIntent",
                table: "Tbl_MediaAsset");
        }
    }
}
