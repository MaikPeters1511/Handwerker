# Leistungen (Services) Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add a "Leistungen" (services/labor items) catalog to the Handwerker app — full CRUD backend API, a standalone Angular management page, and integration as an autocomplete source for line-item positions in offers, invoices, and (newly) orders.

**Architecture:** Mirrors the existing `Article` (Artikelstamm) backend pattern one-for-one: Domain record → repository interface/impl → thin application service → REST controller with hand-mapped DTOs. On the frontend it mirrors the `Products` single-page master-detail pattern for the new catalog page, and extends the existing `Product`-line-item autocomplete already present in `offer-detail`/`invoice-detail` with a second, parallel "search service" source. Orders currently have no line-item UI at all, so a new tab (`order-positions`, mirroring the existing `order-materials`/`order-worktime` tabs) is added to expose the `Order.Products` list that the backend already persists.

**Tech Stack:** .NET 10 / EF Core (Handwerker.Domain/Application/Infrastructure/ApiService), Angular 20 standalone components + signals + Reactive Forms, Vitest for frontend tests, xUnit v3 + EF Core InMemory for backend tests.

## Global Constraints

- Domain entity is named `ServiceItem`, not `Service` — avoids collision with .NET application services and Angular `@Injectable` services (spec: naming section).
- No warehouse/stock tracking for `ServiceItem` — out of scope (spec: Out of Scope).
- `ServiceNumber` is generated server-side, sequential, format `L-0001` — never accepted from the client on create (spec: Application layer).
- No concurrency-safe number sequence (simple `Count + 1`) — consistent with the rest of the codebase's simplicity level (spec: Out of Scope).
- Orders' detail feature (`order-detail`, `order-materials`, `order-worktime`) uses **no i18n** — all strings are hardcoded German. The new `order-positions` tab must follow this local convention, not the `TranslatePipe` used in `offers`/`invoices`/`products`.
- Offers/Invoices/Products use `TranslatePipe` + `assets/i18n/{de,en,fr}.json` — new keys there must be added to all three files.
- No AutoMapper, no MediatR, no FluentValidation anywhere in this codebase — do not introduce them for this feature.

---

## Task 1: Domain entity and repository interface

**Files:**
- Create: `Handwerker.Domain/Entities/ServiceItem.cs`
- Create: `Handwerker.Domain/Interfaces/IServiceItemRepository.cs`

**Interfaces:**
- Produces: `ServiceItem` record (`Id`, `ServiceNumber`, `Name`, `Description`, `Unit`, `UnitPrice`, `TaxRate`, `IsActive`, `CreatedAt`, `UpdatedAt`), `IServiceItemRepository` with `GetByIdAsync`, `GetAllAsync`, `GetActiveAsync`, `SearchAsync`, `AddAsync`, `UpdateAsync`, `DeleteAsync`, `ExistsAsync`, `CountAsync`.

- [ ] **Step 1: Create the `ServiceItem` entity**

```csharp
// Handwerker.Domain/Entities/ServiceItem.cs
using System.ComponentModel.DataAnnotations;

namespace Handwerker.Domain.Entities;

/// <summary>
/// Leistungsstamm für Dienstleistungen/Arbeitsleistungen (z.B. Montage, Beratung, Wartung).
/// </summary>
public record ServiceItem
{
    public int Id { get; set; }

    [MaxLength(50)]
    public string ServiceNumber { get; set; } = string.Empty;

    [MaxLength(255)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(2000)]
    public string? Description { get; set; }

    [MaxLength(50)]
    public string Unit { get; set; } = string.Empty; // Std., Pauschale, m², etc.

    [Range(0, double.MaxValue)]
    public decimal UnitPrice { get; set; }

    [Range(0, 100)]
    public decimal TaxRate { get; set; } = 19;

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }
}
```

- [ ] **Step 2: Create the repository interface**

```csharp
// Handwerker.Domain/Interfaces/IServiceItemRepository.cs
using Handwerker.Domain.Entities;

namespace Handwerker.Domain.Interfaces;

public interface IServiceItemRepository
{
    Task<ServiceItem?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<IEnumerable<ServiceItem>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<ServiceItem>> GetActiveAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<ServiceItem>> SearchAsync(string searchTerm, CancellationToken cancellationToken = default);
    Task<ServiceItem> AddAsync(ServiceItem serviceItem, CancellationToken cancellationToken = default);
    Task UpdateAsync(ServiceItem serviceItem, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(string serviceNumber, CancellationToken cancellationToken = default);
    Task<int> CountAsync(CancellationToken cancellationToken = default);
}
```

- [ ] **Step 3: Build the Domain project**

Run: `dotnet build Handwerker.Domain/Handwerker.Domain.csproj`
Expected: Build succeeded, 0 errors.

- [ ] **Step 4: Commit**

```bash
git add Handwerker.Domain/Entities/ServiceItem.cs Handwerker.Domain/Interfaces/IServiceItemRepository.cs
git commit -m "feat(domain): add ServiceItem entity and repository interface"
```

---

## Task 2: Infrastructure — repository, DbContext wiring, migration

**Files:**
- Create: `Handwerker.Infrastructure/Repositories/ServiceItemRepository.cs`
- Modify: `Handwerker.Infrastructure/Data/HandwerkerDbContext.cs`
- Create (generated): `Handwerker.Infrastructure/Data/Migrations/<timestamp>_AddServiceItems.cs` (+ `.Designer.cs`, updated `HandwerkerDbContextModelSnapshot.cs`)

**Interfaces:**
- Consumes: `ServiceItem`, `IServiceItemRepository` (Task 1).
- Produces: `ServiceItemRepository : IServiceItemRepository`, `HandwerkerDbContext.ServiceItems` (`DbSet<ServiceItem>`).

- [ ] **Step 1: Add the `DbSet` to `HandwerkerDbContext`**

In `Handwerker.Infrastructure/Data/HandwerkerDbContext.cs`, add next to the existing `Articles` DbSet (near line 21):

```csharp
public DbSet<ServiceItem> ServiceItems => Set<ServiceItem>();
```

- [ ] **Step 2: Add EF fluent configuration**

In the same file's `OnModelCreating`, immediately after the `// Artikel-Konfiguration` block (after line 58), add:

```csharp
// Leistungen-Konfiguration
modelBuilder.Entity<ServiceItem>()
    .HasIndex(s => s.ServiceNumber)
    .IsUnique();

modelBuilder.Entity<ServiceItem>()
    .HasIndex(s => s.Name);

modelBuilder.Entity<ServiceItem>()
    .HasIndex(s => s.IsActive);
```

- [ ] **Step 3: Create the repository implementation**

```csharp
// Handwerker.Infrastructure/Repositories/ServiceItemRepository.cs
using Handwerker.Domain.Entities;
using Handwerker.Domain.Interfaces;
using Handwerker.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Handwerker.Infrastructure.Repositories;

public class ServiceItemRepository(HandwerkerDbContext context) : IServiceItemRepository
{
    public async Task<ServiceItem?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await context.ServiceItems
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<ServiceItem>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await context.ServiceItems
            .OrderBy(s => s.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<ServiceItem>> GetActiveAsync(CancellationToken cancellationToken = default)
    {
        return await context.ServiceItems
            .Where(s => s.IsActive)
            .OrderBy(s => s.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<ServiceItem>> SearchAsync(string searchTerm, CancellationToken cancellationToken = default)
    {
        var term = searchTerm.ToLower();
        return await context.ServiceItems
            .Where(s => s.IsActive && (
                s.Name.ToLower().Contains(term) ||
                s.ServiceNumber.ToLower().Contains(term) ||
                (s.Description != null && s.Description.ToLower().Contains(term))
            ))
            .OrderBy(s => s.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<ServiceItem> AddAsync(ServiceItem serviceItem, CancellationToken cancellationToken = default)
    {
        context.ServiceItems.Add(serviceItem);
        await context.SaveChangesAsync(cancellationToken);
        return serviceItem;
    }

    public async Task UpdateAsync(ServiceItem serviceItem, CancellationToken cancellationToken = default)
    {
        serviceItem.UpdatedAt = DateTime.UtcNow;
        context.ServiceItems.Update(serviceItem);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var serviceItem = await context.ServiceItems.FindAsync(new object[] { id }, cancellationToken);
        if (serviceItem != null)
        {
            serviceItem.IsActive = false;
            serviceItem.UpdatedAt = DateTime.UtcNow;
            await context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task<bool> ExistsAsync(string serviceNumber, CancellationToken cancellationToken = default)
    {
        return await context.ServiceItems
            .AnyAsync(s => s.ServiceNumber == serviceNumber, cancellationToken);
    }

    public async Task<int> CountAsync(CancellationToken cancellationToken = default)
    {
        return await context.ServiceItems.CountAsync(cancellationToken);
    }
}
```

- [ ] **Step 4: Build to confirm the entity/config compile**

Run: `dotnet build Handwerker.Infrastructure/Handwerker.Infrastructure.csproj`
Expected: Build succeeded, 0 errors.

- [ ] **Step 5: Generate the EF Core migration**

Run:
```bash
dotnet ef migrations add AddServiceItems --project Handwerker.Infrastructure --startup-project Handwerker.ApiService --output-dir Data/Migrations
```
Expected: New files `Data/Migrations/<timestamp>_AddServiceItems.cs` and `.Designer.cs` created, `HandwerkerDbContextModelSnapshot.cs` updated, command exits 0.

- [ ] **Step 6: Commit**

```bash
git add Handwerker.Infrastructure/Repositories/ServiceItemRepository.cs Handwerker.Infrastructure/Data/HandwerkerDbContext.cs Handwerker.Infrastructure/Data/Migrations/
git commit -m "feat(infrastructure): add ServiceItem EF repository, config, and migration"
```

---

## Task 3: Application service (with sequential number generation) + unit tests

**Files:**
- Create: `Handwerker.Application/Services/IServiceItemService.cs`
- Create: `Handwerker.Application/Services/ServiceItemService.cs`
- Create: `Handwerker.Tests/ServiceItemServiceTests.cs`

