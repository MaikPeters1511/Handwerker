# Löhne (Lohnarten-Katalog) – Design

## Kontext

Der Sidenav enthält bereits einen (toten) Link auf `/wages` (`sidenav.html:60,73`, Übersetzungsschlüssel `nav.wages` = "Löhne"), aber es existiert weder Backend noch Frontend dafür. Die Feature "Leistungen" (ServiceItem) wurde kürzlich fertiggestellt und dient als direktes Vorbild: ein eigenständiger Stammdaten-Katalog mit Suche, Anlegen/Bearbeiten-Formular und Löschfunktion, ohne Verzahnung mit Angeboten/Rechnungen/Aufträgen.

## Scope

Löhne wird als **eigenständiger Katalog** gebaut, strukturell identisch zu Leistungen:

- Backend-CRUD für "Lohnarten" (Personalkosten-Sätze nach Rolle, z. B. "Facharbeiter", "Meister", "Azubi")
- Frontend-Seite unter `/wages` mit Suche, Liste, Anlegen/Bearbeiten, Löschen
- **Keine** Integration in Order/Invoice/Offer-Positionen (das wäre ein separates, späteres Feature)

## DSGVO-Einordnung

Lohnarten sind ein anonymer Rollen-/Satzkatalog ("Facharbeiter = 45€/h"), **nicht** an konkrete, identifizierbare Mitarbeiter gebunden. Es werden keine personenbezogenen Daten (Name, Gehalt einer bestimmten Person) gespeichert. Damit greift kein besonderer DSGVO-Mechanismus (keine eigene Rechtsgrundlage, kein Löschkonzept für Betroffenenrechte nötig) – Standard-Zugriffsschutz über bestehende Auth/Rollen (wie bei anderen Stammdaten, z. B. Company, Product) genügt.

Falls das Feature später um mitarbeiterbezogene Lohnsätze erweitert wird (z. B. Verknüpfung mit `WorkTimeEntry.UserId`), muss diese Einordnung neu bewertet werden (Rechtsgrundlage Arbeitsvertrag Art. 6(1)(b), Zugriffsbeschränkung auf Admin/HR-Rolle, Lösch-/Aufbewahrungskonzept).

## Datenmodell

Neue Entity `WageType` (Namespace `Handwerker.Domain.Entities`), analog zu `ServiceItem`:

```csharp
public class WageType
{
    public int Id { get; set; }
    public string WageNumber { get; set; }   // auto-generiert, z. B. "LN-0001"
    public string Name { get; set; }          // z. B. "Facharbeiter", max 255
    public string? Description { get; set; }  // max 2000
    public decimal HourlyRate { get; set; }
    public decimal TaxRate { get; set; } = 19;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
```

Nummernpräfix `LN-` (statt `L-` bei Leistungen), damit sich Lohnarten- und Leistungsnummern nicht überschneiden.

Indizes: `WageNumber` (unique), `Name`, `IsActive` – wie bei `ServiceItem`.

## Backend-Architektur

1:1-Spiegelung der Leistungen-Architektur:

- `Handwerker.Domain.Entities.WageType`
- `Handwerker.Domain.Interfaces.IWageTypeRepository`
- `Handwerker.Infrastructure.Repositories.WageTypeRepository` (EF Core, `HandwerkerDbContext.WageTypes`)
- `Handwerker.Application.Services.IWageTypeService` / `WageTypeService`
  - `CreateAsync` generiert `WageNumber` fortlaufend (`LN-0001`, `LN-0002`, …), analog zu `ServiceItemService`
  - `DeleteAsync` ist Soft-Delete (`IsActive = false`), analog zu `ServiceItemService`
- `Handwerker.ApiService.Controllers.WageTypesController` (`[Route("api/wagetypes")]`, `[Authorize]`)
  - `GET /api/wagetypes`
  - `GET /api/wagetypes/active`
  - `GET /api/wagetypes/search?term=`
  - `GET /api/wagetypes/{id}`
  - `POST /api/wagetypes`
  - `PUT /api/wagetypes/{id}`
  - `DELETE /api/wagetypes/{id}` (Soft-Delete)
- `NotificationService`: drei neue Methoden `NotifyWageTypeCreatedAsync`, `NotifyWageTypeUpdatedAsync`, `NotifyWageTypeDeletedAsync`, analog zu den ServiceItem-Benachrichtigungen
- EF-Migration `AddWageTypes`
- DI-Registrierung in `Program.cs`: `IWageTypeRepository`/`WageTypeRepository`, `IWageTypeService`/`WageTypeService`

## Frontend-Architektur

- `Handwerker-Client/src/app/core/entities/wage-type.model.ts` (`WageType`, `CreateWageTypeRequest`, `UpdateWageTypeRequest`)
- `Handwerker-Client/src/app/core/interfaces/form/IWageTypeFormModel.ts`
- `Handwerker-Client/src/app/core/services/wage-type.service.ts` (HTTP-Client, Endpunkte wie oben)
- `Handwerker-Client/src/app/features/wages/wages.ts` + `.html`
  - Live-Suche (debounced), Dropdown mit Treffern
  - Listenansicht aller Lohnarten (Tabelle: Nummer, Name, Stundensatz, Aktiv-Status)
  - Formular: Name (required), Beschreibung, Stundensatz (required, ≥0), MwSt. %, Aktiv-Flag
  - Löschen via `DeleteComponent` (bereits vorhandene, wiederverwendbare Komponente)
- Route: `{ path: 'wages', loadComponent: () => import('./features/wages/wages').then(m => m.Wages) }`
- Barrel-Exports (`core/entities/index.ts`, `core/services/index.ts`) ergänzen
- Übersetzungsschlüssel `wages.*` in `de.json`/`en.json`/`fr.json` (Struktur analog zu `services.*`: title, subtitle, masterData, toast, actions, list)
- `nav.wages` existiert bereits

## Tests

- Backend: `Handwerker.Tests/WageTypeServiceTests.cs` (analog `ServiceItemServiceTests`): fortlaufende Nummernvergabe, `IsActive` bei Create, Soft-Delete-Verhalten
- Frontend: `wage-type.service.spec.ts` (HTTP-Mock-Tests wie `service-item.service.spec.ts`), `wages.test.ts` (Komponenten-Unit-Tests wie `services.test.ts`)

## Out of Scope

- Verknüpfung mit `WorkTimeEntry` oder einzelnen Mitarbeitern
- Verwendung in Order-/Invoice-/Offer-Positionen
- Historisierung von Satzänderungen (Gültig-ab-Datum)
