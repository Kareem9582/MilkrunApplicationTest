namespace WooliesX.Products.Application.Features.Products.Commands.UpdateProduct;

public record UpdateProductCommand(int Id, string Title, string? Description, decimal Price, string? Brand, string? Category) : IRequest<Product?>;

