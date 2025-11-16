using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Collections.Concurrent;
using System.Text.Json;
using System.Text.Json.Serialization;
using WooliesX.Products.Domain.Entities;
using WooliesX.Products.Infrastructure.Contracts;

namespace WooliesX.Products.Infrastructure.Persistance;

public class JsonSeededInMemoryProductsRepository : IProductsRepository
{
    private readonly ConcurrentDictionary<int, Product> _products = new();
    private int _nextId;

    public JsonSeededInMemoryProductsRepository(IOptions<DataOptions> options, IHostEnvironment env, ILogger<JsonSeededInMemoryProductsRepository> logger)
    {
        var path = options.Value.ProductsPath;
        if (string.IsNullOrWhiteSpace(path))
        {
            path = Path.Combine(env.ContentRootPath, "Products.json");
        }
        else if (!Path.IsPathFullyQualified(path))
        {
            path = Path.Combine(env.ContentRootPath, path);
        }

        if (!File.Exists(path))
        {
            logger.LogWarning("Products JSON not found at {Path}. Starting with empty set.", path);
            _nextId = 1;
            return;
        }

        try
        {
            var json = File.ReadAllText(path);
            var data = JsonSerializer.Deserialize<ProductsContainer>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (data?.Products != null)
            {
                foreach (var p in data.Products)
                {
                    _products[p.Id] = p;
                }
                _nextId = _products.Keys.DefaultIfEmpty(0).Max();
            }
            else
            {
                _nextId = 0;
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to read products JSON. Starting with empty set.");
            _nextId = 0;
        }
    }

    public IReadOnlyList<Product> GetAll() => _products.Values.OrderBy(p => p.Id).ToList();

    public Product? GetById(int id) => _products.TryGetValue(id, out var p) ? p : null;

    public void Add(Product product)
    {
        product.Id = Interlocked.Increment(ref _nextId);
        _products[product.Id] = product;
    }

    public bool Update(int id, Product product)
    {
        if (!_products.ContainsKey(id)) return false;
        product.Id = id;
        _products[id] = product;
        return true;
    }

    public bool ExistsDuplicate(string title, string? brand, int? excludeId)
    {
        var t = title?.Trim() ?? string.Empty;
        var b = brand?.Trim() ?? string.Empty;
        return _products.Values.Any(p =>
            (!excludeId.HasValue || p.Id != excludeId.Value) &&
            string.Equals(p.Title?.Trim(), t, StringComparison.OrdinalIgnoreCase) &&
            string.Equals(p.Brand?.Trim() ?? string.Empty, b, StringComparison.OrdinalIgnoreCase));
    }

    public bool Delete(int id)
    {
        return _products.TryRemove(id, out _);
    }
}

internal sealed class ProductsContainer
{
    [JsonPropertyName("products")] public List<Product> Products { get; set; } = new();
}
