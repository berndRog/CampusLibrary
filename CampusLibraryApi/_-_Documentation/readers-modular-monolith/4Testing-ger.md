# Teststrategie

Dieses Dokument beschreibt die Teststrategie im Projekt `CampusLibrary`.

Das Ziel ist nicht nur die Prüfung der Korrektheit, sondern auch die Sichtbarmachung verschiedener Testebenen für die Lehre.

## Überblick

Das aktuelle Testprojekt ist:

```text
CampusLibraryApiTest
```

Die Tests decken ab:

```text
Domain-Tests
Application-UseCase-Tests mit Mocks
Application-Integrationstests mit SQLite und UnitOfWork
Infrastructure-Tests für Repositories und ReadModels
Controller-/End-to-End-Tests mit WebApplicationFactory
```

Aktueller Stand:

```text
Test summary: total: 72, failed: 0, succeeded: 72, skipped: 0
```

Alle Tests ausführen:

```bash
dotnet test
```

## 1. Domain-Tests

Domain-Tests prüfen das Verhalten von Domainobjekten ohne Infrastructure.

Typische Beispiele:

```text
Reader.Create(...)
Reader.UpdateProfile(...)
Reader.Deactivate(...)
EmailVo.Create(...)
AddressVo.Create(...)
```

Domain-Tests konzentrieren sich auf fachliche Regeln, Normalisierung, ungültige Eingaben, Domain Errors und Soft-Delete-Zustandsänderungen.

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

`ReaderUcDeactivate` prüft die Id, lädt den Reader, ruft `Reader.Deactivate(...)` auf, speichert nur bei erfolgreicher Domainoperation und gibt Domainfehler wie `InvalidId`, `ReaderNotFound` oder `IsAlreadyDeactivated` zurück.

## 3. Application-Integrationstests

Application-Integrationstests verwenden echte Infrastructure-Bestandteile, wenn dies sinnvoll ist:

```text
echte Repository-Implementierung
echte UnitOfWork
SQLite-Testdatenbank
EF-Core-Tracking
```

Für die Deaktivierung prüfen Integrationstests, dass der Reader über normale ReadModel-Abfragen nicht mehr sichtbar ist, aber über `WithInactive`-Abfragen weiterhin gefunden werden kann.

## 4. Infrastructure-Tests

Infrastructure-Tests prüfen Persistenzadapter:

```text
ReaderRepositoryEf
ReaderReadModelEf
AppDbContext
EF-Core-Mappings
SQLite-Verhalten
Migrations
```

Für das aktuelle Soft-Delete-Verhalten prüfen ReadModel-Tests zwei verschiedene Sichten:

```text
normale Queries       -> nur aktive Reader
WithInactive-Queries  -> aktive und inaktive Reader
```

## 5. Controller-/End-to-End-Tests

Controller-Tests verwenden `WebApplicationFactory<Program>` und rufen die API über HTTP auf.

Sie prüfen Routing, Model Binding, Controller Actions, Status Codes, JSON-Serialisierung, ProblemDetails-Mapping, Dependency Injection und Datenbankintegration.

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

Der `DELETE`-Endpunkt wird als Deaktivierungsendpunkt getestet, nicht als physisches Löschen aus der Datenbank.

## Deaktivierungstests

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
warum Migrations angepasst werden müssen, wenn persistenter Domänenzustand geändert wird
```
