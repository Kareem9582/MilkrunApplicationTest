using FluentAssertions;
using WooliesX.Products.Application.Features.Products.Commands.CreateProduct;

namespace WooliesX.Products.Api.Tests.Application.Features.Products.Commands.CreateProduct;

public class CreateProductCommandValidatorTests
{
    [Fact]
    public void Validate_Valid_Request_Succeeds()
    {
        // Arrange
        var validator = new CreateProductCommandValidator();
        var cmd = new CreateProductCommand("Widget", "Nice", 10m, "Acme", "Gadgets");

        // Act
        var result = validator.Validate(cmd);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Validate_Empty_Title_Fails()
    {
        // Arrange
        var validator = new CreateProductCommandValidator();
        var cmd = new CreateProductCommand(string.Empty, "desc", 1m, null, null);

        // Act
        var result = validator.Validate(cmd);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateProductCommand.Title));
    }

    [Fact]
    public void Validate_Too_Long_Description_Fails()
    {
        // Arrange
        var validator = new CreateProductCommandValidator();
        var cmd = new CreateProductCommand("Title", new string('x', 101), 1m, null, null);

        // Act
        var result = validator.Validate(cmd);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateProductCommand.Description));
    }

    [Fact]
    public void Validate_Non_Positive_Price_Fails()
    {
        // Arrange
        var validator = new CreateProductCommandValidator();
        var cmd = new CreateProductCommand("Title", null, 0m, null, null);

        // Act
        var result = validator.Validate(cmd);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateProductCommand.Price));
    }
}

