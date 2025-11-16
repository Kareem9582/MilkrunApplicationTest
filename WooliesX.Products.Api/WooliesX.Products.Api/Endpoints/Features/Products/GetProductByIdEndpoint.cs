using MediatR;
using WooliesX.Products.Application.Features.Products.Queries.GetProductById;

namespace WooliesX.Products.Api.Endpoints.Features.Products;

public static class GetProductByIdEndpoint
{
    public static IEndpointRouteBuilder MapGetProductById(this IEndpointRouteBuilder group)
    {
        group.MapGet("/{id:int}", async (int id, IMediator mediator) =>
        {
            var product = await mediator.Send(new GetProductByIdQuery(id));
            return product is not null ? Results.Ok(product) : Results.NotFound();
        })
        .WithName("GetProductById")
        ;

        return group;
    }
}
