namespace Ecom.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());

            // 1. Chạy cái này trước để lấy UserId từ ICurrentUser gán vào Command
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(UnhandledExceptionBehaviour<,>));
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(LoggingBehaviour<,>));
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(PerformanceBehaviour<,>));
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(CurrentUserBehaviour<,>));
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(AuthorizationBehaviour<,>));

            // 2. Sau đó mới chạy cái này để Validate (Lúc này UserId đã có giá trị nên pass)
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehaviour<,>));
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(UnitOfWorkBehavior<,>));
        });
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
        services.AddScoped<IMediaAccessService, Common.Services.MediaAccessService>();
        services.AddScoped<Common.Services.MediaUploadOrchestrator>();
        services.AddScoped<INotificationService, Common.Services.NotificationService>();
        services.AddScoped<IProductMediaReader, Common.Services.ProductMediaReader>();

        services.AddScoped<IEffectivePriceResolver, Common.Services.EffectivePriceResolver>();
        services.AddScoped<ICheckoutPricingService, Common.Services.CheckoutPricingService>();
        services.AddScoped<ICatalogProductAccessService, Common.Services.CatalogProductAccessService>();
        services.AddScoped<Features.Catalog.Products.Services.ICatalogProductMutationService,
            Features.Catalog.Products.Services.CatalogProductMutationService>();
        services.AddScoped<Features.Catalog.Categories.CatalogCategoryCommandService>();
		services.AddScoped<IAuthenticationSessionEngine, Common.Services.AuthenticationSessionEngine>();
        services.AddScoped<IUserAuthorizationSnapshotService, Common.Services.UserAuthorizationSnapshotService>();

        return services;
    }
}

