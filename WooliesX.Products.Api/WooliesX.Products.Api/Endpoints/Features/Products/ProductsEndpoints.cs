namespace WooliesX.Products.Api.Endpoints.Features.Products;

public static class ProductsEndpoints
{
    public static IEndpointRouteBuilder MapProductsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/products");

        group
            .MapListProducts()
            .MapGetProductById()
            .MapCreateProduct()
            .MapUpdateProduct()
            .MapStreamProductsSse();

        return app;
    }
}
