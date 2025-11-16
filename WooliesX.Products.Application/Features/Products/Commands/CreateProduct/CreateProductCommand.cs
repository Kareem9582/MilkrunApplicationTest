namespace WooliesX.Products.Application.Features.Products.Commands.CreateProduct;

public record CreateProductCommand(string Title, string? Description, decimal Price, string? Brand, string? Category) : IRequest<AddProductResponse>;
