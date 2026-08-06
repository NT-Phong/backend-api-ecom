namespace Ecom.Application.Features.Catalog.Categories;

public sealed record CreateCatalogCategoryCommand(
    Guid? ParentId,
    string Name,
    string Slug,
    string? Description,
    int DisplayOrder) : IRequest<TResult<CatalogCategoryManagementDto>>, ITransactionalRequest;

public sealed class CreateCatalogCategoryCommandValidator : AbstractValidator<CreateCatalogCategoryCommand>
{
    public CreateCatalogCategoryCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Slug).NotEmpty().MaximumLength(250);
        RuleFor(x => x.Description).MaximumLength(10000);
        RuleFor(x => x.DisplayOrder).GreaterThanOrEqualTo(0);
    }
}

public sealed class CreateCatalogCategoryCommandHandler(CatalogCategoryCommandService service)
    : IRequestHandler<CreateCatalogCategoryCommand, TResult<CatalogCategoryManagementDto>>
{
    public Task<TResult<CatalogCategoryManagementDto>> Handle(CreateCatalogCategoryCommand request, CancellationToken cancellationToken) =>
        service.CreateAsync(request, cancellationToken);
}
