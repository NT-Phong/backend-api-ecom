namespace Ecom.Application.Features.Catalog.Categories;

public sealed record UpdateCatalogCategoryCommand(
    Guid CategoryId,
    Guid ConcurrencyStamp,
    Guid? ParentId,
    string Name,
    string Slug,
    string? Description,
    int DisplayOrder) : IRequest<TResult<CatalogCategoryManagementDto>>, ITransactionalRequest;

public sealed class UpdateCatalogCategoryCommandValidator : AbstractValidator<UpdateCatalogCategoryCommand>
{
    public UpdateCatalogCategoryCommandValidator()
    {
        RuleFor(x => x.CategoryId).NotEmpty();
        RuleFor(x => x.ConcurrencyStamp).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Slug).NotEmpty().MaximumLength(250);
        RuleFor(x => x.Description).MaximumLength(10000);
        RuleFor(x => x.DisplayOrder).GreaterThanOrEqualTo(0);
    }
}

public sealed class UpdateCatalogCategoryCommandHandler(CatalogCategoryCommandService service)
    : IRequestHandler<UpdateCatalogCategoryCommand, TResult<CatalogCategoryManagementDto>>
{
    public Task<TResult<CatalogCategoryManagementDto>> Handle(UpdateCatalogCategoryCommand request, CancellationToken cancellationToken) =>
        service.UpdateAsync(request, cancellationToken);
}
