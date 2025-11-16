using AutoMapper;

namespace WooliesX.Products.Application.Features.Products.Queries.GetProductById;

public class GetProductByIdHandler : IRequestHandler<GetProductByIdQuery, GetProductByIdResponse?>
{
    private readonly IProductsRepository _repo;
    private readonly IValidator<GetProductByIdQuery> _validator;
    private readonly IMapper _mapper;
    public GetProductByIdHandler(IProductsRepository repo, IValidator<GetProductByIdQuery> validator, IMapper mapper)
    {
        _repo = repo;
        _validator = validator;
        _mapper = mapper;
    }

    public Task<GetProductByIdResponse?> Handle(GetProductByIdQuery request, CancellationToken cancellationToken = default)
    {
        var result = _validator.Validate(request);
        if (!result.IsValid)
        {
            var dict = result.Errors
                .GroupBy(e => e.PropertyName ?? "request", StringComparer.OrdinalIgnoreCase)
                .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray(), StringComparer.OrdinalIgnoreCase);
            throw new ValidationException(dict);
        }

        var product = _repo.GetById(request.Id);
        var mapped = product is null ? null : _mapper.Map<GetProductByIdResponse>(product);
        return Task.FromResult(mapped);
    }
}
