using Handwerker.Domain.Entities;
using Handwerker.Domain.Interfaces;

namespace Handwerker.Application.Services;

public class CompanyService(ICompanyRepository repo)
{
    public Task<IEnumerable<Company>> GetAllAsync()
    {
        return repo.GetAllAsync();
    }

    public Task<Company?> GetByIdAsync(int id)
    {
        return repo.GetByIdAsync(id);
    }

    public Task<Company> CreateAsync(Company company)
    {
        // Simple validation example
        if (string.IsNullOrWhiteSpace(company.Name)) throw new ArgumentException("Name is required");
        return repo.CreateAsync(company);
    }

    public Task UpdateAsync(Company company)
    {
        if (company.Id <= 0) throw new ArgumentException("Invalid id");
        return repo.UpdateAsync(company);
    }

    public Task DeleteAsync(int id)
    {
        return repo.DeleteAsync(id);
    }
}
