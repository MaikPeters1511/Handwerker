using Handwerker.Domain.Entities;
using Handwerker.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Handwerker.Application.Services;

/// <summary>
/// Implementierung des Leistungs-Application-Service.
/// Vergibt beim Anlegen automatisch eine fortlaufende Leistungsnummer (Format "L-0001").
/// </summary>
public class ServiceItemService(IServiceItemRepository serviceItemRepository) : IServiceItemService
{
    private const int MaxCreateAttempts = 5;

    public Task<IEnumerable<ServiceItem>> GetAllAsync(CancellationToken cancellationToken = default)
        => serviceItemRepository.GetAllAsync(cancellationToken);

    public Task<IEnumerable<ServiceItem>> GetActiveAsync(CancellationToken cancellationToken = default)
        => serviceItemRepository.GetActiveAsync(cancellationToken);

    public Task<IEnumerable<ServiceItem>> SearchAsync(string searchTerm, CancellationToken cancellationToken = default)
        => serviceItemRepository.SearchAsync(searchTerm, cancellationToken);

    public Task<ServiceItem?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        => serviceItemRepository.GetByIdAsync(id, cancellationToken);

    public Task<bool> ExistsAsync(string serviceNumber, CancellationToken cancellationToken = default)
        => serviceItemRepository.ExistsAsync(serviceNumber, cancellationToken);

    public async Task<ServiceItem> CreateAsync(ServiceItem serviceItem, CancellationToken cancellationToken = default)
    {
        serviceItem.IsActive = true;

        for (var attempt = 1; attempt <= MaxCreateAttempts; attempt++)
        {
            var count = await serviceItemRepository.CountAsync(cancellationToken);
            serviceItem.ServiceNumber = $"L-{count + 1:D4}";

            try
            {
                return await serviceItemRepository.AddAsync(serviceItem, cancellationToken);
            }
            catch (DbUpdateException) when (attempt < MaxCreateAttempts)
            {
                // Zwei gleichzeitige Erstellungen haben dieselbe Leistungsnummer berechnet
                // (Unique-Index-Verletzung) — Zähler neu einlesen und erneut versuchen.
            }
        }

        throw new InvalidOperationException(
            "Leistungsnummer konnte nach mehreren Versuchen nicht eindeutig vergeben werden.");
    }

    public Task UpdateAsync(ServiceItem serviceItem, CancellationToken cancellationToken = default)
        => serviceItemRepository.UpdateAsync(serviceItem, cancellationToken);

    public Task DeleteAsync(int id, CancellationToken cancellationToken = default)
        => serviceItemRepository.DeleteAsync(id, cancellationToken);
}