**Interfaces:**
- Consumes: `ServiceItem`, `IServiceItemRepository` (Task 1), `ServiceItemRepository` + `HandwerkerDbContext` (Task 2, for the InMemory test).
- Produces: `IServiceItemService` with `GetAllAsync`, `GetActiveAsync`, `SearchAsync`, `GetByIdAsync`, `ExistsAsync`, `CreateAsync(ServiceItem)`, `UpdateAsync`, `DeleteAsync`.

- [ ] **Step 1: Write the failing test for sequential number generation**

```csharp
// Handwerker.Tests/ServiceItemServiceTests.cs
using Handwerker.Application.Services;
using Handwerker.Domain.Entities;
using Handwerker.Infrastructure.Data;
using Handwerker.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Handwerker.Tests;

public class ServiceItemServiceTests
{
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
```

- [ ] **Step 2: Run the test to verify it fails**

Run: `dotnet test Handwerker.Tests/Handwerker.Tests.csproj --filter ServiceItemServiceTests`
Expected: FAIL to compile — `ServiceItemService` does not exist yet.

- [ ] **Step 3: Write the service interface and implementation**

```csharp
// Handwerker.Application/Services/IServiceItemService.cs
using Handwerker.Domain.Entities;

namespace Handwerker.Application.Services;

/// <summary>
/// Application-Service für Leistungs-Verwaltung.
/// Controller injizieren ausschließlich dieses Interface — kein direkter Repository-Zugriff.
/// </summary>
public interface IServiceItemService
{
    Task<IEnumerable<ServiceItem>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<ServiceItem>> GetActiveAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<ServiceItem>> SearchAsync(string searchTerm, CancellationToken cancellationToken = default);
    Task<ServiceItem?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(string serviceNumber, CancellationToken cancellationToken = default);
    Task<ServiceItem> CreateAsync(ServiceItem serviceItem, CancellationToken cancellationToken = default);
    Task UpdateAsync(ServiceItem serviceItem, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
}
```

```csharp
// Handwerker.Application/Services/ServiceItemService.cs
using Handwerker.Domain.Entities;
using Handwerker.Domain.Interfaces;

namespace Handwerker.Application.Services;

/// <summary>
/// Implementierung des Leistungs-Application-Service.
/// Vergibt beim Anlegen automatisch eine fortlaufende Leistungsnummer (Format "L-0001").
/// </summary>
public class ServiceItemService(IServiceItemRepository serviceItemRepository) : IServiceItemService
{
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
        var count = await serviceItemRepository.CountAsync(cancellationToken);
        serviceItem.ServiceNumber = $"L-{count + 1:D4}";
        serviceItem.IsActive = true;
        return await serviceItemRepository.AddAsync(serviceItem, cancellationToken);
    }

    public Task UpdateAsync(ServiceItem serviceItem, CancellationToken cancellationToken = default)
        => serviceItemRepository.UpdateAsync(serviceItem, cancellationToken);

    public Task DeleteAsync(int id, CancellationToken cancellationToken = default)
        => serviceItemRepository.DeleteAsync(id, cancellationToken);
}
```

- [ ] **Step 4: Run the test to verify it passes**

Run: `dotnet test Handwerker.Tests/Handwerker.Tests.csproj --filter ServiceItemServiceTests`
Expected: PASS (3 tests).

- [ ] **Step 5: Commit**

```bash
git add Handwerker.Application/Services/IServiceItemService.cs Handwerker.Application/Services/ServiceItemService.cs Handwerker.Tests/ServiceItemServiceTests.cs
git commit -m "feat(application): add ServiceItemService with sequential number generation"
```

---

## Task 4: API — DTOs, controller, notifications, DI registration

**Files:**
- Create: `Handwerker.ApiService/Controllers/ServiceRequests.cs`
- Create: `Handwerker.ApiService/Controllers/ServicesController.cs`
- Modify: `Handwerker.Application/Services/NotificationService.cs`
- Modify: `Handwerker.ApiService/Program.cs`

**Interfaces:**
- Consumes: `IServiceItemService` (Task 3), `NotificationService`, `ApiControllerBase`.
- Produces: REST endpoints under `/api/services`.

- [ ] **Step 1: Add DTOs**

```csharp
// Handwerker.ApiService/Controllers/ServiceRequests.cs
namespace Handwerker.ApiService.Controllers;

// ── Response-DTOs ─────────────────────────────────────────────────────────────

public record ServiceItemDto
{
    public int Id                { get; init; }
    public string ServiceNumber  { get; init; } = string.Empty;
    public string Name           { get; init; } = string.Empty;
    public string? Description   { get; init; }
    public string Unit           { get; init; } = string.Empty;
    public decimal UnitPrice     { get; init; }
    public decimal TaxRate       { get; init; }
    public bool IsActive         { get; init; }
}

// ── Request-Typen ─────────────────────────────────────────────────────────────

public record CreateServiceItemRequest
{
    public string Name          { get; init; } = string.Empty;
    public string? Description  { get; init; }
    public string Unit          { get; init; } = string.Empty;
    public decimal UnitPrice    { get; init; }
    public decimal TaxRate      { get; init; } = 19;
}

public record UpdateServiceItemRequest
{
    public int Id                { get; init; }
    public string Name           { get; init; } = string.Empty;
    public string? Description   { get; init; }
    public string Unit           { get; init; } = string.Empty;
    public decimal UnitPrice     { get; init; }
    public decimal TaxRate       { get; init; }
    public bool IsActive         { get; init; }
}
```

- [ ] **Step 2: Add notification methods**

In `Handwerker.Application/Services/NotificationService.cs`, add next to the existing `// Article-Benachrichtigungen` block:

```csharp
// ServiceItem-Benachrichtigungen
public Task NotifyServiceItemCreatedAsync(string userId, int serviceItemId, string serviceItemName)
    => CreateNotificationAsync(
        userId,
        NotificationType.Success,
        $"Leistung '{serviceItemName}' wurde erfolgreich erstellt.",
        "ServiceItem",
        serviceItemId);

public Task NotifyServiceItemUpdatedAsync(string userId, int serviceItemId, string serviceItemName)
    => CreateNotificationAsync(
        userId,
        NotificationType.Success,
        $"Leistung '{serviceItemName}' wurde erfolgreich aktualisiert.",
        "ServiceItem",
        serviceItemId);

public Task NotifyServiceItemDeletedAsync(string userId, string serviceItemName)
    => CreateNotificationAsync(
        userId,
        NotificationType.Info,
        $"Leistung '{serviceItemName}' wurde deaktiviert.",
        "ServiceItem");
```

- [ ] **Step 3: Add the controller**

```csharp
// Handwerker.ApiService/Controllers/ServicesController.cs
using Handwerker.Application.Services;
using Handwerker.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Handwerker.ApiService.Controllers;

[Route("api/services")]
[Authorize]
public class ServicesController(
    IServiceItemService serviceItemService,
    NotificationService notificationService) : ApiControllerBase
{
    /// <summary>
    /// Lädt alle Leistungen
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetServices(CancellationToken cancellationToken = default)
    {
        var services = await serviceItemService.GetAllAsync(cancellationToken);
        return Ok(services.Select(MapToDto));
    }

    /// <summary>
    /// Lädt alle aktiven Leistungen
    /// </summary>
    [HttpGet("active")]
    public async Task<IActionResult> GetActiveServices(CancellationToken cancellationToken = default)
    {
        var services = await serviceItemService.GetActiveAsync(cancellationToken);
        return Ok(services.Select(MapToDto));
    }

    /// <summary>
    /// Sucht nach Leistungen
    /// </summary>
    [HttpGet("search")]
    public async Task<IActionResult> SearchServices([FromQuery] string term, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(term))
            return BadRequest(new { error = "Suchbegriff darf nicht leer sein." });

        var services = await serviceItemService.SearchAsync(term, cancellationToken);
        return Ok(services.Select(MapToDto));
    }

    /// <summary>
    /// Lädt eine spezifische Leistung
    /// </summary>
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetService(int id, CancellationToken cancellationToken = default)
    {
        var service = await serviceItemService.GetByIdAsync(id, cancellationToken);
        if (service is null)
            return NotFound(new { error = "Leistung nicht gefunden." });

        return Ok(MapToDto(service));
    }

    /// <summary>
    /// Erstellt eine neue Leistung. Die Leistungsnummer wird automatisch vergeben.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateService([FromBody] CreateServiceItemRequest request, CancellationToken cancellationToken = default)
    {
        var serviceItem = new ServiceItem
        {
            Name = request.Name,
            Description = request.Description,
            Unit = request.Unit,
            UnitPrice = request.UnitPrice,
            TaxRate = request.TaxRate
        };

        var created = await serviceItemService.CreateAsync(serviceItem, cancellationToken);
        await notificationService.NotifyServiceItemCreatedAsync(GetUserId(), created.Id, created.Name);

        return CreatedAtAction(nameof(GetService), new { id = created.Id }, MapToDto(created));
    }

    /// <summary>
    /// Aktualisiert eine Leistung
    /// </summary>
    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateService(int id, [FromBody] UpdateServiceItemRequest request, CancellationToken cancellationToken = default)
    {
        if (id != request.Id)
            return BadRequest(new { error = "ID in URL und Body stimmen nicht überein." });

        var existing = await serviceItemService.GetByIdAsync(id, cancellationToken);
        if (existing is null)
            return NotFound(new { error = "Leistung nicht gefunden." });

        existing.Name = request.Name;
        existing.Description = request.Description;
        existing.Unit = request.Unit;
        existing.UnitPrice = request.UnitPrice;
        existing.TaxRate = request.TaxRate;
        existing.IsActive = request.IsActive;

        await serviceItemService.UpdateAsync(existing, cancellationToken);
        await notificationService.NotifyServiceItemUpdatedAsync(GetUserId(), existing.Id, existing.Name);

        return NoContent();
    }

    /// <summary>
    /// Löscht eine Leistung (Soft Delete)
    /// </summary>
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteService(int id, CancellationToken cancellationToken = default)
    {
        var existing = await serviceItemService.GetByIdAsync(id, cancellationToken);
        if (existing is null)
            return NotFound(new { error = "Leistung nicht gefunden." });

        await serviceItemService.DeleteAsync(id, cancellationToken);
        await notificationService.NotifyServiceItemDeletedAsync(GetUserId(), existing.Name);

        return NoContent();
    }

    private static ServiceItemDto MapToDto(ServiceItem serviceItem) => new()
    {
        Id = serviceItem.Id,
        ServiceNumber = serviceItem.ServiceNumber,
        Name = serviceItem.Name,
        Description = serviceItem.Description,
        Unit = serviceItem.Unit,
        UnitPrice = serviceItem.UnitPrice,
        TaxRate = serviceItem.TaxRate,
        IsActive = serviceItem.IsActive
    };
}
```

