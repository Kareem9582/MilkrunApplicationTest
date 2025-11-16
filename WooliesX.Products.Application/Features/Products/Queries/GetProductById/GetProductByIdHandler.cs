namespace WooliesX.Products.Application.Features.Products.Queries.GetProductById;

public class GetProductByIdHandler : IRequestHandler<GetProductByIdQuery, Product?>
{
    private readonly IProductsRepository _repo;
    private readonly IValidator<GetProductByIdQuery> _validator;
    public GetProductByIdHandler(IProductsRepository repo, IValidator<GetProductByIdQuery> validator)
    {
        _repo = repo;
        _validator = validator;
    }

    public Task<Product?> Handle(GetProductByIdQuery request, CancellationToken cancellationToken = default)
    {
        var result = _validator.Validate(request);
        if (!result.IsValid)
        {
            var dict = result.Errors
                .GroupBy(e => e.PropertyName ?? "request", StringComparer.OrdinalIgnoreCase)
                .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray(), StringComparer.OrdinalIgnoreCase);
            throw new ValidationException(dict);
        }

        return Task.FromResult(_repo.GetById(request.Id));
    }
}
