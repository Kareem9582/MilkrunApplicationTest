namespace WooliesX.Products.Application.Features.Products.Commands.CreateProduct;

public class ProductCreateRequest
{
    //Using this hear for .Net 10 Demo Purpose only . we usually relay on validation Layers . 
    public string Title 
    { 
        get;
        set => field = string.IsNullOrWhiteSpace(value)
            ? throw new ArgumentException("Title cannot be empty")
            : value.Trim(); // Access backing field with 'field' keyword
    } = string.Empty;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public string? Brand { get; set; }
    public string? Category { get; set; }
}