- [ ] **Step 4: Register repository and service in `Program.cs`**

In `Handwerker.ApiService/Program.cs`, add next to the existing `IArticleRepository` registration (near line 46):

```csharp
builder.Services.AddScoped<IServiceItemRepository, ServiceItemRepository>();
```

And next to the existing `IArticleService` registration (near line 72):

```csharp
builder.Services.AddScoped<IServiceItemService, ServiceItemService>();
```

- [ ] **Step 5: Build the whole solution**

Run: `dotnet build Handwerker.sln`
Expected: Build succeeded, 0 errors.

- [ ] **Step 6: Run all backend tests**

Run: `dotnet test Handwerker.Tests/Handwerker.Tests.csproj --filter ServiceItemServiceTests`
Expected: PASS (3 tests) — confirms nothing in Task 3 broke from the DI/controller additions.

- [ ] **Step 7: Commit**

```bash
git add Handwerker.ApiService/Controllers/ServiceRequests.cs Handwerker.ApiService/Controllers/ServicesController.cs Handwerker.Application/Services/NotificationService.cs Handwerker.ApiService/Program.cs
git commit -m "feat(api): add ServicesController with CRUD endpoints and notifications"
```

---

## Task 5: Frontend — entities, service, i18n

**Files:**
- Create: `Handwerker-Client/src/app/core/entities/service-item.model.ts`
- Modify: `Handwerker-Client/src/app/core/entities/index.ts`
- Create: `Handwerker-Client/src/app/core/services/service-item.service.ts`
- Create: `Handwerker-Client/src/app/core/services/service-item.service.spec.ts`
- Modify: `Handwerker-Client/src/app/core/services/index.ts`
- Modify: `Handwerker-Client/src/assets/i18n/de.json`
- Modify: `Handwerker-Client/src/assets/i18n/en.json`
- Modify: `Handwerker-Client/src/assets/i18n/fr.json`

**Interfaces:**
- Produces: `ServiceItem`, `CreateServiceItemRequest`, `UpdateServiceItemRequest` (TS interfaces); `ServiceItemService` with `getServices()`, `getActiveServices()`, `searchServices(term)`, `getService(id)`, `createService(request)`, `updateService(id, request)`, `deleteService(id)`.

- [ ] **Step 1: Add the entity model**

```typescript
// Handwerker-Client/src/app/core/entities/service-item.model.ts
export interface ServiceItem {
  id: number;
  serviceNumber: string;
  name: string;
  description?: string;
  unit: string;
  unitPrice: number;
  taxRate: number;
  isActive: boolean;
}

export interface CreateServiceItemRequest {
  name: string;
  description?: string;
  unit: string;
  unitPrice: number;
  taxRate: number;
}

export interface UpdateServiceItemRequest {
  id: number;
  name: string;
  description?: string;
  unit: string;
  unitPrice: number;
  taxRate: number;
  isActive: boolean;
}
```

- [ ] **Step 2: Export it from the entities barrel**

In `Handwerker-Client/src/app/core/entities/index.ts`, add (alphabetically, after `recipient.model`):

```typescript
export * from './service-item.model';
```

- [ ] **Step 3: Write the failing service test**

```typescript
// Handwerker-Client/src/app/core/services/service-item.service.spec.ts
import { TestBed } from '@angular/core/testing';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { provideHttpClient, withXhr } from '@angular/common/http';

import { ServiceItemService } from './service-item.service';
import { ServiceItem } from '../entities';

describe('ServiceItemService', () => {
  let service: ServiceItemService;
  let httpMock: HttpTestingController;

  const mockService: ServiceItem = {
    id: 1,
    serviceNumber: 'L-0001',
    name: 'Montage',
    description: 'Montage vor Ort',
    unit: 'Std.',
    unitPrice: 65,
    taxRate: 19,
    isActive: true
  };

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [ServiceItemService, provideHttpClient(withXhr()), provideHttpClientTesting()]
    });

    service = TestBed.inject(ServiceItemService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('should fetch all services', (done) => {
    service.getServices().subscribe(services => {
      expect(services).toEqual([mockService]);
      done();
    });

    const req = httpMock.expectOne('/api/services');
    expect(req.request.method).toBe('GET');
    req.flush([mockService]);
  });

  it('should search services by term', (done) => {
    service.searchServices('Montage').subscribe(services => {
      expect(services).toEqual([mockService]);
      done();
    });

    const req = httpMock.expectOne(r => r.url === '/api/services/search' && r.params.get('term') === 'Montage');
    expect(req.request.method).toBe('GET');
    req.flush([mockService]);
  });

  it('should create a service', (done) => {
    const request = { name: 'Montage', unit: 'Std.', unitPrice: 65, taxRate: 19 };
    service.createService(request).subscribe(created => {
      expect(created).toEqual(mockService);
      done();
    });

    const req = httpMock.expectOne('/api/services');
    expect(req.request.method).toBe('POST');
    req.flush(mockService);
  });
});
```

- [ ] **Step 4: Run the test to verify it fails**

Run: `cd Handwerker-Client && npx vitest run src/app/core/services/service-item.service.spec.ts`
Expected: FAIL — `service-item.service` module not found.

- [ ] **Step 5: Implement the service**

```typescript
// Handwerker-Client/src/app/core/services/service-item.service.ts
import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { ServiceItem, CreateServiceItemRequest, UpdateServiceItemRequest } from '../entities';

@Injectable({
  providedIn: 'root'
})
export class ServiceItemService {
  private http = inject(HttpClient);
  private apiUrl = '/api/services';

  getServices(): Observable<ServiceItem[]> {
    return this.http.get<ServiceItem[]>(this.apiUrl);
  }

  getActiveServices(): Observable<ServiceItem[]> {
    return this.http.get<ServiceItem[]>(`${this.apiUrl}/active`);
  }

  searchServices(term: string): Observable<ServiceItem[]> {
    return this.http.get<ServiceItem[]>(`${this.apiUrl}/search`, {
      params: { term }
    });
  }

  getService(id: number): Observable<ServiceItem> {
    return this.http.get<ServiceItem>(`${this.apiUrl}/${id}`);
  }

  createService(request: CreateServiceItemRequest): Observable<ServiceItem> {
    return this.http.post<ServiceItem>(this.apiUrl, request);
  }

  updateService(id: number, request: UpdateServiceItemRequest): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/${id}`, request);
  }

  deleteService(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }
}
```

- [ ] **Step 6: Run the test to verify it passes**

Run: `cd Handwerker-Client && npx vitest run src/app/core/services/service-item.service.spec.ts`
Expected: PASS (3 tests).

- [ ] **Step 7: Export the service from the services barrel**

In `Handwerker-Client/src/app/core/services/index.ts`, the entries from `auth.service` onward are alphabetical. Add the new line between `profile.service` (and its `export type {...}` line) and `settings.service`:

```typescript
export * from './service-item.service';
```

- [ ] **Step 8: Add i18n keys — `de.json`**

In `Handwerker-Client/src/assets/i18n/de.json`, add a new top-level `"services"` object (sibling of `"products"`):

```json
"services": {
  "title": "Leistungen",
  "subtitle": "Verwalten Sie Ihren Leistungsstamm",
  "newService": "Neue Leistung",
  "editMode": "Leistung bearbeiten",
  "createMode": "Neue Leistung anlegen",
  "editBadge": "Bearbeitung",
  "saving": "Speichert...",
  "saved": "Gespeichert",
  "save": "Speichern",
  "search": {
    "label": "Leistung suchen",
    "placeholder": "Name oder Leistungsnummer eingeben..."
  },
  "masterData": {
    "title": "Stammdaten",
    "serviceNumber": "Leistungsnummer",
    "name": "Bezeichnung",
    "namePlaceholder": "z.B. Montage",
    "nameRequired": "Bezeichnung ist erforderlich",
    "description": "Beschreibung",
    "descriptionPlaceholder": "Beschreibung der Leistung",
    "unit": "Einheit",
    "unitPlaceholder": "z.B. Std., Pauschale, m²",
    "unitPrice": "Preis pro Einheit",
    "unitPriceRequired": "Preis ist erforderlich",
    "taxRate": "MwSt. %",
    "isActive": "Aktiv"
  },
  "toast": {
    "created": "Leistung wurde erfolgreich angelegt",
    "updated": "Leistung wurde erfolgreich aktualisiert",
    "errorSave": "Fehler beim Speichern",
    "errorCreate": "Fehler beim Anlegen",
    "validationError": "Bitte prüfen Sie Ihre Eingaben"
  },
  "actions": {
    "endEdit": "Bearbeitung beenden",
    "reset": "Zurücksetzen",
    "saveChanges": "Änderungen speichern",
    "createService": "Leistung anlegen"
  }
}
```

Place it directly after the closing `}` of the `"products"` top-level key (search for the top-level `"products"` object, not the `nav.products` key at line 42).

- [ ] **Step 9: Add i18n keys — `en.json`**

Same structure, English copy:

```json
"services": {
  "title": "Services",
  "subtitle": "Manage your service catalog",
  "newService": "New Service",
  "editMode": "Edit Service",
  "createMode": "Create New Service",
  "editBadge": "Editing",
  "saving": "Saving...",
  "saved": "Saved",
  "save": "Save",
  "search": {
    "label": "Search Service",
    "placeholder": "Enter name or service number..."
  },
  "masterData": {
    "title": "Master Data",
    "serviceNumber": "Service Number",
    "name": "Name",
    "namePlaceholder": "e.g. Installation",
    "nameRequired": "Name is required",
    "description": "Description",
    "descriptionPlaceholder": "Description of the service",
    "unit": "Unit",
    "unitPlaceholder": "e.g. hr, flat rate, m²",
    "unitPrice": "Price per Unit",
    "unitPriceRequired": "Price is required",
    "taxRate": "Tax %",
    "isActive": "Active"
  },
  "toast": {
    "created": "Service was created successfully",
    "updated": "Service was updated successfully",
    "errorSave": "Error saving",
    "errorCreate": "Error creating",
    "validationError": "Please check your input"
  },
  "actions": {
    "endEdit": "End Editing",
    "reset": "Reset",
    "saveChanges": "Save Changes",
    "createService": "Create Service"
  }
}
```

- [ ] **Step 10: Add i18n keys — `fr.json`**

Same structure, French copy:

```json
"services": {
  "title": "Prestations",
  "subtitle": "Gérez votre catalogue de prestations",
  "newService": "Nouvelle prestation",
  "editMode": "Modifier la prestation",
  "createMode": "Créer une nouvelle prestation",
  "editBadge": "Édition",
  "saving": "Enregistrement...",
  "saved": "Enregistré",
  "save": "Enregistrer",
  "search": {
    "label": "Rechercher une prestation",
    "placeholder": "Saisir un nom ou un numéro de prestation..."
  },
  "masterData": {
    "title": "Données de base",
    "serviceNumber": "Numéro de prestation",
    "name": "Désignation",
    "namePlaceholder": "p.ex. Montage",
    "nameRequired": "La désignation est requise",
    "description": "Description",
    "descriptionPlaceholder": "Description de la prestation",
    "unit": "Unité",
    "unitPlaceholder": "p.ex. h, forfait, m²",
    "unitPrice": "Prix par unité",
    "unitPriceRequired": "Le prix est requis",
    "taxRate": "TVA %",
    "isActive": "Actif"
  },
  "toast": {
    "created": "La prestation a été créée avec succès",
    "updated": "La prestation a été mise à jour avec succès",
    "errorSave": "Erreur lors de l'enregistrement",
    "errorCreate": "Erreur lors de la création",
    "validationError": "Veuillez vérifier votre saisie"
  },
  "actions": {
    "endEdit": "Terminer l'édition",
    "reset": "Réinitialiser",
    "saveChanges": "Enregistrer les modifications",
    "createService": "Créer la prestation"
  }
}
```

- [ ] **Step 11: Verify all i18n files parse and TypeScript compiles**

Run:
```bash
cd Handwerker-Client && node -e "JSON.parse(require('fs').readFileSync('src/assets/i18n/de.json'))" && node -e "JSON.parse(require('fs').readFileSync('src/assets/i18n/en.json'))" && node -e "JSON.parse(require('fs').readFileSync('src/assets/i18n/fr.json'))" && npx tsc -p tsconfig.app.json --noEmit
```
Expected: No output from the `node -e` calls (valid JSON), tsc reports 0 errors.

- [ ] **Step 12: Commit**

```bash
git add Handwerker-Client/src/app/core/entities/service-item.model.ts Handwerker-Client/src/app/core/entities/index.ts Handwerker-Client/src/app/core/services/service-item.service.ts Handwerker-Client/src/app/core/services/service-item.service.spec.ts Handwerker-Client/src/app/core/services/index.ts Handwerker-Client/src/assets/i18n/de.json Handwerker-Client/src/assets/i18n/en.json Handwerker-Client/src/assets/i18n/fr.json
git commit -m "feat(frontend): add ServiceItem model, service, and i18n content"
```

---

## Task 6: Frontend — Leistungen catalog page (`features/services`)

**Files:**
- Create: `Handwerker-Client/src/app/core/interfaces/form/IServiceItemFormModel.ts`
- Create: `Handwerker-Client/src/app/features/services/services.ts`
- Create: `Handwerker-Client/src/app/features/services/services.html`
- Create: `Handwerker-Client/src/app/features/services/services.test.ts`
- Modify: `Handwerker-Client/src/app/app.routes.ts`

**Interfaces:**
- Consumes: `ServiceItemService` (Task 5), `ServiceItem`/`CreateServiceItemRequest`/`UpdateServiceItemRequest` (Task 5), `TranslationService`, `TranslatePipe`.
- Produces: routed component `Services` at path `services`.

- [ ] **Step 1: Add the form model**

```typescript
// Handwerker-Client/src/app/core/interfaces/form/IServiceItemFormModel.ts
import { FormControl } from '@angular/forms';

