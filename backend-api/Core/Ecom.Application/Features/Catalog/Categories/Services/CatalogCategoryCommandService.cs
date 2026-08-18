using Ecom.Domain.Entities;

namespace Ecom.Application.Features.Catalog.Categories;

public sealed class CatalogCategoryCommandService(IUnitOfWork unitOfWork, ICatalogProductAccessService access)
{
    public async Task<TResult<CatalogCategoryManagementDto>> CreateAsync(CreateCatalogCategoryCommand request, CancellationToken cancellationToken)
    {
        var authorization = access.Ensure(Permissions.CatalogCategories.Create);
        if (!authorization.IsSuccess) return Failure(authorization);
        var parent = await ValidateParentAsync(request.ParentId, Guid.Empty, cancellationToken);
        if (parent is not null) return Failure(parent);
        if (await unitOfWork.Repository<Category>().AnyAsync([x => x.Slug == request.Slug.Trim()]))
            return TResult<CatalogCategoryManagementDto>.Failure("Category slug already exists.", ErrorCodes.ALREADY_EXISTS);

        var category = Category.Create(request.ParentId, request.Name, request.Slug, request.Description, request.DisplayOrder);
        await unitOfWork.Repository<Category>().InsertAsync(category, cancellationToken);
        return TResult<CatalogCategoryManagementDto>.Success(Map(category));
    }

    public async Task<TResult<CatalogCategoryManagementDto>> UpdateAsync(UpdateCatalogCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = await LoadForMutationAsync(request.CategoryId, request.ConcurrencyStamp,
            Permissions.CatalogCategories.Update, cancellationToken);
        if (category.Result is not null) return category.Result;
        var entity = category.Category!;
        var parent = await ValidateParentAsync(request.ParentId, entity.Id, cancellationToken);
        if (parent is not null) return Failure(parent);
        if (await HasPublishedDependentsAsync(entity.Id, cancellationToken))
        {
            var ancestors = await ValidatePublishedParentChainAsync(request.ParentId, cancellationToken);
            if (ancestors is not null) return Failure(ancestors);
        }
        if (await unitOfWork.Repository<Category>().AnyAsync([x => x.Id != entity.Id && x.Slug == request.Slug.Trim()]))
            return TResult<CatalogCategoryManagementDto>.Failure("Category slug already exists.", ErrorCodes.ALREADY_EXISTS);

        entity.UpdateDetails(request.ParentId, request.Name, request.Slug, request.Description, request.DisplayOrder);
        entity.RenewConcurrencyStamp();
        await unitOfWork.Repository<Category>().UpdateAsync(entity, cancellationToken);
        return TResult<CatalogCategoryManagementDto>.Success(Map(entity));
    }

    public Task<TResult<CatalogCategoryManagementDto>> PublishAsync(PublishCatalogCategoryCommand request, CancellationToken cancellationToken) =>
        TransitionAsync(request.CategoryId, request.ConcurrencyStamp, Permissions.CatalogCategories.Publish,
            category => category.Publish(), true, false, cancellationToken);

    public Task<TResult<CatalogCategoryManagementDto>> PauseAsync(PauseCatalogCategoryCommand request, CancellationToken cancellationToken) =>
        TransitionAsync(request.CategoryId, request.ConcurrencyStamp, Permissions.CatalogCategories.Publish,
            category => category.Pause(), false, true, cancellationToken);

    public Task<TResult<CatalogCategoryManagementDto>> HideAsync(HideCatalogCategoryCommand request, CancellationToken cancellationToken) =>
        TransitionAsync(request.CategoryId, request.ConcurrencyStamp, Permissions.CatalogCategories.Deactivate,
            category => category.Hide(), false, true, cancellationToken);

    private async Task<TResult<CatalogCategoryManagementDto>> TransitionAsync(
        Guid id,
        Guid concurrencyStamp,
        string permission,
        Action<Category> transition,
        bool requiresPublishedAncestors,
        bool blocksPublishedDependents,
        CancellationToken cancellationToken)
    {
        var category = await LoadForMutationAsync(id, concurrencyStamp, permission, cancellationToken);
        if (category.Result is not null) return category.Result;
        if (requiresPublishedAncestors)
        {
            var parent = await ValidatePublishedAncestorsAsync(category.Category!, cancellationToken);
            if (parent is not null) return Failure(parent);
        }
        if (blocksPublishedDependents)
        {
            var dependent = await ValidateNoPublishedDependentsAsync(category.Category!, cancellationToken);
            if (dependent is not null) return Failure(dependent);
        }

        transition(category.Category!);
        category.Category!.RenewConcurrencyStamp();
        await unitOfWork.Repository<Category>().UpdateAsync(category.Category!, cancellationToken);
        return TResult<CatalogCategoryManagementDto>.Success(Map(category.Category!));
    }

    private async Task<(Category? Category, TResult<CatalogCategoryManagementDto>? Result)> LoadForMutationAsync(
        Guid id,
        Guid concurrencyStamp,
        string permission,
        CancellationToken cancellationToken)
    {
        var authorization = access.Ensure(permission);
        if (!authorization.IsSuccess) return (null, Failure(authorization));
        var category = await unitOfWork.Repository<Category>().FindByIdAsync(id);
        if (category is null)
            return (null, TResult<CatalogCategoryManagementDto>.Failure(MessageKey.ResourceNotFound, ErrorCodes.NOT_FOUND));
        if (category.ConcurrencyStamp != concurrencyStamp)
            return (null, TResult<CatalogCategoryManagementDto>.Failure(MessageKey.DataHasBeenChanged, ErrorCodes.ALREADY_EXISTS));
        return (category, null);
    }

