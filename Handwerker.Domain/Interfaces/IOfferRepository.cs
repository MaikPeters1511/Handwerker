using Handwerker.Domain.Entities;

namespace Handwerker.Domain.Interfaces;

public interface IOfferRepository
{
    Task<IEnumerable<Offer>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Offer?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<Offer> AddAsync(Offer offer, CancellationToken cancellationToken = default);
    Task UpdateAsync(Offer offer, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
    Task<int> CountAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<Offer>> GetSentOffersAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<Offer>> GetReceivedOffersAsync(CancellationToken cancellationToken = default);
}
