using AutoMapper;
using FluentAssertions;
using WooliesX.Products.Application.Exceptions;
using WooliesX.Products.Application.Features.Products.Commands.UpdateProduct;
using WooliesX.Products.Application.Features.Products.Mapping;
using WooliesX.Products.Api.Tests.Factories;
using WooliesX.Products.Domain.Entities;

namespace WooliesX.Products.Api.Tests.Application.Features.Products.Commands.UpdateProduct;

public class UpdateProductHandlerTests
{
    private static IMapper CreateMapper()
    {
        var config = new MapperConfiguration(cfg => cfg.AddProfile<ProductMappingProfile>());
        return config.CreateMapper();
    }

    [Fact]
    public async Task Handle_Returns_Null_When_Not_Found()
    {
        // Arrange
        var repo = TestRepositoryFactory.CreateProductsRepository();
        var validator = new UpdateProductCommandValidator();
        var mapper = CreateMapper();
        var handler = new UpdateProductHandler(repo, validator, mapper);

        var cmd = new UpdateProductCommand(999, "X", null, 1m, null, null);

        // Act
        var result = await handler.Handle(cmd);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task Handle_Throws_DuplicateProductException_When_Duplicate_Exists()
    {
        // Arrange
        var repo = TestRepositoryFactory.CreateProductsRepository();
        var p1 = new Product { Title = "First", Brand = "Acme", Price = 1 };
        var p2 = new Product { Title = "Second", Brand = "BrandX", Price = 2 };
        repo.Add(p1);
        repo.Add(p2);
        var id1 = repo.GetAll().First(p => p.Title == "First").Id;
        var validator = new UpdateProductCommandValidator();
        var mapper = CreateMapper();
        var handler = new UpdateProductHandler(repo, validator, mapper);

        // attempt to update p1 to same title/brand as p2
        var cmd = new UpdateProductCommand(id1, " second ", null, 3m, " brandx ", null);

        // Act
        var act = async () => await handler.Handle(cmd);

        // Assert
        await act.Should().ThrowAsync<DuplicateProductException>();
    }

    [Fact]
    public async Task Handle_Updates_And_Maps_Response_With_Trimmed_Values()
    {
        // Arrange
        var repo = TestRepositoryFactory.CreateProductsRepository();
        repo.Add(new Product { Title = "Old", Description = "Desc", Brand = " B ", Category = " C ", Price = 5 });
        var id = repo.GetAll().First(p => p.Title == "Old").Id;
        var validator = new UpdateProductCommandValidator();
        var mapper = CreateMapper();
        var handler = new UpdateProductHandler(repo, validator, mapper);

        var cmd = new UpdateProductCommand(id, "  New  ", "  NewDesc  ", 7.5m, "  NewBrand  ", "  NewCat  ");

        // Act
        var result = await handler.Handle(cmd);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(id);
        result.Title.Should().Be("New");
        result.Description.Should().Be("NewDesc");
        result.Price.Should().Be(7.5m);
        result.Brand.Should().Be("NewBrand");
        result.Category.Should().Be("NewCat");

        var saved = repo.GetById(id);
        saved!.Title.Should().Be("New");
        saved.Description.Should().Be("NewDesc");
        saved.Price.Should().Be(7.5m);
        saved.Brand.Should().Be("NewBrand");
        saved.Category.Should().Be("NewCat");
    }

    

}
