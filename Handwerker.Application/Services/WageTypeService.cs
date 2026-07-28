using Handwerker.Domain.Entities;
using Handwerker.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Handwerker.Application.Services;

/// <summary>
/// Implementierung des Lohnarten-Application-Service.
/// Vergibt beim Anlegen automatisch eine fortlaufende Lohnartennummer (Format "LN-0001").
/// </summary>
public class WageTypeService(IWageTypeRepository wageTypeRepository) : IWageTypeService
{
    private const int MaxCreateAttempts = 5;

    public Task<IEnumerable<WageType>> GetAllAsync(CancellationToken cancellationToken = default)
        => wageTypeRepository.GetAllAsync(cancellationToken);

    public Task<IEnumerable<WageType>> GetActiveAsync(CancellationToken cancellationToken = default)
        => wageTypeRepository.GetActiveAsync(cancellationToken);

    public Task<IEnumerable<WageType>> SearchAsync(string searchTerm, CancellationToken cancellationToken = default)
        => wageTypeRepository.SearchAsync(searchTerm, cancellationToken);

    public Task<WageType?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        => wageTypeRepository.GetByIdAsync(id, cancellationToken);

    public Task<bool> ExistsAsync(string wageNumber, CancellationToken cancellationToken = default)
        => wageTypeRepository.ExistsAsync(wageNumber, cancellationToken);

    public async Task<WageType> CreateAsync(WageType wageType, CancellationToken cancellationToken = default)
    {
        wageType.IsActive = true;

        for (var attempt = 1; attempt <= MaxCreateAttempts; attempt++)
        {
            var count = await wageTypeRepository.CountAsync(cancellationToken);
            wageType.WageNumber = $"LN-{count + 1:D4}";

            try
            {
                return await wageTypeRepository.AddAsync(wageType, cancellationToken);
            }
            catch (DbUpdateException) when (attempt < MaxCreateAttempts)
            {
                // Zwei gleichzeitige Erstellungen haben dieselbe Lohnartennummer berechnet
                // (Unique-Index-Verletzung) — Zähler neu einlesen und erneut versuchen.
            }
        }

        throw new InvalidOperationException(
            "Lohnartennummer konnte nach mehreren Versuchen nicht eindeutig vergeben werden.");
    }

    public Task UpdateAsync(WageType wageType, CancellationToken cancellationToken = default)
        => wageTypeRepository.UpdateAsync(wageType, cancellationToken);

    public Task DeleteAsync(int id, CancellationToken cancellationToken = default)
        => wageTypeRepository.DeleteAsync(id, cancellationToken);
}
