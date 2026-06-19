# Teststrategie

Dieses Dokument beschreibt die Teststrategie im Projekt `CampusLibrary`.

Das Ziel ist nicht nur die Prüfung der Korrektheit, sondern auch die Sichtbarmachung verschiedener Testebenen für die Lehre. Das Projekt trennt deshalb Domain-Tests, Application-UseCase-Tests, Infrastructure-Integrationstests und Controller-/End-to-End-Tests.

## Überblick

Das aktuelle Testprojekt ist:

```text
CampusLibraryApiTest
```

Die Tests decken folgende Bereiche ab:

```text
Domain-Tests
Application-UseCase-Tests mit Mocks
Application-Integrationstests mit SQLite und UnitOfWork
Infrastructure-Tests für Repositories und ReadModels
Controller-/End-to-End-Tests mit WebApplicationFactory
```

Im aktuellen Projektstand laufen alle Tests grün:

```text
Test summary: total: 72, failed: 0, succeeded: 72, skipped: 0
```

Alle Tests ausführen:

```bash
dotnet test
```

## Testebenen

### 1. Domain-Tests

Domain-Tests prüfen das Verhalten von Domainobjekten ohne Infrastructure.

Typische Beispiele:

```text
Reader.Create(...)
Reader.UpdateProfile(...)
Reader.Deactivate(...)
EmailVo.Create(...)
AddressVo.Create(...)
```

Domain-Tests konzentrieren sich auf fachliche Regeln:

```text
Pflichtwerte
gültige Wertebereiche
Normalisierung
ungültige Eingaben
partielle Änderungen
Domain Errors
Soft-Delete-Zustandsänderungen
```

Die Domain-Schicht verwendet kein EF Core, kein ASP.NET Core, keine Repositories, keine Controller und kein HTTP.

Das wichtigste Ziel ist zu prüfen, dass Aggregates und Value Objects ihre eigenen Invarianten schützen.

Für den aktuellen Reader-Lebenszyklus prüfen Domain-Tests außerdem, dass ein Reader deaktiviert werden kann und dass ein bereits deaktivierter Reader nicht erneut deaktiviert werden kann.

## 2. Application-UseCase-Tests mit Mocks

Application-UseCase-Tests prüfen die Orchestrierungslogik der Use Cases.

Typische Beispiele:

```text
ReaderUcCreate
ReaderUcUpdate
ReaderUcDeactivate
```

Diese Tests verwenden Mocks für Ports wie:

```text
IReaderRepository
IUnitOfWork
IClock
```

Der Zweck ist zu prüfen, ob der Use Case den Ablauf korrekt koordiniert:

```text
Aggregate laden
Eingabe prüfen
Value Objects erzeugen
Eindeutigkeitsregeln prüfen
Domainmethoden aufrufen
nur nach erfolgreicher Domainoperation speichern
DTOs oder Fehler zurückgeben
```

Beispielsweise prüft `ReaderUcUpdate`, ob eine neue E-Mail-Adresse bereits von einem anderen Reader verwendet wird, bevor das Aggregate geändert wird.

`ReaderUcDeactivate` prüft die Id, lädt den Reader, ruft `Reader.Deactivate(...)` auf, speichert nur bei erfolgreicher Domainoperation und gibt Domainfehler wie `ReaderNotFound` oder `IsAlreadyDeactivated` zurück.

## 3. Application-Integrationstests

Application-Integrationstests verwenden echte Infrastructure-Bestandteile, wenn dies sinnvoll ist.

Sie prüfen, ob Use Cases zusammenarbeiten mit:

```text
echter Repository-Implementierung
echter UnitOfWork
SQLite-Testdatenbank
EF-Core-Tracking
```

Das ist nützlich, weil manche Fehler erst im Zusammenspiel von EF Core, Repository und UnitOfWork auftreten.

Diese Tests sind langsamer als reine Domain-Tests, geben aber mehr Sicherheit, dass Application und Persistenz zusammen funktionieren.

Für die Deaktivierung prüfen Integrationstests, dass der Reader über normale ReadModel-Abfragen nicht mehr sichtbar ist, aber über `WithInactive`-Abfragen weiterhin gefunden werden kann.

## 4. Infrastructure-Tests

Infrastructure-Tests prüfen die Persistenzadapter.

Typische Bereiche:

```text
ReaderRepositoryEf
ReaderReadModelEf
AppDbContext
EF-Core-Mappings
SQLite-Verhalten
```

Das Repository gehört zur Write-Seite.

Das ReadModel gehört zur Query-Seite.

Diese Trennung ist bewusst:

```text
Repository -> domänenorientierter Schreibzugriff
ReadModel  -> DTO-orientierter Lesezugriff
```

Infrastructure-Tests helfen zu prüfen, ob Entities, Value Objects, Conversions und Queries korrekt mit der Datenbank funktionieren.

Für das aktuelle Soft-Delete-Verhalten prüfen ReadModel-Tests zwei verschiedene Sichten:

```text
normale Queries       -> nur aktive Reader
WithInactive-Queries  -> aktive und inaktive Reader
```

## 5. Controller-/End-to-End-Tests

Controller-Tests verwenden:

```text
WebApplicationFactory<Program>
TestBaseFactory
TestBaseEndToEnd
TestAuthHandler
```

