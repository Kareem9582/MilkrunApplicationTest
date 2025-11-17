using AutoMapper;
using FluentAssertions;
using WooliesX.Products.Api.Tests.Factories;
using WooliesX.Products.Application.Exceptions;
using WooliesX.Products.Application.Features.Products.Mapping;
using WooliesX.Products.Application.Features.Products.Queries.GetProductById;
using WooliesX.Products.Domain.Entities;

namespace WooliesX.Products.Api.Tests.Application.Features.Products.Queries.GetProductById;

public class GetProductByIdHandlerTests
{
    private static IMapper CreateMapper()
    {
        var config = new MapperConfiguration(cfg => cfg.AddProfile<ProductMappingProfile>());
        return config.CreateMapper();
    }

    [Fact]
    public async Task Handle_Returns_Mapped_Product_When_Found()
    {
        // Arrange
        var repo = TestRepositoryFactory.CreateProductsRepository();
        repo.Add(new Product { Title = "Item", Price = 5, Brand = "B" });
        var id = repo.GetAll().First().Id;
        var validator = new GetProductByIdQueryValidator();
        var mapper = CreateMapper();
        var handler = new GetProductByIdHandler(repo, validator, mapper);

        // Act
        var result = await handler.Handle(new GetProductByIdQuery(id), default);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(id);
        result.Title.Should().Be("Item");
    }

    [Fact]
    public async Task Handle_Returns_Null_When_Not_Found()
    {
        // Arrange
        var repo = TestRepositoryFactory.CreateProductsRepository();
        var validator = new GetProductByIdQueryValidator();
        var mapper = CreateMapper();
        var handler = new GetProductByIdHandler(repo, validator, mapper);

        // Act
        var result = await handler.Handle(new GetProductByIdQuery(999), default);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task Handle_Throws_ValidationException_For_Invalid_Id()
    {
        // Arrange
        var repo = TestRepositoryFactory.CreateProductsRepository();
        var validator = new GetProductByIdQueryValidator();
        var mapper = CreateMapper();
        var handler = new GetProductByIdHandler(repo, validator, mapper);

        // Act
        var act = async () => await handler.Handle(new GetProductByIdQuery(0), default);

        // Assert
        await act.Should().ThrowAsync<ValidationException>();
    }
}

