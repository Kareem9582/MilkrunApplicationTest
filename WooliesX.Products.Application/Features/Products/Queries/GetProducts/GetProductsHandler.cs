namespace WooliesX.Products.Application.Features.Products.Queries.GetProducts;

public class GetProductsHandler : IRequestHandler<GetProductsQuery, GetProductsResult>
{
    private readonly IProductsRepository _repo;
    public GetProductsHandler(IProductsRepository repo) => _repo = repo;

    public Task<GetProductsResult> Handle(GetProductsQuery r, CancellationToken ct)
    {
        IEnumerable<Product> items = _repo.GetAll();

        if (!string.IsNullOrWhiteSpace(r.Q))
        {
            var term = r.Q.Trim();
            items = items.Where(p =>
                (!string.IsNullOrEmpty(p.Title) && p.Title.Contains(term, StringComparison.OrdinalIgnoreCase)) ||
                (!string.IsNullOrEmpty(p.Brand) && p.Brand.Contains(term, StringComparison.OrdinalIgnoreCase)) ||
                (!string.IsNullOrEmpty(p.Description) && p.Description.Contains(term, StringComparison.OrdinalIgnoreCase)) ||
                (!string.IsNullOrEmpty(p.Category) && p.Category.Contains(term, StringComparison.OrdinalIgnoreCase))
            );
        }

        if (!string.IsNullOrWhiteSpace(r.Title))
        {
            var titles = r.Title.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(s => s.ToLowerInvariant()).ToHashSet();
            items = items.Where(p => p.Title != null && titles.Contains(p.Title.ToLowerInvariant()));
        }
        if (!string.IsNullOrWhiteSpace(r.Brand))
        {
            var brands = r.Brand.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(s => s.ToLowerInvariant()).ToHashSet();
            items = items.Where(p => p.Brand != null && brands.Contains(p.Brand.ToLowerInvariant()));
        }
        if (!string.IsNullOrWhiteSpace(r.Category))
        {
            var cats = r.Category.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(s => s.ToLowerInvariant()).ToHashSet();
            items = items.Where(p => p.Category != null && cats.Contains(p.Category.ToLowerInvariant()));
        }

        if (r.MinPrice.HasValue) items = items.Where(p => p.Price >= r.MinPrice.Value);
        if (r.MaxPrice.HasValue) items = items.Where(p => p.Price <= r.MaxPrice.Value);
        if (r.MinRating.HasValue) items = items.Where(p => (p.Rating ?? 0) >= r.MinRating.Value);
        if (r.InStock.HasValue)
            items = r.InStock.Value ? items.Where(p => (p.Stock ?? 0) > 0) : items.Where(p => (p.Stock ?? 0) <= 0);

        var sortBy = string.IsNullOrWhiteSpace(r.SortBy) ? "id" : r.SortBy.Trim().ToLowerInvariant();
        var descending = string.Equals(r.Order, "desc", StringComparison.OrdinalIgnoreCase);
        items = sortBy switch
        {
            "title" => (descending ? items.OrderByDescending(p => p.Title) : items.OrderBy(p => p.Title)),
            "price" => (descending ? items.OrderByDescending(p => p.Price) : items.OrderBy(p => p.Price)),
            "rating" => (descending ? items.OrderByDescending(p => p.Rating) : items.OrderBy(p => p.Rating)),
            "brand" => (descending ? items.OrderByDescending(p => p.Brand) : items.OrderBy(p => p.Brand)),
            "stock" => (descending ? items.OrderByDescending(p => p.Stock) : items.OrderBy(p => p.Stock)),
            "discount" => (descending ? items.OrderByDescending(p => p.DiscountPercentage) : items.OrderBy(p => p.DiscountPercentage)),
            "category" => (descending ? items.OrderByDescending(p => p.Category) : items.OrderBy(p => p.Category)),
            _ => (descending ? items.OrderByDescending(p => p.Id) : items.OrderBy(p => p.Id))
        };

        var total = items.Count();
        var p = r.Page.GetValueOrDefault(1);
        var ps = r.PageSize.GetValueOrDefault(20);
        if (p < 1) p = 1; if (ps < 1) ps = 20; if (ps > 100) ps = 100;
        var pageItems = items.Skip((p - 1) * ps).Take(ps).ToList();

        return Task.FromResult(new GetProductsResult(total, p, ps, pageItems));
    }
}

