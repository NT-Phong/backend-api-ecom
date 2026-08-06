namespace Ecom.Application.Features.Catalog.Categories;

public sealed record HideCatalogCategoryCommand(Guid CategoryId, Guid ConcurrencyStamp)
    : IRequest<TResult<CatalogCategoryManagementDto>>, ITransactionalRequest;

public sealed class HideCatalogCategoryCommandValidator : AbstractValidator<HideCatalogCategoryCommand>
{
    public HideCatalogCategoryCommandValidator()
    {
        RuleFor(x => x.CategoryId).NotEmpty();
        RuleFor(x => x.ConcurrencyStamp).NotEmpty();
    }
}

public sealed class HideCatalogCategoryCommandHandler(CatalogCategoryCommandService service)
    : IRequestHandler<HideCatalogCategoryCommand, TResult<CatalogCategoryManagementDto>>
{
    public Task<TResult<CatalogCategoryManagementDto>> Handle(HideCatalogCategoryCommand request, CancellationToken cancellationToken) =>
        service.HideAsync(request, cancellationToken);
}
