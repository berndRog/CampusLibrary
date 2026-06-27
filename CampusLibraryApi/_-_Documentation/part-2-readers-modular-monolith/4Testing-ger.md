# Teststrategie: CampusLibrary Teil 2

Dieses Dokument beschreibt die Teststrategie von **Teil 2 – Readers Modular Monolith**.

Das Ziel besteht nicht nur darin, Korrektheit zu prüfen. Die Testsuite macht auch die architektonischen Schichten und Testebenen für die Lehre sichtbar.

In Teil 2 wurde die Anwendung von einem ordnerbasierten Monolithen in einen projektbasierten modularen Monolithen überführt. Der fachliche Umfang bleibt auf das Readers-Modul begrenzt.

Der aktuelle Teststand lautet:

```text
Test summary: total: 70, failed: 0, succeeded: 70, skipped: 0
```

Alle Tests ausführen:

```bash
dotnet test
```

## Getestete Produktivprojekte

Der Produktivcode ist auf folgende Projekte verteilt:

```text
CampusLibraryApi
CampusLibraryApi_1_Web
CampusLibraryApi_2_BuildingBlocks
CampusLibraryApi_3_Core_Readers
CampusLibraryApi_4_Infrastructure
```

Das Testprojekt ist:

```text
CampusLibraryApiTest
```

## Testebenen

Die Tests decken folgende Ebenen ab:

```text
Domain Tests
Application UseCase Tests mit Mocks
Application Integration Tests mit SQLite und UnitOfWork
Infrastructure Tests für Repositories und ReadModels
Controller-/End-to-End-Tests mit WebApplicationFactory
```

## 1. Domain Tests

Domain Tests prüfen fachliches Verhalten ohne Infrastructure und ohne ASP.NET Core.

Typische Beispiele:

```text
Reader.Create(...)
Reader.UpdateProfile(...)
Reader.Deactivate(...)
EmailVo.Create(...)
AddressVo.Create(...)
```

Der Fokus liegt auf fachlichen Regeln:

```text
Pflichtwerte
gültige Wertebereiche
Normalisierung
partielle Updates
Soft-Deactivation
ungültige Zustandsübergänge
Domain Errors
```

Die Domain-Schicht hängt nicht von EF Core, Controllern, Repositories oder HTTP ab.

Der wichtigste didaktische Punkt lautet:

```text
Aggregates und Value Objects schützen ihre eigenen Invarianten.
```

## 2. Application UseCase Tests mit Mocks

Application UseCase Tests prüfen die Orchestrierungslogik.

Typische UseCases:

```text
ReaderUcCreate
ReaderUcUpdate
ReaderUcDeactivate
ReaderUseCases
```

Diese Tests verwenden Mocks oder Test Doubles für Ports wie:

```text
IReaderRepository
IUnitOfWork
IClock
```

Sie prüfen, ob der UseCase den Workflow korrekt koordiniert:

```text
Eingaben validieren
Value Objects erzeugen
Aggregate laden
Eindeutigkeit prüfen
Domain-Methoden aufrufen
Änderungen speichern
DTOs oder Domain Errors zurückgeben
```

Der wichtige didaktische Punkt lautet:

```text
UseCases orchestrieren. Sie enthalten keinen Persistenzcode und kein HTTP-Verhalten.
```

## 3. Application Integration Tests

Application Integration Tests prüfen UseCases zusammen mit echten Infrastructure-Komponenten.

Sie verwenden:

```text
echte Repository-Implementierung
echten UnitOfWork
SQLite-Testdatenbank
EF-Core-Tracking
Fake Clock für deterministische Zeitstempel
```

Diese Tests sind nützlich, weil manche Fehler erst auftreten, wenn Application, Repository, DbContext und UnitOfWork zusammenspielen.

In Teil 2 ist das besonders relevant, weil die UseCases im Readers-Core-Projekt liegen, während Repository- und UnitOfWork-Implementierungen im Infrastructure-Projekt liegen.

