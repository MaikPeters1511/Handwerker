using Handwerker.Domain.Entities;
using Handwerker.Domain.Interfaces;

namespace Handwerker.Application.Services;

public class RecipientService(IRecipientRepository repo)
{
    public async Task<IEnumerable<Recipient>> GetAllAsync()
    {
        return await repo.GetAllAsync();
    }

    public async Task<Recipient?> GetByIdAsync(int id)
    {
        return await repo.GetByIdAsync(id);
    }

    public async Task<Recipient> CreateAsync(Recipient recipient)
    {
        await repo.AddAsync(recipient);
        await repo.SaveChangesAsync();
        return recipient;
    }

    public async Task<Recipient> UpdateAsync(Recipient recipient)
    {
        if (!await repo.ExistsAsync(recipient.Id))
            throw new KeyNotFoundException("Recipient not found");

        repo.Update(recipient);
        await repo.SaveChangesAsync();
        return recipient;
    }

    public async Task DeleteAsync(int id)
    {
        var recipient = await repo.GetByIdAsync(id);
        if (recipient == null)
            throw new KeyNotFoundException("Recipient not found");

        repo.Remove(recipient);
        await repo.SaveChangesAsync();
    }

    public async Task<int> CountAsync(CancellationToken cancellationToken)
    {
        return await repo.CountAsync(cancellationToken);
    }
}

