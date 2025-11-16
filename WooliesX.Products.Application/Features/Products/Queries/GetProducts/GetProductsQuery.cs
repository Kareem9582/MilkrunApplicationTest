namespace WooliesX.Products.Application.Features.Products.Queries.GetProducts;

public record GetProductsQuery(
    string? Q,
    string? Title,
    string? Brand,
    string? Category,
    decimal? MinPrice,
    decimal? MaxPrice,
    decimal? MinRating,
    bool? InStock,
    int? Page,
    int? PageSize,
    string? SortBy,
    string? Order) : IRequest<GetProductsResult>;

public record GetProductsResult(int Total, int Page, int PageSize, List<Product> Items);

