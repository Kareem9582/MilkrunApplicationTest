using WooliesX.Products.Domain.Entities;

namespace WooliesX.Products.Infrastructure.Contracts;

public interface IProductsRepository
{
    IReadOnlyList<Product> GetAll();
    Product? GetById(int id);
    void Add(Product product);
    bool Update(int id, Product product);
    bool ExistsDuplicate(string title, string? brand, int? excludeId);
    bool Delete(int id);
}

