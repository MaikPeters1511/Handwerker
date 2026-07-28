using Handwerker.Domain.Entities;

namespace Handwerker.Domain.Interfaces;

public interface ISettingsRepository
{
    Task<UserSettings> GetSettingsAsync(string userId, CancellationToken cancellationToken);
    Task CreateSettingsAsync(UserSettings defaultSettings, CancellationToken cancellationToken);
    Task UpdateSettingsAsync(UserSettings settings, CancellationToken cancellationToken);
}