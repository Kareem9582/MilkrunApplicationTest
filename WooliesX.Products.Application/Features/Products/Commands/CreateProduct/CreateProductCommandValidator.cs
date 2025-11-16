namespace WooliesX.Products.Application.Features.Products.Commands.CreateProduct;

public class CreateProductCommandValidator : AbstractValidator<CreateProductCommand>
{
    public CreateProductCommandValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required.");

        RuleFor(x => x.Description)
            .MaximumLength(100).WithMessage("Description must be 100 characters or fewer.");

        RuleFor(x => x.Price)
            .GreaterThan(0).WithMessage("Price must be greater than 0.");
    }
}

