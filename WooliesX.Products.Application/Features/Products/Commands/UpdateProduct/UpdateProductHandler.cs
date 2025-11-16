using AutoMapper;

namespace WooliesX.Products.Application.Features.Products.Commands.UpdateProduct;

public class UpdateProductHandler : IRequestHandler<UpdateProductCommand, UpdateProductResponse?>
{
    private readonly IProductsRepository _repo;
    private readonly IValidator<UpdateProductCommand> _validator;
    private readonly IMapper _mapper;
    public UpdateProductHandler(IProductsRepository repo, IValidator<UpdateProductCommand> validator, IMapper mapper)
    {
        _repo = repo;
        _validator = validator;
        _mapper = mapper;
    }

    public Task<UpdateProductResponse?> Handle(UpdateProductCommand r, CancellationToken cancellationToken = default)
    {
        var result = _validator.Validate(r);
        if (!result.IsValid)
        {
            var dict = result.Errors
                .GroupBy(e => e.PropertyName ?? "request", StringComparer.OrdinalIgnoreCase)
                .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray(), StringComparer.OrdinalIgnoreCase);
            throw new ValidationException(dict);
        }

        var existing = _repo.GetById(r.Id);
        if (existing is null) return Task.FromResult<UpdateProductResponse?>(null);

        if (_repo.ExistsDuplicate(r.Title, r.Brand, r.Id))
            throw new DuplicateProductException(r.Title, r.Brand);

        existing.Title = r.Title.Trim();
        existing.Description = r.Description?.Trim();
        existing.Price = r.Price;
        existing.Brand = r.Brand?.Trim();
        existing.Category = r.Category?.Trim();

        var updated = _repo.Update(r.Id, existing);
        if (!updated) return Task.FromResult<UpdateProductResponse?>(null);
        var response = _mapper.Map<UpdateProductResponse>(existing);
        return Task.FromResult<UpdateProductResponse?>(response);
    }
}
