using MediatR;
using System.Net.ServerSentEvents;
using System.Runtime.CompilerServices;
using WooliesX.Products.Application.Features.Products.Queries.GetAllProducts;
using WooliesX.Products.Application.Features.Products.Queries.GetProducts;


namespace WooliesX.Products.Api.Endpoints.Features.Products;

public static class GetProductsStreamEndpoint
{
    public static IEndpointRouteBuilder MapStreamProductsSse(this IEndpointRouteBuilder group)
    {
        group.MapGet("/stream", (
            IMediator mediator,
            CancellationToken cancellationToken) =>
        {
            async IAsyncEnumerable<SseItem<object>> Stream([EnumeratorCancellation] CancellationToken ct)
            {
                // Encourage headers to flush immediately
                await Task.Yield();

                List<GetProductsResponse> products;
                try
                {
                    products = await mediator.Send(new GetAllProductsQuery(), ct);
                }
                catch (OperationCanceledException)
                {
                    yield break;
                }
                if (products.IsNotEmpty)
                {
                    var startPayload = new { message = "Streaming products started", total = products.Count };
                    yield return new SseItem<object>(startPayload, eventType: "start") { ReconnectionInterval = TimeSpan.FromSeconds(5) };
                    try
                    {
                        await Task.Delay(TimeSpan.FromSeconds(0.5), ct);
                    }
                    catch (OperationCanceledException)
                    {
                        yield break;
                    }
                }
                foreach (var product in products)
                {
                    yield return new SseItem<object>(product!, eventType: "product") { ReconnectionInterval = TimeSpan.FromSeconds(5) };
                    try
                    {
                        await Task.Delay(TimeSpan.FromSeconds(0.5), ct);
                    }
                    catch (OperationCanceledException)
                    {
                        yield break;
                    }
                }
                // send a completion event for clients that care
                yield return new SseItem<object>(new { message = "Streaming complete", total = products.Count }, eventType: "complete") { ReconnectionInterval = TimeSpan.FromSeconds(5) };
               
            }

            return TypedResults.ServerSentEvents(Stream(cancellationToken));
        })
        .WithName("StreamProductsSse");

        return group;
    }
}