export interface ServiceItemFormModel {
  id: FormControl<number>;
  serviceNumber: FormControl<string>;
  name: FormControl<string>;
  description: FormControl<string>;
  unit: FormControl<string>;
  unitPrice: FormControl<number>;
  taxRate: FormControl<number>;
  isActive: FormControl<boolean>;
}
```

- [ ] **Step 2: Write the failing component test**

```typescript
// Handwerker-Client/src/app/features/services/services.test.ts
import '@angular/compiler';
import { Injector, runInInjectionContext } from '@angular/core';
import { of } from 'rxjs';
import { describe, expect, it, vi } from 'vitest';

import { Services } from './services';
import { ServiceItemService } from '../../core/services';
import { TranslationService } from '../../core/services';
import { ServiceItem } from '../../core/entities';

describe('Services', () => {
  const mockService: ServiceItem = {
    id: 1,
    serviceNumber: 'L-0001',
    name: 'Montage',
    description: '',
    unit: 'Std.',
    unitPrice: 65,
    taxRate: 19,
    isActive: true
  };

  const serviceItemServiceMock = {
    getServices: vi.fn(),
    getActiveServices: vi.fn(),
    searchServices: vi.fn(() => of([mockService])),
    getService: vi.fn(),
    createService: vi.fn(() => of(mockService)),
    updateService: vi.fn(() => of(undefined)),
    deleteService: vi.fn()
  };

  const translationServiceMock = {
    translate: vi.fn((key: string) => key)
  };

  function createInstance(): Services {
    const injector = Injector.create({
      providers: [
        { provide: ServiceItemService, useValue: serviceItemServiceMock },
        { provide: TranslationService, useValue: translationServiceMock }
      ]
    });
    return runInInjectionContext(injector, () => new Services());
  }

  it('initializes with an empty, non-editing form', () => {
    const component = createInstance();
    expect(component.isEditing()).toBe(false);
    expect(component.serviceForm.value.name).toBe('');
  });

  it('populates the form when selecting a service', () => {
    const component = createInstance();
    component.selectService(mockService);

    expect(component.isEditing()).toBe(true);
    expect(component.serviceForm.value.name).toBe('Montage');
    expect(component.serviceForm.value.serviceNumber).toBe('L-0001');
  });

  it('calls createService when saving a new service', () => {
    const component = createInstance();
    component.serviceForm.patchValue({ name: 'Beratung', unit: 'Std.', unitPrice: 90, taxRate: 19 });

    component.saveService();

    expect(serviceItemServiceMock.createService).toHaveBeenCalledWith(
      expect.objectContaining({ name: 'Beratung', unit: 'Std.', unitPrice: 90, taxRate: 19 })
    );
  });

  it('calls updateService when saving an existing service', () => {
    const component = createInstance();
    component.selectService(mockService);
    component.serviceForm.patchValue({ name: 'Montage (angepasst)' });

    component.saveService();

    expect(serviceItemServiceMock.updateService).toHaveBeenCalledWith(
      1,
      expect.objectContaining({ id: 1, name: 'Montage (angepasst)' })
    );
  });
});
```

- [ ] **Step 3: Run the test to verify it fails**

Run: `cd Handwerker-Client && npx vitest run src/app/features/services/services.test.ts`
Expected: FAIL — `./services` module not found.

- [ ] **Step 4: Implement the component**

```typescript
// Handwerker-Client/src/app/features/services/services.ts
import { Component, ChangeDetectionStrategy, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormControl, FormGroup, Validators } from '@angular/forms';
import { TranslatePipe } from '../../shared';
import { ServiceItemService } from '../../core/services';
import { TranslationService } from '../../core/services';
import { ServiceItem, CreateServiceItemRequest, UpdateServiceItemRequest } from '../../core/entities';
import { ServiceItemFormModel } from '../../core/interfaces/form/IServiceItemFormModel';

