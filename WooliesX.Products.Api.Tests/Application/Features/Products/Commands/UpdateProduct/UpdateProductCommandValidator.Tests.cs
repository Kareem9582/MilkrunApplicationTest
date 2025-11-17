using FluentAssertions;
using WooliesX.Products.Application.Features.Products.Commands.UpdateProduct;

namespace WooliesX.Products.Api.Tests.Application.Features.Products.Commands.UpdateProduct;

public class UpdateProductCommandValidatorTests
{
    [Fact]
    public void Validate_Valid_Request_Succeeds()
    {
        // Arrange
        var validator = new UpdateProductCommandValidator();
        var cmd = new UpdateProductCommand(1, "Title", "Desc", 5m, "Brand", "Cat");

        // Act
        var result = validator.Validate(cmd);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Validate_Invalid_Id_Fails()
    {
        // Arrange
        var validator = new UpdateProductCommandValidator();
        var cmd = new UpdateProductCommand(0, "Title", null, 1m, null, null);

        // Act
        var result = validator.Validate(cmd);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(UpdateProductCommand.Id));
    }

    [Fact]
    public void Validate_Empty_Title_Fails()
    {
        // Arrange
        var validator = new UpdateProductCommandValidator();
        var cmd = new UpdateProductCommand(1, string.Empty, null, 1m, null, null);

        // Act
        var result = validator.Validate(cmd);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(UpdateProductCommand.Title));
    }

    [Fact]
    public void Validate_Too_Long_Description_Fails()
    {
        // Arrange
        var validator = new UpdateProductCommandValidator();
        var cmd = new UpdateProductCommand(1, "Title", new string('x', 101), 1m, null, null);

        // Act
        var result = validator.Validate(cmd);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(UpdateProductCommand.Description));
    }

    [Fact]
    public void Validate_Non_Positive_Price_Fails()
    {
        // Arrange
        var validator = new UpdateProductCommandValidator();
        var cmd = new UpdateProductCommand(1, "Title", null, 0m, null, null);

        // Act
        var result = validator.Validate(cmd);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(UpdateProductCommand.Price));
    }
}

