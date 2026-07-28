# Leistungen (Dienstleistungen) — Design

## Kontext

Die App verwaltet bereits Artikel (Warenwirtschaft/Lager, `Article`), Produkte (`Product`, generische Rechnungs-/Angebotspositionen) und einen Nav-Punkt "Leistungen" (`/services`), der bisher auf keine Route zeigt. Dieses Feature führt einen eigenständigen Leistungsstamm (Dienstleistungen/Arbeitsleistungen wie Montage, Beratung, Wartung) ein und macht ihn in Angeboten, Rechnungen und neu auch in Aufträgen als Positions-Vorbelegung nutzbar.

Namenskollision vermieden: Die Domain-Entität heißt `ServiceItem` (nicht `Service`), um Verwechslung mit .NET-Anwendungsservices und Angular-`@Injectable`-Services zu vermeiden.

## Ziel

1. Eigenständige Stammdatenverwaltung für Leistungen (Backend-CRUD-API + Frontend-Seite).
2. Leistungen als Autocomplete-Quelle zur Vorbelegung von Positionen in Angeboten und Rechnungen (bestehende Positionsliste, analog zur bestehenden Produkt-Autocomplete).
3. Neuer Positionen-Tab in Aufträgen (existiert bisher nicht), der dieselbe Positionsliste + Produkt-/Leistungs-Autocomplete erhält wie Angebote/Rechnungen.

## Backend

### Domain (`Handwerker.Domain`)

- **`Entities/ServiceItem.cs`** (record), Felder: `Id`, `ServiceNumber` (string, eindeutig, serverseitig generiert), `Name`, `Description`, `Unit`, `UnitPrice` (decimal), `TaxRate` (decimal, Default 19), `IsActive` (bool), `CreatedAt`/`UpdatedAt` (DateTime, Audit). DataAnnotations (`MaxLength`, `Range`) analog `Article.cs`.
- **`Interfaces/IServiceItemRepository.cs`**: `GetByIdAsync`, `GetAllAsync`, `GetActiveAsync`, `SearchAsync(term)`, `AddAsync`, `UpdateAsync`, `DeleteAsync` (soft, setzt `IsActive=false`), `ExistsAsync(serviceNumber, excludeId?)`, `CountAsync`. Alle Methoden mit `CancellationToken`, analog `IArticleRepository`.

### Infrastructure (`Handwerker.Infrastructure`)

- **`Repositories/ServiceItemRepository.cs`**: primärer Konstruktor mit `HandwerkerDbContext`, `OrderBy(s => s.Name)`, Soft-Delete in `DeleteAsync`.
- **`Data/HandwerkerDbContext.cs`**: neues `DbSet<ServiceItem> Services { get; set; }`, EF-Fluent-Konfiguration inline in `OnModelCreating` unter Kommentar-Banner `// Leistungen-Konfiguration` (kein separates `IEntityTypeConfiguration`, analog Artikel-Muster).
- Migration `AddServiceItems` (EF Core, `dotnet ef migrations add`).

### Application (`Handwerker.Application`)

- **`Services/IServiceItemService.cs`** + **`ServiceItemService.cs`**: primärer Konstruktor mit `IServiceItemRepository`. `CreateAsync` generiert `ServiceNumber` automatisch fortlaufend im Format `L-0001`, `L-0002`, … (basierend auf `CountAsync()+1`, wie bei den übrigen fortlaufenden Nummern in der Codebase — keine Concurrency-Absicherung nötig, da Single-User-Kontext pro Mandant ausreichend für dieses Projektstadium) und setzt `IsActive=true`. Restliche Methoden dünne Pass-throughs zum Repository.

### API (`Handwerker.ApiService`)

- **`Controllers/ServicesController.cs`**: erbt von `ApiControllerBase`, `[Route("api/services")]`, `[Authorize]`. DI: `IServiceItemService`, `NotificationService` (feuert `NotifyServiceCreated/Updated/Deleted` analog Artikel).
- **`Controllers/ServiceRequests.cs`**: `ServiceItemDto`, `CreateServiceItemRequest` (ohne `ServiceNumber` — wird serverseitig vergeben), `UpdateServiceItemRequest` (records, manuelles Mapping via statische `MapToDto`-Methode, kein AutoMapper).
- Endpunkte: `GET /api/services`, `GET /api/services/active`, `GET /api/services/search?term=`, `GET /api/services/{id:int}`, `POST /api/services`, `PUT /api/services/{id:int}`, `DELETE /api/services/{id:int}` (soft delete, `NoContent()`).

## Frontend (`Handwerker-Client`)

### Leistungsstamm-Verwaltung

