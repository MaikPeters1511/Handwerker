using Handwerker.Domain.Entities;
using Handwerker.Domain.Interfaces;
using Handwerker.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Handwerker.Infrastructure.Repositories;

public class CompanyRepository(HandwerkerDbContext db) : ICompanyRepository
{
    public async Task<IEnumerable<Company>> GetAllAsync()
    {
        return await db.Companies.AsNoTracking().ToListAsync();
    }

    public async Task<Company?> GetByIdAsync(int id)
    {
        return await db.Companies.FindAsync(id);
    }

    public async Task<Company> CreateAsync(Company company)
    {
        company.CreatedAt = DateTime.UtcNow;
        company.UpdatedAt = DateTime.UtcNow;
        db.Companies.Add(company);
        await db.SaveChangesAsync();
        return company;
    }

    public async Task UpdateAsync(Company company)
    {
        var existing = await db.Companies.FindAsync(company.Id);
        if (existing == null) throw new KeyNotFoundException();

        existing.Name = company.Name;
        existing.TaxId = company.TaxId;
        existing.Street = company.Street;
        existing.ZipCode = company.ZipCode;
        existing.City = company.City;
        existing.Country = company.Country;
        existing.Email = company.Email;
        existing.Phone = company.Phone;
        existing.UpdatedAt = DateTime.UtcNow;

        db.Companies.Update(existing);
        await db.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var existing = await db.Companies.FindAsync(id);
        if (existing == null) throw new KeyNotFoundException();
        db.Companies.Remove(existing);
        await db.SaveChangesAsync();
    }
}
