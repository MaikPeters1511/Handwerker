# Changelog

Alle nennenswerten Änderungen an diesem Projekt werden in dieser Datei dokumentiert.

Das Format basiert auf [Keep a Changelog](https://keepachangelog.com/de/1.0.0/),
und dieses Projekt hält sich an [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [0.4.0] - 2026-03-02

### Hinzugefügt
- Angebotsverwaltung (Offer Management) im Backend und Frontend implementiert. [58abdf5](https://github.com/MaikPeters1511/Handwerker/commit/58abdf5)
- Installations-Assistent zur Ersteinrichtung mit neuen Migrationen und UI-Updates hinzugefügt. [9264774](https://github.com/MaikPeters1511/Handwerker/commit/9264774)
- Dashboard-Funktionen mit monatlichen Statistiken, Beträgen und Rechnungsstatistik-Endpunkten. [7d81c7c](https://github.com/MaikPeters1511/Handwerker/commit/7d81c7c)
- Rollenbasierte Dashboard-Berechtigungen eingeführt. [1b48181](https://github.com/MaikPeters1511/Handwerker/commit/1b48181)
- Profilbearbeitungs-Funktionalität inklusive i18n und Keycloak-Mappings. [df8143b](https://github.com/MaikPeters1511/Handwerker/commit/df8143b), [bd369db](https://github.com/MaikPeters1511/Handwerker/commit/bd369db)
- Benutzerverwaltung mit Rollenzuweisung im Frontend implementiert. [d71f0a1](https://github.com/MaikPeters1511/Handwerker/commit/d71f0a1), [94a71c5](https://github.com/MaikPeters1511/Handwerker/commit/94a71c5)
- Unterstützung für MAUI inklusive OpenTelemetry-Integration und Android Emulator Support. [0228488](https://github.com/MaikPeters1511/Handwerker/commit/0228488)
- Integration von HybridCache in ApiService und Application-Layer. [942ca65](https://github.com/MaikPeters1511/Handwerker/commit/942ca65)
- Mehrsprachigkeits-Unterstützung (i18n) für Installations-Assistent und Firmenverwaltung wieder eingeführt. [3812103](https://github.com/MaikPeters1511/Handwerker/commit/3812103)
- Anbieterverwaltung (Provider Management) im Frontend hinzugefügt. [d4b5ba5](https://github.com/MaikPeters1511/Handwerker/commit/d4b5ba5)
- Keycloak-Datenbankkonfiguration und Umgebungseinstellungen ergänzt. [96f12da](https://github.com/MaikPeters1511/Handwerker/commit/96f12da)

### Geändert
- Architektur auf Onion Architecture umgestellt. [9a4059c](https://github.com/MaikPeters1511/Handwerker/commit/9a4059c)
- Firmenverwaltung verbessert: Barrierefreiheit, konsistente UI und Unterstützung für Firmenlogos. [fe3e237](https://github.com/MaikPeters1511/Handwerker/commit/fe3e237), [6b37aaa](https://github.com/MaikPeters1511/Handwerker/commit/6b37aaa)
- Keycloak-Konfiguration optimiert (Direct Access Grants, Service Accounts, Scope-Anpassungen). [b2e500d](https://github.com/MaikPeters1511/Handwerker/commit/b2e500d), [a284994](https://github.com/MaikPeters1511/Handwerker/commit/a284994)
- README aktualisiert mit Details zu MAUI und Infrastruktur. [7922c83](https://github.com/MaikPeters1511/Handwerker/commit/7922c83), [1732376](https://github.com/MaikPeters1511/Handwerker/commit/1732376)
- NuGet-Pakete und npm-Abhängigkeiten regelmäßig aktualisiert.

### Behoben
- Keycloak-Umgebungsvariablen standardisiert und Fehlerbehandlung verbessert. [f1a8181](https://github.com/MaikPeters1511/Handwerker/commit/f1a8181)
- Schreibfehler in Keycloak-Variablen korrigiert (`keycloakBaseURL` zu `keycloakBaseUrl`). [0228488](https://github.com/MaikPeters1511/Handwerker/commit/0228488)

### Entfernt
- Elasticsearch-Integration und zugehörige Einstellungen vollständig entfernt. [83a7309](https://github.com/MaikPeters1511/Handwerker/commit/83a7309)
- Veraltete Company- und Auth-Module im Angular-Client entfernt. [0071621](https://github.com/MaikPeters1511/Handwerker/commit/0071621)
- Veraltete Testdokumentation und InvoicesControllerTests gelöscht. [2be530b](https://github.com/MaikPeters1511/Handwerker/commit/2be530b), [d65094e](https://github.com/MaikPeters1511/Handwerker/commit/d65094e)

## [0.3.0] - 2026-01-20

### Hinzugefügt
- Elasticsearch- und Realtime-Suche in den Benutzereinstellungen implementiert. [2b227d7](https://github.com/MaikPeters1511/Handwerker/commit/2b227d7)
- Benachrichtigungsfunktionalität eingeführt. [9389d89](https://github.com/MaikPeters1511/Handwerker/commit/9389d89)
- UserSettings eingeführt und API entsprechend angepasst, inklusive Test-E-Mail-Feldern. [bb525cf](https://github.com/MaikPeters1511/Handwerker/commit/bb525cf), [1aea46a](https://github.com/MaikPeters1511/Handwerker/commit/1aea46a)
- Kundenverwaltung (Recipients) hinzugefügt. [25b2be3](https://github.com/MaikPeters1511/Handwerker/commit/25b2be3)
- AppSettings und zugehörige Erweiterungen implementiert. [a5161c7](https://github.com/MaikPeters1511/Handwerker/commit/a5161c7)

### Geändert
- Seitenmenü (Sidebar) überarbeitet und dynamische Anpassungen hinzugefügt. [a52a78b](https://github.com/MaikPeters1511/Handwerker/commit/a52a78b)
- Bibliotheken aktualisiert und Tailwind CSS integriert. [903955b](https://github.com/MaikPeters1511/Handwerker/commit/903955b)
- Projektstruktur optimiert und umstrukturiert. [04ddb58](https://github.com/MaikPeters1511/Handwerker/commit/04ddb58)
- .gitignore Verzeichnisregeln präzisiert. [26b2d34](https://github.com/MaikPeters1511/Handwerker/commit/26b2d34)

### Entfernt
- Mehrsprachige Unterstützung (i18n) vorerst entfernt. [903955b](https://github.com/MaikPeters1511/Handwerker/commit/903955b)

## [0.2.0] - 2026-01-15

### Hinzugefügt
- Mehrsprachige Unterstützung (i18n) und Lokalisierung eingeführt. [9d7347f](https://github.com/MaikPeters1511/Handwerker/commit/9d7347f)
- Vitest als Test-Runner eingeführt und neue Tests für Zoneless-Komponenten hinzugefügt. [c407924](https://github.com/MaikPeters1511/Handwerker/commit/c407924)
- Autorisierung und Implementierung des `ProvidersController` im ApiService hinzugefügt. [8b00e16](https://github.com/MaikPeters1511/Handwerker/commit/8b00e16)
- Integration von Keycloak-Authentifizierung und Hinzufügen von Autorisierung in der API. [75fad07](https://github.com/MaikPeters1511/Handwerker/commit/75fad07)
- Neue Datenbank-Migrationen zur Erstellung von Tabellen und Beziehungen hinzugefügt. [1c70b60]
- Implementierung der Produktverwaltung und Optimierung der Services. [4d6976d]
- Datenbank-Migration zur Einführung von Validierungsattributen und Datentypanpassungen. [b11353b]
- Anpassung der Validierung für Bank- und Rechnungsdaten. [9102800]
- Modulare Sidenav-Komponente für das Frontend implementiert. [286007b]
- API-Controller für Rechnungen, Produkte, Empfänger, Anbieter und Banken implementiert. [c276694]
- Initialisierung der Datenbank-Migrationen für das Rechnungsmanagement. [1982bce]
- Basis-Modelle für die Rechnungsverwaltung im API-Service hinzugefügt. [d9561c6]

### Geändert
- Anpassungen an Bank- und Rechnungsmodellen sowie Aktualisierung der Abhängigkeiten. [f8c3551], [85c244c]
- Umstellung auf Zoneless Change Detection in Angular (Entfernung von Zone.js). [dd2a32f]
- Projekt-Richtlinien hinzugefügt und Komponenten-Konfiguration aktualisiert. [70a7b76]

### Entfernt
- Wettervorhersage-Endpunkt und zugehöriges Modell aus dem ApiService entfernt. [bc49a21](https://github.com/MaikPeters1511/Handwerker/commit/bc49a21)
- Veraltetes `BankAccount`-Modell entfernt und Tabellenstruktur angepasst. [4dcb678]

## [0.1.0] - 2026-01-11

### Hinzugefügt
- Font Awesome Icons für die Benutzeroberfläche integriert. [685edf2]
- Sidebar-Toggle-Funktionalität implementiert. [685edf2]
- Willkommensseite mit Routing und bedingtem Rendering hinzugefügt. [7ded30f]
- Konfigurationsdateien für die Angular-Anwendung und Benutzerverwaltung hinzugefügt. [43d2a6e]
- Benutzerauthentifizierung und Profilverwaltungsfunktionen implementiert. [6688874]

### Geändert
- .gitignore aktualisiert, um Entwicklungs- und Datenbankdateien auszuschließen. [5ac6112]

## [0.0.1] - 2026-01-08

### Hinzugefügt
- Initiales Projekt-Setup. [38dffb6]
