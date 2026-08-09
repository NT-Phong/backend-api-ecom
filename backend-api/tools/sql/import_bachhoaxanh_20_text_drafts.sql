BEGIN;

CREATE TEMP TABLE "_catalog_text_import"
(
    "SourceKey" text PRIMARY KEY,
    "ProducerCode" text NOT NULL,
    "ProducerName" text NOT NULL,
    "CategorySlug" text NOT NULL,
    "Name" text NOT NULL,
    "Slug" text NOT NULL,
    "ShortDescription" text,
    "UsageInstructions" text,
    "StorageInstructions" text,
    "WarningText" text,
    "MetaTitle" text,
    "VariantName" text NOT NULL,
    "PriceVnd" numeric(18,2)
) ON COMMIT DROP;

INSERT INTO "_catalog_text_import"
VALUES
('0001','BRAND-NAM-NGU','Nam Ngư','nuoc-mam','Nước mắm Nam Ngư nhãn vàng 14 độ đạm chai 650ml','nuoc-mam-nam-ngu-nhan-vang-chai-650ml','Nước mắm Nam Ngư nhãn vàng 14 độ đạm chai 650ml - chấm hoặc nêm nếm.','Chấm hoặc nêm nếm','Khô thoáng, đậy kín; dùng trong 2–3 tháng sau mở','Có nguyên liệu thủy sản','Nước mắm Nam Ngư nhãn vàng 14 độ đạm chai 650ml | Bách Hoá Xanh','650ml',48500),
('0002','BRAND-KHAI-HOAN','Khải Hoàn','nuoc-mam','Nước mắm Phú Quốc Khải Hoàn 38 độ đạm chai 520ml','nuoc-mam-phu-quoc-khai-hoan-38-do-dam-chai-520ml','Nước mắm Phú Quốc Khải Hoàn 38 độ đạm chai 520ml - gia vị hoặc nước chấm.','Gia vị hoặc nước chấm','Khô ráo, tránh nắng, đậy kín','Không đun quá lâu; không dùng khi hư/hết hạn','Nước mắm Phú Quốc Khải Hoàn 38 độ đạm chai 520ml | Bách Hoá Xanh','520ml',NULL),
('0003','BRAND-LIEN-THANH','Liên Thành','nuoc-mam','Nước mắm truyền thống Liên Thành 20 độ đạm can 1,8 lít','nuoc-mam-lien-thanh-1-8l','Nước mắm truyền thống Liên Thành 20 độ đạm can 1,8 lít - pha chấm, nêm, ướp.','Pha chấm, nêm, ướp','Khô ráo, tránh nắng; dùng 2–3 tháng',NULL,'Nước mắm truyền thống Liên Thành 20 độ đạm can 1,8 lít | Bách Hoá Xanh','1.8L',NULL),
('0004','BRAND-CHINSU','Chinsu','nuoc-mam','Nước mắm hương cá hồi hảo hạng Chinsu 16 độ đạm chai 500ml','nuoc-mam-huong-ca-hoi-hao-hang-chinsu-chai-500ml','Nước mắm hương cá hồi hảo hạng Chinsu 16 độ đạm chai 500ml - chấm hoặc tẩm ướp.','Chấm hoặc tẩm ướp','Khô ráo, thoáng mát, đậy kín','Chai thủy tinh, tránh va đập','Nước mắm hương cá hồi hảo hạng Chinsu 16 độ đạm chai 500ml | Bách Hoá Xanh','500ml',55000),
('0005','BRAND-BARONA','Barona','nuoc-mam','Nước mắm cao cấp Vị Xưa Barona 40 độ đạm chai 500ml','nuoc-mam-vi-xua-500ml','Nước mắm cao cấp Vị Xưa Barona 40 độ đạm chai 500ml - chấm, ướp, nêm, nấu.','Chấm, ướp, nêm, nấu','Khô thoáng, tránh nắng, đậy kín','Kết tinh muối/sẫm màu có thể tự nhiên','Nước mắm cao cấp Vị Xưa Barona 40 độ đạm chai 500ml | Bách Hoá Xanh','500ml',92000),
('0006','BRAND-LIEN-THANH','Liên Thành','nuoc-mam','Nước mắm cao đạm Liên Thành nhãn vàng 35 độ đạm chai 600ml','nuoc-mam-lien-thanh-nhan-vang','Nước mắm cao đạm Liên Thành nhãn vàng 35 độ đạm chai 600ml - chấm, nêm, tẩm ướp.','Chấm, nêm, tẩm ướp','Khô ráo, tránh nhiệt cao/nắng','Không dùng khi hết hạn, mùi/kết cấu lạ','Nước mắm cao đạm Liên Thành nhãn vàng 35 độ đạm chai 600ml | Bách Hoá Xanh','600ml',NULL),
('0007','BRAND-HANH-PHUC','Hạnh Phúc','nuoc-mam','Nước mắm cá cơm Hạnh Phúc 60 độ đạm chai 250ml','nuoc-mam-hanh-phuc-60n-chai-tt-250ml','Nước mắm cá cơm Hạnh Phúc 60 độ đạm chai 250ml - chấm, pha, nêm, ướp.','Chấm, pha, nêm, ướp','Đậy kín; khô ráo, tránh nhiệt/nắng','Không dùng khi hết hạn/ẩm mốc/mùi lạ','Nước mắm cá cơm Hạnh Phúc 60 độ đạm chai 250ml | Bách Hoá Xanh','250ml',NULL),
('0008','BRAND-NAM-NGU','Nam Ngư','nuoc-mam','Nước mắm cá cơm Nam Ngư 12 độ đạm chai 750ml','nuoc-mam-nam-ngu-chai-pet-750ml-18','Nước mắm cá cơm Nam Ngư 12 độ đạm chai 750ml - chấm, nêm, tẩm ướp.','Chấm, nêm, tẩm ướp','Khô thoáng; dùng 2–3 tháng sau mở','Có nguyên liệu nguồn gốc thủy sản','Nước mắm cá cơm Nam Ngư 12 độ đạm chai 750ml | Bách Hoá Xanh','750ml',52500),
('0009','BRAND-LIEN-THANH','Liên Thành','nuoc-mam','Nước mắm Liên Thành nhãn đồng 25 độ đạm chai 600ml','nuoc-man-lien-thanh-nhan-dong-600ml','Nước mắm Liên Thành nhãn đồng 25 độ đạm chai 600ml - nước chấm hoặc gia vị.','Nước chấm hoặc gia vị','Khô ráo, tránh nắng; dùng 2–3 tháng',NULL,'Nước mắm Liên Thành nhãn đồng 25 độ đạm chai 600ml | Bách Hoá Xanh','600ml',NULL),
('0010','BRAND-BARONA','Barona','nuoc-mam','Nước mắm đặc biệt Hải Nhi Barona 40 độ đạm chai 50ml','nuoc-mam-barona-hai-nhi-chai-50ml','Nước mắm đặc biệt Hải Nhi Barona 40 độ đạm chai 50ml - chấm, ướp, nêm cho bé.','Chấm, ướp, nêm cho bé','Khô thoáng, tránh nhiệt, đậy kín','Cho trẻ từ 6 tháng; kết tinh muối có thể tự nhiên','Nước mắm đặc biệt Hải Nhi Barona 40 độ đạm chai 50ml | Bách Hoá Xanh','50ml',27500),
('0011','BRAND-HUNG-THINH','Hưng Thịnh','nuoc-mam','Nước mắm cá cơm đặc sản Hưng Thịnh 40 độ đạm chai 750ml','nmam-dac-san-40-do-750ml','Nước mắm cá cơm đặc sản Hưng Thịnh 40 độ đạm chai 750ml - chấm, pha, nêm, ướp.','Chấm, pha, nêm, ướp','Thoáng mát, tránh nắng/nhiệt; dùng 2–3 tháng',NULL,'Nước mắm cá cơm đặc sản Hưng Thịnh 40 độ đạm chai 750ml | Bách Hoá Xanh','750ml',81000),
('0012','BRAND-HUNG-THINH','Hưng Thịnh','nuoc-mam','Nước mắm cá cơm thượng hạng Hưng Thịnh 25 độ đạm chai nhựa 650ml','nuoc-mam-hung-thinh-25-do-620ml','Nước mắm cá cơm thượng hạng Hưng Thịnh 25 độ đạm chai nhựa 650ml - chấm, nêm, tẩm ướp.','Chấm, nêm, tẩm ướp','Thoáng mát, tránh nắng/nhiệt; dùng 2–3 tháng',NULL,'Nước mắm cá cơm thượng hạng Hưng Thịnh 25 độ đạm chai nhựa 650ml | Bách Hoá Xanh','650ml',39000),
('0013','BRAND-HAPPI-KOKI','Happi Koki','dau-an','Dầu ăn cao cấp Happi Koki can 2 lít','dau-an-cao-cap-happi-koki-can-2-lit','Dầu ăn cao cấp Happi Koki can 2 lít - chiên, xào, salad, bánh, món chay.','Chiên, xào, salad, bánh, món chay','Khô ráo, tránh nắng, đóng kín','Không chiên đi chiên lại nhiều lần','Dầu ăn cao cấp Happi Koki can 2 lít | Bách Hoá Xanh','2L; 1L; 400ml',96000),
('0014','BRAND-TUONG-AN','Tường An','dau-an','Dầu ăn cao cấp Tường An Gold can 5 lít','dau-an-cao-cap-tuong-an-gold-can-5-lit','Dầu ăn cao cấp Tường An Gold can 5 lít - chiên, rán, xào, làm bánh, salad, món chay.','Chiên, rán, xào, làm bánh, salad, món chay','Khô ráo, tránh nắng, đóng kín','Không tái sử dụng dầu nhiều lần','Dầu ăn cao cấp Tường An Gold can 5 lít | Bách Hoá Xanh','5L',NULL),
('0015','BRAND-TUONG-AN','Tường An','dau-an','Dầu thực vật Tường An Cooking Oil chai 1 lít','dau-thuc-vat-tuong-an-cooking-oil-chai-1-lit','Dầu thực vật Tường An Cooking Oil chai 1 lít - chiên, xào, salad, sốt, bánh.','Chiên, xào, salad, sốt, bánh','Khô thoáng, tránh nắng; dùng 2–3 tháng sau mở',NULL,'Dầu thực vật Tường An Cooking Oil chai 1 lít | Bách Hoá Xanh','1L; 2L',59000),
('0016','BRAND-OLIVITALY','Olivitaly','dau-an','Dầu olive Olivitaly Extra Virgin chai 250ml','dau-olive-olivitaly-extra-virgin-chai-250ml','Dầu olive Olivitaly Extra Virgin chai 250ml - salad, tẩm ướp, làm đẹp, xào.','Salad, tẩm ướp, làm đẹp, xào','Thoáng mát; dùng trong 1 tháng sau mở',NULL,'Dầu olive Olivitaly Extra Virgin chai 250ml | Bách Hoá Xanh','250ml',NULL),
('0017','BRAND-VIETCOCO','Vietcoco','dau-an','Dầu dừa nguyên chất Organic Vietcoco 500ml','dau-dua-nguyen-chat-organic-vietcoco-chai-500ml','Dầu dừa nguyên chất Organic Vietcoco 500ml - chiên xào, làm bánh, salad, sốt.','Chiên xào, làm bánh, salad, sốt','Thoáng mát; dùng trong 1 tháng sau mở',NULL,'Dầu dừa nguyên chất Organic Vietcoco 500ml | Bách Hoá Xanh','500ml',NULL),
('0018','BRAND-SLOBODA','Sloboda','dau-an','Dầu hướng dương hữu cơ Sloboda nhãn xanh chai 1 lít','dau-huong-duong-huu-co-sloboda-nhan-xanh-chai-1-lit','Dầu hướng dương hữu cơ Sloboda nhãn xanh chai 1 lít - chiên, xào, kho, salad, bánh, sốt.','Chiên, xào, kho, salad, bánh, sốt','Khô ráo, tránh nhiệt/nắng; dùng 2–3 tháng','Không dùng khi hết hạn/ẩm mốc/mùi lạ','Dầu hướng dương hữu cơ Sloboda nhãn xanh chai 1 lít | Bách Hoá Xanh','1L',NULL),
('0019','BRAND-VINAMILK','Vinamilk','sua-tuoi','Sữa tươi tiệt trùng không đường Vinamilk 100% Sữa tươi bịch 220ml','sua-tuoi-tiet-trung-khong-duong-vinamilk-100-sua-tuoi-bich-220ml','Sữa tươi tiệt trùng không đường Vinamilk 100% Sữa tươi bịch 220ml - lắc đều; ngon hơn khi uống lạnh.','Lắc đều; ngon hơn khi uống lạnh','Khô ráo, thoáng mát; sau mở dùng sớm','Không dùng cho trẻ dưới 1 tuổi; có chứa sữa','Sữa tươi tiệt trùng không đường Vinamilk 100% Sữa tươi bịch 220ml | Bách Hoá Xanh','220ml; thùng 48 bịch',9400),
('0020','BRAND-DUTCH-LADY','Dutch Lady','sua-tuoi','Sữa tiệt trùng có đường Dutch Lady Protein+ hộp 1 lít','sua-tiet-trung-dutch-lady-protein-co-duong-hop-1lit','Sữa tiệt trùng có đường Dutch Lady Protein+ hộp 1 lít - lắc đều; ngon hơn khi uống lạnh.','Lắc đều; ngon hơn khi uống lạnh','Khô ráo, thoáng mát, tránh nắng','Trẻ từ 1 tuổi trở lên','Sữa tiệt trùng có đường Dutch Lady Protein+ hộp 1 lít | Bách Hoá Xanh','1L',30000);

