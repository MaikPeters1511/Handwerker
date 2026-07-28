using Handwerker.Domain.Entities;
using Handwerker.Domain.Interfaces;

namespace Handwerker.Application.Services;

public class ProviderService(IProviderRepository repo)
{
    public async Task<IEnumerable<Provider>> GetAsync(int page, int pageSize)
    {
        return await repo.GetAllAsync(page, pageSize);
    }

    public async Task<Provider> GetByIdAsync(int id)
    {
        return await repo.GetByIdAsync(id);    
    }

    public async Task<IEnumerable<Provider>> SearchAsync(string expression)
    {
        if (string.IsNullOrWhiteSpace(expression))
            return new List<Provider>();
             
        expression = expression.ToLower();
        
        return await repo.SearchAsync(expression);

    }

    public async Task<int> CountAsync(CancellationToken cancellationToken)
    {
        return await repo.CountAsync(cancellationToken);
    }
    
    public async Task DeleteAsync(int id)
    {
        var provider = await repo.GetByIdAsync(id);
        if (provider == null)
            throw new KeyNotFoundException();

        repo.Remove(provider);
        await repo.SaveChangesAsync();
    }

    public async Task<Provider> CreateAsync(Provider provider)
    {
        await repo.AddAsync(provider);
        await repo.SaveChangesAsync();
        return provider;
    }

    public async Task<Provider> UpdateAsync(Provider provider)
    {
        if (!await repo.ExistsAsync(provider.Id))
            throw new KeyNotFoundException("Provider not found");

        repo.Update(provider);
        await repo.SaveChangesAsync();
        return provider;
    }

    public async Task<IEnumerable<Provider>> GetAllAsync()
    {
        return await repo.GetAllAsync(1, int.MaxValue);
    }
}