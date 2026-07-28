using Handwerker.Domain.Entities;
using Handwerker.Domain.Interfaces;

namespace Handwerker.Application.Services;

public class SettingsService(ISettingsRepository repo)
{
    public async Task<UserSettings> GetSettingsAsync(string userId, CancellationToken cancellationToken)
    {
        return await repo.GetSettingsAsync(userId, cancellationToken);
    }

    public async Task CreateSettingsAsync(UserSettings defaultSettings, CancellationToken cancellationToken)
    {
        await repo.CreateSettingsAsync(defaultSettings, cancellationToken);
    }

    public async Task UpdateSettingsAsync(UserSettings settings, CancellationToken cancellationToken)
    {
        await repo.UpdateSettingsAsync(settings, cancellationToken);
    }
}