@Component({
  selector: 'app-services',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, TranslatePipe],
  templateUrl: './services.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class Services {
  private serviceItemService = inject(ServiceItemService);
  private translationService = inject(TranslationService);

  searchTerm = signal<string>('');
  searchResults = signal<ServiceItem[]>([]);
  showDropdown = signal<boolean>(false);

  selectedService = signal<ServiceItem | null>(null);
  isEditing = signal<boolean>(false);
  isSaving = signal<boolean>(false);
  lastSaved = signal<Date | null>(null);

  readonly saveSuccess = signal(false);
  readonly saveError = signal<string | null>(null);
  readonly successMessage = signal<string>('');

  serviceForm = new FormGroup<ServiceItemFormModel>({
    id: new FormControl<number>(0, { nonNullable: true }),
    serviceNumber: new FormControl<string>('', { nonNullable: true }),
    name: new FormControl<string>('', { nonNullable: true, validators: [Validators.required, Validators.maxLength(255)] }),
    description: new FormControl<string>('', { nonNullable: true, validators: [Validators.maxLength(2000)] }),
    unit: new FormControl<string>('', { nonNullable: true, validators: [Validators.maxLength(50)] }),
    unitPrice: new FormControl<number>(0, { nonNullable: true, validators: [Validators.required, Validators.min(0)] }),
    taxRate: new FormControl<number>(19, { nonNullable: true, validators: [Validators.required, Validators.min(0), Validators.max(100)] }),
    isActive: new FormControl<boolean>(true, { nonNullable: true })
  });

  onSearchInput(event: Event) {
    const term = (event.target as HTMLInputElement).value;
    this.searchTerm.set(term);

    if (term.length < 2) {
      this.searchResults.set([]);
      this.showDropdown.set(false);
      return;
    }

    setTimeout(() => {
      if (this.searchTerm() !== term) return;

      this.serviceItemService.searchServices(term).subscribe({
        next: (services) => {
          this.searchResults.set(services);
          this.showDropdown.set(services.length > 0);
        },
        error: () => this.searchResults.set([])
      });
    }, 300);
  }

  manualSearch() {
    const term = this.searchTerm();
    if (term.length < 2) {
      this.searchResults.set([]);
      this.showDropdown.set(false);
      return;
    }

    this.serviceItemService.searchServices(term).subscribe({
      next: (services) => {
        this.searchResults.set(services);
        this.showDropdown.set(services.length > 0);
      },
      error: () => this.searchResults.set([])
    });
  }

  selectService(service: ServiceItem) {
    this.selectedService.set(service);
    this.isEditing.set(true);
    this.lastSaved.set(null);
    this.serviceForm.patchValue(service, { emitEvent: false });
    this.showDropdown.set(false);
    this.searchTerm.set('');
  }

  addNewService() {
    this.selectedService.set(null);
    this.isEditing.set(false);
    this.lastSaved.set(null);
    this.serviceForm.reset({
      id: 0,
      serviceNumber: '',
      name: '',
      description: '',
      unit: '',
      unitPrice: 0,
      taxRate: 19,
      isActive: true
    }, { emitEvent: false });
  }

  saveService() {
    if (this.serviceForm.invalid) {
      this.serviceForm.markAllAsTouched();
      this.saveError.set(this.translationService.translate('services.toast.validationError'));
      setTimeout(() => this.saveError.set(null), 5000);
      return;
    }

    const formValue = this.serviceForm.getRawValue();
    this.isSaving.set(true);

    if (this.isEditing() && formValue.id) {
      const request: UpdateServiceItemRequest = {
        id: formValue.id,
        name: formValue.name,
        description: formValue.description,
        unit: formValue.unit,
        unitPrice: formValue.unitPrice,
        taxRate: formValue.taxRate,
        isActive: formValue.isActive
      };

      this.serviceItemService.updateService(formValue.id, request).subscribe({
        next: () => {
          this.isSaving.set(false);
          this.lastSaved.set(new Date());
          this.successMessage.set(this.translationService.translate('services.toast.updated'));
          this.saveSuccess.set(true);
          setTimeout(() => this.saveSuccess.set(false), 5000);
        },
        error: (err) => {
          this.isSaving.set(false);
          this.saveError.set(`${this.translationService.translate('services.toast.errorSave')}: ${err?.error?.message || err?.message || 'Unknown error'}`);
          setTimeout(() => this.saveError.set(null), 5000);
        }
      });
    } else {
      const request: CreateServiceItemRequest = {
        name: formValue.name,
        description: formValue.description,
        unit: formValue.unit,
        unitPrice: formValue.unitPrice,
        taxRate: formValue.taxRate
      };

      this.serviceItemService.createService(request).subscribe({
        next: (created) => {
          this.isSaving.set(false);
          this.lastSaved.set(new Date());
          this.successMessage.set(this.translationService.translate('services.toast.created'));
          this.saveSuccess.set(true);
          setTimeout(() => this.saveSuccess.set(false), 5000);
          this.selectService(created);
        },
        error: (err) => {
          this.isSaving.set(false);
          this.saveError.set(`${this.translationService.translate('services.toast.errorCreate')}: ${err?.error?.message || err?.message || 'Unknown error'}`);
          setTimeout(() => this.saveError.set(null), 5000);
        }
      });
    }
  }
}
```

- [ ] **Step 5: Implement the template**

```html
<!-- Handwerker-Client/src/app/features/services/services.html -->
<div class="w-full max-w-full">
  <div class="flex flex-col sm:flex-row justify-between items-start sm:items-center mb-8 gap-4">
    <div>
      <h1 class="text-3xl font-bold tracking-tight">{{ 'services.title' | translate }}</h1>
      <p class="text-base-content/70 mt-1">{{ 'services.subtitle' | translate }}</p>
    </div>
    <button class="btn btn-primary shadow-lg" (click)="addNewService()">
      <i class="fa-solid fa-plus"></i> {{ 'services.newService' | translate }}
    </button>
  </div>

  <!-- Search Section -->
  <div class="form-control w-full max-w-xl relative mb-8 z-20">
    <label class="label">
      <span class="label-text font-semibold">{{ 'services.search.label' | translate }}</span>
    </label>
    <div class="flex gap-2">
      <div class="relative flex-1">
        <i class="fa-solid fa-search absolute left-4 top-1/2 transform -translate-y-1/2 text-base-content/50"></i>
        <input
          type="text"
          placeholder="{{ 'services.search.placeholder' | translate }}"
          class="input input-bordered w-full pl-10 shadow-sm focus:input-primary transition-all"
          (input)="onSearchInput($event)"
          (keyup.enter)="manualSearch()"
          [value]="searchTerm()"
        />
      </div>
      <button type="button" class="btn btn-primary" (click)="manualSearch()" [disabled]="searchTerm().length < 2">
        <i class="fa-solid fa-search"></i>
      </button>
    </div>

    @if (showDropdown() && searchResults().length > 0) {
      <ul class="menu bg-base-100 w-full rounded-box shadow-xl absolute top-full mt-2 z-50 max-h-60 overflow-y-auto border border-base-300">
        @for (item of searchResults(); track item.id) {
          <li>
            <a (click)="selectService(item)" class="hover:bg-base-200">
              <div class="flex flex-col">
                <span class="font-bold">{{ item.name }}</span>
                <span class="text-xs text-base-content/70">{{ item.serviceNumber }}</span>
              </div>
            </a>
          </li>
        }
      </ul>
    }
  </div>

  <div class="card bg-base-100 shadow-xl border border-base-200 w-full">
    <div class="card-body p-6 md:p-8">
      <div class="flex justify-between items-center pb-4 border-b border-base-200 mb-6">
        <h2 class="card-title text-xl">
          {{ (isEditing() ? 'services.editMode' : 'services.createMode') | translate }}
        </h2>
        <div class="flex gap-2 items-center">
          @if (isEditing()) {
            <div class="badge badge-accent badge-outline">{{ 'services.editBadge' | translate }}</div>
            @if (isSaving()) {
              <span class="loading loading-spinner loading-sm text-primary"></span>
              <span class="text-sm text-base-content/70">{{ 'services.saving' | translate }}</span>
            }
            @if (!isSaving() && lastSaved()) {
              <div class="flex items-center gap-1 text-success text-sm">
                <i class="fa-solid fa-check"></i>
                <span>{{ 'services.saved' | translate }}</span>
              </div>
            }
          }
        </div>
      </div>

      <form id="service-form" [formGroup]="serviceForm" (ngSubmit)="saveService()" class="space-y-8">
        <div class="bg-base-50/50 rounded-xl p-2 md:p-0">
          <h3 class="font-semibold text-lg mb-4 text-base-content/80 flex items-center gap-2">
            <i class="fa-solid fa-screwdriver-wrench text-primary"></i> {{ 'services.masterData.title' | translate }}
          </h3>
          <div class="grid grid-cols-1 xl:grid-cols-2 gap-6">

            @if (isEditing()) {
              <div class="form-control">
                <label class="label"><span class="label-text font-medium">{{ 'services.masterData.serviceNumber' | translate }}</span></label>
                <input type="text" formControlName="serviceNumber" class="input input-bordered w-full bg-base-200" readonly tabindex="-1" />
              </div>
            }

            <div class="form-control">
              <label class="label"><span class="label-text font-medium">{{ 'services.masterData.name' | translate }} *</span></label>
              <input type="text" formControlName="name" class="input input-bordered focus:input-primary w-full"
                [class.input-error]="serviceForm.get('name')?.invalid && serviceForm.get('name')?.touched"
                placeholder="{{ 'services.masterData.namePlaceholder' | translate }}" />
              @if (serviceForm.get('name')?.invalid && serviceForm.get('name')?.touched) {
                <div class="label pb-0">
                  <span class="label-text-alt text-error">{{ 'services.masterData.nameRequired' | translate }}</span>
                </div>
              }
            </div>

            <div class="form-control">
              <label class="label"><span class="label-text font-medium">{{ 'services.masterData.unit' | translate }}</span></label>
              <input type="text" formControlName="unit" class="input input-bordered focus:input-primary w-full" placeholder="{{ 'services.masterData.unitPlaceholder' | translate }}" />
            </div>

            <div class="form-control">
              <label class="label"><span class="label-text font-medium">{{ 'services.masterData.unitPrice' | translate }} *</span></label>
              <div class="relative">
                <input type="number" step="0.01" formControlName="unitPrice" class="input input-bordered w-full pr-8 focus:input-primary"
                  [class.input-error]="serviceForm.get('unitPrice')?.invalid && serviceForm.get('unitPrice')?.touched" />
                <span class="absolute right-3 top-3 text-gray-500">€</span>
              </div>
              @if (serviceForm.get('unitPrice')?.invalid && serviceForm.get('unitPrice')?.touched) {
                <div class="label pb-0">
                  <span class="label-text-alt text-error">{{ 'services.masterData.unitPriceRequired' | translate }}</span>
                </div>
              }
            </div>

            <div class="form-control">
              <label class="label"><span class="label-text font-medium">{{ 'services.masterData.taxRate' | translate }}</span></label>
              <input type="number" formControlName="taxRate" class="input input-bordered focus:input-primary w-full" />
            </div>

            <div class="form-control">
              <label class="label cursor-pointer justify-start gap-3">
                <input type="checkbox" formControlName="isActive" class="checkbox checkbox-primary" />
                <span class="label-text font-medium">{{ 'services.masterData.isActive' | translate }}</span>
              </label>
            </div>

            <div class="form-control xl:col-span-2">
              <label class="label"><span class="label-text font-medium">{{ 'services.masterData.description' | translate }}</span></label>
              <textarea formControlName="description" class="textarea textarea-bordered h-24 focus:textarea-primary w-full" placeholder="{{ 'services.masterData.descriptionPlaceholder' | translate }}"></textarea>
            </div>
          </div>
        </div>

        <div class="card-actions justify-end mt-8 pt-6 border-t border-base-200 gap-4">
          <button type="button" class="btn btn-ghost" (click)="addNewService()">
            @if (isEditing()) {
              <i class="fa-solid fa-times mr-2"></i>
              {{ 'services.actions.endEdit' | translate }}
            } @else {
              {{ 'services.actions.reset' | translate }}
            }
          </button>
          <button type="submit" class="btn btn-primary px-8 min-w-[150px] shadow-md" [disabled]="isSaving()">
            <i class="fa-solid fa-save mr-2"></i>
            @if (isEditing()) {
              {{ 'services.actions.saveChanges' | translate }}
            } @else {
              {{ 'services.actions.createService' | translate }}
            }
          </button>
        </div>
      </form>
    </div>
  </div>
