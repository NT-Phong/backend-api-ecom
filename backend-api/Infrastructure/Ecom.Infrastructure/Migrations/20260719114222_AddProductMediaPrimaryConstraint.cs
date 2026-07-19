using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ecom.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddProductMediaPrimaryConstraint : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Tbl_ProductMedia_ProductId",
                table: "Tbl_ProductMedia",
                column: "ProductId",
                unique: true,
                filter: "\"IsDeleted\" = false AND \"IsPrimary\" = true");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Tbl_ProductMedia_ProductId",
                table: "Tbl_ProductMedia");
        }
    }
}
