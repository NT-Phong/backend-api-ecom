using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Ecom.Application.Common.Interfaces;
using Ecom.Domain.Entities;
using Ecom.Infrastructure.Persistence.Outbox;

namespace Ecom.Infrastructure.Persistence.Database;

public class ApplicationDbContext : DbContext, IApplicationDbContext
{
    private readonly ICurrentUser _currentUser;
    private readonly IDateTimeService _dateTime;
    private readonly IConnectionService _connectionService;

    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options,
        ICurrentUser currentUser,
        IDateTimeService dateTime,
        IConnectionService connectionService) : base(options)
    {
        _currentUser = currentUser;
        _dateTime = dateTime;
        _connectionService = connectionService;
    }

    // Flag to indicate if this context should use read-only connection
    public bool IsReadOnly { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        base.OnModelCreating(builder);
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        // Configure connection based on read/write context
        if (!optionsBuilder.IsConfigured)
        {
            var connectionString = IsReadOnly
                ? _connectionService.GetReadConnectionString()
                : _connectionService.GetWriteConnectionString();
            optionsBuilder.UseNpgsql(connectionString);
        }

        // Optimize for read operations
        if (IsReadOnly)
        {
            optionsBuilder.EnableSensitiveDataLogging(false);
        }
    }

    public DbSet<Permission> Permissions => Set<Permission>();
    public DbSet<Policy> Policies => Set<Policy>();
    public DbSet<JwtRefreshToken> JwtRefreshTokens => Set<JwtRefreshToken>();
    public DbSet<OtpToken> OtpTokens => Set<OtpToken>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<RolePolicy> RolePolicies => Set<RolePolicy>();
    public DbSet<UserPolicy> UserPolicies => Set<UserPolicy>();
    public DbSet<UserDeviceToken> UserDeviceTokens => Set<UserDeviceToken>();
    public DbSet<VerificationChallenge> VerificationChallenges => Set<VerificationChallenge>();
    public DbSet<UserSession> UserSessions => Set<UserSession>();
    public DbSet<SessionRefreshToken> SessionRefreshTokens => Set<SessionRefreshToken>();
    public DbSet<SecurityEvent> SecurityEvents => Set<SecurityEvent>();
    public DbSet<PasswordCredential> PasswordCredentials => Set<PasswordCredential>();

    // Commerce persistence schema. Application-facing abstractions remain unchanged until CQRS use cases require them.
    public DbSet<CustomerProfile> CustomerProfiles => Set<CustomerProfile>();
    public DbSet<AdministrativeArea> AdministrativeAreas => Set<AdministrativeArea>();
    public DbSet<CustomerAddress> CustomerAddresses => Set<CustomerAddress>();
    public DbSet<Producer> Producers => Set<Producer>();
    public DbSet<ProducerContact> ProducerContacts => Set<ProducerContact>();
    public DbSet<ProductionFacility> ProductionFacilities => Set<ProductionFacility>();
    public DbSet<PointOfSale> PointsOfSale => Set<PointOfSale>();
    public DbSet<PointOfSaleProduct> PointOfSaleProducts => Set<PointOfSaleProduct>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<ProductCategory> ProductCategories => Set<ProductCategory>();
    public DbSet<ProductSlugHistory> ProductSlugHistories => Set<ProductSlugHistory>();
    public DbSet<ProductOption> ProductOptions => Set<ProductOption>();
    public DbSet<ProductOptionValue> ProductOptionValues => Set<ProductOptionValue>();
    public DbSet<ProductVariant> ProductVariants => Set<ProductVariant>();
    public DbSet<ProductVariantOptionValue> ProductVariantOptionValues => Set<ProductVariantOptionValue>();
    public DbSet<MediaAsset> MediaAssets => Set<MediaAsset>();
    public DbSet<ProductMedia> ProductMedia => Set<ProductMedia>();
    public DbSet<PriceList> PriceLists => Set<PriceList>();
    public DbSet<VariantPrice> VariantPrices => Set<VariantPrice>();
    public DbSet<Promotion> Promotions => Set<Promotion>();
    public DbSet<Coupon> Coupons => Set<Coupon>();
    public DbSet<CouponProduct> CouponProducts => Set<CouponProduct>();
    public DbSet<CouponCategory> CouponCategories => Set<CouponCategory>();
    public DbSet<CouponRedemption> CouponRedemptions => Set<CouponRedemption>();
    public DbSet<StockLocation> StockLocations => Set<StockLocation>();
    public DbSet<InventoryItem> InventoryItems => Set<InventoryItem>();
    public DbSet<InventoryLevel> InventoryLevels => Set<InventoryLevel>();
    public DbSet<InventoryReservation> InventoryReservations => Set<InventoryReservation>();
    public DbSet<InventoryMovement> InventoryMovements => Set<InventoryMovement>();
    public DbSet<Cart> Carts => Set<Cart>();
    public DbSet<CartItem> CartItems => Set<CartItem>();
    public DbSet<IdempotencyRecord> IdempotencyRecords => Set<IdempotencyRecord>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<OrderStatusHistory> OrderStatusHistories => Set<OrderStatusHistory>();
    public DbSet<OrderNote> OrderNotes => Set<OrderNote>();
    public DbSet<OrderDiscount> OrderDiscounts => Set<OrderDiscount>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<PaymentTransaction> PaymentTransactions => Set<PaymentTransaction>();
    public DbSet<Shipment> Shipments => Set<Shipment>();
    public DbSet<ShipmentItem> ShipmentItems => Set<ShipmentItem>();
    public DbSet<ShipmentHistory> ShipmentHistories => Set<ShipmentHistory>();
    public DbSet<Certification> Certifications => Set<Certification>();
    public DbSet<CertificationEvidence> CertificationEvidences => Set<CertificationEvidence>();
    public DbSet<ProductCertification> ProductCertifications => Set<ProductCertification>();
    public DbSet<ProducerCertification> ProducerCertifications => Set<ProducerCertification>();
    public DbSet<FacilityCertification> FacilityCertifications => Set<FacilityCertification>();
    public DbSet<TraceProfile> TraceProfiles => Set<TraceProfile>();
    public DbSet<TraceLot> TraceLots => Set<TraceLot>();
    public DbSet<TraceEvent> TraceEvents => Set<TraceEvent>();
    public DbSet<TraceEventEvidence> TraceEventEvidences => Set<TraceEventEvidence>();
    public DbSet<Wishlist> Wishlists => Set<Wishlist>();
    public DbSet<WishlistItem> WishlistItems => Set<WishlistItem>();
    public DbSet<ProductReview> ProductReviews => Set<ProductReview>();
    public DbSet<ProductReviewMedia> ProductReviewMedia => Set<ProductReviewMedia>();
    public DbSet<ProductQuestion> ProductQuestions => Set<ProductQuestion>();
    public DbSet<ProductAnswer> ProductAnswers => Set<ProductAnswer>();
    public DbSet<NewsletterSubscription> NewsletterSubscriptions => Set<NewsletterSubscription>();
    public DbSet<Page> Pages => Set<Page>();
    public DbSet<PageSection> PageSections => Set<PageSection>();
    public DbSet<PageSectionProduct> PageSectionProducts => Set<PageSectionProduct>();
    public DbSet<Article> Articles => Set<Article>();
    public DbSet<ArticleCategory> ArticleCategories => Set<ArticleCategory>();
    public DbSet<ArticleCategoryMap> ArticleCategoryMaps => Set<ArticleCategoryMap>();
    public DbSet<Campaign> Campaigns => Set<Campaign>();
    public DbSet<Banner> Banners => Set<Banner>();
    public DbSet<NavigationItem> NavigationItems => Set<NavigationItem>();
    public DbSet<SeoRedirect> SeoRedirects => Set<SeoRedirect>();
    public DbSet<TradeInquiry> TradeInquiries => Set<TradeInquiry>();
    public DbSet<TradeInquiryItem> TradeInquiryItems => Set<TradeInquiryItem>();
    public DbSet<TradeInquiryStatusHistory> TradeInquiryStatusHistories => Set<TradeInquiryStatusHistory>();
    public DbSet<PartnerApplication> PartnerApplications => Set<PartnerApplication>();
    public DbSet<InquiryAttachment> InquiryAttachments => Set<InquiryAttachment>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<UserNotification> UserNotifications => Set<UserNotification>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<SystemSetting> SystemSettings => Set<SystemSetting>();
    public DbSet<VisitorSession> VisitorSessions => Set<VisitorSession>();
    public DbSet<AnalyticsEvent> AnalyticsEvents => Set<AnalyticsEvent>();
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        if (IsReadOnly)
        {
            throw new InvalidOperationException("Cannot save changes on a read-only context");
        }

        if (ChangeTracker.Entries<SecurityEvent>().Any(x => x.State is EntityState.Modified or EntityState.Deleted))
            throw new InvalidOperationException("Security events are append-only.");
        return await base.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Create a read-only context for queries with optimizations
    /// </summary>
    public ApplicationDbContext AsReadOnly()
    {
        var readOnlyContext = new ApplicationDbContext(
            new DbContextOptionsBuilder<ApplicationDbContext>().Options,
            _currentUser, _dateTime, _connectionService)
        {
            IsReadOnly = true
        };

        return readOnlyContext;
    }
}

