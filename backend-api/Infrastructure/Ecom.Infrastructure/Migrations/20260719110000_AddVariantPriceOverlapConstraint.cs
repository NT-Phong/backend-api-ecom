using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Ecom.Infrastructure.Persistence.Database;

#nullable disable

namespace Ecom.Infrastructure.Migrations;

[DbContext(typeof(ApplicationDbContext))]
[Migration("20260719110000_AddVariantPriceOverlapConstraint")]
public partial class AddVariantPriceOverlapConstraint : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("CREATE EXTENSION IF NOT EXISTS btree_gist;");
        migrationBuilder.Sql("""
            ALTER TABLE "Tbl_VariantPrice"
            ADD CONSTRAINT "EX_VariantPrice_ActivePeriod"
            EXCLUDE USING gist (
                "ProductVariantId" WITH =,
                COALESCE("PriceListId", '00000000-0000-0000-0000-000000000000'::uuid) WITH =,
                "PriceType" WITH =,
                tstzrange("EffectiveFrom", "EffectiveTo", '[)') WITH &&
            ) WHERE ("IsDeleted" = false);
            """);
    }

    protected override void Down(MigrationBuilder migrationBuilder) =>
        migrationBuilder.Sql("ALTER TABLE \"Tbl_VariantPrice\" DROP CONSTRAINT IF EXISTS \"EX_VariantPrice_ActivePeriod\";");
}
