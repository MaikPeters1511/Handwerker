# 🏗️ Handwerker - Full-Stack-Anwendung

## 📋 Überblick

Handwerker ist eine moderne Full-Stack-Anwendung, die auf den neuesten Technologien basiert:
- **Frontend:** Angular 21 (TypeScript, TailwindCSS 4, DaisyUI 5, Vite, Vitest, Playwright/Axe A11y)
- **Mobile:** .NET MAUI (C# 15, Community Toolkit)
- **Backend:** .NET 10 (C# 15, ASP.NET Core, Entity Framework Core 10)
- **Infrastruktur:** Docker, PostgreSQL 18.2, Redis 8.4.1, Keycloak 26.5.3, Mailpit 1.28.2
- **Orchestrierung:** .NET Aspire 10
- **Tools:** pgAdmin 9.11.0, Redis Insight 3.0

Das Projekt ist als robuster Ausgangspunkt für den Aufbau skalierbarer Anwendungen mit einer einheitlichen Entwicklererfahrung konzipiert.

## 🛠️ Voraussetzungen

Um das Projekt lokal auszuführen, benötigen Sie:
- **.NET 10 SDK** (definiert in `global.json`)
- **Docker Desktop** oder eine kompatible Container-Runtime (für Postgres, Redis, Keycloak)
- **pnpm** (Paketmanager für das Angular-Frontend)
- **Node.js** (LTS empfohlen)

## 🚀 Schnellstart

### 1. Repository klonen
```bash
git clone <repo-url>
cd Handwerker
```

### 2. Frontend-Zertifikate (Optional, aber empfohlen)
Für die lokale Entwicklung mit HTTPS:
```bash
cd Handwerker-Client
pnpm run setup:cert
cd ..
```

### 3. Solution bauen
```bash
dotnet build Handwerker.slnx
```

### 4. Anwendung ausführen
Starten Sie das `AppHost`-Projekt, um alle Dienste (Backend, Frontend, Datenbanken, Auth) gleichzeitig zu starten:
```bash
dotnet run --project Handwerker.AppHost/Handwerker.AppHost.csproj
```

Das Aspire Dashboard wird gestartet und bietet Zugriff auf:
- **Angular Client:** Von Aspire zugewiesener Port
- **API Service:** Backend-Endpunkte (mit Scalar Dokumentation)
- **Keycloak:** http://localhost:8080 (Authentifizierung)
- **pgAdmin:** http://localhost:5050 (Datenbank-Management)
- **Mailpit:** http://localhost:8025 (E-Mail-Testing)
- **Redis Insight:** http://localhost:8001 (Redis-GUI)

## 🔐 Authentifizierung

Das Projekt verwendet **Keycloak** für OpenID Connect (OIDC).

- **Admin-Konsole:** http://localhost:8080/admin
- **Anmeldedaten:** Werden beim ersten Start über AppHost-Parameter abgefragt oder sind vorkonfiguriert.
- **Realm:** `handwerker` (wird automatisch aus `Handwerker.AppHost/keycloak` importiert).

## 📂 Projektstruktur

```text
Handwerker/
├── Handwerker-Client/          # Angular 21 Frontend (Analogjs, Vite, Vitest, Playwright/Axe)
├── Handwerker.ApiService/      # .NET 10 REST API (Backend - Minimal APIs)
├── Handwerker.Application/     # App-Logik (CQRS, Mapper, Validatoren)
├── Handwerker.Domain/          # Domain-Modelle & Interfaces (Entitäten, Value Objects)
├── Handwerker.AppHost/         # .NET Aspire Orchestrator (Infrastruktur & Service-Verdrahtung)
│   ├── keycloak/               # Keycloak Realm Import-Dateien
│   ├── postgresql/             # Persistente Postgres-Daten
│   └── mailpit-data/           # Persistente Mailpit-Daten
├── Handwerker.Maui/            # .NET MAUI Mobile App (Android, iOS, Windows, Mac)
├── Handwerker.Infrastructure/  # Datenzugriffsschicht (EF Core 10, Repositories)
├── Handwerker.ServiceDefaults/ # Gemeinsame .NET Service-Konfigurationen (OpenTelemetry, Health checks)
├── Handwerker.Web/             # Blazor-basiertes Web-Frontend (Referenz-Implementierung)
├── Handwerker.Tests/           # Integrations- und Unit-Tests
├── Handwerker.slnx             # Moderne Visual Studio Solution Datei
└── global.json                 # .NET SDK Versionierung (10.0.0)
```

## 🏗️ Architekturen & Patterns

Das Projekt folgt bewährten Architekturprinzipien, um Wartbarkeit und Testbarkeit zu gewährleisten:

- **Clean Architecture (Onion Architecture):** Klare Trennung von Belangen durch Layer (`Domain` -> `Application` -> `Infrastructure` -> `API`). Die Domain bleibt frei von externen Abhängigkeiten.
- **CQRS (Command Query Responsibility Segregation):** Trennung von Lese- und Schreiboperationen. Implementiert über eigene Dispatcher und Handler in der `Application`-Schicht.
- **Repository Pattern:** Abstraktion des Datenzugriffs in der `Infrastructure`-Schicht zur Entkoppelung der Geschäftslogik von der Datenbanktechnologie (EF Core).
- **Dependency Injection:** Konsistente Nutzung des .NET Built-in Containers für lose Kopplung.
- **Minimal APIs:** Effiziente und performante Endpunkt-Definition im `ApiService`.
- **Options Pattern:** Typsichere Konfiguration (z. B. für Keycloak-Einstellungen).
- **Service Discovery & Orchestrierung:** Automatisierte Vernetzung der Microservices und Infrastruktur-Komponenten durch **.NET Aspire**.

## 🔧 Entwicklung & Skripte

### Frontend (Handwerker-Client)
Befindet sich in `Handwerker-Client/`.
- `pnpm install`: Abhängigkeiten installieren.
- `pnpm start`: Angular Dev-Server starten (wird normalerweise von Aspire gesteuert).
- `pnpm run build`: Production Build erstellen.
- `pnpm test`: Unit-Tests mit Vitest ausführen.
- `pnpm run test:ui`: Vitest mit Benutzeroberfläche starten.
- `pnpm run test:coverage`: Test-Abdeckung generieren.
- `pnpm run setup:cert`: Lokale SSL-Zertifikate generieren.

### Mobile (.NET MAUI)
Befindet sich in `Handwerker.Maui/`.
- Kann über Visual Studio oder Rider gestartet werden.
- Die AppHost-Integration ist vorhanden, wird jedoch standardmäßig manuell gestartet, um die Performance zu optimieren (Dev Tunnel vorbereitet).
- `dotnet build Handwerker.Maui/Handwerker.Maui/Handwerker.Maui.csproj`: Einzelnes Projekt bauen.

### Backend (.NET)
Befindet sich in verschiedenen Projekten (siehe Projektstruktur).
- `dotnet build`: Gesamte Solution bauen.
- `dotnet test`: Alle Tests in der Solution ausführen.
- `dotnet run --project Handwerker.AppHost/Handwerker.AppHost.csproj`: Startet die gesamte Umgebung.

## 🧪 Testing

Das Projekt implementiert eine mehrschichtige Teststrategie:
- **Integrationstests:** Befinden sich in `Handwerker.Tests`, verwenden `Aspire.Hosting.Testing` und xUnit, um das Zusammenspiel der Dienste zu testen.
- **Frontend-Tests:** Befinden sich in `Handwerker-Client`, verwenden **Vitest** für Komponenten- und Unit-Tests.
- **Mobile-Tests:** Unit-Tests für ViewModels und Services befinden sich innerhalb des `Handwerker.Maui` Projekts (verwenden xUnit).

Um alle Tests auszuführen:
```bash
dotnet test
```

## 🌍 Umgebungsvariablen & Konfiguration

- **AppHost-Parameter:** Beim Ausführen über `dotnet run` fragt Aspire möglicherweise nach:
  - `username` / `password`: Postgres-Anmeldedaten.
  - `keycloak-username` / `keycloak-password`: Keycloak-Admin-Anmeldedaten.
- **Angular-Umgebung:** Wird über `app.config.ts` und die Aspire-Service-Discovery verwaltet.

## 📄 Lizenz

(TODO: Lizenz einfügen, z. B. MIT)

---
**Status:** 🏗️ In aktiver Entwicklung (.NET 10 / Angular 21 / MAUI)
**Letztes Update:** 2. März 2026