DO $$
BEGIN
    IF EXISTS
    (
        SELECT 1
        FROM "_catalog_text_import" source
        LEFT JOIN "Tbl_Category" category
            ON category."Slug" = source."CategorySlug"
           AND category."IsDeleted" = false
        WHERE category."Id" IS NULL OR category."Status" <> 'Published'
    ) THEN
        RAISE EXCEPTION 'Required published category is missing.';
    END IF;

    IF EXISTS
    (
        SELECT 1
        FROM "_catalog_text_import" source
        JOIN "Tbl_Product" product
            ON product."Slug" = source."Slug"
           AND product."IsDeleted" = false
    ) THEN
        RAISE EXCEPTION 'One or more import product slugs already exist.';
    END IF;

    IF EXISTS
    (
        SELECT 1
        FROM "_catalog_text_import" source
        JOIN "Tbl_Producer" producer
            ON producer."Code" = source."ProducerCode"
           AND producer."IsDeleted" = false
        WHERE producer."Name" <> source."ProducerName"
    ) THEN
        RAISE EXCEPTION 'An existing producer code belongs to a different producer.';
    END IF;
END $$;

INSERT INTO "Tbl_Producer"
(
    "Id", "Code", "Name", "PublicStatus", "IsVerified", "ConcurrencyStamp", "CreatedAt"
)
SELECT
    (
        substr(md5('producer:' || source."ProducerCode"), 1, 8) || '-' ||
        substr(md5('producer:' || source."ProducerCode"), 9, 4) || '-' ||
        substr(md5('producer:' || source."ProducerCode"), 13, 4) || '-' ||
        substr(md5('producer:' || source."ProducerCode"), 17, 4) || '-' ||
        substr(md5('producer:' || source."ProducerCode"), 21, 12)
    )::uuid,
    source."ProducerCode",
    source."ProducerName",
    'Draft',
    false,
    (
        substr(md5('producer-stamp:' || source."ProducerCode"), 1, 8) || '-' ||
        substr(md5('producer-stamp:' || source."ProducerCode"), 9, 4) || '-' ||
        substr(md5('producer-stamp:' || source."ProducerCode"), 13, 4) || '-' ||
        substr(md5('producer-stamp:' || source."ProducerCode"), 17, 4) || '-' ||
        substr(md5('producer-stamp:' || source."ProducerCode"), 21, 12)
    )::uuid,
    NOW()