- **`core/entities/service-item.model.ts`**: `ServiceItem`, `CreateServiceItemRequest`, `UpdateServiceItemRequest` — camelCase 1:1 zu Backend-DTOs. Export über bestehenden `core/entities/index.ts`-Barrel.
- **`core/services/service-item.service.ts`**: `ServiceItemService` (`providedIn: 'root'`, `inject(HttpClient)`, `apiUrl = '/api/services'`), Methoden `getServices()`, `getActiveServices()`, `searchServices(term)`, `getService(id)`, `createService(request)`, `updateService(id, request)`, `deleteService(id)`. Export über `core/services/index.ts`-Barrel.
- **`features/services/services.ts` + `.html`**: Single-Page-Master-Detail-Komponente analog `features/products/products.ts`. `ChangeDetectionStrategy.OnPush`, Signals (`searchTerm`, `searchResults`, `selectedService`, `isEditing`, Toast-Signals), Reactive Form via `IServiceItemFormModel` (`core/interfaces/form/IServiceItemFormModel.ts`). Formularfelder: Leistungsnummer (read-only, automatisch generiert), Bezeichnung, Beschreibung, Einheit, Preis/Einheit, MwSt.-Satz, Aktiv-Status.
- **Route**: `{ path: 'services', component: Services }` in `app.routes.ts` (Sidenav-Link `/services` existiert bereits).
- **i18n**: neues Top-Level-Objekt `"services": {...}` in `de.json`/`en.json`/`fr.json`, Struktur parallel zu `"products"` (title/subtitle/search/masterData/toast/actions). `nav.services` existiert bereits und wird nicht verändert.

### Integration in Angebote (`offer-detail.ts`/`.html`) und Rechnungen (`invoice-detail.ts`/`.html`)

Zusätzlich zur bestehenden "Produkt suchen"-Dropdown (`searchProduct`/`addProduct`) wird eine zweite, parallele Dropdown "Leistung suchen" ergänzt:

- Injiziert `ServiceItemService`, eigenes Signal `services`.
- `searchService(term)`: in `offer-detail.ts` client-seitige Filterung über `getServices()` (analog bestehendem `searchProduct`); in `invoice-detail.ts` über `searchServices(term)` (Server-Suche, analog bestehendem `searchProducts(value)`).
- `addService(service: ServiceItem)`: mappt Felder in eine neue `Product`-Zeile (`name`, `description`, `unit`, `unitPrice`, `taxRate`, `articleNumber` ← `serviceNumber`, `quantity: 1`, `position` fortlaufend, `discountPercent: 0`) und wendet dieselbe Berechnungslogik wie `addProduct` an (Netto/Steuer/Brutto).
- Neue i18n-Keys: `offers.detail.actions.searchService`/`searchServicePlaceholder`, `invoices.detail.actions.searchService`/`searchServicePlaceholder` (parallel zu den bestehenden `searchProduct`-Keys).

### Neuer Positionen-Tab in Aufträgen (`order-detail.ts`/`.html`)

Backend unterstützt `Order.Products` bereits vollständig (Persistenz über `OrderService`/`OrdersController`), es fehlt nur die UI:

- Neue Tab-Komponente `tabs/order-positions/order-positions.ts` + `.html`, analog zu den bestehenden Tabs `order-materials`/`order-worktime`.
- `order = input<Order|null>(null)`, `positionsChange = output<Product[]>()`. Enthält lokale Positionsliste (Signal, initialisiert aus `order().products`), dieselbe Tabelle + "Position hinzufügen" + "Produkt suchen" + "Leistung suchen"-Dropdowns wie in Angeboten (Logik übernommen, keine Dopplung der Berechnungsformeln — falls sinnvoll extrahierbar in eine gemeinsame Utility, sonst bewusste Kopie wie im Bestand üblich).
- `order-detail.ts`: `activeTab`-Union erweitert um `'positions'`, neuer Tab-Button, hört auf `positionsChange` und übernimmt die Liste ins eigene Order-Signal/Formular; Speicherung erfolgt über den regulären Auftrags-Speichervorgang (kein separater Save-Call).
- Neue i18n-Sektion `orders.detail.tabs.positions` sowie `orders.positions.*` (Tabellenspalten, Actions), analog zu `offers.detail.*`.

## Out of Scope

- Keine Warenwirtschaft/Lagerbestand für Leistungen (im Gegensatz zu Artikeln).
- Keine Änderung an `order-materials` (Artikel-Tab) oder dessen Backend.
- Keine Concurrency-sichere Nummernvergabe (Sequenz-Tabelle o.ä.) — einfaches `Count+1`-Schema wie sonst in der Codebase üblich.

## Testing

- Backend: Unit-/Integrationstests für `ServiceItemService` (Nummerngenerierung, Soft-Delete) und `ServicesController`-Endpunkte, analog bestehenden Artikel-Tests (falls vorhanden — sonst neue Tests nach Muster von `Handwerker.Tests`).
- Frontend: Komponenten-/Service-Tests für `ServiceItemService`, `Services`-Komponente sowie die neuen `searchService`/`addService`-Methoden in Angebot/Rechnung/Auftrag, analog bestehenden Tests (z. B. `invoice-detail.test.ts`).
- Manuelle Verifikation: Leistung anlegen → in Angebot/Rechnung/Auftrag per Suche hinzufügen → Position korrekt vorbelegt (Preis/Steuer/Summe) → Beleg speichern und neu laden.
