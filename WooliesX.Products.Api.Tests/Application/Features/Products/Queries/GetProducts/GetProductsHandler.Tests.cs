using AutoMapper;
using FluentAssertions;
using WooliesX.Products.Api.Tests.Factories;
using WooliesX.Products.Application.Features.Products.Mapping;
using WooliesX.Products.Application.Features.Products.Queries.GetProducts;
using WooliesX.Products.Domain.Entities;

namespace WooliesX.Products.Api.Tests.Application.Features.Products.Queries.GetProducts;

public class GetProductsHandlerTests
{
    private static IMapper CreateMapper()
    {
        var config = new MapperConfiguration(cfg => cfg.AddProfile<ProductMappingProfile>());
        return config.CreateMapper();
    }

    [Fact]
    public async Task Handle_Filters_By_Search_And_Sorts_And_Paginates()
    {
        // Arrange
        var repo = TestRepositoryFactory.CreateProductsRepository();
        repo.Add(new Product { Title = "Apple iPhone", Brand = "Apple", Category = "Phones", Price = 1200, Rating = 4.8m, Stock = 5 });
        repo.Add(new Product { Title = "Galaxy S", Brand = "Samsung", Category = "Phones", Price = 900, Rating = 4.5m, Stock = 0 });
        repo.Add(new Product { Title = "Pixel", Brand = "Google", Category = "Phones", Price = 800, Rating = 4.2m, Stock = 3 });
        var mapper = CreateMapper();
        var handler = new GetProductsHandler(repo, mapper);

        var query = new GetProductsQuery(
            SearchTerm: "apple", // matches title/brand
            Title: null,
            Brand: null,
            Category: null,
            MinPrice: 1000,
            MaxPrice: null,
            MinRating: 4.5m,
            InStock: true,
            Page: 1,
            PageSize: 10,
            SortBy: "price",
            Order: "desc");

        // Act
        var result = await handler.Handle(query, default);

        // Assert
        result.Total.Should().Be(1);
        result.Items.Should().HaveCount(1);
        result.Items[0].Title.Should().Contain("Apple");
    }

    [Fact]
    public async Task Handle_Filters_By_Lists_And_Paginates()
    {
        // Arrange
        var repo = TestRepositoryFactory.CreateProductsRepository();
        repo.Add(new Product { Title = "A", Brand = "B1", Category = "C1", Price = 10 });
        repo.Add(new Product { Title = "B", Brand = "B2", Category = "C2", Price = 20 });
        repo.Add(new Product { Title = "C", Brand = "B1", Category = "C2", Price = 30 });
        var mapper = CreateMapper();
        var handler = new GetProductsHandler(repo, mapper);

        var query = new GetProductsQuery(
            SearchTerm: null,
            Title: "A,C",
            Brand: "B1",
            Category: "C2",
            MinPrice: null,
            MaxPrice: 30,
            MinRating: null,
            InStock: null,
            Page: 1,
            PageSize: 1,
            SortBy: "title",
            Order: "asc");

        // Act
        var result = await handler.Handle(query, default);

        // Assert
        result.Total.Should().Be(1);
        result.Items.Should().HaveCount(1);
        result.Items[0].Title.Should().Be("C");
    }
}