Die Abhängigkeitsrichtung bleibt:

```text
Core definiert Ports.
Infrastructure implementiert Ports.
Tests prüfen, dass beides korrekt zusammenspielt.
```

## 4. Infrastructure Tests

Infrastructure Tests prüfen die Persistenzadapter.

Typische getestete Komponenten:

```text
ReaderRepositoryEf
ReaderReadModelEf
ReaderDbContextEf
AppDbContext
ConfigReader
UtcDateTimeConverter
```

Das Repository gehört zur Schreibseite:

```text
ReaderRepositoryEf -> Reader-Aggregate
```

Das ReadModel gehört zur Leseseite:

```text
ReaderReadModelEf -> ReaderDto
```

Das aktuelle Reader-ReadModel-Verhalten ist wichtig:

```text
normale Abfragen liefern nur aktive Reader
spezielle Abfragen können inaktive Reader einschließen
```

Infrastructure Tests prüfen, dass dieses Verhalten mit einer echten SQLite-Datenbank funktioniert.

## 5. Controller- / End-to-End-Tests

Controller-/End-to-End-Tests verwenden den ASP.NET-Core-Testhost.

Typische Infrastruktur:

```text
WebApplicationFactory<Program>
TestBaseFactory
TestBaseEndToEnd
Test Authentication
SQLite-Testdatenbank
```

Diese Tests rufen die API über HTTP auf und prüfen:

```text
Routing
Model Binding
Statuscodes
JSON-Serialisierung
ProblemDetails-Mapping
Dependency Injection
Datenbankintegration
```

Die aktuellen Reader-Controller-Tests decken das wichtigste API-Verhalten ab:

```text
GET    /camplib/v1/readers
GET    /camplib/v1/readers/with-inactive
GET    /camplib/v1/readers/{id}
GET    /camplib/v1/readers/{id}/with-inactive
GET    /camplib/v1/readers/email?email=...
POST   /camplib/v1/readers
PUT    /camplib/v1/readers/{id}
DELETE /camplib/v1/readers/{id}
```

Der `DELETE`-Endpunkt wird als Deactivate-Operation getestet. Er darf den Reader nicht physisch löschen.

## Testdatenbank

Die Tests verwenden SQLite.

Die Testinfrastruktur erzeugt eine Testdatenbank und ersetzt ausgewählte Laufzeitdienste.

Typische Ersetzungen sind:

```text
AppDbContext
IUnitOfWork
IClock
Test-Seed-Daten
Authentication
```

Eine Fake Clock wird verwendet, damit `CreatedAt`- und `UpdatedAt`-Werte deterministisch getestet werden können.

## Wichtiges Verhalten unter Test

Das wichtigste aktuelle Reader-Verhalten ist:

```text
Reader anlegen
Reader aktualisieren
Reader deaktivieren
doppelte Email/Subject ablehnen, soweit fachlich vorgesehen
deaktivierte Reader in der Datenbank behalten
deaktivierte Reader aus normalen Leseabfragen ausblenden
deaktivierte Reader nur über explizite with-inactive-Abfragen zurückgeben
```

## Warum sich die Testanzahl geändert hat

Die aktuelle Testsuite enthält 70 Tests.

Ältere Dokumentation nannte 66 Tests. Im Zuge der Angleichung an das aktuelle Reader-Modell wurden Tests aktualisiert und ein doppelter historischer Delete-Test entfernt.

Maßgeblich ist der aktuell geprüfte Stand:

```text
70 total
0 failed
0 skipped
```

## Didaktischer Wert

Teil 2 ist für die Lehre besonders nützlich, weil die Tests zeigen, dass ein architektonisches Refactoring sicher durchgeführt werden kann.

Studierende können vergleichen:

```text
Teil 1: ein Projekt, ordnerbasierte Struktur
Teil 2: mehrere Projekte, explizite Modulgrenzen
```

Das Verhalten bleibt auf Readers konzentriert, aber die Architektur bereitet das Projekt auf spätere Module vor.
