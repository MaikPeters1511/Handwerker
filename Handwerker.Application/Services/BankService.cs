using Handwerker.Domain.Entities;
using Handwerker.Domain.Interfaces;

namespace Handwerker.Application.Services;

public class BankService(IBankRepository repo)
{
    public async Task<int> CreateAsync(Bank bank)
    {
        if (await repo.ExistsByIbanAsync(bank.Iban))
            throw new InvalidOperationException("Bank exists");

        await repo.AddAsync(bank);
        await repo.SaveChangesAsync();

        return bank.Id;
    }

    public async Task DeleteAsync(int id)
    {
        var bank = await repo.GetByIdAsync(id);
        if (bank == null)
            throw new KeyNotFoundException();

        repo.Remove(bank);
        await repo.SaveChangesAsync();
    }

    public async Task<IEnumerable<Bank>> GetAsync()
    {
        return await repo.GetAllAsync();
    }

    public async Task<IEnumerable<Bank>> SearchAsync(string expression)
    {
        if (string.IsNullOrWhiteSpace(expression))
            return new List<Bank>();
             
        expression = expression.ToLower();
        
        return await repo.SearchAsync(expression);
    }

    public async Task<Bank> GetByIdAsync(int id)
    {
        return await repo.GetByIdAsync(id);    
    }

    public async Task<Bank> UpdateAsync(Bank bank)
    {
        if (!await repo.ExistsAsync(bank.Id))
            throw new KeyNotFoundException("Bank not found");

        repo.Update(bank);
        await repo.SaveChangesAsync();
        return bank;
    }
}