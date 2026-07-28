using Handwerker.Domain.Entities;

namespace Handwerker.Domain.Interfaces;

public interface ICompanyRepository
{
    Task<IEnumerable<Company>> GetAllAsync();
    Task<Company?> GetByIdAsync(int id);
    Task<Company> CreateAsync(Company company);
    Task UpdateAsync(Company company);
    Task DeleteAsync(int id);
}