FROM
(
    SELECT DISTINCT "ProducerCode", "ProducerName"
    FROM "_catalog_text_import"
) source
WHERE NOT EXISTS
(
    SELECT 1
    FROM "Tbl_Producer" producer
    WHERE producer."Code" = source."ProducerCode"
      AND producer."IsDeleted" = false
);

INSERT INTO "Tbl_Product"
(
    "Id", "ProducerId", "Name", "Slug", "ShortDescription", "Description", "UsageInstructions",
    "StorageInstructions", "WarningText", "Status", "MetaTitle", "MetaDescription", "ConcurrencyStamp", "CreatedAt"
)
SELECT
    (
        substr(md5('product:' || source."Slug"), 1, 8) || '-' ||
        substr(md5('product:' || source."Slug"), 9, 4) || '-' ||
        substr(md5('product:' || source."Slug"), 13, 4) || '-' ||
        substr(md5('product:' || source."Slug"), 17, 4) || '-' ||
        substr(md5('product:' || source."Slug"), 21, 12)
    )::uuid,
    producer."Id",
    source."Name",
    source."Slug",
    source."ShortDescription",
    NULL,
    source."UsageInstructions",
    source."StorageInstructions",
    source."WarningText",
    'Draft',
    source."MetaTitle",
    NULL,
    (
        substr(md5('product-stamp:' || source."Slug"), 1, 8) || '-' ||
        substr(md5('product-stamp:' || source."Slug"), 9, 4) || '-' ||
        substr(md5('product-stamp:' || source."Slug"), 13, 4) || '-' ||
        substr(md5('product-stamp:' || source."Slug"), 17, 4) || '-' ||
        substr(md5('product-stamp:' || source."Slug"), 21, 12)
    )::uuid,
    NOW()
