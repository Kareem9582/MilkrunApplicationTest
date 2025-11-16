using MediatR;
using WooliesX.Products.Application.Features.Products.Commands.CreateProduct;

namespace WooliesX.Products.Api.Endpoints.Features.Products;

public static class CreateProductEndpoint
{
    public static IEndpointRouteBuilder MapCreateProduct(this IEndpointRouteBuilder group)
    {
        group.MapPost("", async (ProductCreateRequest? request, IMediator mediator) =>
        {
            if (request is null)
            {
                throw new MissingBodyException();
            }
            var product = await mediator.Send(new CreateProductCommand(request.Title, request.Description, request.Price, request.Brand, request.Category));
            return Results.Created($"/products/{product.Id}", product);
        })
        .RequireAuthorization()
        .WithName("AddProduct");

        return group;
    }
}
