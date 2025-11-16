using System.Text.Json.Serialization;

namespace WooliesX.Products.Application.Features.Products.Commands.UpdateProduct;

public record UpdateProductResponse(
    [property: JsonPropertyName("id")] int Id,
    [property: JsonPropertyName("title")] string Title,
    [property: JsonPropertyName("description")] string? Description,
    [property: JsonPropertyName("price")] decimal Price,
    [property: JsonPropertyName("discountPercentage")] decimal? DiscountPercentage,
    [property: JsonPropertyName("rating")] decimal? Rating,
    [property: JsonPropertyName("stock")] int? Stock,
    [property: JsonPropertyName("brand")] string? Brand,
    [property: JsonPropertyName("category")] string? Category,
    [property: JsonPropertyName("thumbnail")] string? Thumbnail,
    [property: JsonPropertyName("images")] List<string>? Images
);

