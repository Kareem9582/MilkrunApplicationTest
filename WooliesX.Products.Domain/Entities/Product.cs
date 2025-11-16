using System.Text.Json.Serialization;

namespace WooliesX.Products.Domain.Entities;

public class Product
{
    [JsonPropertyName("id")] public int Id { get; set; }
    [JsonPropertyName("title")] public string Title { get; set; } = string.Empty;
    [JsonPropertyName("description")] public string? Description { get; set; }
    [JsonPropertyName("price")] public decimal Price { get; set; }
    [JsonPropertyName("discountPercentage")] public decimal? DiscountPercentage { get; set; }
    [JsonPropertyName("rating")] public decimal? Rating { get; set; }
    [JsonPropertyName("stock")] public int? Stock { get; set; }
    [JsonPropertyName("brand")] public string? Brand { get; set; }
    [JsonPropertyName("category")] public string? Category { get; set; }
    [JsonPropertyName("thumbnail")] public string? Thumbnail { get; set; }
    [JsonPropertyName("images")] public List<string>? Images { get; set; }
}