</div>

<div class="toast toast-top toast-end z-50">
  @if (saveSuccess()) {
    <div class="alert alert-success shadow-lg flex gap-4" role="status" aria-live="polite">
      <i class="fa-solid fa-circle-check text-xl"></i>
      <span>{{ successMessage() }}</span>
      <button class="btn btn-sm btn-ghost btn-circle" (click)="saveSuccess.set(false)" aria-label="Schließen">
        <i class="fa-solid fa-xmark"></i>
      </button>
    </div>
  }

  @if (saveError(); as err) {
    <div class="alert alert-error shadow-lg flex gap-4" role="alert" aria-live="assertive">
      <i class="fa-solid fa-circle-exclamation text-xl"></i>
      <span>{{ err }}</span>
      <button class="btn btn-sm btn-ghost btn-circle" (click)="saveError.set(null)" aria-label="Schließen">
        <i class="fa-solid fa-xmark"></i>
      </button>
    </div>
  }
</div>
```

- [ ] **Step 6: Register the route**

In `Handwerker-Client/src/app/app.routes.ts`, add (after the `invoices/:id` route, before `orders`):

```typescript
{ path: 'services', loadComponent: () => import('./features/services/services').then(m => m.Services) },
```

- [ ] **Step 7: Run the test to verify it passes**

Run: `cd Handwerker-Client && npx vitest run src/app/features/services/services.test.ts`
Expected: PASS (4 tests).

- [ ] **Step 8: Type-check the whole frontend**

Run: `cd Handwerker-Client && npx tsc -p tsconfig.app.json --noEmit`
Expected: 0 errors.

- [ ] **Step 9: Manual verification in the browser**

Run: `cd Handwerker-Client && npm start` (or use the project's existing dev-server task), then navigate to `/services`. Confirm: the sidenav "Leistungen" link now opens the page (previously a dead link), you can create a service, it appears with an auto-generated number `L-0001`, editing and saving works, and the toast messages show.

- [ ] **Step 10: Commit**

```bash
git add Handwerker-Client/src/app/core/interfaces/form/IServiceItemFormModel.ts Handwerker-Client/src/app/features/services/ Handwerker-Client/src/app/app.routes.ts
git commit -m "feat(frontend): add Leistungen catalog page and route"
```

---

## Task 7: Integrate service search into Offers (`offer-detail`)

**Files:**
- Modify: `Handwerker-Client/src/app/features/offers/offer-detail/offer-detail.ts`
- Modify: `Handwerker-Client/src/app/features/offers/offer-detail/offer-detail.html`
- Modify: `Handwerker-Client/src/assets/i18n/de.json`
- Modify: `Handwerker-Client/src/assets/i18n/en.json`
- Modify: `Handwerker-Client/src/assets/i18n/fr.json`

**Interfaces:**
- Consumes: `ServiceItemService` (Task 5), `ServiceItem` (Task 5), existing `Product` type and `offerItems` signal, `calculateTotals()`.
- Produces: `searchService(term)`, `addService(service: ServiceItem)` on `OfferDetailComponent`.

- [ ] **Step 1: Add the import and injected service**

In `Handwerker-Client/src/app/features/offers/offer-detail/offer-detail.ts`, add to the imports near the top:

```typescript
import { ServiceItemService } from '../../../core/services';
import { ServiceItem } from '../../../core/entities';
```

And add the injected dependency alongside `productService`:

```typescript
serviceItemService = inject(ServiceItemService);
```

- [ ] **Step 2: Add the `services` signal**

Near the existing `products = signal<Product[]>([]);` line, add:

```typescript
services = signal<ServiceItem[]>([]);
```

- [ ] **Step 3: Add `searchService` and `addService`**

Directly after the existing `addProduct(product: Product)` method, add:

```typescript
  // Service Search
  searchService(term: string) {
      this.serviceItemService.getServices().subscribe(list => {
           if (!term) {
               this.services.set(list);
           } else {
               const filtered = list.filter(s => s.name.toLowerCase().includes(term.toLowerCase()) || s.serviceNumber.includes(term));
               this.services.set(filtered);
           }
      });
  }

  addService(service: ServiceItem) {
      const taxRate = service.taxRate || 19;
      const totalNet = service.unitPrice;
      const taxAmount = totalNet * (taxRate / 100);
      const totalGross = totalNet + taxAmount;

      const newItem: Product = {
          id: 0,
          articleNumber: service.serviceNumber,
          name: service.name,
          description: service.description || '',
          unit: service.unit,
          unitPrice: service.unitPrice,
          taxRate: taxRate,
          position: this.offerItems().length + 1,
          quantity: 1,
          discountPercent: 0,
          discountAmount: 0,
          taxAmount: taxAmount,
          totalNet: totalNet,
          totalGross: totalGross
      };

      this.offerItems.update(items => [...items, newItem]);
      this.calculateTotals();
  }
```

- [ ] **Step 4: Add the second dropdown to the template**

In `Handwerker-Client/src/app/features/offers/offer-detail/offer-detail.html`, directly after the closing `</div>` of the existing "Produkt suchen" dropdown block (the one containing `searchProduct($any($event.target).value)`), add a parallel block:

```html
            <div class="dropdown dropdown-top dropdown-end">
                <div tabindex="0" role="button" class="text-primary hover:underline flex items-center gap-1">
                     <i class="fa-solid fa-magnifying-glass"></i> {{ 'offers.detail.actions.searchService' | translate }}
                </div>
                <div tabindex="0" class="dropdown-content z-[1] card card-compact w-80 p-2 shadow bg-base-100 text-primary-content mb-2 border border-base-200">
                    <div class="card-body p-2">
                        <input type="text" class="input input-bordered input-sm w-full text-black bg-white"
                               [placeholder]="'offers.detail.actions.searchServicePlaceholder' | translate"
                               (input)="searchService($any($event.target).value)"
                               (focus)="searchService('')">
                        <ul class="menu bg-white w-full rounded-box max-h-48 overflow-y-auto text-black p-0 mt-2">
                            @for(s of services(); track s.id) {
                                <li><a (click)="addService(s)" class="flex justify-between text-xs">
                                    <span class="truncate max-w-[150px]">{{ s.name }}</span>
                                    <span class="font-bold">{{ s.unitPrice }} €</span>
                                </a></li>
                            }
                        </ul>
                    </div>
                </div>
            </div>
```

- [ ] **Step 5: Add the i18n keys**

In `de.json`, inside the `"offers": { "detail": { "actions": { ... } } }` object, add next to the existing `searchProduct`/`searchProductPlaceholder` keys:

```json
"searchService": "Leistung suchen",
"searchServicePlaceholder": "Leistung nach Name suchen..."
```

In `en.json`, same location:

```json
"searchService": "Search Service",
"searchServicePlaceholder": "Search service by name..."
```

In `fr.json`, same location:

```json
"searchService": "Rechercher une prestation",
"searchServicePlaceholder": "Rechercher une prestation par nom..."
```

- [ ] **Step 6: Type-check**

Run: `cd Handwerker-Client && npx tsc -p tsconfig.app.json --noEmit`
Expected: 0 errors.

- [ ] **Step 7: Manual verification**

Open an offer in edit mode, click "Leistung suchen", pick a service created in Task 6, confirm a new position row appears with the service's name/price/tax prefilled and totals recalculate correctly.

- [ ] **Step 8: Commit**

```bash
git add Handwerker-Client/src/app/features/offers/offer-detail/offer-detail.ts Handwerker-Client/src/app/features/offers/offer-detail/offer-detail.html Handwerker-Client/src/assets/i18n/de.json Handwerker-Client/src/assets/i18n/en.json Handwerker-Client/src/assets/i18n/fr.json
git commit -m "feat(offers): add Leistungen autocomplete to offer positions"
```

---

## Task 8: Integrate service search into Invoices (`invoice-detail`)

**Files:**
- Modify: `Handwerker-Client/src/app/features/invoices/invoice-detail/invoice-detail.ts`
- Modify: `Handwerker-Client/src/app/features/invoices/invoice-detail/invoice-detail.html`
- Modify: `Handwerker-Client/src/assets/i18n/de.json`
- Modify: `Handwerker-Client/src/assets/i18n/en.json`
- Modify: `Handwerker-Client/src/assets/i18n/fr.json`

**Interfaces:**
- Consumes: `ServiceItemService` (Task 5), `ServiceItem` (Task 5), existing `productsArray` (`FormArray`), `createProductFormGroup(product?)`, `updateTotals()`.
- Produces: `searchServiceOptions(term)`, `addServiceRow(service: ServiceItem)` on `InvoiceDetail`. This is intentionally a second, simpler bottom dropdown (not a per-row inline autocomplete like the existing product search) — it appends a whole new pre-filled row, keeping the per-row keyboard-navigable suggestion mechanism exclusive to product search.

- [ ] **Step 1: Add the import and injected service**

In `Handwerker-Client/src/app/features/invoices/invoice-detail/invoice-detail.ts`, add:

```typescript
import { ServiceItemService } from '../../../core/services';
import { ServiceItem } from '../../../core/entities';
```

```typescript
serviceItemService = inject(ServiceItemService);
```

- [ ] **Step 2: Add the `serviceOptions` signal**

Near the existing `productSuggestions = signal<Record<number, Product[]>>({});` line, add:

```typescript
serviceOptions = signal<ServiceItem[]>([]);
showServiceDropdown = signal<boolean>(false);
```

- [ ] **Step 3: Add `searchServiceOptions` and `addServiceRow`**

Directly after the existing `addProduct()` method, add:

```typescript
  searchServiceOptions(term: string) {
    this.serviceItemService.getServices().subscribe(list => {
      if (!term) {
        this.serviceOptions.set(list);
      } else {
        const filtered = list.filter(s => s.name.toLowerCase().includes(term.toLowerCase()) || s.serviceNumber.includes(term));
        this.serviceOptions.set(filtered);
      }
      this.showServiceDropdown.set(true);
    });
  }

  addServiceRow(service: ServiceItem) {
    const row = this.createProductFormGroup({
      id: 0,
      articleNumber: service.serviceNumber,
      name: service.name,
      description: service.description || '',
      unit: service.unit,
      unitPrice: service.unitPrice,
      taxRate: service.taxRate,
      position: this.productsArray.length + 1,
      quantity: 1,
      discountPercent: 0,
      discountAmount: 0,
      taxAmount: 0,
      totalNet: 0,
      totalGross: 0
    });
    this.productsArray.push(row);
    this.calculateProductTotals(this.productsArray.length - 1);
    this.updateTotals();
    this.showServiceDropdown.set(false);
  }
