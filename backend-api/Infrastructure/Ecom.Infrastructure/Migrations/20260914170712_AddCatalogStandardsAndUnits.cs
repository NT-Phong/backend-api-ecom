using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ecom.Infrastructure.Migrations;

public partial class AddCatalogStandardsAndUnits : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(name: "UnitLabel", table: "Tbl_ProductVariant",
            type: "character varying(50)", maxLength: 50, nullable: true);
        migrationBuilder.AddColumn<string>(name: "Standard", table: "Tbl_Product",
            type: "character varying(50)", maxLength: 50, nullable: true);
        migrationBuilder.CreateIndex(
            name: "IX_CouponRedemption_CouponId_UserId_Active",
            table: "Tbl_CouponRedemption",
            columns: new[] { "CouponId", "UserId" },
            filter: "\"IsDeleted\" = false");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(name: "IX_CouponRedemption_CouponId_UserId_Active",
            table: "Tbl_CouponRedemption");
        migrationBuilder.DropColumn(name: "UnitLabel", table: "Tbl_ProductVariant");
        migrationBuilder.DropColumn(name: "Standard", table: "Tbl_Product");
    }
}
