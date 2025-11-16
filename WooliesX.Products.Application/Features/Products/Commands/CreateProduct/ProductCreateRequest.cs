namespace WooliesX.Products.Application.Features.Products.Commands.CreateProduct;

public class ProductCreateRequest
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public string? Brand { get; set; }
    public string? Category { get; set; }
}

