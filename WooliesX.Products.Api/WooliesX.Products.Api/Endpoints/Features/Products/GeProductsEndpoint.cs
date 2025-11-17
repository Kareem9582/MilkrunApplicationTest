using MediatR;
using WooliesX.Products.Application.Features.Products.Queries.GetProducts;

namespace WooliesX.Products.Api.Endpoints.Features.Products;

public static class GetProductsEndpoint
{
    public static IEndpointRouteBuilder MapListProducts(this IEndpointRouteBuilder group)
    {

        group.MapGet("", async (
            HttpContext http,
            IMediator mediator,
            string? q,
            string? title,
            string? brand,
            string? category,
            decimal? minPrice,
            decimal? maxPrice,
            decimal? minRating,
            bool? inStock,
            int? page,
            int? pageSize,
            string? sortBy,
            string? order) =>
        {
            var result = await mediator.Send(new GetProductsQuery(q, title, brand, category, minPrice, maxPrice, minRating, inStock, page, pageSize, sortBy, order));

            return Results.Ok(new { total = result.Total, page = result.Page, pageSize = result.PageSize, items = result.Items });
        })
        .WithName("GetProducts")
        ;

        return group;
    }
}