FROM "_catalog_text_import" source
JOIN "Tbl_Producer" producer
    ON producer."Code" = source."ProducerCode"
   AND producer."IsDeleted" = false;

INSERT INTO "Tbl_ProductCategory"
(
    "Id", "ProductId", "CategoryId", "IsPrimary", "ConcurrencyStamp", "CreatedAt"
)
SELECT
    (
        substr(md5('product-category:' || product."Slug" || ':' || category."Slug"), 1, 8) || '-' ||
        substr(md5('product-category:' || product."Slug" || ':' || category."Slug"), 9, 4) || '-' ||
        substr(md5('product-category:' || product."Slug" || ':' || category."Slug"), 13, 4) || '-' ||
        substr(md5('product-category:' || product."Slug" || ':' || category."Slug"), 17, 4) || '-' ||
        substr(md5('product-category:' || product."Slug" || ':' || category."Slug"), 21, 12)
    )::uuid,
    product."Id",
    category."Id",
    true,
    (
        substr(md5('product-category-stamp:' || product."Slug" || ':' || category."Slug"), 1, 8) || '-' ||
        substr(md5('product-category-stamp:' || product."Slug" || ':' || category."Slug"), 9, 4) || '-' ||
        substr(md5('product-category-stamp:' || product."Slug" || ':' || category."Slug"), 13, 4) || '-' ||
        substr(md5('product-category-stamp:' || product."Slug" || ':' || category."Slug"), 17, 4) || '-' ||
        substr(md5('product-category-stamp:' || product."Slug" || ':' || category."Slug"), 21, 12)
    )::uuid,
    NOW()
