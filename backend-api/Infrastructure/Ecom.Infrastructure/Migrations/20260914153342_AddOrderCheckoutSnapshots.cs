using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ecom.Infrastructure.Migrations;

/// <summary>
/// Restored source for an already-applied migration. The migration ID must remain unchanged so
/// EF matches the row recorded in __EFMigrationsHistory.
/// </summary>
public partial class AddOrderCheckoutSnapshots : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(name: "AppliedCouponCodeSnapshot", table: "Tbl_Order",
            type: "character varying(50)", maxLength: 50, nullable: true);
        migrationBuilder.AddColumn<string>(name: "CustomerNotesSnapshot", table: "Tbl_Order",
            type: "character varying(1000)", maxLength: 1000, nullable: true);
        migrationBuilder.AddColumn<string>(name: "DeliverySlotSnapshot", table: "Tbl_Order",
            type: "character varying(100)", maxLength: 100, nullable: true);
        migrationBuilder.AddColumn<string>(name: "PackagingOptionSnapshot", table: "Tbl_Order",
            type: "character varying(50)", maxLength: 50, nullable: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(name: "AppliedCouponCodeSnapshot", table: "Tbl_Order");
        migrationBuilder.DropColumn(name: "CustomerNotesSnapshot", table: "Tbl_Order");
        migrationBuilder.DropColumn(name: "DeliverySlotSnapshot", table: "Tbl_Order");
        migrationBuilder.DropColumn(name: "PackagingOptionSnapshot", table: "Tbl_Order");
    }
}
