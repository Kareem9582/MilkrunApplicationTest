using AutoMapper;
using WooliesX.Products.Application.Features.Products.Queries.GetProductById;
using WooliesX.Products.Application.Features.Products.Queries.GetProducts;
using WooliesX.Products.Domain.Entities;

namespace WooliesX.Products.Application.Features.Products.Mapping;

public class ProductMappingProfile : Profile
{
    public ProductMappingProfile()
    {
        CreateMap<Product, GetProductsResponse>();
        CreateMap<Product, GetProductByIdResponse>();
    }
}
