using Handwerker.Domain.Entities;
namespace Handwerker.Domain.Interfaces;

public interface IProductRepository
{
    Task<Product?> GetByIdAsync(int id);
    Task<bool> ExistsAsync(int id);
    Task<IEnumerable<Product>> GetAllAsync(int page, int pageSize);
    Task AddAsync(Product product);
    void Update(Product product);
    void Remove(Product product);
    Task SaveChangesAsync();
    Task<IEnumerable<Product>> SearchAsync(string expression);
    Task<int> CountAsync(CancellationToken cancellationToken);
}