```

Note: `createProductFormGroup(product?: Product): FormGroup` (defined earlier in this file, used by `loadInvoice`/`addProduct` to seed a row) accepts the object literal above as-is, since it supplies every `Product` field.

- [ ] **Step 4: Add the second dropdown to the template**

In `Handwerker-Client/src/app/features/invoices/invoice-detail/invoice-detail.html`, directly after the existing "add position" button block (the one with `(click)="addProduct()"`, around line 278-285), add:

```html
            @if (mode() !== 'view') {
              <div class="dropdown dropdown-end">
                <button
                  type="button"
                  tabindex="0"
                  class="btn btn-sm btn-outline btn-primary"
                  (click)="searchServiceOptions('')">
                  <i class="fa-solid fa-magnifying-glass mr-2"></i>
                  {{ 'invoices.detail.actions.searchService' | translate }}
                </button>
                @if (showServiceDropdown()) {
                  <div tabindex="0" class="dropdown-content z-[1] card card-compact w-80 p-2 shadow bg-base-100 border border-base-200">
                    <div class="card-body p-2">
                      <input type="text" class="input input-bordered input-sm w-full"
                             [placeholder]="'invoices.detail.actions.searchServicePlaceholder' | translate"
                             (input)="searchServiceOptions($any($event.target).value)">
                      <ul class="menu w-full rounded-box max-h-48 overflow-y-auto p-0 mt-2">
                        @for (s of serviceOptions(); track s.id) {
                          <li><a (click)="addServiceRow(s)" class="flex justify-between text-xs">
                            <span class="truncate max-w-[150px]">{{ s.name }}</span>
                            <span class="font-bold">{{ s.unitPrice }} €</span>
                          </a></li>
                        }
                      </ul>
                    </div>
                  </div>
                }
              </div>
            }
```

- [ ] **Step 5: Add the i18n keys**

In `de.json`, inside `"invoices": { "detail": { "actions": { ... } } }`, add:

```json
"searchService": "Leistung suchen",
"searchServicePlaceholder": "Leistung nach Name suchen..."
```

In `en.json`:

```json
"searchService": "Search Service",
"searchServicePlaceholder": "Search service by name..."
```

In `fr.json`:

```json
"searchService": "Rechercher une prestation",
"searchServicePlaceholder": "Rechercher une prestation par nom..."
```

- [ ] **Step 6: Type-check**

Run: `cd Handwerker-Client && npx tsc -p tsconfig.app.json --noEmit`
Expected: 0 errors.

- [ ] **Step 7: Manual verification**

Open an invoice in edit mode, click "Leistung suchen", pick a service, confirm a new position row is appended with correct name/price/tax and totals.

- [ ] **Step 8: Commit**

```bash
git add Handwerker-Client/src/app/features/invoices/invoice-detail/invoice-detail.ts Handwerker-Client/src/app/features/invoices/invoice-detail/invoice-detail.html Handwerker-Client/src/assets/i18n/de.json Handwerker-Client/src/assets/i18n/en.json Handwerker-Client/src/assets/i18n/fr.json
git commit -m "feat(invoices): add Leistungen autocomplete to invoice positions"
```

---

## Task 9: New "Positionen" tab for Orders

**Files:**
- Create: `Handwerker-Client/src/app/features/orders/order-detail/tabs/order-positions/order-positions.ts`
- Create: `Handwerker-Client/src/app/features/orders/order-detail/tabs/order-positions/order-positions.html`
- Create: `Handwerker-Client/src/app/features/orders/order-detail/tabs/order-positions/order-positions.test.ts`
- Modify: `Handwerker-Client/src/app/features/orders/order-detail/order-detail.ts`
- Modify: `Handwerker-Client/src/app/features/orders/order-detail/order-detail.html`

**Interfaces:**
- Consumes: `ProductService` (`getProducts`), `ServiceItemService` (`getServices`), `Product`/`ServiceItem` (Task 5).
- Produces: `OrderPositions` component with `products = input<Product[]>([])`, `productsChange = output<Product[]>()`.

**Note on i18n:** this tab follows the existing `order-detail`/`order-materials`/`order-worktime` convention of hardcoded German text — no `TranslatePipe`, no i18n keys added.

- [ ] **Step 1: Write the failing component test**

```typescript
// Handwerker-Client/src/app/features/orders/order-detail/tabs/order-positions/order-positions.test.ts
import '@angular/compiler';
import { Injector, runInInjectionContext } from '@angular/core';
import { of } from 'rxjs';
import { describe, expect, it, vi } from 'vitest';

import { OrderPositions } from './order-positions';
import { ProductService } from '../../../../products/services/product.service';
import { ServiceItemService } from '../../../../../core/services';
import { ServiceItem, Product } from '../../../../../core/entities';

describe('OrderPositions', () => {
  const mockService: ServiceItem = {
    id: 1,
    serviceNumber: 'L-0001',
    name: 'Montage',
    description: '',
    unit: 'Std.',
    unitPrice: 65,
    taxRate: 19,
    isActive: true
  };

  const productServiceMock = { getProducts: vi.fn(() => of([])) };
  const serviceItemServiceMock = { getServices: vi.fn(() => of([mockService])) };

  function createInstance(initialProducts: Product[] = []): OrderPositions {
    const injector = Injector.create({
      providers: [
        { provide: ProductService, useValue: productServiceMock },
        { provide: ServiceItemService, useValue: serviceItemServiceMock }
      ]
    });
    const instance = runInInjectionContext(injector, () => new OrderPositions());
    (instance as any).products = () => initialProducts;
    return instance;
  }

  it('emits productsChange with an appended service position', () => {
    const component = createInstance([]);
    let emitted: Product[] | undefined;
    component.productsChange.subscribe((value: Product[]) => (emitted = value));

    component.addService(mockService);

    expect(emitted).toHaveLength(1);
    expect(emitted![0].name).toBe('Montage');
    expect(emitted![0].unitPrice).toBe(65);
    expect(emitted![0].totalGross).toBeCloseTo(77.35);
  });

  it('emits productsChange with a manual empty position', () => {
    const component = createInstance([]);
    let emitted: Product[] | undefined;
    component.productsChange.subscribe((value: Product[]) => (emitted = value));

    component.addManualPosition();

    expect(emitted).toHaveLength(1);
    expect(emitted![0].name).toBe('');
    expect(emitted![0].position).toBe(1);
  });

  it('removes a position and emits the updated list', () => {
    const component = createInstance([]);
    component.addManualPosition();
    let emitted: Product[] | undefined;
    component.productsChange.subscribe((value: Product[]) => (emitted = value));

    component.removeItem(0);

    expect(emitted).toHaveLength(0);
  });
});
```

- [ ] **Step 2: Run the test to verify it fails**

Run: `cd Handwerker-Client && npx vitest run src/app/features/orders/order-detail/tabs/order-positions/order-positions.test.ts`
Expected: FAIL — `./order-positions` module not found.

- [ ] **Step 3: Implement the component**

```typescript
// Handwerker-Client/src/app/features/orders/order-detail/tabs/order-positions/order-positions.ts
import { ChangeDetectionStrategy, Component, inject, input, output, signal } from '@angular/core';
import { Product } from '../../../../../core/entities';
import { ServiceItem } from '../../../../../core/entities';
import { ProductService } from '../../../../products/services/product.service';
import { ServiceItemService } from '../../../../../core/services';

