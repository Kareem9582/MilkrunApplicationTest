namespace WooliesX.Products.Application.Features.Products.Queries.GetProductById;

public record GetProductByIdQuery(int Id) : IRequest<Product?>;
