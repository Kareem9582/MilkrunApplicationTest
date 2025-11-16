using System.Runtime.CompilerServices;               
using System.Net.ServerSentEvents;                   
using MediatR;
using WooliesX.Products.Domain.Entities;
using WooliesX.Products.Application.Features.Products.Queries.GetProducts;

namespace WooliesX.Products.Api.Endpoints.Features.Products;

public static class StreamProductsSseEndpoint
{
    public static IEndpointRouteBuilder MapStreamProductsSse(this IEndpointRouteBuilder group)
    {
        group.MapGet("/stream", (
            IMediator mediator,
            CancellationToken cancellationToken,
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
            // Local async iterator that generates SSE items
            async IAsyncEnumerable<SseItem<Product>> Stream(
                [EnumeratorCancellation] CancellationToken ct)
            {
                while (!ct.IsCancellationRequested)
                {
                    // Get latest products
                    var result = await mediator.Send(
                        new GetProductsQuery(
                            q, title, brand, category,
                            minPrice, maxPrice, minRating,
                            inStock, page, pageSize, sortBy, order),
                        ct);

                    foreach (var product in result.Items)
                    {
                        yield return new SseItem<Product>(product, eventType: "product")
                        {
                            // Client reconnection hint
                            ReconnectionInterval = TimeSpan.FromSeconds(5)
                        };
                    }

                    // Wait before sending next batch
                    await Task.Delay(TimeSpan.FromSeconds(2), ct);
                }
            }

            return TypedResults.ServerSentEvents(Stream(cancellationToken));
        })
        .WithName("StreamProductsSse");

        return group;
    }
}
