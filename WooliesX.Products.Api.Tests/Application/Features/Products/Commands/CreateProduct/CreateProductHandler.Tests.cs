using AutoMapper;
using FluentAssertions;
using WooliesX.Products.Application.Exceptions;
using WooliesX.Products.Application.Features.Products.Commands.CreateProduct;
using WooliesX.Products.Application.Features.Products.Mapping;
using WooliesX.Products.Domain.Entities;
using WooliesX.Products.Api.Tests.Factories;
using WooliesX.Products.Infrastructure.Contracts;

namespace WooliesX.Products.Api.Tests.Application.Features.Products.Commands.CreateProduct;

public class CreateProductHandlerTests
{
    private readonly IProductsRepository _repo;
    private readonly CreateProductCommandValidator _validator;
    private readonly IMapper _mapper;
    private readonly CreateProductHandler _handler;

    public CreateProductHandlerTests()
    {
        var config = new MapperConfiguration(cfg => cfg.AddProfile<ProductMappingProfile>());
        _mapper = config.CreateMapper();
        _validator = new CreateProductCommandValidator();
        _repo = TestRepositoryFactory.CreateProductsRepository();
        _handler = new CreateProductHandler(_repo, _validator, _mapper);
    }

    [Fact]
    public async Task Handle_Returns_Mapped_Response_And_Trims_Fields()
    {
        // Arrange
        var cmd = new CreateProductCommand("  Widget  ", "  Nice thing  ", 9.99m, "  Acme  ", "  Gadgets  ");

        // Act
        var response = await _handler.Handle(cmd);

        // Assert
        response.Should().NotBeNull();
        response.Id.Should().BeGreaterThan(0);
        response.Title.Should().Be("Widget");
        response.Description.Should().Be("Nice thing");
        response.Price.Should().Be(9.99m);
        response.Brand.Should().Be("Acme");
        response.Category.Should().Be("Gadgets");

        var saved = _repo.GetById(response.Id);
        saved.Should().NotBeNull();
        saved!.Title.Should().Be("Widget");
        saved.Description.Should().Be("Nice thing");
        saved.Price.Should().Be(9.99m);
        saved.Brand.Should().Be("Acme");
        saved.Category.Should().Be("Gadgets");
    }

    [Fact]
    public async Task Handle_Throws_DuplicateProductException_When_Duplicate_Exists()
    {
        // Arrange
        // Seed an existing matching product to trigger duplicate
        _repo.Add(new Product { Title = "widget", Brand = "acme", Price = 1 });
        var cmd = new CreateProductCommand("widget", null, 10m, "  acme ", null);

        // Act
        var act = async () => await _handler.Handle(cmd);

        // Assert
        await act.Should().ThrowAsync<DuplicateProductException>();
    }

}
