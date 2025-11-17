namespace WooliesX.Products.Api.Extensions
{
    public static class EnumerableExtensions
    {
        // Extension block for IEnumerable<T>
        extension<T>(IEnumerable<T> source)
        {
            // Using this hear for .Net 10 Demo Purpose only
            public bool IsEmpty => !source.Any();
            public bool IsNotEmpty => source.Any();
        }
    }
}
