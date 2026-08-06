namespace Ecom.Application.Features.Catalog.Categories;

public sealed record PauseCatalogCategoryCommand(Guid CategoryId, Guid ConcurrencyStamp)
    : IRequest<TResult<CatalogCategoryManagementDto>>, ITransactionalRequest;

public sealed class PauseCatalogCategoryCommandValidator : AbstractValidator<PauseCatalogCategoryCommand>
{
    public PauseCatalogCategoryCommandValidator()
    {
        RuleFor(x => x.CategoryId).NotEmpty();
        RuleFor(x => x.ConcurrencyStamp).NotEmpty();
    }
}

public sealed class PauseCatalogCategoryCommandHandler(CatalogCategoryCommandService service)
    : IRequestHandler<PauseCatalogCategoryCommand, TResult<CatalogCategoryManagementDto>>
{
    public Task<TResult<CatalogCategoryManagementDto>> Handle(PauseCatalogCategoryCommand request, CancellationToken cancellationToken) =>
        service.PauseAsync(request, cancellationToken);
}
