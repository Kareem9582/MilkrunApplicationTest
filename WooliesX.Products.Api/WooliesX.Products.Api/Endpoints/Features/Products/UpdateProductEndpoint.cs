using MediatR;
using WooliesX.Products.Application.Features.Products.Commands.UpdateProduct;

namespace WooliesX.Products.Api.Endpoints.Features.Products;

public static class UpdateProductEndpoint
{
    public static IEndpointRouteBuilder MapUpdateProduct(this IEndpointRouteBuilder group)
    {
        group.MapPut("/{id:int}", async (int id, UpdateRequest? request, IMediator mediator) =>
        {
            if (request is null)
            {
                throw new MissingBodyException();
            }
            var updated = await mediator.Send(new UpdateProductCommand(id, request.Title, request.Description, request.Price, request.Brand, request.Category));
            return updated is not null ? Results.Ok(updated) : Results.NotFound();
        })
        .RequireAuthorization()
        .WithName("UpdateProduct")
        ;

        return group;
    }
}
