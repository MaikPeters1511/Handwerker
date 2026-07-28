using Handwerker.Application.Services;
using Handwerker.Domain.Entities;
using Handwerker.Domain.Interfaces;
using Handwerker.Infrastructure.Data;
using Handwerker.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Handwerker.Tests;

public class ServiceItemServiceTests
{
    /// <summary>
    /// Simuliert eine gleichzeitige Erstellung: der erste AddAsync-Versuch schlägt fehl
    /// (Unique-Index-Verletzung), als hätte eine parallele Anfrage dieselbe Nummer
    /// bereits erfolgreich vergeben — CountAsync spiegelt das ab dem nächsten Aufruf wider.
    /// </summary>
    private class CollidingOnceRepository(IServiceItemRepository inner) : IServiceItemRepository
    {
        private int _remainingCollisions = 1;
        private int _countOverride = -1;

        public Task<ServiceItem?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
            => inner.GetByIdAsync(id, cancellationToken);

        public Task<IEnumerable<ServiceItem>> GetAllAsync(CancellationToken cancellationToken = default)
            => inner.GetAllAsync(cancellationToken);

        public Task<IEnumerable<ServiceItem>> GetActiveAsync(CancellationToken cancellationToken = default)
            => inner.GetActiveAsync(cancellationToken);

        public Task<IEnumerable<ServiceItem>> SearchAsync(string searchTerm, CancellationToken cancellationToken = default)
            => inner.SearchAsync(searchTerm, cancellationToken);

        public Task<ServiceItem> AddAsync(ServiceItem serviceItem, CancellationToken cancellationToken = default)
        {
            if (_remainingCollisions > 0)
            {
                _remainingCollisions--;
                _countOverride = 1; // eine "fremde" gleichzeitige Erstellung ist jetzt sichtbar
                throw new DbUpdateException("Simulierte Kollision der Leistungsnummer.");
            }

            return inner.AddAsync(serviceItem, cancellationToken);
        }

        public Task UpdateAsync(ServiceItem serviceItem, CancellationToken cancellationToken = default)
            => inner.UpdateAsync(serviceItem, cancellationToken);

        public Task DeleteAsync(int id, CancellationToken cancellationToken = default)
            => inner.DeleteAsync(id, cancellationToken);

        public Task<bool> ExistsAsync(string serviceNumber, CancellationToken cancellationToken = default)
            => inner.ExistsAsync(serviceNumber, cancellationToken);

        public Task<int> CountAsync(CancellationToken cancellationToken = default)
            => Task.FromResult(_countOverride >= 0 ? _countOverride : 0);
    }

    private static ServiceItemService CreateService(out HandwerkerDbContext context)
    {
        var options = new DbContextOptionsBuilder<HandwerkerDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        context = new HandwerkerDbContext(options);
        var repository = new ServiceItemRepository(context);
        return new ServiceItemService(repository);
    }

    [Fact]
    public async Task CreateAsync_GeneratesSequentialServiceNumbers()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var service = CreateService(out var context);

        var first = await service.CreateAsync(new ServiceItem { Name = "Montage", Unit = "Std.", UnitPrice = 65 }, cancellationToken);
        var second = await service.CreateAsync(new ServiceItem { Name = "Beratung", Unit = "Std.", UnitPrice = 90 }, cancellationToken);

        Assert.Equal("L-0001", first.ServiceNumber);
        Assert.Equal("L-0002", second.ServiceNumber);
        context.Dispose();
    }

    [Fact]
    public async Task CreateAsync_SetsIsActiveTrue()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var service = CreateService(out var context);

        var created = await service.CreateAsync(new ServiceItem { Name = "Wartung", Unit = "Pauschale", UnitPrice = 120, IsActive = false }, cancellationToken);

        Assert.True(created.IsActive);
        context.Dispose();
    }

    [Fact]
    public async Task CreateAsync_RetriesAfterNumberCollisionAndSucceeds()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var options = new DbContextOptionsBuilder<HandwerkerDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        using var context = new HandwerkerDbContext(options);
        var innerRepository = new ServiceItemRepository(context);
        var collidingRepository = new CollidingOnceRepository(innerRepository);
        var service = new ServiceItemService(collidingRepository);

        var created = await service.CreateAsync(
            new ServiceItem { Name = "Montage", Unit = "Std.", UnitPrice = 65 }, cancellationToken);

        Assert.Equal("L-0002", created.ServiceNumber);
        Assert.True(created.IsActive);
    }

    [Fact]
    public async Task DeleteAsync_SoftDeletesInsteadOfRemoving()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var service = CreateService(out var context);
        var created = await service.CreateAsync(new ServiceItem { Name = "Anfahrt", Unit = "Pauschale", UnitPrice = 25 }, cancellationToken);

        await service.DeleteAsync(created.Id, cancellationToken);
        var reloaded = await service.GetByIdAsync(created.Id, cancellationToken);

        Assert.NotNull(reloaded);
        Assert.False(reloaded!.IsActive);
        context.Dispose();
    }
}
