using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Ecom.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddThanhHoaCommerceSchemaV2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Tbl_AdministrativeArea",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ParentId = table.Column<Guid>(type: "uuid", nullable: true),
                    Code = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Level = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    No = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ConcurrencyStamp = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tbl_AdministrativeArea", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Tbl_AdministrativeArea_Tbl_AdministrativeArea_ParentId",
                        column: x => x.ParentId,
                        principalTable: "Tbl_AdministrativeArea",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Tbl_ArticleCategory",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Slug = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    No = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ConcurrencyStamp = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tbl_ArticleCategory", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Tbl_AuditLog",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ActorUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    Action = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    EntityName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    EntityId = table.Column<Guid>(type: "uuid", nullable: true),
                    CorrelationId = table.Column<Guid>(type: "uuid", nullable: true),
                    BeforeData = table.Column<string>(type: "jsonb", nullable: true),
                    AfterData = table.Column<string>(type: "jsonb", nullable: true),
                    OccurredAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IpAddress = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: true),
                    No = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ConcurrencyStamp = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tbl_AuditLog", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Tbl_Campaign",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    StartsAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    EndsAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    No = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ConcurrencyStamp = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tbl_Campaign", x => x.Id);
                    table.CheckConstraint("CK_Campaign_TimeWindow", "\"EndsAt\" IS NULL OR \"StartsAt\" IS NULL OR \"EndsAt\" > \"StartsAt\"");
                });

            migrationBuilder.CreateTable(
                name: "Tbl_Cart",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: true),
                    GuestTokenHash = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    Status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    CurrencyCode = table.Column<string>(type: "character(3)", fixedLength: true, maxLength: 3, nullable: false, defaultValue: "VND"),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    No = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ConcurrencyStamp = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tbl_Cart", x => x.Id);
                    table.CheckConstraint("CK_Cart_Owner", "(\"UserId\" IS NULL) <> (\"GuestTokenHash\" IS NULL)");
                    table.ForeignKey(
                        name: "FK_Tbl_Cart_Tbl_User_UserId",
                        column: x => x.UserId,
                        principalTable: "Tbl_User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Tbl_Category",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ParentId = table.Column<Guid>(type: "uuid", nullable: true),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Slug = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    Status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    No = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ConcurrencyStamp = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tbl_Category", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Tbl_Category_Tbl_Category_ParentId",
                        column: x => x.ParentId,
                        principalTable: "Tbl_Category",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Tbl_Certification",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CertificationType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    CertificateNumber = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    IssuingAuthority = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    IssuedAt = table.Column<DateOnly>(type: "date", nullable: true),
                    EffectiveFrom = table.Column<DateOnly>(type: "date", nullable: true),
                    EffectiveTo = table.Column<DateOnly>(type: "date", nullable: true),
                    VerificationStatus = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    VerifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    VerifiedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    No = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ConcurrencyStamp = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tbl_Certification", x => x.Id);
                    table.CheckConstraint("CK_Certification_TimeWindow", "\"EffectiveTo\" IS NULL OR \"EffectiveFrom\" IS NULL OR \"EffectiveTo\" >= \"EffectiveFrom\"");
                });

            migrationBuilder.CreateTable(
                name: "Tbl_CustomerProfile",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    PreferredName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    DateOfBirth = table.Column<DateOnly>(type: "date", nullable: true),
                    Gender = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    MarketingConsentAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    No = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ConcurrencyStamp = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tbl_CustomerProfile", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Tbl_CustomerProfile_Tbl_User_UserId",
                        column: x => x.UserId,
                        principalTable: "Tbl_User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Tbl_MediaAsset",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    StorageKey = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    OriginalFileName = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    ContentType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    SizeBytes = table.Column<long>(type: "bigint", nullable: false),
                    MediaType = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    AltText = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Visibility = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    ScanStatus = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    No = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ConcurrencyStamp = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tbl_MediaAsset", x => x.Id);
                    table.CheckConstraint("CK_MediaAsset_SizeBytes", "\"SizeBytes\" >= 0");
                });

            migrationBuilder.CreateTable(
                name: "Tbl_NewsletterSubscription",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: true),
                    Email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    ConsentAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UnsubscribedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Source = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    No = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ConcurrencyStamp = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tbl_NewsletterSubscription", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Tbl_NewsletterSubscription_Tbl_User_UserId",
                        column: x => x.UserId,
                        principalTable: "Tbl_User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Tbl_Notification",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    NotificationType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Title = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    Body = table.Column<string>(type: "text", nullable: false),
                    Data = table.Column<string>(type: "jsonb", nullable: true),
                    CreatedBySystem = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    No = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ConcurrencyStamp = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tbl_Notification", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Tbl_Page",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    Slug = table.Column<string>(type: "character varying(350)", maxLength: 350, nullable: false),
                    Status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    PublishedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    MetaTitle = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    MetaDescription = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    No = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ConcurrencyStamp = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tbl_Page", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Tbl_PartnerApplication",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: true),
                    ApplicantName = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    OrganizationName = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    Email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    PhoneNumber = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    ApplicationType = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Message = table.Column<string>(type: "text", nullable: true),
                    AssignedToUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    No = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ConcurrencyStamp = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tbl_PartnerApplication", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Tbl_PartnerApplication_Tbl_User_UserId",
                        column: x => x.UserId,
                        principalTable: "Tbl_User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Tbl_PriceList",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    StartsAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    EndsAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    No = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ConcurrencyStamp = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tbl_PriceList", x => x.Id);
                    table.CheckConstraint("CK_PriceList_TimeWindow", "\"EndsAt\" IS NULL OR \"StartsAt\" IS NULL OR \"EndsAt\" > \"StartsAt\"");
                });

            migrationBuilder.CreateTable(
                name: "Tbl_Producer",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    LegalName = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    Description = table.Column<string>(type: "text", nullable: true),
                    WebsiteUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    PublicStatus = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    IsVerified = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    VerifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    VerifiedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    No = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ConcurrencyStamp = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tbl_Producer", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Tbl_Promotion",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    PromotionType = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Value = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    StartsAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndsAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    MinOrderAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    No = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ConcurrencyStamp = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tbl_Promotion", x => x.Id);
                    table.CheckConstraint("CK_Promotion_MinOrder", "\"MinOrderAmount\" IS NULL OR \"MinOrderAmount\" >= 0");
                    table.CheckConstraint("CK_Promotion_TimeWindow", "\"EndsAt\" IS NULL OR \"EndsAt\" > \"StartsAt\"");
                    table.CheckConstraint("CK_Promotion_Value", "\"Value\" >= 0");
                });

            migrationBuilder.CreateTable(
                name: "Tbl_SeoRedirect",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SourcePath = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    TargetPath = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    StatusCode = table.Column<int>(type: "integer", nullable: false, defaultValue: 301),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    No = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ConcurrencyStamp = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tbl_SeoRedirect", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Tbl_SystemSetting",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SettingKey = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Value = table.Column<string>(type: "jsonb", nullable: false),
                    IsPublic = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    No = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ConcurrencyStamp = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tbl_SystemSetting", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Tbl_TradeInquiry",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    InquiryNumber = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: true),
                    ContactName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    CompanyName = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    Email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    PhoneNumber = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    InquiryType = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    AssignedToUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    Message = table.Column<string>(type: "text", nullable: true),
                    No = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ConcurrencyStamp = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tbl_TradeInquiry", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Tbl_TradeInquiry_Tbl_User_UserId",
                        column: x => x.UserId,
                        principalTable: "Tbl_User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Tbl_VisitorSession",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: true),
                    SessionHash = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Source = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Medium = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Campaign = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    ConsentStatus = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    StartedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    No = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ConcurrencyStamp = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tbl_VisitorSession", x => x.Id);
                    table.CheckConstraint("CK_VisitorSession_TimeWindow", "\"EndedAt\" IS NULL OR \"EndedAt\" >= \"StartedAt\"");
                    table.ForeignKey(
                        name: "FK_Tbl_VisitorSession_Tbl_User_UserId",
                        column: x => x.UserId,
                        principalTable: "Tbl_User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Tbl_Wishlist",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false, defaultValue: "Default"),
                    No = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ConcurrencyStamp = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tbl_Wishlist", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Tbl_Wishlist_Tbl_User_UserId",
                        column: x => x.UserId,
                        principalTable: "Tbl_User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Tbl_CustomerAddress",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    AdministrativeAreaId = table.Column<Guid>(type: "uuid", nullable: true),
                    RecipientName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    PhoneNumber = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    AddressLine = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    PostalCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Latitude = table.Column<decimal>(type: "numeric(10,7)", precision: 10, scale: 7, nullable: true),
                    Longitude = table.Column<decimal>(type: "numeric(10,7)", precision: 10, scale: 7, nullable: true),
                    Label = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    IsDefault = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    No = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ConcurrencyStamp = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tbl_CustomerAddress", x => x.Id);
                    table.CheckConstraint("CK_CustomerAddress_Latitude", "\"Latitude\" BETWEEN -90 AND 90");
                    table.CheckConstraint("CK_CustomerAddress_Longitude", "\"Longitude\" BETWEEN -180 AND 180");
                    table.ForeignKey(
                        name: "FK_Tbl_CustomerAddress_Tbl_AdministrativeArea_AdministrativeAr~",
                        column: x => x.AdministrativeAreaId,
                        principalTable: "Tbl_AdministrativeArea",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Tbl_CustomerAddress_Tbl_User_UserId",
                        column: x => x.UserId,
                        principalTable: "Tbl_User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Tbl_Order",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrderNumber = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: true),
                    CustomerEmailSnapshot = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    CustomerPhoneSnapshot = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    RecipientNameSnapshot = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    RecipientPhoneSnapshot = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    AdministrativeAreaId = table.Column<Guid>(type: "uuid", nullable: true),
                    ShippingAddressSnapshot = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    Status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    CurrencyCode = table.Column<string>(type: "character(3)", fixedLength: true, maxLength: 3, nullable: false, defaultValue: "VND"),
                    SubtotalAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    DiscountAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false, defaultValue: 0m),
                    ShippingAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false, defaultValue: 0m),
                    GrandTotalAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    PlacedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    No = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ConcurrencyStamp = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tbl_Order", x => x.Id);
                    table.CheckConstraint("CK_Order_Totals", "\"SubtotalAmount\" >= 0 AND \"DiscountAmount\" >= 0 AND \"ShippingAmount\" >= 0 AND \"GrandTotalAmount\" = \"SubtotalAmount\" - \"DiscountAmount\" + \"ShippingAmount\"");
                    table.ForeignKey(
                        name: "FK_Tbl_Order_Tbl_AdministrativeArea_AdministrativeAreaId",
                        column: x => x.AdministrativeAreaId,
                        principalTable: "Tbl_AdministrativeArea",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Tbl_Order_Tbl_User_UserId",
                        column: x => x.UserId,
                        principalTable: "Tbl_User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Tbl_StockLocation",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    AdministrativeAreaId = table.Column<Guid>(type: "uuid", nullable: true),
                    AddressLine = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    No = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ConcurrencyStamp = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tbl_StockLocation", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Tbl_StockLocation_Tbl_AdministrativeArea_AdministrativeArea~",
                        column: x => x.AdministrativeAreaId,
                        principalTable: "Tbl_AdministrativeArea",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Tbl_Article",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AuthorUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    Title = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    Slug = table.Column<string>(type: "character varying(350)", maxLength: 350, nullable: false),
                    Summary = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    Content = table.Column<string>(type: "text", nullable: false),
                    CoverMediaAssetId = table.Column<Guid>(type: "uuid", nullable: true),
                    Status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    PublishedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    MetaTitle = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    MetaDescription = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    No = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ConcurrencyStamp = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tbl_Article", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Tbl_Article_Tbl_MediaAsset_CoverMediaAssetId",
                        column: x => x.CoverMediaAssetId,
                        principalTable: "Tbl_MediaAsset",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Tbl_Article_Tbl_User_AuthorUserId",
                        column: x => x.AuthorUserId,
                        principalTable: "Tbl_User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Tbl_Banner",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CampaignId = table.Column<Guid>(type: "uuid", nullable: true),
                    MediaAssetId = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    AltText = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    TargetUrl = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    StartsAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    EndsAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    Status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    No = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ConcurrencyStamp = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tbl_Banner", x => x.Id);
                    table.CheckConstraint("CK_Banner_TimeWindow", "\"EndsAt\" IS NULL OR \"StartsAt\" IS NULL OR \"EndsAt\" > \"StartsAt\"");
                    table.ForeignKey(
                        name: "FK_Tbl_Banner_Tbl_Campaign_CampaignId",
                        column: x => x.CampaignId,
                        principalTable: "Tbl_Campaign",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Tbl_Banner_Tbl_MediaAsset_MediaAssetId",
                        column: x => x.MediaAssetId,
                        principalTable: "Tbl_MediaAsset",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Tbl_CertificationEvidence",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CertificationId = table.Column<Guid>(type: "uuid", nullable: false),
                    MediaAssetId = table.Column<Guid>(type: "uuid", nullable: false),
                    EvidenceType = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    No = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ConcurrencyStamp = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tbl_CertificationEvidence", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Tbl_CertificationEvidence_Tbl_Certification_CertificationId",
                        column: x => x.CertificationId,
                        principalTable: "Tbl_Certification",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Tbl_CertificationEvidence_Tbl_MediaAsset_MediaAssetId",
                        column: x => x.MediaAssetId,
                        principalTable: "Tbl_MediaAsset",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Tbl_UserNotification",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    NotificationId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    DeliveryStatus = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    ReadAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeliveredAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    No = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ConcurrencyStamp = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tbl_UserNotification", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Tbl_UserNotification_Tbl_Notification_NotificationId",
                        column: x => x.NotificationId,
                        principalTable: "Tbl_Notification",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Tbl_UserNotification_Tbl_User_UserId",
                        column: x => x.UserId,
                        principalTable: "Tbl_User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Tbl_NavigationItem",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ParentId = table.Column<Guid>(type: "uuid", nullable: true),
                    PageId = table.Column<Guid>(type: "uuid", nullable: true),
                    Label = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    TargetUrl = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    Status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    No = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ConcurrencyStamp = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tbl_NavigationItem", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Tbl_NavigationItem_Tbl_NavigationItem_ParentId",
                        column: x => x.ParentId,
                        principalTable: "Tbl_NavigationItem",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Tbl_NavigationItem_Tbl_Page_PageId",
                        column: x => x.PageId,
                        principalTable: "Tbl_Page",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Tbl_PageSection",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PageId = table.Column<Guid>(type: "uuid", nullable: false),
                    SectionType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Title = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    Content = table.Column<string>(type: "jsonb", nullable: true),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    Status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    No = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ConcurrencyStamp = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tbl_PageSection", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Tbl_PageSection_Tbl_Page_PageId",
                        column: x => x.PageId,
                        principalTable: "Tbl_Page",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Tbl_PointOfSale",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProducerId = table.Column<Guid>(type: "uuid", nullable: true),
                    AdministrativeAreaId = table.Column<Guid>(type: "uuid", nullable: true),
                    Name = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    AddressLine = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    PhoneNumber = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Latitude = table.Column<decimal>(type: "numeric(10,7)", precision: 10, scale: 7, nullable: true),
                    Longitude = table.Column<decimal>(type: "numeric(10,7)", precision: 10, scale: 7, nullable: true),
                    OpeningHours = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    PublicStatus = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    No = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ConcurrencyStamp = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tbl_PointOfSale", x => x.Id);
                    table.CheckConstraint("CK_PointOfSale_Latitude", "\"Latitude\" BETWEEN -90 AND 90");
                    table.CheckConstraint("CK_PointOfSale_Longitude", "\"Longitude\" BETWEEN -180 AND 180");
                    table.ForeignKey(
                        name: "FK_Tbl_PointOfSale_Tbl_AdministrativeArea_AdministrativeAreaId",
                        column: x => x.AdministrativeAreaId,
                        principalTable: "Tbl_AdministrativeArea",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Tbl_PointOfSale_Tbl_Producer_ProducerId",
                        column: x => x.ProducerId,
                        principalTable: "Tbl_Producer",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Tbl_ProducerCertification",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProducerId = table.Column<Guid>(type: "uuid", nullable: false),
                    CertificationId = table.Column<Guid>(type: "uuid", nullable: false),
                    No = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ConcurrencyStamp = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tbl_ProducerCertification", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Tbl_ProducerCertification_Tbl_Certification_CertificationId",
                        column: x => x.CertificationId,
                        principalTable: "Tbl_Certification",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Tbl_ProducerCertification_Tbl_Producer_ProducerId",
                        column: x => x.ProducerId,
                        principalTable: "Tbl_Producer",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Tbl_ProducerContact",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProducerId = table.Column<Guid>(type: "uuid", nullable: false),
                    ContactType = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    ContactValue = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    ContactName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    IsPublic = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    No = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ConcurrencyStamp = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tbl_ProducerContact", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Tbl_ProducerContact_Tbl_Producer_ProducerId",
                        column: x => x.ProducerId,
                        principalTable: "Tbl_Producer",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Tbl_Product",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProducerId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    Slug = table.Column<string>(type: "character varying(350)", maxLength: 350, nullable: false),
                    ShortDescription = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    Description = table.Column<string>(type: "text", nullable: true),
                    UsageInstructions = table.Column<string>(type: "text", nullable: true),
                    StorageInstructions = table.Column<string>(type: "text", nullable: true),
                    WarningText = table.Column<string>(type: "text", nullable: true),
                    Status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    PublishedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UnpublishedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    MetaTitle = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    MetaDescription = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    No = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ConcurrencyStamp = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tbl_Product", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Tbl_Product_Tbl_Producer_ProducerId",
                        column: x => x.ProducerId,
                        principalTable: "Tbl_Producer",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Tbl_ProductionFacility",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProducerId = table.Column<Guid>(type: "uuid", nullable: false),
                    AdministrativeAreaId = table.Column<Guid>(type: "uuid", nullable: true),
                    Name = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    AddressLine = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Latitude = table.Column<decimal>(type: "numeric(10,7)", precision: 10, scale: 7, nullable: true),
                    Longitude = table.Column<decimal>(type: "numeric(10,7)", precision: 10, scale: 7, nullable: true),
                    PublicStatus = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    No = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ConcurrencyStamp = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tbl_ProductionFacility", x => x.Id);
                    table.CheckConstraint("CK_ProductionFacility_Latitude", "\"Latitude\" BETWEEN -90 AND 90");
                    table.CheckConstraint("CK_ProductionFacility_Longitude", "\"Longitude\" BETWEEN -180 AND 180");
                    table.ForeignKey(
                        name: "FK_Tbl_ProductionFacility_Tbl_AdministrativeArea_Administrativ~",
                        column: x => x.AdministrativeAreaId,
                        principalTable: "Tbl_AdministrativeArea",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Tbl_ProductionFacility_Tbl_Producer_ProducerId",
                        column: x => x.ProducerId,
                        principalTable: "Tbl_Producer",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Tbl_Coupon",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PromotionId = table.Column<Guid>(type: "uuid", nullable: true),
                    Code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    UsageLimit = table.Column<int>(type: "integer", nullable: true),
                    PerUserLimit = table.Column<int>(type: "integer", nullable: true),
                    StartsAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndsAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    No = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ConcurrencyStamp = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tbl_Coupon", x => x.Id);
                    table.CheckConstraint("CK_Coupon_PerUserLimit", "\"PerUserLimit\" IS NULL OR \"PerUserLimit\" >= 0");
                    table.CheckConstraint("CK_Coupon_TimeWindow", "\"EndsAt\" IS NULL OR \"EndsAt\" > \"StartsAt\"");
                    table.CheckConstraint("CK_Coupon_UsageLimit", "\"UsageLimit\" IS NULL OR \"UsageLimit\" >= 0");
                    table.ForeignKey(
                        name: "FK_Tbl_Coupon_Tbl_Promotion_PromotionId",
                        column: x => x.PromotionId,
                        principalTable: "Tbl_Promotion",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Tbl_InquiryAttachment",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TradeInquiryId = table.Column<Guid>(type: "uuid", nullable: true),
                    PartnerApplicationId = table.Column<Guid>(type: "uuid", nullable: true),
                    MediaAssetId = table.Column<Guid>(type: "uuid", nullable: false),
                    Visibility = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "Internal"),
                    No = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ConcurrencyStamp = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tbl_InquiryAttachment", x => x.Id);
                    table.CheckConstraint("CK_InquiryAttachment_Parent", "(\"TradeInquiryId\" IS NULL) <> (\"PartnerApplicationId\" IS NULL)");
                    table.ForeignKey(
                        name: "FK_Tbl_InquiryAttachment_Tbl_MediaAsset_MediaAssetId",
                        column: x => x.MediaAssetId,
                        principalTable: "Tbl_MediaAsset",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Tbl_InquiryAttachment_Tbl_PartnerApplication_PartnerApplica~",
                        column: x => x.PartnerApplicationId,
                        principalTable: "Tbl_PartnerApplication",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Tbl_InquiryAttachment_Tbl_TradeInquiry_TradeInquiryId",
                        column: x => x.TradeInquiryId,
                        principalTable: "Tbl_TradeInquiry",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Tbl_TradeInquiryStatusHistory",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TradeInquiryId = table.Column<Guid>(type: "uuid", nullable: false),
                    FromStatus = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    ToStatus = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Reason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    ChangedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    ChangedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    No = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ConcurrencyStamp = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tbl_TradeInquiryStatusHistory", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Tbl_TradeInquiryStatusHistory_Tbl_TradeInquiry_TradeInquiry~",
                        column: x => x.TradeInquiryId,
                        principalTable: "Tbl_TradeInquiry",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Tbl_OrderNote",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrderId = table.Column<Guid>(type: "uuid", nullable: false),
                    AuthorUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    NoteType = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Content = table.Column<string>(type: "text", nullable: false),
                    IsVisibleToCustomer = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    No = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ConcurrencyStamp = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tbl_OrderNote", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Tbl_OrderNote_Tbl_Order_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Tbl_Order",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Tbl_OrderStatusHistory",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrderId = table.Column<Guid>(type: "uuid", nullable: false),
                    FromStatus = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    ToStatus = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Reason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    ChangedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    ChangedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    No = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ConcurrencyStamp = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tbl_OrderStatusHistory", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Tbl_OrderStatusHistory_Tbl_Order_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Tbl_Order",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Tbl_Payment",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrderId = table.Column<Guid>(type: "uuid", nullable: false),
                    Method = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    DueAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    PaidAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    No = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ConcurrencyStamp = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tbl_Payment", x => x.Id);
                    table.CheckConstraint("CK_Payment_Amount", "\"Amount\" >= 0");
                    table.ForeignKey(
                        name: "FK_Tbl_Payment_Tbl_Order_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Tbl_Order",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Tbl_Shipment",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrderId = table.Column<Guid>(type: "uuid", nullable: false),
                    ShippingMethod = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CarrierName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    TrackingCode = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    ShippedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeliveredAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    No = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ConcurrencyStamp = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tbl_Shipment", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Tbl_Shipment_Tbl_Order_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Tbl_Order",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Tbl_ArticleCategoryMap",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ArticleId = table.Column<Guid>(type: "uuid", nullable: false),
                    ArticleCategoryId = table.Column<Guid>(type: "uuid", nullable: false),
                    No = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ConcurrencyStamp = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tbl_ArticleCategoryMap", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Tbl_ArticleCategoryMap_Tbl_ArticleCategory_ArticleCategoryId",
                        column: x => x.ArticleCategoryId,
                        principalTable: "Tbl_ArticleCategory",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Tbl_ArticleCategoryMap_Tbl_Article_ArticleId",
                        column: x => x.ArticleId,
                        principalTable: "Tbl_Article",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Tbl_AnalyticsEvent",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    VisitorSessionId = table.Column<Guid>(type: "uuid", nullable: true),
                    ProductId = table.Column<Guid>(type: "uuid", nullable: true),
                    CampaignId = table.Column<Guid>(type: "uuid", nullable: true),
                    EventType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Path = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    SearchTerm = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    OccurredAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    No = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ConcurrencyStamp = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tbl_AnalyticsEvent", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Tbl_AnalyticsEvent_Tbl_Campaign_CampaignId",
                        column: x => x.CampaignId,
                        principalTable: "Tbl_Campaign",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Tbl_AnalyticsEvent_Tbl_Product_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Tbl_Product",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Tbl_AnalyticsEvent_Tbl_VisitorSession_VisitorSessionId",
                        column: x => x.VisitorSessionId,
                        principalTable: "Tbl_VisitorSession",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Tbl_PageSectionProduct",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PageSectionId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductId = table.Column<Guid>(type: "uuid", nullable: false),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    No = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ConcurrencyStamp = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tbl_PageSectionProduct", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Tbl_PageSectionProduct_Tbl_PageSection_PageSectionId",
                        column: x => x.PageSectionId,
                        principalTable: "Tbl_PageSection",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Tbl_PageSectionProduct_Tbl_Product_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Tbl_Product",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Tbl_PointOfSaleProduct",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PointOfSaleId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsAvailable = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    No = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ConcurrencyStamp = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tbl_PointOfSaleProduct", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Tbl_PointOfSaleProduct_Tbl_PointOfSale_PointOfSaleId",
                        column: x => x.PointOfSaleId,
                        principalTable: "Tbl_PointOfSale",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Tbl_PointOfSaleProduct_Tbl_Product_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Tbl_Product",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Tbl_ProductCategory",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductId = table.Column<Guid>(type: "uuid", nullable: false),
                    CategoryId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsPrimary = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    No = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ConcurrencyStamp = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tbl_ProductCategory", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Tbl_ProductCategory_Tbl_Category_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Tbl_Category",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Tbl_ProductCategory_Tbl_Product_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Tbl_Product",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Tbl_ProductCertification",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductId = table.Column<Guid>(type: "uuid", nullable: false),
                    CertificationId = table.Column<Guid>(type: "uuid", nullable: false),
                    No = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ConcurrencyStamp = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tbl_ProductCertification", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Tbl_ProductCertification_Tbl_Certification_CertificationId",
                        column: x => x.CertificationId,
                        principalTable: "Tbl_Certification",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Tbl_ProductCertification_Tbl_Product_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Tbl_Product",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Tbl_ProductMedia",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductId = table.Column<Guid>(type: "uuid", nullable: false),
                    MediaAssetId = table.Column<Guid>(type: "uuid", nullable: false),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    IsPrimary = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    Caption = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    No = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ConcurrencyStamp = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tbl_ProductMedia", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Tbl_ProductMedia_Tbl_MediaAsset_MediaAssetId",
                        column: x => x.MediaAssetId,
                        principalTable: "Tbl_MediaAsset",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Tbl_ProductMedia_Tbl_Product_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Tbl_Product",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Tbl_ProductOption",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductId = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    No = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ConcurrencyStamp = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tbl_ProductOption", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Tbl_ProductOption_Tbl_Product_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Tbl_Product",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Tbl_ProductQuestion",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: true),
                    GuestName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    GuestEmail = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    Content = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    AskedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    No = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ConcurrencyStamp = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tbl_ProductQuestion", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Tbl_ProductQuestion_Tbl_Product_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Tbl_Product",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Tbl_ProductQuestion_Tbl_User_UserId",
                        column: x => x.UserId,
                        principalTable: "Tbl_User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Tbl_ProductSlugHistory",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductId = table.Column<Guid>(type: "uuid", nullable: false),
                    Slug = table.Column<string>(type: "character varying(350)", maxLength: 350, nullable: false),
                    RedirectStatusCode = table.Column<int>(type: "integer", nullable: false, defaultValue: 301),
                    No = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ConcurrencyStamp = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tbl_ProductSlugHistory", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Tbl_ProductSlugHistory_Tbl_Product_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Tbl_Product",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Tbl_ProductVariant",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductId = table.Column<Guid>(type: "uuid", nullable: false),
                    Sku = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Name = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    Barcode = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    InventoryMode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    AllowBackorder = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    WeightGrams = table.Column<decimal>(type: "numeric(12,3)", precision: 12, scale: 3, nullable: true),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    No = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ConcurrencyStamp = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tbl_ProductVariant", x => x.Id);
                    table.CheckConstraint("CK_ProductVariant_WeightGrams", "\"WeightGrams\" IS NULL OR \"WeightGrams\" >= 0");
                    table.ForeignKey(
                        name: "FK_Tbl_ProductVariant_Tbl_Product_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Tbl_Product",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Tbl_TraceProfile",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductId = table.Column<Guid>(type: "uuid", nullable: false),
                    PublicCode = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Summary = table.Column<string>(type: "text", nullable: true),
                    PublicStatus = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    No = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ConcurrencyStamp = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tbl_TraceProfile", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Tbl_TraceProfile_Tbl_Product_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Tbl_Product",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Tbl_WishlistItem",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    WishlistId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductId = table.Column<Guid>(type: "uuid", nullable: false),
                    No = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ConcurrencyStamp = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tbl_WishlistItem", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Tbl_WishlistItem_Tbl_Product_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Tbl_Product",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Tbl_WishlistItem_Tbl_Wishlist_WishlistId",
                        column: x => x.WishlistId,
                        principalTable: "Tbl_Wishlist",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Tbl_FacilityCertification",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductionFacilityId = table.Column<Guid>(type: "uuid", nullable: false),
                    CertificationId = table.Column<Guid>(type: "uuid", nullable: false),
                    No = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ConcurrencyStamp = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tbl_FacilityCertification", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Tbl_FacilityCertification_Tbl_Certification_CertificationId",
                        column: x => x.CertificationId,
                        principalTable: "Tbl_Certification",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Tbl_FacilityCertification_Tbl_ProductionFacility_Production~",
                        column: x => x.ProductionFacilityId,
                        principalTable: "Tbl_ProductionFacility",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Tbl_CouponCategory",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CouponId = table.Column<Guid>(type: "uuid", nullable: false),
                    CategoryId = table.Column<Guid>(type: "uuid", nullable: false),
                    No = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ConcurrencyStamp = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tbl_CouponCategory", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Tbl_CouponCategory_Tbl_Category_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Tbl_Category",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Tbl_CouponCategory_Tbl_Coupon_CouponId",
                        column: x => x.CouponId,
                        principalTable: "Tbl_Coupon",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Tbl_CouponProduct",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CouponId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductId = table.Column<Guid>(type: "uuid", nullable: false),
                    No = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ConcurrencyStamp = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tbl_CouponProduct", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Tbl_CouponProduct_Tbl_Coupon_CouponId",
                        column: x => x.CouponId,
                        principalTable: "Tbl_Coupon",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Tbl_CouponProduct_Tbl_Product_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Tbl_Product",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Tbl_CouponRedemption",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CouponId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: true),
                    OrderId = table.Column<Guid>(type: "uuid", nullable: false),
                    RedeemedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DiscountAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    No = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ConcurrencyStamp = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tbl_CouponRedemption", x => x.Id);
                    table.CheckConstraint("CK_CouponRedemption_DiscountAmount", "\"DiscountAmount\" >= 0");
                    table.ForeignKey(
                        name: "FK_Tbl_CouponRedemption_Tbl_Coupon_CouponId",
                        column: x => x.CouponId,
                        principalTable: "Tbl_Coupon",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Tbl_CouponRedemption_Tbl_Order_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Tbl_Order",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Tbl_CouponRedemption_Tbl_User_UserId",
                        column: x => x.UserId,
                        principalTable: "Tbl_User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Tbl_PaymentTransaction",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PaymentId = table.Column<Guid>(type: "uuid", nullable: false),
                    Provider = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ProviderReference = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    TransactionType = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    OccurredAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ProofMediaAssetId = table.Column<Guid>(type: "uuid", nullable: true),
                    No = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ConcurrencyStamp = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tbl_PaymentTransaction", x => x.Id);
                    table.CheckConstraint("CK_PaymentTransaction_Amount", "\"Amount\" >= 0");
                    table.ForeignKey(
                        name: "FK_Tbl_PaymentTransaction_Tbl_MediaAsset_ProofMediaAssetId",
                        column: x => x.ProofMediaAssetId,
                        principalTable: "Tbl_MediaAsset",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Tbl_PaymentTransaction_Tbl_Payment_PaymentId",
                        column: x => x.PaymentId,
                        principalTable: "Tbl_Payment",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Tbl_ShipmentHistory",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ShipmentId = table.Column<Guid>(type: "uuid", nullable: false),
                    FromStatus = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    ToStatus = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Reason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    ChangedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    OccurredAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    No = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ConcurrencyStamp = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tbl_ShipmentHistory", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Tbl_ShipmentHistory_Tbl_Shipment_ShipmentId",
                        column: x => x.ShipmentId,
                        principalTable: "Tbl_Shipment",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Tbl_ProductOptionValue",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductOptionId = table.Column<Guid>(type: "uuid", nullable: false),
                    Value = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    No = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ConcurrencyStamp = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tbl_ProductOptionValue", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Tbl_ProductOptionValue_Tbl_ProductOption_ProductOptionId",
                        column: x => x.ProductOptionId,
                        principalTable: "Tbl_ProductOption",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Tbl_ProductAnswer",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductQuestionId = table.Column<Guid>(type: "uuid", nullable: false),
                    AnsweredByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    Content = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    AnsweredAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    No = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ConcurrencyStamp = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tbl_ProductAnswer", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Tbl_ProductAnswer_Tbl_ProductQuestion_ProductQuestionId",
                        column: x => x.ProductQuestionId,
                        principalTable: "Tbl_ProductQuestion",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Tbl_ProductAnswer_Tbl_User_AnsweredByUserId",
                        column: x => x.AnsweredByUserId,
                        principalTable: "Tbl_User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Tbl_CartItem",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CartId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductVariantId = table.Column<Guid>(type: "uuid", nullable: false),
                    Quantity = table.Column<int>(type: "integer", nullable: false),
                    No = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ConcurrencyStamp = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tbl_CartItem", x => x.Id);
                    table.CheckConstraint("CK_CartItem_Quantity", "\"Quantity\" > 0");
                    table.ForeignKey(
                        name: "FK_Tbl_CartItem_Tbl_Cart_CartId",
                        column: x => x.CartId,
                        principalTable: "Tbl_Cart",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Tbl_CartItem_Tbl_ProductVariant_ProductVariantId",
                        column: x => x.ProductVariantId,
                        principalTable: "Tbl_ProductVariant",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Tbl_InventoryItem",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductVariantId = table.Column<Guid>(type: "uuid", nullable: false),
                    RequiresShipping = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    No = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ConcurrencyStamp = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tbl_InventoryItem", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Tbl_InventoryItem_Tbl_ProductVariant_ProductVariantId",
                        column: x => x.ProductVariantId,
                        principalTable: "Tbl_ProductVariant",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Tbl_OrderItem",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrderId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductVariantId = table.Column<Guid>(type: "uuid", nullable: true),
                    ProductNameSnapshot = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    VariantNameSnapshot = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    SkuSnapshot = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    UnitPriceSnapshot = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Quantity = table.Column<int>(type: "integer", nullable: false),
                    DiscountAmountSnapshot = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false, defaultValue: 0m),
                    LineTotalAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    No = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ConcurrencyStamp = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tbl_OrderItem", x => x.Id);
                    table.CheckConstraint("CK_OrderItem_Amounts", "\"UnitPriceSnapshot\" >= 0 AND \"DiscountAmountSnapshot\" >= 0 AND \"LineTotalAmount\" >= 0");
                    table.CheckConstraint("CK_OrderItem_Quantity", "\"Quantity\" > 0");
                    table.ForeignKey(
                        name: "FK_Tbl_OrderItem_Tbl_Order_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Tbl_Order",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Tbl_OrderItem_Tbl_ProductVariant_ProductVariantId",
                        column: x => x.ProductVariantId,
                        principalTable: "Tbl_ProductVariant",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Tbl_TradeInquiryItem",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TradeInquiryId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductId = table.Column<Guid>(type: "uuid", nullable: true),
                    ProductVariantId = table.Column<Guid>(type: "uuid", nullable: true),
                    RequestedQuantity = table.Column<decimal>(type: "numeric(18,3)", precision: 18, scale: 3, nullable: true),
                    RequirementText = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    No = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ConcurrencyStamp = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tbl_TradeInquiryItem", x => x.Id);
                    table.CheckConstraint("CK_TradeInquiryItem_Quantity", "\"RequestedQuantity\" IS NULL OR \"RequestedQuantity\" > 0");
                    table.ForeignKey(
                        name: "FK_Tbl_TradeInquiryItem_Tbl_ProductVariant_ProductVariantId",
                        column: x => x.ProductVariantId,
                        principalTable: "Tbl_ProductVariant",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Tbl_TradeInquiryItem_Tbl_Product_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Tbl_Product",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Tbl_TradeInquiryItem_Tbl_TradeInquiry_TradeInquiryId",
                        column: x => x.TradeInquiryId,
                        principalTable: "Tbl_TradeInquiry",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Tbl_VariantPrice",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductVariantId = table.Column<Guid>(type: "uuid", nullable: false),
                    PriceListId = table.Column<Guid>(type: "uuid", nullable: true),
                    CurrencyCode = table.Column<string>(type: "character(3)", fixedLength: true, maxLength: 3, nullable: false, defaultValue: "VND"),
                    Amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    MinQuantity = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    PriceType = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    EffectiveFrom = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EffectiveTo = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    No = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ConcurrencyStamp = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tbl_VariantPrice", x => x.Id);
                    table.CheckConstraint("CK_VariantPrice_Amount", "\"Amount\" >= 0");
                    table.CheckConstraint("CK_VariantPrice_MinQuantity", "\"MinQuantity\" > 0");
                    table.CheckConstraint("CK_VariantPrice_TimeWindow", "\"EffectiveTo\" IS NULL OR \"EffectiveTo\" > \"EffectiveFrom\"");
                    table.ForeignKey(
                        name: "FK_Tbl_VariantPrice_Tbl_PriceList_PriceListId",
                        column: x => x.PriceListId,
                        principalTable: "Tbl_PriceList",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Tbl_VariantPrice_Tbl_ProductVariant_ProductVariantId",
                        column: x => x.ProductVariantId,
                        principalTable: "Tbl_ProductVariant",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Tbl_TraceLot",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TraceProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                    LotCode = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ProducedAt = table.Column<DateOnly>(type: "date", nullable: true),
                    ExpiresAt = table.Column<DateOnly>(type: "date", nullable: true),
                    Status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    No = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ConcurrencyStamp = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tbl_TraceLot", x => x.Id);
                    table.CheckConstraint("CK_TraceLot_TimeWindow", "\"ExpiresAt\" IS NULL OR \"ProducedAt\" IS NULL OR \"ExpiresAt\" >= \"ProducedAt\"");
                    table.ForeignKey(
                        name: "FK_Tbl_TraceLot_Tbl_TraceProfile_TraceProfileId",
                        column: x => x.TraceProfileId,
                        principalTable: "Tbl_TraceProfile",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Tbl_ProductVariantOptionValue",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductVariantId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductOptionValueId = table.Column<Guid>(type: "uuid", nullable: false),
                    No = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ConcurrencyStamp = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tbl_ProductVariantOptionValue", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Tbl_ProductVariantOptionValue_Tbl_ProductOptionValue_Produc~",
                        column: x => x.ProductOptionValueId,
                        principalTable: "Tbl_ProductOptionValue",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Tbl_ProductVariantOptionValue_Tbl_ProductVariant_ProductVar~",
                        column: x => x.ProductVariantId,
                        principalTable: "Tbl_ProductVariant",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Tbl_InventoryLevel",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    InventoryItemId = table.Column<Guid>(type: "uuid", nullable: false),
                    StockLocationId = table.Column<Guid>(type: "uuid", nullable: false),
                    StockedQuantity = table.Column<decimal>(type: "numeric(18,3)", precision: 18, scale: 3, nullable: false, defaultValue: 0m),
                    ReservedQuantity = table.Column<decimal>(type: "numeric(18,3)", precision: 18, scale: 3, nullable: false, defaultValue: 0m),
                    IncomingQuantity = table.Column<decimal>(type: "numeric(18,3)", precision: 18, scale: 3, nullable: false, defaultValue: 0m),
                    No = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ConcurrencyStamp = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tbl_InventoryLevel", x => x.Id);
                    table.CheckConstraint("CK_InventoryLevel_Quantities", "\"StockedQuantity\" >= 0 AND \"ReservedQuantity\" >= 0 AND \"IncomingQuantity\" >= 0 AND \"ReservedQuantity\" <= \"StockedQuantity\"");
                    table.ForeignKey(
                        name: "FK_Tbl_InventoryLevel_Tbl_InventoryItem_InventoryItemId",
                        column: x => x.InventoryItemId,
                        principalTable: "Tbl_InventoryItem",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Tbl_InventoryLevel_Tbl_StockLocation_StockLocationId",
                        column: x => x.StockLocationId,
                        principalTable: "Tbl_StockLocation",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Tbl_InventoryMovement",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    InventoryItemId = table.Column<Guid>(type: "uuid", nullable: false),
                    StockLocationId = table.Column<Guid>(type: "uuid", nullable: false),
                    OrderItemId = table.Column<Guid>(type: "uuid", nullable: true),
                    MovementType = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    QuantityDelta = table.Column<decimal>(type: "numeric(18,3)", precision: 18, scale: 3, nullable: false),
                    Reason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    OccurredAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    No = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ConcurrencyStamp = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tbl_InventoryMovement", x => x.Id);
                    table.CheckConstraint("CK_InventoryMovement_QuantityDelta", "\"QuantityDelta\" <> 0");
                    table.ForeignKey(
                        name: "FK_Tbl_InventoryMovement_Tbl_InventoryItem_InventoryItemId",
                        column: x => x.InventoryItemId,
                        principalTable: "Tbl_InventoryItem",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Tbl_InventoryMovement_Tbl_OrderItem_OrderItemId",
                        column: x => x.OrderItemId,
                        principalTable: "Tbl_OrderItem",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Tbl_InventoryMovement_Tbl_StockLocation_StockLocationId",
                        column: x => x.StockLocationId,
                        principalTable: "Tbl_StockLocation",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Tbl_InventoryReservation",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    InventoryItemId = table.Column<Guid>(type: "uuid", nullable: false),
                    StockLocationId = table.Column<Guid>(type: "uuid", nullable: false),
                    OrderItemId = table.Column<Guid>(type: "uuid", nullable: false),
                    Quantity = table.Column<decimal>(type: "numeric(18,3)", precision: 18, scale: 3, nullable: false),
                    Status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ReleasedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    No = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ConcurrencyStamp = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tbl_InventoryReservation", x => x.Id);
                    table.CheckConstraint("CK_InventoryReservation_Quantity", "\"Quantity\" > 0");
                    table.ForeignKey(
                        name: "FK_Tbl_InventoryReservation_Tbl_InventoryItem_InventoryItemId",
                        column: x => x.InventoryItemId,
                        principalTable: "Tbl_InventoryItem",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Tbl_InventoryReservation_Tbl_OrderItem_OrderItemId",
                        column: x => x.OrderItemId,
                        principalTable: "Tbl_OrderItem",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Tbl_InventoryReservation_Tbl_StockLocation_StockLocationId",
                        column: x => x.StockLocationId,
                        principalTable: "Tbl_StockLocation",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Tbl_OrderDiscount",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrderId = table.Column<Guid>(type: "uuid", nullable: false),
                    OrderItemId = table.Column<Guid>(type: "uuid", nullable: true),
                    PromotionId = table.Column<Guid>(type: "uuid", nullable: true),
                    CouponId = table.Column<Guid>(type: "uuid", nullable: true),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    DiscountAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    No = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ConcurrencyStamp = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tbl_OrderDiscount", x => x.Id);
                    table.CheckConstraint("CK_OrderDiscount_Amount", "\"DiscountAmount\" >= 0");
                    table.ForeignKey(
                        name: "FK_Tbl_OrderDiscount_Tbl_Coupon_CouponId",
                        column: x => x.CouponId,
                        principalTable: "Tbl_Coupon",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Tbl_OrderDiscount_Tbl_OrderItem_OrderItemId",
                        column: x => x.OrderItemId,
                        principalTable: "Tbl_OrderItem",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Tbl_OrderDiscount_Tbl_Order_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Tbl_Order",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Tbl_OrderDiscount_Tbl_Promotion_PromotionId",
                        column: x => x.PromotionId,
                        principalTable: "Tbl_Promotion",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Tbl_ProductReview",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    OrderItemId = table.Column<Guid>(type: "uuid", nullable: true),
                    Rating = table.Column<int>(type: "integer", nullable: false),
                    Title = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    Content = table.Column<string>(type: "text", nullable: true),
                    ModerationStatus = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    ReviewedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    No = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ConcurrencyStamp = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tbl_ProductReview", x => x.Id);
                    table.CheckConstraint("CK_ProductReview_Rating", "\"Rating\" BETWEEN 1 AND 5");
                    table.ForeignKey(
                        name: "FK_Tbl_ProductReview_Tbl_OrderItem_OrderItemId",
                        column: x => x.OrderItemId,
                        principalTable: "Tbl_OrderItem",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Tbl_ProductReview_Tbl_Product_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Tbl_Product",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Tbl_ProductReview_Tbl_User_UserId",
                        column: x => x.UserId,
                        principalTable: "Tbl_User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Tbl_ShipmentItem",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ShipmentId = table.Column<Guid>(type: "uuid", nullable: false),
                    OrderItemId = table.Column<Guid>(type: "uuid", nullable: false),
                    Quantity = table.Column<int>(type: "integer", nullable: false),
                    No = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ConcurrencyStamp = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tbl_ShipmentItem", x => x.Id);
                    table.CheckConstraint("CK_ShipmentItem_Quantity", "\"Quantity\" > 0");
                    table.ForeignKey(
                        name: "FK_Tbl_ShipmentItem_Tbl_OrderItem_OrderItemId",
                        column: x => x.OrderItemId,
                        principalTable: "Tbl_OrderItem",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Tbl_ShipmentItem_Tbl_Shipment_ShipmentId",
                        column: x => x.ShipmentId,
                        principalTable: "Tbl_Shipment",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Tbl_TraceEvent",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TraceLotId = table.Column<Guid>(type: "uuid", nullable: false),
                    EventType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    OccurredAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LocationText = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Description = table.Column<string>(type: "text", nullable: true),
                    VerifiedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    No = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ConcurrencyStamp = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tbl_TraceEvent", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Tbl_TraceEvent_Tbl_TraceLot_TraceLotId",
                        column: x => x.TraceLotId,
                        principalTable: "Tbl_TraceLot",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Tbl_ProductReviewMedia",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductReviewId = table.Column<Guid>(type: "uuid", nullable: false),
                    MediaAssetId = table.Column<Guid>(type: "uuid", nullable: false),
                    No = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ConcurrencyStamp = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tbl_ProductReviewMedia", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Tbl_ProductReviewMedia_Tbl_MediaAsset_MediaAssetId",
                        column: x => x.MediaAssetId,
                        principalTable: "Tbl_MediaAsset",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Tbl_ProductReviewMedia_Tbl_ProductReview_ProductReviewId",
                        column: x => x.ProductReviewId,
                        principalTable: "Tbl_ProductReview",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Tbl_TraceEventEvidence",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TraceEventId = table.Column<Guid>(type: "uuid", nullable: false),
                    MediaAssetId = table.Column<Guid>(type: "uuid", nullable: false),
                    No = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ConcurrencyStamp = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tbl_TraceEventEvidence", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Tbl_TraceEventEvidence_Tbl_MediaAsset_MediaAssetId",
                        column: x => x.MediaAssetId,
                        principalTable: "Tbl_MediaAsset",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Tbl_TraceEventEvidence_Tbl_TraceEvent_TraceEventId",
                        column: x => x.TraceEventId,
                        principalTable: "Tbl_TraceEvent",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_AdministrativeArea_Code",
                table: "Tbl_AdministrativeArea",
                column: "Code",
                unique: true,
                filter: "\"IsDeleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_AdministrativeArea_CreatedAt",
                table: "Tbl_AdministrativeArea",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_AdministrativeArea_IsDeleted",
                table: "Tbl_AdministrativeArea",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_AdministrativeArea_No",
                table: "Tbl_AdministrativeArea",
                column: "No");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_AdministrativeArea_ParentId",
                table: "Tbl_AdministrativeArea",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_AnalyticsEvent_CampaignId",
                table: "Tbl_AnalyticsEvent",
                column: "CampaignId");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_AnalyticsEvent_CreatedAt",
                table: "Tbl_AnalyticsEvent",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_AnalyticsEvent_IsDeleted",
                table: "Tbl_AnalyticsEvent",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_AnalyticsEvent_No",
                table: "Tbl_AnalyticsEvent",
                column: "No");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_AnalyticsEvent_ProductId",
                table: "Tbl_AnalyticsEvent",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_AnalyticsEvent_VisitorSessionId",
                table: "Tbl_AnalyticsEvent",
                column: "VisitorSessionId");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_Article_AuthorUserId",
                table: "Tbl_Article",
                column: "AuthorUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_Article_CoverMediaAssetId",
                table: "Tbl_Article",
                column: "CoverMediaAssetId");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_Article_CreatedAt",
                table: "Tbl_Article",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_Article_IsDeleted",
                table: "Tbl_Article",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_Article_No",
                table: "Tbl_Article",
                column: "No");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_Article_Slug",
                table: "Tbl_Article",
                column: "Slug",
                unique: true,
                filter: "\"IsDeleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_ArticleCategory_CreatedAt",
                table: "Tbl_ArticleCategory",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_ArticleCategory_IsDeleted",
                table: "Tbl_ArticleCategory",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_ArticleCategory_No",
                table: "Tbl_ArticleCategory",
                column: "No");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_ArticleCategory_Slug",
                table: "Tbl_ArticleCategory",
                column: "Slug",
                unique: true,
                filter: "\"IsDeleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_ArticleCategoryMap_ArticleCategoryId",
                table: "Tbl_ArticleCategoryMap",
                column: "ArticleCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_ArticleCategoryMap_ArticleId_ArticleCategoryId",
                table: "Tbl_ArticleCategoryMap",
                columns: new[] { "ArticleId", "ArticleCategoryId" },
                unique: true,
                filter: "\"IsDeleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_ArticleCategoryMap_CreatedAt",
                table: "Tbl_ArticleCategoryMap",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_ArticleCategoryMap_IsDeleted",
                table: "Tbl_ArticleCategoryMap",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_ArticleCategoryMap_No",
                table: "Tbl_ArticleCategoryMap",
                column: "No");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_AuditLog_CreatedAt",
                table: "Tbl_AuditLog",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_AuditLog_IsDeleted",
                table: "Tbl_AuditLog",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_AuditLog_No",
                table: "Tbl_AuditLog",
                column: "No");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_Banner_CampaignId",
                table: "Tbl_Banner",
                column: "CampaignId");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_Banner_CreatedAt",
                table: "Tbl_Banner",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_Banner_IsDeleted",
                table: "Tbl_Banner",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_Banner_MediaAssetId",
                table: "Tbl_Banner",
                column: "MediaAssetId");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_Banner_No",
                table: "Tbl_Banner",
                column: "No");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_Campaign_Code",
                table: "Tbl_Campaign",
                column: "Code",
                unique: true,
                filter: "\"IsDeleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_Campaign_CreatedAt",
                table: "Tbl_Campaign",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_Campaign_IsDeleted",
                table: "Tbl_Campaign",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_Campaign_No",
                table: "Tbl_Campaign",
                column: "No");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_Cart_CreatedAt",
                table: "Tbl_Cart",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_Cart_GuestTokenHash",
                table: "Tbl_Cart",
                column: "GuestTokenHash",
                unique: true,
                filter: "\"IsDeleted\" = false AND \"Status\" = 'Active' AND \"GuestTokenHash\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_Cart_IsDeleted",
                table: "Tbl_Cart",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_Cart_No",
                table: "Tbl_Cart",
                column: "No");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_Cart_UserId",
                table: "Tbl_Cart",
                column: "UserId",
                unique: true,
                filter: "\"IsDeleted\" = false AND \"Status\" = 'Active' AND \"UserId\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_CartItem_CartId_ProductVariantId",
                table: "Tbl_CartItem",
                columns: new[] { "CartId", "ProductVariantId" },
                unique: true,
                filter: "\"IsDeleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_CartItem_CreatedAt",
                table: "Tbl_CartItem",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_CartItem_IsDeleted",
                table: "Tbl_CartItem",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_CartItem_No",
                table: "Tbl_CartItem",
                column: "No");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_CartItem_ProductVariantId",
                table: "Tbl_CartItem",
                column: "ProductVariantId");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_Category_CreatedAt",
                table: "Tbl_Category",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_Category_IsDeleted",
                table: "Tbl_Category",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_Category_No",
                table: "Tbl_Category",
                column: "No");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_Category_ParentId",
                table: "Tbl_Category",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_Category_Slug",
                table: "Tbl_Category",
                column: "Slug",
                unique: true,
                filter: "\"IsDeleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_Certification_CertificationType_CertificateNumber",
                table: "Tbl_Certification",
                columns: new[] { "CertificationType", "CertificateNumber" },
                unique: true,
                filter: "\"IsDeleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_Certification_CreatedAt",
                table: "Tbl_Certification",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_Certification_IsDeleted",
                table: "Tbl_Certification",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_Certification_No",
                table: "Tbl_Certification",
                column: "No");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_CertificationEvidence_CertificationId_MediaAssetId",
                table: "Tbl_CertificationEvidence",
                columns: new[] { "CertificationId", "MediaAssetId" },
                unique: true,
                filter: "\"IsDeleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_CertificationEvidence_CreatedAt",
                table: "Tbl_CertificationEvidence",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_CertificationEvidence_IsDeleted",
                table: "Tbl_CertificationEvidence",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_CertificationEvidence_MediaAssetId",
                table: "Tbl_CertificationEvidence",
                column: "MediaAssetId");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_CertificationEvidence_No",
                table: "Tbl_CertificationEvidence",
                column: "No");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_Coupon_Code",
                table: "Tbl_Coupon",
                column: "Code",
                unique: true,
                filter: "\"IsDeleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_Coupon_CreatedAt",
                table: "Tbl_Coupon",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_Coupon_IsDeleted",
                table: "Tbl_Coupon",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_Coupon_No",
                table: "Tbl_Coupon",
                column: "No");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_Coupon_PromotionId",
                table: "Tbl_Coupon",
                column: "PromotionId");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_CouponCategory_CategoryId",
                table: "Tbl_CouponCategory",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_CouponCategory_CouponId_CategoryId",
                table: "Tbl_CouponCategory",
                columns: new[] { "CouponId", "CategoryId" },
                unique: true,
                filter: "\"IsDeleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_CouponCategory_CreatedAt",
                table: "Tbl_CouponCategory",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_CouponCategory_IsDeleted",
                table: "Tbl_CouponCategory",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_CouponCategory_No",
                table: "Tbl_CouponCategory",
                column: "No");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_CouponProduct_CouponId_ProductId",
                table: "Tbl_CouponProduct",
                columns: new[] { "CouponId", "ProductId" },
                unique: true,
                filter: "\"IsDeleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_CouponProduct_CreatedAt",
                table: "Tbl_CouponProduct",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_CouponProduct_IsDeleted",
                table: "Tbl_CouponProduct",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_CouponProduct_No",
                table: "Tbl_CouponProduct",
                column: "No");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_CouponProduct_ProductId",
                table: "Tbl_CouponProduct",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_CouponRedemption_CouponId_OrderId",
                table: "Tbl_CouponRedemption",
                columns: new[] { "CouponId", "OrderId" },
                unique: true,
                filter: "\"IsDeleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_CouponRedemption_CreatedAt",
                table: "Tbl_CouponRedemption",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_CouponRedemption_IsDeleted",
                table: "Tbl_CouponRedemption",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_CouponRedemption_No",
                table: "Tbl_CouponRedemption",
                column: "No");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_CouponRedemption_OrderId",
                table: "Tbl_CouponRedemption",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_CouponRedemption_UserId",
                table: "Tbl_CouponRedemption",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_CustomerAddress_AdministrativeAreaId",
                table: "Tbl_CustomerAddress",
                column: "AdministrativeAreaId");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_CustomerAddress_CreatedAt",
                table: "Tbl_CustomerAddress",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_CustomerAddress_IsDeleted",
                table: "Tbl_CustomerAddress",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_CustomerAddress_No",
                table: "Tbl_CustomerAddress",
                column: "No");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_CustomerAddress_UserId",
                table: "Tbl_CustomerAddress",
                column: "UserId",
                unique: true,
                filter: "\"IsDeleted\" = false AND \"IsDefault\" = true");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_CustomerProfile_CreatedAt",
                table: "Tbl_CustomerProfile",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_CustomerProfile_IsDeleted",
                table: "Tbl_CustomerProfile",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_CustomerProfile_No",
                table: "Tbl_CustomerProfile",
                column: "No");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_CustomerProfile_UserId",
                table: "Tbl_CustomerProfile",
                column: "UserId",
                unique: true,
                filter: "\"IsDeleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_FacilityCertification_CertificationId",
                table: "Tbl_FacilityCertification",
                column: "CertificationId");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_FacilityCertification_CreatedAt",
                table: "Tbl_FacilityCertification",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_FacilityCertification_IsDeleted",
                table: "Tbl_FacilityCertification",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_FacilityCertification_No",
                table: "Tbl_FacilityCertification",
                column: "No");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_FacilityCertification_ProductionFacilityId_Certificatio~",
                table: "Tbl_FacilityCertification",
                columns: new[] { "ProductionFacilityId", "CertificationId" },
                unique: true,
                filter: "\"IsDeleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_InquiryAttachment_CreatedAt",
                table: "Tbl_InquiryAttachment",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_InquiryAttachment_IsDeleted",
                table: "Tbl_InquiryAttachment",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_InquiryAttachment_MediaAssetId",
                table: "Tbl_InquiryAttachment",
                column: "MediaAssetId");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_InquiryAttachment_No",
                table: "Tbl_InquiryAttachment",
                column: "No");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_InquiryAttachment_PartnerApplicationId",
                table: "Tbl_InquiryAttachment",
                column: "PartnerApplicationId");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_InquiryAttachment_TradeInquiryId",
                table: "Tbl_InquiryAttachment",
                column: "TradeInquiryId");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_InventoryItem_CreatedAt",
                table: "Tbl_InventoryItem",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_InventoryItem_IsDeleted",
                table: "Tbl_InventoryItem",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_InventoryItem_No",
                table: "Tbl_InventoryItem",
                column: "No");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_InventoryItem_ProductVariantId",
                table: "Tbl_InventoryItem",
                column: "ProductVariantId",
                unique: true,
                filter: "\"IsDeleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_InventoryLevel_CreatedAt",
                table: "Tbl_InventoryLevel",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_InventoryLevel_InventoryItemId_StockLocationId",
                table: "Tbl_InventoryLevel",
                columns: new[] { "InventoryItemId", "StockLocationId" },
                unique: true,
                filter: "\"IsDeleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_InventoryLevel_IsDeleted",
                table: "Tbl_InventoryLevel",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_InventoryLevel_No",
                table: "Tbl_InventoryLevel",
                column: "No");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_InventoryLevel_StockLocationId",
                table: "Tbl_InventoryLevel",
                column: "StockLocationId");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_InventoryMovement_CreatedAt",
                table: "Tbl_InventoryMovement",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_InventoryMovement_InventoryItemId",
                table: "Tbl_InventoryMovement",
                column: "InventoryItemId");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_InventoryMovement_IsDeleted",
                table: "Tbl_InventoryMovement",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_InventoryMovement_No",
                table: "Tbl_InventoryMovement",
                column: "No");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_InventoryMovement_OrderItemId",
                table: "Tbl_InventoryMovement",
                column: "OrderItemId");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_InventoryMovement_StockLocationId",
                table: "Tbl_InventoryMovement",
                column: "StockLocationId");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_InventoryReservation_CreatedAt",
                table: "Tbl_InventoryReservation",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_InventoryReservation_InventoryItemId_StockLocationId_St~",
                table: "Tbl_InventoryReservation",
                columns: new[] { "InventoryItemId", "StockLocationId", "Status", "ExpiresAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_InventoryReservation_IsDeleted",
                table: "Tbl_InventoryReservation",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_InventoryReservation_No",
                table: "Tbl_InventoryReservation",
                column: "No");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_InventoryReservation_OrderItemId",
                table: "Tbl_InventoryReservation",
                column: "OrderItemId");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_InventoryReservation_StockLocationId",
                table: "Tbl_InventoryReservation",
                column: "StockLocationId");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_MediaAsset_CreatedAt",
                table: "Tbl_MediaAsset",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_MediaAsset_IsDeleted",
                table: "Tbl_MediaAsset",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_MediaAsset_No",
                table: "Tbl_MediaAsset",
                column: "No");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_MediaAsset_StorageKey",
                table: "Tbl_MediaAsset",
                column: "StorageKey",
                unique: true,
                filter: "\"IsDeleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_NavigationItem_CreatedAt",
                table: "Tbl_NavigationItem",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_NavigationItem_IsDeleted",
                table: "Tbl_NavigationItem",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_NavigationItem_No",
                table: "Tbl_NavigationItem",
                column: "No");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_NavigationItem_PageId",
                table: "Tbl_NavigationItem",
                column: "PageId");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_NavigationItem_ParentId",
                table: "Tbl_NavigationItem",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_NewsletterSubscription_CreatedAt",
                table: "Tbl_NewsletterSubscription",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_NewsletterSubscription_IsDeleted",
                table: "Tbl_NewsletterSubscription",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_NewsletterSubscription_No",
                table: "Tbl_NewsletterSubscription",
                column: "No");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_NewsletterSubscription_UserId",
                table: "Tbl_NewsletterSubscription",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_Notification_CreatedAt",
                table: "Tbl_Notification",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_Notification_IsDeleted",
                table: "Tbl_Notification",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_Notification_No",
                table: "Tbl_Notification",
                column: "No");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_Order_AdministrativeAreaId",
                table: "Tbl_Order",
                column: "AdministrativeAreaId");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_Order_CreatedAt",
                table: "Tbl_Order",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_Order_IsDeleted",
                table: "Tbl_Order",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_Order_No",
                table: "Tbl_Order",
                column: "No");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_Order_OrderNumber",
                table: "Tbl_Order",
                column: "OrderNumber",
                unique: true,
                filter: "\"IsDeleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_Order_UserId",
                table: "Tbl_Order",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_OrderDiscount_CouponId",
                table: "Tbl_OrderDiscount",
                column: "CouponId");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_OrderDiscount_CreatedAt",
                table: "Tbl_OrderDiscount",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_OrderDiscount_IsDeleted",
                table: "Tbl_OrderDiscount",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_OrderDiscount_No",
                table: "Tbl_OrderDiscount",
                column: "No");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_OrderDiscount_OrderId",
                table: "Tbl_OrderDiscount",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_OrderDiscount_OrderItemId",
                table: "Tbl_OrderDiscount",
                column: "OrderItemId");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_OrderDiscount_PromotionId",
                table: "Tbl_OrderDiscount",
                column: "PromotionId");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_OrderItem_CreatedAt",
                table: "Tbl_OrderItem",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_OrderItem_IsDeleted",
                table: "Tbl_OrderItem",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_OrderItem_No",
                table: "Tbl_OrderItem",
                column: "No");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_OrderItem_OrderId",
                table: "Tbl_OrderItem",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_OrderItem_ProductVariantId",
                table: "Tbl_OrderItem",
                column: "ProductVariantId");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_OrderNote_CreatedAt",
                table: "Tbl_OrderNote",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_OrderNote_IsDeleted",
                table: "Tbl_OrderNote",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_OrderNote_No",
                table: "Tbl_OrderNote",
                column: "No");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_OrderNote_OrderId",
                table: "Tbl_OrderNote",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_OrderStatusHistory_CreatedAt",
                table: "Tbl_OrderStatusHistory",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_OrderStatusHistory_IsDeleted",
                table: "Tbl_OrderStatusHistory",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_OrderStatusHistory_No",
                table: "Tbl_OrderStatusHistory",
                column: "No");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_OrderStatusHistory_OrderId",
                table: "Tbl_OrderStatusHistory",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_Page_CreatedAt",
                table: "Tbl_Page",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_Page_IsDeleted",
                table: "Tbl_Page",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_Page_No",
                table: "Tbl_Page",
                column: "No");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_Page_Slug",
                table: "Tbl_Page",
                column: "Slug",
                unique: true,
                filter: "\"IsDeleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_PageSection_CreatedAt",
                table: "Tbl_PageSection",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_PageSection_IsDeleted",
                table: "Tbl_PageSection",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_PageSection_No",
                table: "Tbl_PageSection",
                column: "No");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_PageSection_PageId",
                table: "Tbl_PageSection",
                column: "PageId");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_PageSectionProduct_CreatedAt",
                table: "Tbl_PageSectionProduct",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_PageSectionProduct_IsDeleted",
                table: "Tbl_PageSectionProduct",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_PageSectionProduct_No",
                table: "Tbl_PageSectionProduct",
                column: "No");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_PageSectionProduct_PageSectionId_ProductId",
                table: "Tbl_PageSectionProduct",
                columns: new[] { "PageSectionId", "ProductId" },
                unique: true,
                filter: "\"IsDeleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_PageSectionProduct_ProductId",
                table: "Tbl_PageSectionProduct",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_PartnerApplication_CreatedAt",
                table: "Tbl_PartnerApplication",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_PartnerApplication_IsDeleted",
                table: "Tbl_PartnerApplication",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_PartnerApplication_No",
                table: "Tbl_PartnerApplication",
                column: "No");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_PartnerApplication_UserId",
                table: "Tbl_PartnerApplication",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_Payment_CreatedAt",
                table: "Tbl_Payment",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_Payment_IsDeleted",
                table: "Tbl_Payment",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_Payment_No",
                table: "Tbl_Payment",
                column: "No");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_Payment_OrderId",
                table: "Tbl_Payment",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_PaymentTransaction_CreatedAt",
                table: "Tbl_PaymentTransaction",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_PaymentTransaction_IsDeleted",
                table: "Tbl_PaymentTransaction",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_PaymentTransaction_No",
                table: "Tbl_PaymentTransaction",
                column: "No");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_PaymentTransaction_PaymentId",
                table: "Tbl_PaymentTransaction",
                column: "PaymentId");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_PaymentTransaction_ProofMediaAssetId",
                table: "Tbl_PaymentTransaction",
                column: "ProofMediaAssetId");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_PaymentTransaction_Provider_ProviderReference",
                table: "Tbl_PaymentTransaction",
                columns: new[] { "Provider", "ProviderReference" },
                unique: true,
                filter: "\"IsDeleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_PointOfSale_AdministrativeAreaId",
                table: "Tbl_PointOfSale",
                column: "AdministrativeAreaId");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_PointOfSale_CreatedAt",
                table: "Tbl_PointOfSale",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_PointOfSale_IsDeleted",
                table: "Tbl_PointOfSale",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_PointOfSale_No",
                table: "Tbl_PointOfSale",
                column: "No");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_PointOfSale_ProducerId",
                table: "Tbl_PointOfSale",
                column: "ProducerId");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_PointOfSaleProduct_CreatedAt",
                table: "Tbl_PointOfSaleProduct",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_PointOfSaleProduct_IsDeleted",
                table: "Tbl_PointOfSaleProduct",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_PointOfSaleProduct_No",
                table: "Tbl_PointOfSaleProduct",
                column: "No");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_PointOfSaleProduct_PointOfSaleId_ProductId",
                table: "Tbl_PointOfSaleProduct",
                columns: new[] { "PointOfSaleId", "ProductId" },
                unique: true,
                filter: "\"IsDeleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_PointOfSaleProduct_ProductId",
                table: "Tbl_PointOfSaleProduct",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_PriceList_Code",
                table: "Tbl_PriceList",
                column: "Code",
                unique: true,
                filter: "\"IsDeleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_PriceList_CreatedAt",
                table: "Tbl_PriceList",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_PriceList_IsDeleted",
                table: "Tbl_PriceList",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_PriceList_No",
                table: "Tbl_PriceList",
                column: "No");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_Producer_Code",
                table: "Tbl_Producer",
                column: "Code",
                unique: true,
                filter: "\"IsDeleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_Producer_CreatedAt",
                table: "Tbl_Producer",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_Producer_IsDeleted",
                table: "Tbl_Producer",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_Producer_No",
                table: "Tbl_Producer",
                column: "No");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_ProducerCertification_CertificationId",
                table: "Tbl_ProducerCertification",
                column: "CertificationId");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_ProducerCertification_CreatedAt",
                table: "Tbl_ProducerCertification",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_ProducerCertification_IsDeleted",
                table: "Tbl_ProducerCertification",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_ProducerCertification_No",
                table: "Tbl_ProducerCertification",
                column: "No");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_ProducerCertification_ProducerId_CertificationId",
                table: "Tbl_ProducerCertification",
                columns: new[] { "ProducerId", "CertificationId" },
                unique: true,
                filter: "\"IsDeleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_ProducerContact_CreatedAt",
                table: "Tbl_ProducerContact",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_ProducerContact_IsDeleted",
                table: "Tbl_ProducerContact",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_ProducerContact_No",
                table: "Tbl_ProducerContact",
                column: "No");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_ProducerContact_ProducerId",
                table: "Tbl_ProducerContact",
                column: "ProducerId");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_Product_CreatedAt",
                table: "Tbl_Product",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_Product_IsDeleted",
                table: "Tbl_Product",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_Product_No",
                table: "Tbl_Product",
                column: "No");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_Product_ProducerId",
                table: "Tbl_Product",
                column: "ProducerId");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_Product_Slug",
                table: "Tbl_Product",
                column: "Slug",
                unique: true,
                filter: "\"IsDeleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_ProductAnswer_AnsweredByUserId",
                table: "Tbl_ProductAnswer",
                column: "AnsweredByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_ProductAnswer_CreatedAt",
                table: "Tbl_ProductAnswer",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_ProductAnswer_IsDeleted",
                table: "Tbl_ProductAnswer",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_ProductAnswer_No",
                table: "Tbl_ProductAnswer",
                column: "No");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_ProductAnswer_ProductQuestionId",
                table: "Tbl_ProductAnswer",
                column: "ProductQuestionId");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_ProductCategory_CategoryId",
                table: "Tbl_ProductCategory",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_ProductCategory_CreatedAt",
                table: "Tbl_ProductCategory",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_ProductCategory_IsDeleted",
                table: "Tbl_ProductCategory",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_ProductCategory_No",
                table: "Tbl_ProductCategory",
                column: "No");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_ProductCategory_ProductId",
                table: "Tbl_ProductCategory",
                column: "ProductId",
                unique: true,
                filter: "\"IsDeleted\" = false AND \"IsPrimary\" = true");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_ProductCategory_ProductId_CategoryId",
                table: "Tbl_ProductCategory",
                columns: new[] { "ProductId", "CategoryId" },
                unique: true,
                filter: "\"IsDeleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_ProductCertification_CertificationId",
                table: "Tbl_ProductCertification",
                column: "CertificationId");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_ProductCertification_CreatedAt",
                table: "Tbl_ProductCertification",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_ProductCertification_IsDeleted",
                table: "Tbl_ProductCertification",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_ProductCertification_No",
                table: "Tbl_ProductCertification",
                column: "No");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_ProductCertification_ProductId_CertificationId",
                table: "Tbl_ProductCertification",
                columns: new[] { "ProductId", "CertificationId" },
                unique: true,
                filter: "\"IsDeleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_ProductionFacility_AdministrativeAreaId",
                table: "Tbl_ProductionFacility",
                column: "AdministrativeAreaId");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_ProductionFacility_CreatedAt",
                table: "Tbl_ProductionFacility",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_ProductionFacility_IsDeleted",
                table: "Tbl_ProductionFacility",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_ProductionFacility_No",
                table: "Tbl_ProductionFacility",
                column: "No");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_ProductionFacility_ProducerId",
                table: "Tbl_ProductionFacility",
                column: "ProducerId");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_ProductMedia_CreatedAt",
                table: "Tbl_ProductMedia",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_ProductMedia_IsDeleted",
                table: "Tbl_ProductMedia",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_ProductMedia_MediaAssetId",
                table: "Tbl_ProductMedia",
                column: "MediaAssetId");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_ProductMedia_No",
                table: "Tbl_ProductMedia",
                column: "No");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_ProductMedia_ProductId_MediaAssetId",
                table: "Tbl_ProductMedia",
                columns: new[] { "ProductId", "MediaAssetId" },
                unique: true,
                filter: "\"IsDeleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_ProductOption_CreatedAt",
                table: "Tbl_ProductOption",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_ProductOption_IsDeleted",
                table: "Tbl_ProductOption",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_ProductOption_No",
                table: "Tbl_ProductOption",
                column: "No");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_ProductOption_ProductId_Code",
                table: "Tbl_ProductOption",
                columns: new[] { "ProductId", "Code" },
                unique: true,
                filter: "\"IsDeleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_ProductOptionValue_CreatedAt",
                table: "Tbl_ProductOptionValue",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_ProductOptionValue_IsDeleted",
                table: "Tbl_ProductOptionValue",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_ProductOptionValue_No",
                table: "Tbl_ProductOptionValue",
                column: "No");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_ProductOptionValue_ProductOptionId_Value",
                table: "Tbl_ProductOptionValue",
                columns: new[] { "ProductOptionId", "Value" },
                unique: true,
                filter: "\"IsDeleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_ProductQuestion_CreatedAt",
                table: "Tbl_ProductQuestion",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_ProductQuestion_IsDeleted",
                table: "Tbl_ProductQuestion",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_ProductQuestion_No",
                table: "Tbl_ProductQuestion",
                column: "No");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_ProductQuestion_ProductId",
                table: "Tbl_ProductQuestion",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_ProductQuestion_UserId",
                table: "Tbl_ProductQuestion",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_ProductReview_CreatedAt",
                table: "Tbl_ProductReview",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_ProductReview_IsDeleted",
                table: "Tbl_ProductReview",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_ProductReview_No",
                table: "Tbl_ProductReview",
                column: "No");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_ProductReview_OrderItemId",
                table: "Tbl_ProductReview",
                column: "OrderItemId");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_ProductReview_ProductId",
                table: "Tbl_ProductReview",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_ProductReview_UserId",
                table: "Tbl_ProductReview",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_ProductReviewMedia_CreatedAt",
                table: "Tbl_ProductReviewMedia",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_ProductReviewMedia_IsDeleted",
                table: "Tbl_ProductReviewMedia",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_ProductReviewMedia_MediaAssetId",
                table: "Tbl_ProductReviewMedia",
                column: "MediaAssetId");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_ProductReviewMedia_No",
                table: "Tbl_ProductReviewMedia",
                column: "No");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_ProductReviewMedia_ProductReviewId_MediaAssetId",
                table: "Tbl_ProductReviewMedia",
                columns: new[] { "ProductReviewId", "MediaAssetId" },
                unique: true,
                filter: "\"IsDeleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_ProductSlugHistory_CreatedAt",
                table: "Tbl_ProductSlugHistory",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_ProductSlugHistory_IsDeleted",
                table: "Tbl_ProductSlugHistory",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_ProductSlugHistory_No",
                table: "Tbl_ProductSlugHistory",
                column: "No");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_ProductSlugHistory_ProductId",
                table: "Tbl_ProductSlugHistory",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_ProductSlugHistory_Slug",
                table: "Tbl_ProductSlugHistory",
                column: "Slug",
                unique: true,
                filter: "\"IsDeleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_ProductVariant_CreatedAt",
                table: "Tbl_ProductVariant",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_ProductVariant_IsDeleted",
                table: "Tbl_ProductVariant",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_ProductVariant_No",
                table: "Tbl_ProductVariant",
                column: "No");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_ProductVariant_ProductId",
                table: "Tbl_ProductVariant",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_ProductVariant_Sku",
                table: "Tbl_ProductVariant",
                column: "Sku",
                unique: true,
                filter: "\"IsDeleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_ProductVariantOptionValue_CreatedAt",
                table: "Tbl_ProductVariantOptionValue",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_ProductVariantOptionValue_IsDeleted",
                table: "Tbl_ProductVariantOptionValue",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_ProductVariantOptionValue_No",
                table: "Tbl_ProductVariantOptionValue",
                column: "No");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_ProductVariantOptionValue_ProductOptionValueId",
                table: "Tbl_ProductVariantOptionValue",
                column: "ProductOptionValueId");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_ProductVariantOptionValue_ProductVariantId_ProductOptio~",
                table: "Tbl_ProductVariantOptionValue",
                columns: new[] { "ProductVariantId", "ProductOptionValueId" },
                unique: true,
                filter: "\"IsDeleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_Promotion_Code",
                table: "Tbl_Promotion",
                column: "Code",
                unique: true,
                filter: "\"IsDeleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_Promotion_CreatedAt",
                table: "Tbl_Promotion",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_Promotion_IsDeleted",
                table: "Tbl_Promotion",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_Promotion_No",
                table: "Tbl_Promotion",
                column: "No");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_SeoRedirect_CreatedAt",
                table: "Tbl_SeoRedirect",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_SeoRedirect_IsDeleted",
                table: "Tbl_SeoRedirect",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_SeoRedirect_No",
                table: "Tbl_SeoRedirect",
                column: "No");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_SeoRedirect_SourcePath",
                table: "Tbl_SeoRedirect",
                column: "SourcePath",
                unique: true,
                filter: "\"IsDeleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_Shipment_CreatedAt",
                table: "Tbl_Shipment",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_Shipment_IsDeleted",
                table: "Tbl_Shipment",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_Shipment_No",
                table: "Tbl_Shipment",
                column: "No");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_Shipment_OrderId",
                table: "Tbl_Shipment",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_ShipmentHistory_CreatedAt",
                table: "Tbl_ShipmentHistory",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_ShipmentHistory_IsDeleted",
                table: "Tbl_ShipmentHistory",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_ShipmentHistory_No",
                table: "Tbl_ShipmentHistory",
                column: "No");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_ShipmentHistory_ShipmentId",
                table: "Tbl_ShipmentHistory",
                column: "ShipmentId");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_ShipmentItem_CreatedAt",
                table: "Tbl_ShipmentItem",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_ShipmentItem_IsDeleted",
                table: "Tbl_ShipmentItem",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_ShipmentItem_No",
                table: "Tbl_ShipmentItem",
                column: "No");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_ShipmentItem_OrderItemId",
                table: "Tbl_ShipmentItem",
                column: "OrderItemId");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_ShipmentItem_ShipmentId_OrderItemId",
                table: "Tbl_ShipmentItem",
                columns: new[] { "ShipmentId", "OrderItemId" },
                unique: true,
                filter: "\"IsDeleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_StockLocation_AdministrativeAreaId",
                table: "Tbl_StockLocation",
                column: "AdministrativeAreaId");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_StockLocation_Code",
                table: "Tbl_StockLocation",
                column: "Code",
                unique: true,
                filter: "\"IsDeleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_StockLocation_CreatedAt",
                table: "Tbl_StockLocation",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_StockLocation_IsDeleted",
                table: "Tbl_StockLocation",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_StockLocation_No",
                table: "Tbl_StockLocation",
                column: "No");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_SystemSetting_CreatedAt",
                table: "Tbl_SystemSetting",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_SystemSetting_IsDeleted",
                table: "Tbl_SystemSetting",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_SystemSetting_No",
                table: "Tbl_SystemSetting",
                column: "No");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_SystemSetting_SettingKey",
                table: "Tbl_SystemSetting",
                column: "SettingKey",
                unique: true,
                filter: "\"IsDeleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_TraceEvent_CreatedAt",
                table: "Tbl_TraceEvent",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_TraceEvent_IsDeleted",
                table: "Tbl_TraceEvent",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_TraceEvent_No",
                table: "Tbl_TraceEvent",
                column: "No");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_TraceEvent_TraceLotId",
                table: "Tbl_TraceEvent",
                column: "TraceLotId");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_TraceEventEvidence_CreatedAt",
                table: "Tbl_TraceEventEvidence",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_TraceEventEvidence_IsDeleted",
                table: "Tbl_TraceEventEvidence",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_TraceEventEvidence_MediaAssetId",
                table: "Tbl_TraceEventEvidence",
                column: "MediaAssetId");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_TraceEventEvidence_No",
                table: "Tbl_TraceEventEvidence",
                column: "No");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_TraceEventEvidence_TraceEventId_MediaAssetId",
                table: "Tbl_TraceEventEvidence",
                columns: new[] { "TraceEventId", "MediaAssetId" },
                unique: true,
                filter: "\"IsDeleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_TraceLot_CreatedAt",
                table: "Tbl_TraceLot",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_TraceLot_IsDeleted",
                table: "Tbl_TraceLot",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_TraceLot_LotCode",
                table: "Tbl_TraceLot",
                column: "LotCode",
                unique: true,
                filter: "\"IsDeleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_TraceLot_No",
                table: "Tbl_TraceLot",
                column: "No");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_TraceLot_TraceProfileId",
                table: "Tbl_TraceLot",
                column: "TraceProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_TraceProfile_CreatedAt",
                table: "Tbl_TraceProfile",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_TraceProfile_IsDeleted",
                table: "Tbl_TraceProfile",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_TraceProfile_No",
                table: "Tbl_TraceProfile",
                column: "No");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_TraceProfile_ProductId",
                table: "Tbl_TraceProfile",
                column: "ProductId",
                unique: true,
                filter: "\"IsDeleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_TraceProfile_PublicCode",
                table: "Tbl_TraceProfile",
                column: "PublicCode",
                unique: true,
                filter: "\"IsDeleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_TradeInquiry_CreatedAt",
                table: "Tbl_TradeInquiry",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_TradeInquiry_InquiryNumber",
                table: "Tbl_TradeInquiry",
                column: "InquiryNumber",
                unique: true,
                filter: "\"IsDeleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_TradeInquiry_IsDeleted",
                table: "Tbl_TradeInquiry",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_TradeInquiry_No",
                table: "Tbl_TradeInquiry",
                column: "No");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_TradeInquiry_UserId",
                table: "Tbl_TradeInquiry",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_TradeInquiryItem_CreatedAt",
                table: "Tbl_TradeInquiryItem",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_TradeInquiryItem_IsDeleted",
                table: "Tbl_TradeInquiryItem",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_TradeInquiryItem_No",
                table: "Tbl_TradeInquiryItem",
                column: "No");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_TradeInquiryItem_ProductId",
                table: "Tbl_TradeInquiryItem",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_TradeInquiryItem_ProductVariantId",
                table: "Tbl_TradeInquiryItem",
                column: "ProductVariantId");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_TradeInquiryItem_TradeInquiryId",
                table: "Tbl_TradeInquiryItem",
                column: "TradeInquiryId");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_TradeInquiryStatusHistory_CreatedAt",
                table: "Tbl_TradeInquiryStatusHistory",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_TradeInquiryStatusHistory_IsDeleted",
                table: "Tbl_TradeInquiryStatusHistory",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_TradeInquiryStatusHistory_No",
                table: "Tbl_TradeInquiryStatusHistory",
                column: "No");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_TradeInquiryStatusHistory_TradeInquiryId",
                table: "Tbl_TradeInquiryStatusHistory",
                column: "TradeInquiryId");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_UserNotification_CreatedAt",
                table: "Tbl_UserNotification",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_UserNotification_IsDeleted",
                table: "Tbl_UserNotification",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_UserNotification_No",
                table: "Tbl_UserNotification",
                column: "No");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_UserNotification_NotificationId_UserId",
                table: "Tbl_UserNotification",
                columns: new[] { "NotificationId", "UserId" },
                unique: true,
                filter: "\"IsDeleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_UserNotification_UserId",
                table: "Tbl_UserNotification",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_VariantPrice_CreatedAt",
                table: "Tbl_VariantPrice",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_VariantPrice_IsDeleted",
                table: "Tbl_VariantPrice",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_VariantPrice_No",
                table: "Tbl_VariantPrice",
                column: "No");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_VariantPrice_PriceListId",
                table: "Tbl_VariantPrice",
                column: "PriceListId");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_VariantPrice_ProductVariantId_EffectiveFrom_EffectiveTo",
                table: "Tbl_VariantPrice",
                columns: new[] { "ProductVariantId", "EffectiveFrom", "EffectiveTo" });

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_VisitorSession_CreatedAt",
                table: "Tbl_VisitorSession",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_VisitorSession_IsDeleted",
                table: "Tbl_VisitorSession",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_VisitorSession_No",
                table: "Tbl_VisitorSession",
                column: "No");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_VisitorSession_SessionHash",
                table: "Tbl_VisitorSession",
                column: "SessionHash",
                unique: true,
                filter: "\"IsDeleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_VisitorSession_UserId",
                table: "Tbl_VisitorSession",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_Wishlist_CreatedAt",
                table: "Tbl_Wishlist",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_Wishlist_IsDeleted",
                table: "Tbl_Wishlist",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_Wishlist_No",
                table: "Tbl_Wishlist",
                column: "No");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_Wishlist_UserId_Name",
                table: "Tbl_Wishlist",
                columns: new[] { "UserId", "Name" },
                unique: true,
                filter: "\"IsDeleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_WishlistItem_CreatedAt",
                table: "Tbl_WishlistItem",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_WishlistItem_IsDeleted",
                table: "Tbl_WishlistItem",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_WishlistItem_No",
                table: "Tbl_WishlistItem",
                column: "No");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_WishlistItem_ProductId",
                table: "Tbl_WishlistItem",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_WishlistItem_WishlistId_ProductId",
                table: "Tbl_WishlistItem",
                columns: new[] { "WishlistId", "ProductId" },
                unique: true,
                filter: "\"IsDeleted\" = false");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Tbl_AnalyticsEvent");

            migrationBuilder.DropTable(
                name: "Tbl_ArticleCategoryMap");

            migrationBuilder.DropTable(
                name: "Tbl_AuditLog");

            migrationBuilder.DropTable(
                name: "Tbl_Banner");

            migrationBuilder.DropTable(
                name: "Tbl_CartItem");

            migrationBuilder.DropTable(
                name: "Tbl_CertificationEvidence");

            migrationBuilder.DropTable(
                name: "Tbl_CouponCategory");

            migrationBuilder.DropTable(
                name: "Tbl_CouponProduct");

            migrationBuilder.DropTable(
                name: "Tbl_CouponRedemption");

            migrationBuilder.DropTable(
                name: "Tbl_CustomerAddress");

            migrationBuilder.DropTable(
                name: "Tbl_CustomerProfile");

            migrationBuilder.DropTable(
                name: "Tbl_FacilityCertification");

            migrationBuilder.DropTable(
                name: "Tbl_InquiryAttachment");

            migrationBuilder.DropTable(
                name: "Tbl_InventoryLevel");

            migrationBuilder.DropTable(
                name: "Tbl_InventoryMovement");

            migrationBuilder.DropTable(
                name: "Tbl_InventoryReservation");

            migrationBuilder.DropTable(
                name: "Tbl_NavigationItem");

            migrationBuilder.DropTable(
                name: "Tbl_NewsletterSubscription");

            migrationBuilder.DropTable(
                name: "Tbl_OrderDiscount");

            migrationBuilder.DropTable(
                name: "Tbl_OrderNote");

            migrationBuilder.DropTable(
                name: "Tbl_OrderStatusHistory");

            migrationBuilder.DropTable(
                name: "Tbl_PageSectionProduct");

            migrationBuilder.DropTable(
                name: "Tbl_PaymentTransaction");

            migrationBuilder.DropTable(
                name: "Tbl_PointOfSaleProduct");

            migrationBuilder.DropTable(
                name: "Tbl_ProducerCertification");

            migrationBuilder.DropTable(
                name: "Tbl_ProducerContact");

            migrationBuilder.DropTable(
                name: "Tbl_ProductAnswer");

            migrationBuilder.DropTable(
                name: "Tbl_ProductCategory");

            migrationBuilder.DropTable(
                name: "Tbl_ProductCertification");

            migrationBuilder.DropTable(
                name: "Tbl_ProductMedia");

            migrationBuilder.DropTable(
                name: "Tbl_ProductReviewMedia");

            migrationBuilder.DropTable(
                name: "Tbl_ProductSlugHistory");

            migrationBuilder.DropTable(
                name: "Tbl_ProductVariantOptionValue");

            migrationBuilder.DropTable(
                name: "Tbl_SeoRedirect");

            migrationBuilder.DropTable(
                name: "Tbl_ShipmentHistory");

            migrationBuilder.DropTable(
                name: "Tbl_ShipmentItem");

            migrationBuilder.DropTable(
                name: "Tbl_SystemSetting");

            migrationBuilder.DropTable(
                name: "Tbl_TraceEventEvidence");

            migrationBuilder.DropTable(
                name: "Tbl_TradeInquiryItem");

            migrationBuilder.DropTable(
                name: "Tbl_TradeInquiryStatusHistory");

            migrationBuilder.DropTable(
                name: "Tbl_UserNotification");

            migrationBuilder.DropTable(
                name: "Tbl_VariantPrice");

            migrationBuilder.DropTable(
                name: "Tbl_WishlistItem");

            migrationBuilder.DropTable(
                name: "Tbl_VisitorSession");

            migrationBuilder.DropTable(
                name: "Tbl_ArticleCategory");

            migrationBuilder.DropTable(
                name: "Tbl_Article");

            migrationBuilder.DropTable(
                name: "Tbl_Campaign");

            migrationBuilder.DropTable(
                name: "Tbl_Cart");

            migrationBuilder.DropTable(
                name: "Tbl_ProductionFacility");

            migrationBuilder.DropTable(
                name: "Tbl_PartnerApplication");

            migrationBuilder.DropTable(
                name: "Tbl_InventoryItem");

            migrationBuilder.DropTable(
                name: "Tbl_StockLocation");

            migrationBuilder.DropTable(
                name: "Tbl_Coupon");

            migrationBuilder.DropTable(
                name: "Tbl_PageSection");

            migrationBuilder.DropTable(
                name: "Tbl_Payment");

            migrationBuilder.DropTable(
                name: "Tbl_PointOfSale");

            migrationBuilder.DropTable(
                name: "Tbl_ProductQuestion");

            migrationBuilder.DropTable(
                name: "Tbl_Category");

            migrationBuilder.DropTable(
                name: "Tbl_Certification");

            migrationBuilder.DropTable(
                name: "Tbl_ProductReview");

            migrationBuilder.DropTable(
                name: "Tbl_ProductOptionValue");

            migrationBuilder.DropTable(
                name: "Tbl_Shipment");

            migrationBuilder.DropTable(
                name: "Tbl_TraceEvent");

            migrationBuilder.DropTable(
                name: "Tbl_TradeInquiry");

            migrationBuilder.DropTable(
                name: "Tbl_Notification");

            migrationBuilder.DropTable(
                name: "Tbl_PriceList");

            migrationBuilder.DropTable(
                name: "Tbl_Wishlist");

            migrationBuilder.DropTable(
                name: "Tbl_MediaAsset");

            migrationBuilder.DropTable(
                name: "Tbl_Promotion");

            migrationBuilder.DropTable(
                name: "Tbl_Page");

            migrationBuilder.DropTable(
                name: "Tbl_OrderItem");

            migrationBuilder.DropTable(
                name: "Tbl_ProductOption");

            migrationBuilder.DropTable(
                name: "Tbl_TraceLot");

            migrationBuilder.DropTable(
                name: "Tbl_Order");

            migrationBuilder.DropTable(
                name: "Tbl_ProductVariant");

            migrationBuilder.DropTable(
                name: "Tbl_TraceProfile");

            migrationBuilder.DropTable(
                name: "Tbl_AdministrativeArea");

            migrationBuilder.DropTable(
                name: "Tbl_Product");

            migrationBuilder.DropTable(
                name: "Tbl_Producer");
        }
    }
}