FROM "_catalog_text_import" source
JOIN "Tbl_Product" product ON product."Slug" = source."Slug" AND product."IsDeleted" = false
JOIN "Tbl_Category" category ON category."Slug" = source."CategorySlug" AND category."IsDeleted" = false;

INSERT INTO "Tbl_ProductVariant"
(
    "Id", "ProductId", "Sku", "Name", "Status", "InventoryMode", "AllowBackorder", "DisplayOrder", "ConcurrencyStamp", "CreatedAt"
)
SELECT
    (
        substr(md5('variant:' || source."SourceKey"), 1, 8) || '-' ||
        substr(md5('variant:' || source."SourceKey"), 9, 4) || '-' ||
        substr(md5('variant:' || source."SourceKey"), 13, 4) || '-' ||
        substr(md5('variant:' || source."SourceKey"), 17, 4) || '-' ||
        substr(md5('variant:' || source."SourceKey"), 21, 12)
    )::uuid,
    product."Id",
    'IMP-' || source."SourceKey",
    source."VariantName",
    'Active',
    'NotTracked',
    false,
    0,
    (
        substr(md5('variant-stamp:' || source."SourceKey"), 1, 8) || '-' ||
        substr(md5('variant-stamp:' || source."SourceKey"), 9, 4) || '-' ||
        substr(md5('variant-stamp:' || source."SourceKey"), 13, 4) || '-' ||
        substr(md5('variant-stamp:' || source."SourceKey"), 17, 4) || '-' ||
        substr(md5('variant-stamp:' || source."SourceKey"), 21, 12)
    )::uuid,
    NOW()
