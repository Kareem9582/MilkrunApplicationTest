using AutoMapper;

namespace WooliesX.Products.Application.Features.Products.Commands.CreateProduct;

public class CreateProductHandler : IRequestHandler<CreateProductCommand, AddProductResponse>
{
    private readonly IProductsRepository _repo;
    private readonly IValidator<CreateProductCommand> _validator;
    private readonly IMapper _mapper;
    public CreateProductHandler(IProductsRepository repo, IValidator<CreateProductCommand> validator, IMapper mapper)
    {
        _repo = repo;
        _validator = validator;
        _mapper = mapper;
    }

    public Task<AddProductResponse> Handle(CreateProductCommand r, CancellationToken cancellationToken = default)
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
        var response = _mapper.Map<AddProductResponse>(product);
        return Task.FromResult(response);
    }
}
