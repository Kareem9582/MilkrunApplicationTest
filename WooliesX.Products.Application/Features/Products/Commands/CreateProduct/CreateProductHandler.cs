

namespace WooliesX.Products.Application.Features.Products.Commands.CreateProduct;

public class CreateProductHandler : IRequestHandler<CreateProductCommand, Product>
{
    private readonly IProductsRepository _repo;
    private readonly IValidator<CreateProductCommand> _validator;
    public CreateProductHandler(IProductsRepository repo, IValidator<CreateProductCommand> validator)
    {
        _repo = repo;
        _validator = validator;
    }

    public Task<Product> Handle(CreateProductCommand r, CancellationToken cancellationToken = default)
    {
        var result = _validator.Validate(r);
        if (!result.IsValid)
        {
            var dict = result.Errors
                .GroupBy(e => e.PropertyName ?? "request", StringComparer.OrdinalIgnoreCase)
                .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray(), StringComparer.OrdinalIgnoreCase);
            throw new ValidationException(dict);
        }

        if (_repo.ExistsDuplicate(r.Title, r.Brand, null))
            throw new DuplicateProductException(r.Title, r.Brand);

        var product = new Product
        {
            Title = r.Title.Trim(),
            Description = r.Description?.Trim(),
            Price = r.Price,
            Brand = r.Brand?.Trim(),
            Category = r.Category?.Trim()
        };
        _repo.Add(product);
        return Task.FromResult(product);
    }
}
