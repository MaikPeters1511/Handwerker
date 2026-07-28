// Handwerker.Tests/WageTypeServiceTests.cs
using Handwerker.Application.Services;
using Handwerker.Domain.Entities;
using Handwerker.Domain.Interfaces;
using Handwerker.Infrastructure.Data;
using Handwerker.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Handwerker.Tests;

public class WageTypeServiceTests
{
    /// <summary>
    /// Simuliert eine gleichzeitige Erstellung: der erste AddAsync-Versuch schlägt fehl
    /// (Unique-Index-Verletzung), als hätte eine parallele Anfrage dieselbe Nummer
    /// bereits erfolgreich vergeben — CountAsync spiegelt das ab dem nächsten Aufruf wider.
    /// </summary>
    private class CollidingOnceRepository(IWageTypeRepository inner) : IWageTypeRepository
    {
        private int _remainingCollisions = 1;
        private int _countOverride = -1;

        public Task<WageType?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
            => inner.GetByIdAsync(id, cancellationToken);

        public Task<IEnumerable<WageType>> GetAllAsync(CancellationToken cancellationToken = default)
            => inner.GetAllAsync(cancellationToken);

        public Task<IEnumerable<WageType>> GetActiveAsync(CancellationToken cancellationToken = default)
            => inner.GetActiveAsync(cancellationToken);

        public Task<IEnumerable<WageType>> SearchAsync(string searchTerm, CancellationToken cancellationToken = default)
            => inner.SearchAsync(searchTerm, cancellationToken);

        public Task<WageType> AddAsync(WageType wageType, CancellationToken cancellationToken = default)
        {
            if (_remainingCollisions > 0)
            {
                _remainingCollisions--;
                _countOverride = 1; // eine "fremde" gleichzeitige Erstellung ist jetzt sichtbar
                throw new DbUpdateException("Simulierte Kollision der Lohnartennummer.");
            }

            return inner.AddAsync(wageType, cancellationToken);
        }

        public Task UpdateAsync(WageType wageType, CancellationToken cancellationToken = default)
            => inner.UpdateAsync(wageType, cancellationToken);

        public Task DeleteAsync(int id, CancellationToken cancellationToken = default)
            => inner.DeleteAsync(id, cancellationToken);

        public Task<bool> ExistsAsync(string wageNumber, CancellationToken cancellationToken = default)
            => inner.ExistsAsync(wageNumber, cancellationToken);

        public Task<int> CountAsync(CancellationToken cancellationToken = default)
            => Task.FromResult(_countOverride >= 0 ? _countOverride : 0);
    }

    private static WageTypeService CreateService(out HandwerkerDbContext context)
    {
        var options = new DbContextOptionsBuilder<HandwerkerDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        context = new HandwerkerDbContext(options);
        var repository = new WageTypeRepository(context);
        return new WageTypeService(repository);
    }

    [Fact]
    public async Task CreateAsync_GeneratesSequentialWageNumbers()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var service = CreateService(out var context);

        var first = await service.CreateAsync(new WageType { Name = "Facharbeiter", HourlyRate = 45 }, cancellationToken);
        var second = await service.CreateAsync(new WageType { Name = "Meister", HourlyRate = 60 }, cancellationToken);

        Assert.Equal("LN-0001", first.WageNumber);
        Assert.Equal("LN-0002", second.WageNumber);
        context.Dispose();
    }

    [Fact]
    public async Task CreateAsync_SetsIsActiveTrue()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var service = CreateService(out var context);

        var created = await service.CreateAsync(new WageType { Name = "Azubi", HourlyRate = 18, IsActive = false }, cancellationToken);

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
        var innerRepository = new WageTypeRepository(context);
        var collidingRepository = new CollidingOnceRepository(innerRepository);
        var service = new WageTypeService(collidingRepository);

        var created = await service.CreateAsync(
            new WageType { Name = "Facharbeiter", HourlyRate = 45 }, cancellationToken);

        Assert.Equal("LN-0002", created.WageNumber);
        Assert.True(created.IsActive);
    }

    [Fact]
    public async Task DeleteAsync_SoftDeletesInsteadOfRemoving()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var service = CreateService(out var context);
        var created = await service.CreateAsync(new WageType { Name = "Helfer", HourlyRate = 22 }, cancellationToken);

        await service.DeleteAsync(created.Id, cancellationToken);
        var reloaded = await service.GetByIdAsync(created.Id, cancellationToken);

        Assert.NotNull(reloaded);
        Assert.False(reloaded!.IsActive);
        context.Dispose();
    }
}
