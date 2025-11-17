using AutoMapper;
using FluentAssertions;
using WooliesX.Products.Api.Tests.Factories;
using WooliesX.Products.Application.Features.Products.Mapping;
using WooliesX.Products.Application.Features.Products.Queries.GetAllProducts;
using WooliesX.Products.Application.Features.Products.Queries.GetProducts;
using WooliesX.Products.Domain.Entities;

namespace WooliesX.Products.Api.Tests.Application.Features.Products.Queries.GetAllProducts;

public class GetAllProductsHandlerTests
{
    private static IMapper CreateMapper()
    {
        var config = new MapperConfiguration(cfg => cfg.AddProfile<ProductMappingProfile>());
        return config.CreateMapper();
    }

    [Fact]
    public async Task Handle_Returns_All_Products_Mapped()
    {
        // Arrange
        var repo = TestRepositoryFactory.CreateProductsRepository();
        repo.Add(new Product { Title = "A", Price = 1, Brand = "B1", Category = "C1" });
        repo.Add(new Product { Title = "B", Price = 2, Brand = "B2", Category = "C2" });
        var mapper = CreateMapper();
        var handler = new GetAllProductsHandler(repo, mapper);

        // Act
        var result = await handler.Handle(new GetAllProductsQuery(), default);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result[0].Title.Should().Be("A");
        result[1].Title.Should().Be("B");
    }
}

