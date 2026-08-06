namespace Ecom.Application.Features.Catalog.Products.Commands.UpdateProductDetails;

public sealed class UpdateProductDetailsCommandValidator : AbstractValidator<UpdateProductDetailsCommand>
{
    public UpdateProductDetailsCommandValidator()
    {
        RuleFor(x => x.ProductId).NotEmpty(); RuleFor(x => x.ConcurrencyStamp).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(300); RuleFor(x => x.Slug).NotEmpty().MaximumLength(350);
        RuleFor(x => x.ShortDescription).MaximumLength(1000); RuleFor(x => x.MetaTitle).MaximumLength(255); RuleFor(x => x.MetaDescription).MaximumLength(500);
    }
}
