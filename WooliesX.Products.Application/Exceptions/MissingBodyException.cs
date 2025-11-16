namespace WooliesX.Products.Application.Exceptions;

public class MissingBodyException : Exception
{
    public MissingBodyException(string? message = null)
        : base(message ?? "Request body is required.") { }
}