@Component({
  selector: 'app-order-positions',
  standalone: true,
  imports: [],
  templateUrl: './order-positions.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class OrderPositions {
  private productService = inject(ProductService);
  private serviceItemService = inject(ServiceItemService);

  products = input<Product[]>([]);
  productsChange = output<Product[]>();

  productOptions = signal<Product[]>([]);
  serviceOptions = signal<ServiceItem[]>([]);

  searchProduct(term: string) {
    this.productService.getProducts().subscribe(list => {
      const filtered = term ? list.filter(p => p.name.toLowerCase().includes(term.toLowerCase())) : list;
      this.productOptions.set(filtered);
    });
  }

  searchService(term: string) {
    this.serviceItemService.getServices().subscribe(list => {
      const filtered = term
        ? list.filter(s => s.name.toLowerCase().includes(term.toLowerCase()) || s.serviceNumber.includes(term))
        : list;
      this.serviceOptions.set(filtered);
    });
  }

  addProduct(product: Product) {
    const taxRate = product.taxRate || 19;
    const totalNet = product.unitPrice;
    const taxAmount = totalNet * (taxRate / 100);
    const totalGross = totalNet + taxAmount;

    const newItem: Product = {
      ...product,
      position: this.products().length + 1,
      quantity: 1,
      discountPercent: 0,
      discountAmount: 0,
      taxAmount,
      totalNet,
      totalGross
    };

    this.productsChange.emit([...this.products(), newItem]);
  }

  addService(service: ServiceItem) {
    const taxRate = service.taxRate || 19;
    const totalNet = service.unitPrice;
    const taxAmount = totalNet * (taxRate / 100);
    const totalGross = totalNet + taxAmount;

    const newItem: Product = {
      id: 0,
      articleNumber: service.serviceNumber,
      name: service.name,
      description: service.description || '',
      unit: service.unit,
      unitPrice: service.unitPrice,
      taxRate,
      position: this.products().length + 1,
      quantity: 1,
      discountPercent: 0,
      discountAmount: 0,
      taxAmount,
      totalNet,
      totalGross
    };

    this.productsChange.emit([...this.products(), newItem]);
  }

  addManualPosition() {
    const newItem: Product = {
      id: 0,
      articleNumber: '',
      name: '',
      description: '',
      unit: 'Stk',
      unitPrice: 0,
      taxRate: 19,
      taxAmount: 0,
      quantity: 1,
      discountPercent: 0,
      discountAmount: 0,
      totalNet: 0,
      totalGross: 0,
      position: this.products().length + 1
    };

    this.productsChange.emit([...this.products(), newItem]);
  }

  updateItemProperty(index: number, property: keyof Product, value: any) {
    const items = [...this.products()];
    const item = { ...items[index], [property]: value } as Product;

    if (property === 'quantity' || property === 'unitPrice' || property === 'discountPercent' || property === 'taxRate') {
      const quantity = Number(item.quantity) || 0;
      const unitPrice = Number(item.unitPrice) || 0;
      const discountPercent = Number(item.discountPercent) || 0;
      const taxRate = Number(item.taxRate) || 19;

      item.totalNet = unitPrice * quantity * (1 - discountPercent / 100);
      item.taxAmount = item.totalNet * (taxRate / 100);
      item.totalGross = item.totalNet + item.taxAmount;
    }

    items[index] = item;
    this.productsChange.emit(items);
  }

  removeItem(index: number) {
    this.productsChange.emit(this.products().filter((_, i) => i !== index));
  }
}
```

- [ ] **Step 4: Implement the template**

```html
<!-- Handwerker-Client/src/app/features/orders/order-detail/tabs/order-positions/order-positions.html -->
<div class="space-y-6">
  <div class="card bg-base-200">
    <div class="card-body">
      <div class="overflow-x-auto">
        <table class="table table-zebra">
          <thead>
            <tr>
              <th>Pos.</th>
              <th>Bezeichnung</th>
              <th class="text-right">Menge</th>
              <th class="text-right">Einzelpreis</th>
              <th class="text-right">MwSt. %</th>
              <th class="text-right">Netto</th>
              <th></th>
            </tr>
          </thead>
          <tbody>
            @for (item of products(); track $index; let i = $index) {
              <tr>
                <td>{{ i + 1 }}</td>
                <td>
                  <input type="text" [value]="item.name" (input)="updateItemProperty(i, 'name', $any($event.target).value)" class="input input-ghost input-xs w-full p-0" aria-label="Bezeichnung" />
                </td>
                <td class="text-right">
                  <input type="number" [value]="item.quantity" (input)="updateItemProperty(i, 'quantity', $any($event.target).value)" class="input input-ghost input-xs w-16 text-right p-0" aria-label="Menge" />
                </td>
                <td class="text-right">
                  <input type="number" step="0.01" [value]="item.unitPrice" (input)="updateItemProperty(i, 'unitPrice', $any($event.target).value)" class="input input-ghost input-xs w-24 text-right p-0" aria-label="Einzelpreis" />
                </td>
                <td class="text-right">
                  <input type="number" [value]="item.taxRate" (input)="updateItemProperty(i, 'taxRate', $any($event.target).value)" class="input input-ghost input-xs w-16 text-right p-0" aria-label="MwSt" />
                </td>
                <td class="text-right font-bold">{{ item.totalNet | number:'1.2-2' }} €</td>
                <td class="text-right">
                  <button class="btn btn-xs btn-circle btn-ghost text-error" (click)="removeItem(i)" title="Entfernen" type="button">
                    <i class="fa-solid fa-times"></i>
                  </button>
                </td>
              </tr>
            } @empty {
              <tr>
                <td colspan="7" class="text-center py-8 text-gray-500">Keine Positionen vorhanden</td>
              </tr>
            }
          </tbody>
        </table>
      </div>

      <div class="flex justify-center gap-6 mt-6 text-sm font-medium">
        <button type="button" class="text-primary hover:underline flex items-center gap-1" (click)="addManualPosition()">
          <i class="fa-solid fa-plus"></i> Position hinzufügen
        </button>

        <div class="dropdown dropdown-top">
          <div tabindex="0" role="button" class="text-primary hover:underline flex items-center gap-1">
            <i class="fa-solid fa-magnifying-glass"></i> Produkt suchen
          </div>
          <div tabindex="0" class="dropdown-content z-[1] card card-compact w-80 p-2 shadow bg-base-100 border border-base-200">
            <div class="card-body p-2">
              <input type="text" class="input input-bordered input-sm w-full" placeholder="Produkt suchen..."
                     (input)="searchProduct($any($event.target).value)" (focus)="searchProduct('')">
              <ul class="menu w-full rounded-box max-h-48 overflow-y-auto p-0 mt-2">
                @for (p of productOptions(); track p.id) {
                  <li><a (click)="addProduct(p)" class="flex justify-between text-xs">
                    <span class="truncate max-w-[150px]">{{ p.name }}</span>
                    <span class="font-bold">{{ p.unitPrice }} €</span>
                  </a></li>
                }
              </ul>
            </div>
          </div>
        </div>

        <div class="dropdown dropdown-top">
          <div tabindex="0" role="button" class="text-primary hover:underline flex items-center gap-1">
            <i class="fa-solid fa-magnifying-glass"></i> Leistung suchen
          </div>
          <div tabindex="0" class="dropdown-content z-[1] card card-compact w-80 p-2 shadow bg-base-100 border border-base-200">
            <div class="card-body p-2">
              <input type="text" class="input input-bordered input-sm w-full" placeholder="Leistung suchen..."
                     (input)="searchService($any($event.target).value)" (focus)="searchService('')">
              <ul class="menu w-full rounded-box max-h-48 overflow-y-auto p-0 mt-2">
                @for (s of serviceOptions(); track s.id) {
                  <li><a (click)="addService(s)" class="flex justify-between text-xs">
                    <span class="truncate max-w-[150px]">{{ s.name }}</span>
                    <span class="font-bold">{{ s.unitPrice }} €</span>
                  </a></li>
                }
              </ul>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</div>
```

Note: `number` pipe requires `CommonModule` — add `CommonModule` to the component's `imports` array in Step 3 (`imports: [CommonModule]`) since the template above uses `| number`.

- [ ] **Step 5: Run the test to verify it passes**

Run: `cd Handwerker-Client && npx vitest run src/app/features/orders/order-detail/tabs/order-positions/order-positions.test.ts`
Expected: PASS (3 tests).

- [ ] **Step 6: Wire the new tab into `order-detail.ts`**

In `Handwerker-Client/src/app/features/orders/order-detail/order-detail.ts`:

Add the import:
```typescript
import { OrderPositions } from './tabs/order-positions/order-positions';
```

Add it to the component's `imports` array (alongside `OrderMaterials`, `OrderWorktime`).

Widen the tab union and add a backing signal for positions, near the existing `activeTab` declaration:
```typescript
activeTab = signal<'overview' | 'materials' | 'worktime' | 'positions'>('overview');
orderPositions = signal<Product[]>([]);
```

In `setTab`, widen the parameter type to match:
```typescript
setTab(tab: 'overview' | 'materials' | 'worktime' | 'positions') {
  this.activeTab.set(tab);
}
```

In `loadOrder`, right after `this.order.set(order);`, add:
```typescript
this.orderPositions.set(order.products ?? []);
```

In `onSave()`, replace the hardcoded `products: []` in the `isNew()` branch with `products: this.orderPositions()`, and replace `products: order.products` in the update branch with `products: this.orderPositions()`.

- [ ] **Step 7: Add the tab button and content to `order-detail.html`**

In `Handwerker-Client/src/app/features/orders/order-detail/order-detail.html`, add a fourth tab button next to `worktime` (inside the `tabs tabs-boxed` div):

```html
      <button class="tab" [class.tab-active]="activeTab() === 'positions'" (click)="setTab('positions')">
        Positionen
      </button>
```

And add the tab content block next to the existing Materials/Worktime blocks:

```html
    <!-- Positions Tab -->
    @if (activeTab() === 'positions') {
      <app-order-positions [products]="orderPositions()" (productsChange)="orderPositions.set($event)"></app-order-positions>
    }
```

- [ ] **Step 8: Type-check**

Run: `cd Handwerker-Client && npx tsc -p tsconfig.app.json --noEmit`
Expected: 0 errors.

- [ ] **Step 9: Manual verification**

Open an existing order (or create a new one), switch to the "Positionen" tab, add a manual position, a product, and a service; confirm the table updates and totals compute; save the order and reload it, confirming the positions persisted (backend already supports this via `Order.Products`).

- [ ] **Step 10: Commit**

```bash
git add Handwerker-Client/src/app/features/orders/order-detail/tabs/order-positions/ Handwerker-Client/src/app/features/orders/order-detail/order-detail.ts Handwerker-Client/src/app/features/orders/order-detail/order-detail.html
git commit -m "feat(orders): add Positionen tab with product and Leistungen autocomplete"
```

---

## Task 10: Full regression pass

**Files:** none (verification only)

- [ ] **Step 1: Run the full backend test suite**

Run: `dotnet test Handwerker.sln`
Expected: All tests pass (existing `WebTests` + new `ServiceItemServiceTests`).

- [ ] **Step 2: Run the full frontend test suite**

Run: `cd Handwerker-Client && npx vitest run`
Expected: All tests pass, including the new `service-item.service.spec.ts`, `services.test.ts`, and `order-positions.test.ts`.

- [ ] **Step 3: Full frontend type-check**

Run: `cd Handwerker-Client && npx tsc -p tsconfig.app.json --noEmit`
Expected: 0 errors.

- [ ] **Step 4: Full backend build**

Run: `dotnet build Handwerker.sln`
Expected: Build succeeded, 0 errors, 0 new warnings.

- [ ] **Step 5: Manual end-to-end walkthrough**

Start the app, then: create a Leistung on `/services` → open an offer, add it via "Leistung suchen", save the offer → open an invoice, add a (possibly different) Leistung via "Leistung suchen", save → open an order, go to the new "Positionen" tab, add a Leistung and a Produkt, save, reload the order and confirm both positions are still there.
