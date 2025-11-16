namespace WooliesX.Products.Api.Extensions;

public static class EndpointMappingExtensions
{
    public static WebApplication MapAppEndpoints(this WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
        }

        Endpoints.Features.Auth.AuthEndpoints.MapAuthEndpoints(app);
        Endpoints.Features.Products.ProductsEndpoints.MapProductsEndpoints(app);
        return app;
    }
}