    private async Task<TResult?> ValidateParentAsync(Guid? parentId, Guid categoryId, CancellationToken cancellationToken)
    {
        if (!parentId.HasValue) return null;
        if (parentId == categoryId)
            return TResult.Failure("A category cannot be its own parent.", ErrorCodes.BAD_REQUEST);

        var parent = await unitOfWork.Repository<Category>().FindByIdAsync(parentId.Value);
        if (parent is null || parent.Status == CatalogStatus.Hidden)
            return TResult.Failure(MessageKey.ResourceNotFound, ErrorCodes.NOT_FOUND);

        var cursor = parent;
        var visited = new HashSet<Guid>();
        while (cursor.ParentId.HasValue)
        {
            if (cursor.ParentId == categoryId)
                return TResult.Failure("Category parent would create a cycle.", ErrorCodes.BAD_REQUEST);
            if (!visited.Add(cursor.Id))
                throw new CommerceDomainException("CATEGORY_PARENT_CYCLE", "The category hierarchy contains a cycle.");
            cursor = await unitOfWork.Repository<Category>().FindByIdAsync(cursor.ParentId.Value)
                ?? throw new CommerceDomainException("CATEGORY_PARENT_NOT_FOUND", "A category parent was not found.");
        }
        return null;
    }

    private async Task<TResult?> ValidatePublishedAncestorsAsync(Category category, CancellationToken cancellationToken)
        => await ValidatePublishedParentChainAsync(category.ParentId, cancellationToken);

    private async Task<TResult?> ValidatePublishedParentChainAsync(Guid? parentId, CancellationToken cancellationToken)
    {
        var visited = new HashSet<Guid>();
        while (parentId.HasValue)
        {
            if (!visited.Add(parentId.Value))
                throw new CommerceDomainException("CATEGORY_PARENT_CYCLE", "The category hierarchy contains a cycle.");
            var parent = await unitOfWork.Repository<Category>().FindByIdAsync(parentId.Value)
                ?? throw new CommerceDomainException("CATEGORY_PARENT_NOT_FOUND", "A category parent was not found.");
            if (parent.Status != CatalogStatus.Published)
                return TResult.Failure("All ancestor categories must be published.", ErrorCodes.UNPROCESSABLE_ENTITY);
            parentId = parent.ParentId;
        }
        return null;
    }

    private async Task<bool> HasPublishedDependentsAsync(Guid categoryId, CancellationToken cancellationToken)
    {
        var categories = await unitOfWork.Repository<Category>().QueryNoTracking()
            .Select(x => new { x.Id, x.ParentId, x.Status }).ToListAsync(cancellationToken);
        var parentById = categories.ToDictionary(x => x.Id, x => x.ParentId);
        var hasPublishedCategory = categories.Any(x => x.Status == CatalogStatus.Published
            && (x.Id == categoryId || IsDescendantOf(x.Id, categoryId, parentById)));
        if (hasPublishedCategory) return true;

        var publishedPrimaryCategoryIds = await (
            from mapping in unitOfWork.Repository<ProductCategory>().QueryNoTracking()
            join product in unitOfWork.Repository<Product>().QueryNoTracking() on mapping.ProductId equals product.Id
            where mapping.IsPrimary && product.Status == ProductStatus.Published
            select mapping.CategoryId).ToListAsync(cancellationToken);

        return publishedPrimaryCategoryIds.Any(id => id == categoryId || IsDescendantOf(id, categoryId, parentById));
    }

    private async Task<TResult?> ValidateNoPublishedDependentsAsync(Category category, CancellationToken cancellationToken)
    {
        var categories = await unitOfWork.Repository<Category>().QueryNoTracking()
            .Select(x => new { x.Id, x.ParentId, x.Status }).ToListAsync(cancellationToken);
        var parentById = categories.ToDictionary(x => x.Id, x => x.ParentId);
        if (categories.Any(x => x.Status == CatalogStatus.Published && IsDescendantOf(x.Id, category.Id, parentById)))
            return TResult.Failure("Pause or hide published child categories before changing this category.", ErrorCodes.UNPROCESSABLE_ENTITY);

        var usedByPublishedProduct = await (
            from mapping in unitOfWork.Repository<ProductCategory>().QueryNoTracking()
            join product in unitOfWork.Repository<Product>().QueryNoTracking() on mapping.ProductId equals product.Id
            where mapping.CategoryId == category.Id && mapping.IsPrimary && product.Status == ProductStatus.Published
            select mapping.Id).AnyAsync(cancellationToken);
        return usedByPublishedProduct
            ? TResult.Failure("Move or unpublish products that use this category as their primary category first.", ErrorCodes.UNPROCESSABLE_ENTITY)
            : null;
    }

    private static bool IsDescendantOf(Guid candidateId, Guid ancestorId, IReadOnlyDictionary<Guid, Guid?> parentById)
    {
        var visited = new HashSet<Guid>();
        var cursor = candidateId;
        while (parentById.TryGetValue(cursor, out var parentId) && parentId.HasValue)
        {
            if (!visited.Add(cursor))
                throw new CommerceDomainException("CATEGORY_PARENT_CYCLE", "The category hierarchy contains a cycle.");
            if (parentId.Value == ancestorId) return true;
            cursor = parentId.Value;
        }
        return false;
    }

    private static TResult<CatalogCategoryManagementDto> Failure(TResult result) =>
        TResult<CatalogCategoryManagementDto>.Failure(result.Error!, result.ErrorCode);

    private static CatalogCategoryManagementDto Map(Category category) =>
        new(category.Id, category.ParentId, category.Name, category.Slug, category.Description,
            category.DisplayOrder, category.Status, category.ConcurrencyStamp);
}