Diese Tests starten die ASP.NET Core-Anwendung in einem Testhost und rufen die API über HTTP auf.

Sie prüfen:

```text
Routing
Model Binding
Controller Actions
Status Codes
JSON-Serialisierung
ProblemDetails-Mapping
Dependency Injection
Datenbankintegration
```

Die aktuellen Reader-Controller-Tests decken ab:

```text
GET    /camplib/v1/readers
GET    /camplib/v1/readers/with-inactive
GET    /camplib/v1/readers/{id}
GET    /camplib/v1/readers/{id}/with-inactive
GET    /camplib/v1/readers/email
POST   /camplib/v1/readers
PUT    /camplib/v1/readers/{id}
DELETE /camplib/v1/readers/{id}
```

Diese Tests sind am nächsten an der echten API-Nutzung.

Der `DELETE`-Endpunkt wird als Deaktivierungsendpunkt getestet, nicht als physisches Löschen aus der Datenbank.

## Testdatenbank

Die Tests verwenden SQLite über die Testinfrastruktur.

Die Testdatenbank wird erzeugt durch:

```text
TestDatabase
TestBaseFactory
```

Die Factory ersetzt ausgewählte Produktionsservices:

```text
AppDbContext
IUnitOfWork
IClock
TestSeed
Authentication
```

Eine Fake Clock wird verwendet, um Zeitstempel deterministisch zu machen.

Das ist wichtig, weil die Domain UTC-Zeitstempel erwartet.

Beispiel:

```csharp
public DateTime TestCreatedAt { get; set; } =
   new(2025, 01, 01, 00, 00, 00, DateTimeKind.Utc);
```

## Test Seed

Der Test Seed stellt stabile Demo- und Testdaten bereit.

Typische Reader:

```text
Reader1
Reader2
Reader3
Reader4
Reader5
Reader6
ReaderRegister
```

Tests sollten Seed-Daten gegenüber spontan konstruierten Einzeldaten bevorzugen.

Das hält die Beispiele konsistent und für Studierende leichter nachvollziehbar.

## Tests für partielle Updates

`ReaderUpdateDto` unterstützt partielle Updates:

```csharp
public sealed record ReaderUpdateDto(
   string? Lastname,
   string? Email,
   AddressDto? AddressDto
);
```

Die Bedeutung von `null` ist:

```text
Lastname = null   -> aktuellen Nachnamen behalten
Email = null      -> aktuelle E-Mail behalten
AddressDto = null -> aktuelle Adresse behalten
```

Nur angegebene Werte werden geändert.

Ein leerer oder aus Leerzeichen bestehender Nachname ist nicht dasselbe wie `null`.

```text
null       -> keine Änderung
""         -> ungültiger Wert
"   "      -> ungültiger Wert
"Meier"   -> gültige Änderung
```

Diese Unterscheidung ist für die Semantik partieller Updates wichtig.

## Deaktivierungstests

Das aktuelle Projekt verwendet Soft Delete für Reader.

Die zentrale Regel lautet:

```text
Deactivate ändert den Zustand des Readers.
ReadModels entscheiden über die Sichtbarkeit.
```

Die Tests prüfen:

```text
Reader.Deactivate(...) ist für aktive Reader erfolgreich
Reader.Deactivate(...) schlägt für bereits inaktive Reader fehl
ReaderUcDeactivate liefert InvalidId für Guid.Empty
ReaderUcDeactivate liefert ReaderNotFound für unbekannte Reader
ReaderUcDeactivate liefert IsAlreadyDeactivated für inaktive Reader
normale ReadModel-Abfragen blenden inaktive Reader aus
WithInactive-ReadModel-Abfragen liefern inaktive Reader weiterhin zurück
```

Dadurch wird der Unterschied zwischen physischem Delete und fachlicher Deaktivierung sichtbar.

## Warum verschiedene Testarten?

Jede Testart beantwortet eine andere Frage.

```text
Domain-Test:
Funktioniert die fachliche Regel?

UseCase-Mock-Test:
Ruft der Application-Workflow die richtigen Ports auf und behandelt Fehler korrekt?

Integrationstest:
Funktioniert der Use Case mit echter Persistenz?

Infrastructure-Test:
Speichert und lädt EF Core die Daten korrekt?

Controller-/E2E-Test:
Verhält sich die API von außen korrekt?
```

Zusammen bilden diese Tests eine lehrorientierte Teststrategie.

## Empfohlener Workflow

Während der Entwicklung:

```bash
dotnet test
```

Bei API-Änderungen zusätzlich die Anwendung starten und Swagger prüfen:

```bash
dotnet run --project CampusLibraryApi
```

Swagger ist im Development-Modus verfügbar unter:

```text
https://localhost:8010/swagger
```

## Didaktische Ziele

Die Testsuite soll Studierenden helfen, Folgendes zu verstehen:

```text
Trennung von Testebenen
Domain-Testing ohne Infrastructure
Mock-basierte UseCase-Tests
Integrationstests mit SQLite
Controller-Tests über HTTP
Wiederverwendung von Testdaten über Seed-Objekte
warum Fake Clocks nützlich sind
wie partielle Updates getestet werden sollten
wie Soft Delete getestet werden sollte
wie sich aktive und inaktive Query-Sichten unterscheiden
```

Die Tests sind deshalb nicht nur ein Sicherheitsnetz, sondern auch Teil des Lernmaterials.
