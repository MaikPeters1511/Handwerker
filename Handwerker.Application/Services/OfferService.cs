using Handwerker.Domain.Entities;
using Handwerker.Domain.Interfaces;

namespace Handwerker.Application.Services;

public class OfferService(IOfferRepository repo)
{
    public async Task<IEnumerable<Offer>> GetAllAsync(CancellationToken cancellationToken = default)
        => await repo.GetAllAsync(cancellationToken);

    public async Task<Offer?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        => await repo.GetByIdAsync(id, cancellationToken);

    public async Task<Offer> CreateAsync(Offer offer, CancellationToken cancellationToken = default)
    {
        // Automatische Angebotsnummer generieren, falls leer
        if (string.IsNullOrEmpty(offer.OfferNumber))
        {
            offer.OfferNumber = await GenerateOfferNumberAsync(cancellationToken);
        }

        return await repo.AddAsync(offer, cancellationToken);
    }

    public async Task UpdateAsync(Offer offer, CancellationToken cancellationToken = default)
        => await repo.UpdateAsync(offer, cancellationToken);

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
        => await repo.DeleteAsync(id, cancellationToken);

    public async Task<int> CountAsync(CancellationToken cancellationToken = default)
        => await repo.CountAsync(cancellationToken);

    public async Task<IEnumerable<Offer>> GetSentOffersAsync(CancellationToken cancellationToken = default)
        => await repo.GetSentOffersAsync(cancellationToken);

    public async Task<IEnumerable<Offer>> GetReceivedOffersAsync(CancellationToken cancellationToken = default)
        => await repo.GetReceivedOffersAsync(cancellationToken);

    public async Task<int?> ConvertToOrderAsync(int offerId, CancellationToken cancellationToken = default)
    {
        var offer = await repo.GetByIdAsync(offerId, cancellationToken);
        if (offer == null)
        {
            throw new InvalidOperationException($"Angebot mit ID {offerId} nicht gefunden.");
        }

        if (offer.Status == OfferStatus.Converted)
        {
            throw new InvalidOperationException("Angebot wurde bereits in einen Auftrag umgewandelt.");
        }

        // Für jetzt nur Status setzen und NULL zurückgeben
        offer.Status = OfferStatus.Converted;
        await repo.UpdateAsync(offer, cancellationToken);

        return offer.ConvertedToOrderId;
    }

    private async Task<string> GenerateOfferNumberAsync(CancellationToken cancellationToken = default)
    {
        var count = await repo.CountAsync(cancellationToken);
        var year = DateTime.Now.Year;
        return $"ANG-{year}-{(count + 1):D5}"; // z.B. ANG-2026-00001
    }

    public void ValidateOffer(Offer offer)
    {
        if (offer.ValidUntil < offer.OfferDate)
        {
            throw new InvalidOperationException("Das Gültigkeitsdatum muss nach dem Angebotsdatum liegen.");
        }

        if (offer.Products.Count == 0)
        {
            throw new InvalidOperationException("Ein Angebot muss mindestens eine Position enthalten.");
        }
    }
}
