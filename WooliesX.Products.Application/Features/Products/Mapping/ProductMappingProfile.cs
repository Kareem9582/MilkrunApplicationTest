using AutoMapper;
using WooliesX.Products.Application.Features.Products.Commands.CreateProduct;
using WooliesX.Products.Application.Features.Products.Commands.UpdateProduct;
using WooliesX.Products.Application.Features.Products.Queries.GetProductById;
using WooliesX.Products.Application.Features.Products.Queries.GetProducts;

namespace WooliesX.Products.Application.Features.Products.Mapping;

public class ProductMappingProfile : Profile
{
    public ProductMappingProfile()
    {
        CreateMap<Product, GetProductsResponse>();
        CreateMap<Product, GetProductByIdResponse>();
        CreateMap<Product, AddProductResponse>();
        CreateMap<Product, UpdateProductResponse>();
    }
}
