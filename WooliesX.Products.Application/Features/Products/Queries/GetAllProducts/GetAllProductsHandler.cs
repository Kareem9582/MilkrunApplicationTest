using AutoMapper;
using WooliesX.Products.Application.Features.Products.Queries.GetProducts;

namespace WooliesX.Products.Application.Features.Products.Queries.GetAllProducts;

public class GetAllProductsHandler : IRequestHandler<GetAllProductsQuery, List<GetProductsResponse>>
{
    private readonly IProductsRepository _repo;
    private readonly IMapper _mapper;
    public GetAllProductsHandler(IProductsRepository repo, IMapper mapper)
    {
        _repo = repo;
        _mapper = mapper;
    }

    public Task<List<GetProductsResponse>> Handle(GetAllProductsQuery request, CancellationToken cancellationToken)
    {
        var items = _repo.GetAll();
        var mapped = _mapper.Map<List<GetProductsResponse>>(items);
        return Task.FromResult(mapped);
    }
}
