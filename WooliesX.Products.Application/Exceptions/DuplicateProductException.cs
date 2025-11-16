namespace WooliesX.Products.Application.Exceptions;

public class DuplicateProductException : Exception
{
    public string TitleValue { get; }
    public string BrandValue { get; }

    public DuplicateProductException(string title, string? brand)
        : base($"Duplicate product: '{title}' with brand '{brand ?? string.Empty}'")
    {
        TitleValue = title;
        BrandValue = brand ?? string.Empty;
    }
}
