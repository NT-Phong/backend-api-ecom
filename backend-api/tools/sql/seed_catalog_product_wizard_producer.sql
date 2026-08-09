-- Catalog Product Wizard prerequisite seed (PostgreSQL)
--
-- Purpose
--   1. Create one real, active Producer record that FE can use as ProducerId
--      when calling POST /api/v1/catalog/products.
--   2. Make that Producer Published + verified so a Product using it can later
--      pass the Product publish prerequisite as well.
--   3. Print the exact ProducerId for FE configuration and provide read-only
--      diagnostics for catalog.products.create.
--
-- Safety
--   - This script is idempotent for the exact active record below.
--   - It raises instead of overwriting an existing record with the same ID/code
--     but different identity or a soft-deleted matching ID.
--   - It does NOT create a Product, Category, User, Role, Policy, or UserPolicy.
--   - Review and execute against the intended non-production/test PostgreSQL DB.

BEGIN;

DO $$
BEGIN
    -- Existing ID must be the exact active seed row; do not revive/overwrite it.
    IF EXISTS (
        SELECT 1
        FROM "Tbl_Producer"
        WHERE "Id" = 'c91c495c-5876-4a7c-a33d-8c0be133315a'::uuid
          AND ("IsDeleted" = TRUE OR "Code" <> 'FE_PRODUCT_WIZARD')
    ) THEN
        RAISE EXCEPTION
            'Producer seed ID c91c495c-5876-4a7c-a33d-8c0be133315a is already used by another or deleted record.';
    END IF;

    -- The active business code must not silently point to another Producer.
    IF EXISTS (
        SELECT 1
        FROM "Tbl_Producer"
        WHERE "Code" = 'FE_PRODUCT_WIZARD'
          AND "IsDeleted" = FALSE
          AND "Id" <> 'c91c495c-5876-4a7c-a33d-8c0be133315a'::uuid
    ) THEN
        RAISE EXCEPTION
            'Active Producer code FE_PRODUCT_WIZARD belongs to a different Producer.';
    END IF;
END $$;

INSERT INTO "Tbl_Producer"
    ("Id", "Code", "Name", "LegalName", "Description", "WebsiteUrl",
     "PublicStatus", "IsVerified", "VerifiedAt", "VerifiedByUserId",
     "ConcurrencyStamp", "CreatedAt", "IsDeleted")
SELECT
    'c91c495c-5876-4a7c-a33d-8c0be133315a'::uuid,
    'FE_PRODUCT_WIZARD',
    'Đơn vị sản xuất kiểm thử Catalog',
    'Đơn vị sản xuất kiểm thử Catalog',
    'Producer dữ liệu kiểm thử cho ProductWizard. Không dùng làm dữ liệu thương mại.',
    NULL,
    'Published',
    TRUE,
    NOW(),
    NULL,
    '7ee65503-ecc3-49ee-bb1b-a9b549c9db7b'::uuid,
    NOW(),
    FALSE
WHERE NOT EXISTS (
    SELECT 1
    FROM "Tbl_Producer"
    WHERE "Id" = 'c91c495c-5876-4a7c-a33d-8c0be133315a'::uuid
);

-- Copy this ProducerId value to FE only after this query returns exactly one row.
SELECT
    "Id" AS "ProducerId",
    "Code",
    "Name",
    "PublicStatus",
    "IsVerified",
    "VerifiedAt"
FROM "Tbl_Producer"
WHERE "Id" = 'c91c495c-5876-4a7c-a33d-8c0be133315a'::uuid
  AND "IsDeleted" = FALSE;

COMMIT;

-- -----------------------------------------------------------------------------
-- Read-only diagnostic for condition 1: the logged-in backoffice user must have
-- catalog.products.create in the JWT. Fill in an actual user ID and run this
-- query separately. Re-login/refresh token after any role/policy change.
-- -----------------------------------------------------------------------------
--
-- WITH target AS (
--     SELECT '<ACTUAL_BACKOFFICE_USER_UUID>'::uuid AS "UserId"
-- )
-- SELECT
--     userRow."Id" AS "UserId",
--     roleRow."Code" AS "RoleCode",
--     CASE
--         WHEN EXISTS (
--             SELECT 1
--             FROM "Tbl_UserPolicy" AS userPolicy
--             INNER JOIN "Tbl_Policy" AS policy ON policy."Id" = userPolicy."PolicyId"
--             WHERE userPolicy."UserId" = userRow."Id"
--               AND userPolicy."IsDeleted" = FALSE
--               AND policy."IsDeleted" = FALSE
--               AND policy."IsActive" = TRUE
--               AND policy."Code" = 'catalog.products.create'
--               AND userPolicy."IsGranted" = FALSE
--               AND (userPolicy."ExpiresAt" IS NULL OR userPolicy."ExpiresAt" > NOW())
--         ) THEN FALSE
--         WHEN EXISTS (
--             SELECT 1
--             FROM "Tbl_UserPolicy" AS userPolicy
--             INNER JOIN "Tbl_Policy" AS policy ON policy."Id" = userPolicy."PolicyId"
--             WHERE userPolicy."UserId" = userRow."Id"
--               AND userPolicy."IsDeleted" = FALSE
--               AND policy."IsDeleted" = FALSE
--               AND policy."IsActive" = TRUE
--               AND policy."Code" = 'catalog.products.create'
--               AND userPolicy."IsGranted" = TRUE
--               AND (userPolicy."ExpiresAt" IS NULL OR userPolicy."ExpiresAt" > NOW())
--         ) THEN TRUE
--         WHEN EXISTS (
--             SELECT 1
--             FROM "Tbl_RolePolicy" AS rolePolicy
--             INNER JOIN "Tbl_Policy" AS policy ON policy."Id" = rolePolicy."PolicyId"
--             WHERE rolePolicy."RoleId" = userRow."RoleId"
--               AND rolePolicy."IsDeleted" = FALSE
--               AND policy."IsDeleted" = FALSE
--               AND policy."IsActive" = TRUE
--               AND policy."Code" = 'catalog.products.create'
--         ) THEN TRUE
--         ELSE FALSE
--     END AS "HasCatalogProductsCreate"
-- FROM "Tbl_User" AS userRow
-- LEFT JOIN "Tbl_Role" AS roleRow
--     ON roleRow."Id" = userRow."RoleId" AND roleRow."IsDeleted" = FALSE
-- INNER JOIN target ON target."UserId" = userRow."Id"
-- WHERE userRow."IsDeleted" = FALSE;
