using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Ecom.Infrastructure.Persistence.Database;

#nullable disable

namespace Ecom.Infrastructure.Migrations;

[DbContext(typeof(ApplicationDbContext))]
[Migration("20260830130000_AddCatalogDiscoverySearch")]
public partial class AddCatalogDiscoverySearch : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("CREATE EXTENSION IF NOT EXISTS unaccent;");
        migrationBuilder.Sql("CREATE EXTENSION IF NOT EXISTS pg_trgm;");
        migrationBuilder.Sql("""
            CREATE OR REPLACE FUNCTION public.immutable_unaccent(text)
            RETURNS text
            LANGUAGE sql
            IMMUTABLE
            PARALLEL SAFE
            STRICT
            AS $$ SELECT public.unaccent('public.unaccent', $1); $$;
            """);
        migrationBuilder.Sql("""
            CREATE INDEX IF NOT EXISTS "IX_Product_Search_Name_Trgm"
            ON "Tbl_Product" USING gin (public.immutable_unaccent("Name") gin_trgm_ops)
            WHERE "IsDeleted" = FALSE AND "Status" = 'Published';
            """);
        migrationBuilder.Sql("""
            CREATE INDEX IF NOT EXISTS "IX_Product_Search_ShortDescription_Trgm"
            ON "Tbl_Product" USING gin (public.immutable_unaccent(COALESCE("ShortDescription", '')) gin_trgm_ops)
            WHERE "IsDeleted" = FALSE AND "Status" = 'Published';
            """);
        migrationBuilder.Sql("""
            CREATE INDEX IF NOT EXISTS "IX_Product_Search_BrandName_Trgm"
            ON "Tbl_Product" USING gin (public.immutable_unaccent(COALESCE("BrandName", '')) gin_trgm_ops)
            WHERE "IsDeleted" = FALSE AND "Status" = 'Published';
            """);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("DROP INDEX IF EXISTS \"IX_Product_Search_BrandName_Trgm\";");
        migrationBuilder.Sql("DROP INDEX IF EXISTS \"IX_Product_Search_ShortDescription_Trgm\";");
        migrationBuilder.Sql("DROP INDEX IF EXISTS \"IX_Product_Search_Name_Trgm\";");
        migrationBuilder.Sql("DROP FUNCTION IF EXISTS public.immutable_unaccent(text);");
    }
}
