using WooliesX.Products.Application.Features.Products.Queries.GetProducts;

namespace WooliesX.Products.Application.Features.Products.Queries.GetAllProducts;

public record GetAllProductsQuery() : IRequest<List<GetProductsResponse>>;
