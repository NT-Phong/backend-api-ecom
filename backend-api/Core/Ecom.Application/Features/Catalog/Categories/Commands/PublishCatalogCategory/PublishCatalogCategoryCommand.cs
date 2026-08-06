namespace Ecom.Application.Features.Catalog.Categories;

public sealed record PublishCatalogCategoryCommand(Guid CategoryId, Guid ConcurrencyStamp)
    : IRequest<TResult<CatalogCategoryManagementDto>>, ITransactionalRequest;

public sealed class PublishCatalogCategoryCommandValidator : AbstractValidator<PublishCatalogCategoryCommand>
{
    public PublishCatalogCategoryCommandValidator()
    {
        RuleFor(x => x.CategoryId).NotEmpty();
        RuleFor(x => x.ConcurrencyStamp).NotEmpty();
    }
}

public sealed class PublishCatalogCategoryCommandHandler(CatalogCategoryCommandService service)
    : IRequestHandler<PublishCatalogCategoryCommand, TResult<CatalogCategoryManagementDto>>
{
    public Task<TResult<CatalogCategoryManagementDto>> Handle(PublishCatalogCategoryCommand request, CancellationToken cancellationToken) =>
        service.PublishAsync(request, cancellationToken);
}
