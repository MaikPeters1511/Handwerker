using Handwerker.Domain.Entities;
namespace Handwerker.Domain.Interfaces;

public interface IBankRepository
{
    Task<Bank?> GetByIdAsync(int id);
    Task<bool> ExistsAsync(int id);
    Task<IEnumerable<Bank>> GetAllAsync();
    Task AddAsync(Bank bank);
    void Update(Bank bank);
    void Remove(Bank bank);
    Task<bool> ExistsByIbanAsync(string iban);
    Task SaveChangesAsync();
    Task<IEnumerable<Bank>> SearchAsync(string expression);
}