FROM "_catalog_text_import" source
JOIN "Tbl_Product" product ON product."Slug" = source."Slug" AND product."IsDeleted" = false;

INSERT INTO "Tbl_VariantPrice"
(
    "Id", "ProductVariantId", "CurrencyCode", "Amount", "MinQuantity", "PriceType", "EffectiveFrom", "ConcurrencyStamp", "CreatedAt"
)
SELECT
    (
        substr(md5('variant-price:' || source."SourceKey"), 1, 8) || '-' ||
        substr(md5('variant-price:' || source."SourceKey"), 9, 4) || '-' ||
        substr(md5('variant-price:' || source."SourceKey"), 13, 4) || '-' ||
        substr(md5('variant-price:' || source."SourceKey"), 17, 4) || '-' ||
        substr(md5('variant-price:' || source."SourceKey"), 21, 12)
    )::uuid,
    variant."Id",
    'VND',
    source."PriceVnd",
    1,
    'Public',
    NOW(),
    (
        substr(md5('variant-price-stamp:' || source."SourceKey"), 1, 8) || '-' ||
        substr(md5('variant-price-stamp:' || source."SourceKey"), 9, 4) || '-' ||
        substr(md5('variant-price-stamp:' || source."SourceKey"), 13, 4) || '-' ||
        substr(md5('variant-price-stamp:' || source."SourceKey"), 17, 4) || '-' ||
        substr(md5('variant-price-stamp:' || source."SourceKey"), 21, 12)
    )::uuid,
    NOW()
FROM "_catalog_text_import" source
JOIN "Tbl_ProductVariant" variant
    ON variant."Sku" = 'IMP-' || source."SourceKey"
   AND variant."IsDeleted" = false
WHERE source."PriceVnd" IS NOT NULL;

SELECT
    (SELECT count(*) FROM "Tbl_Product" WHERE "IsDeleted" = false AND "Status" = 'Draft') AS "DraftProductCount",
    (SELECT count(*) FROM "Tbl_ProductVariant" WHERE "IsDeleted" = false AND "Sku" LIKE 'IMP-%') AS "ImportedVariantCount",
    (SELECT count(*) FROM "Tbl_VariantPrice" WHERE "IsDeleted" = false AND "ProductVariantId" IN (SELECT "Id" FROM "Tbl_ProductVariant" WHERE "Sku" LIKE 'IMP-%')) AS "ImportedPriceCount";

COMMIT;
