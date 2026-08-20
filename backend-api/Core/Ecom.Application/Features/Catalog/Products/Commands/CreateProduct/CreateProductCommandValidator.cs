namespace Ecom.Application.Features.Catalog.Products.Commands.CreateProduct;

public sealed class CreateProductCommandValidator : AbstractValidator<CreateProductCommand>
{
    public CreateProductCommandValidator()
    {
        RuleFor(x => x.ProducerId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(300);
        RuleFor(x => x.BrandName).MaximumLength(200);
        RuleFor(x => x.Slug).NotEmpty().MaximumLength(350);
        RuleFor(x => x.ShortDescription).MaximumLength(1000);
        RuleFor(x => x.MetaTitle).MaximumLength(255);
        RuleFor(x => x.MetaDescription).MaximumLength(500);
    }
}
