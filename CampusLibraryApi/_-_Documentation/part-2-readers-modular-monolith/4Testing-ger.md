# Teststrategie

Dieses Dokument beschreibt die Teststrategie, die im Projekt `CampusLibrary` verwendet wird.

Das Ziel besteht nicht nur darin, Korrektheit zu prüfen. Die verschiedenen Testebenen sollen auch für die Lehre sichtbar werden. Das Projekt trennt daher Domain Tests, Application UseCase Tests, Infrastructure Integration Tests und Controller-/End-to-End-Tests.

In Teil 2 wurde die Anwendung von einem Ein-Projekt-Monolithen in einen projektbasierten modularen Monolithen überführt. Der fachliche Umfang ist weiterhin derselbe: Die Anwendung enthält aktuell nur das Readers-Modul. Die Tests prüfen, dass dieses strukturelle Refactoring das fachliche Verhalten nicht verändert hat.

## Überblick

Das aktuelle Testprojekt ist:

```text
CampusLibraryApiTest
```

Der Produktivcode ist auf mehrere Projekte verteilt:

```text
CampusLibraryApi
CampusLibraryApi_1_Web
CampusLibraryApi_2_BuildingBlocks
CampusLibraryApi_3_Core_Readers
CampusLibraryApi_4_Infrastructure
```

Die Tests decken folgende Bereiche ab:

```text
Domain Tests
Application UseCase Tests mit Mocks
Application Integration Tests mit SQLite und UnitOfWork
Infrastructure Tests für Repositories und ReadModels
Controller-/End-to-End-Tests mit WebApplicationFactory
```

Im aktuellen Projektstand sind alle Tests grün:

```text
Test summary: total: 66, failed: 0, succeeded: 66, skipped: 0
```

Alle Tests ausführen:

```bash
dotnet test
```

## Testebenen

## 1. Domain Tests

Domain Tests prüfen das Verhalten von Domain-Objekten ohne Infrastructure.

Typische Beispiele:

```text
Reader.Create(...)
Reader.UpdateProfile(...)
EmailVo.Create(...)
AddressVo.Create(...)
```

Domain Tests konzentrieren sich auf fachliche Regeln:

```text
Pflichtwerte
gültige Wertebereiche
Normalisierung
ungültige Eingaben
partielle Updates
Domain Errors
```

Die Domain-Schicht verwendet kein EF Core, kein ASP.NET Core, keine Repositories, keine Controller und kein HTTP.

Das Hauptziel ist zu prüfen, ob Aggregates und Value Objects ihre eigenen Invarianten schützen.

In der Struktur des modularen Monolithen prüfen diese Tests hauptsächlich Code aus:

```text
CampusLibraryApi_3_Core_Readers
CampusLibraryApi_2_BuildingBlocks
```

## 2. Application UseCase Tests mit Mocks

Application UseCase Tests prüfen die Orchestrierungslogik der UseCases.

Typische Beispiele:

```text
ReaderUcCreate
ReaderUcUpdate
ReaderUcDelete
```

Diese Tests verwenden Mocks oder Test Doubles für Ports wie:

```text
IReaderRepository
IUnitOfWork
IClock
```

Ziel ist zu prüfen, ob der UseCase den Workflow korrekt koordiniert:

```text
Aggregate laden
Eingaben validieren
Value Objects erzeugen
Eindeutigkeit prüfen
Domain-Methoden aufrufen
Änderungen speichern
DTOs oder Fehler zurückgeben
```

`ReaderUcUpdate` prüft zum Beispiel, ob eine neue Email-Adresse bereits von einem anderen Reader verwendet wird, bevor das Aggregate aktualisiert wird.

Diese Tests sind weitgehend unabhängig von EF Core und HTTP. Sie konzentrieren sich auf Application-Logik innerhalb des Readers Core Moduls.

## 3. Application Integration Tests

Application Integration Tests verwenden echte Infrastructure-Bestandteile, wenn das sinnvoll ist.

Sie prüfen, ob UseCases korrekt zusammenarbeiten mit:

```text
echter Repository-Implementierung
echtem UnitOfWork
SQLite-Testdatenbank
EF-Core-Tracking
```

Das ist nützlich, weil manche Fehler erst auftreten, wenn EF Core, Repository und UnitOfWork gemeinsam verwendet werden.

Diese Tests sind langsamer als reine Domain Tests, geben aber mehr Sicherheit, dass Application und Persistence korrekt zusammenspielen.

In Teil 2 sind diese Tests besonders wichtig, weil die UseCases im Readers-Modul liegen, während Repository- und UnitOfWork-Implementierungen im Infrastructure-Projekt liegen.

Die beabsichtigte Abhängigkeitsrichtung lautet:

```text
Core definiert Ports.
Infrastructure implementiert Ports.
Tests prüfen, dass beide korrekt zusammenspielen.
```

## 4. Infrastructure Tests

Infrastructure Tests prüfen die Persistenzadapter.

Typische Bereiche:

```text
ReaderRepositoryEf
ReaderReadModelEf
AppDbContext
EF-Core-Mappings
SQLite-Verhalten
```

Das Repository gehört zur Schreibseite.

Das ReadModel gehört zur Leseseite.

Diese Trennung ist beabsichtigt:

```text
Repository -> domain-orientierter Schreibzugriff
ReadModel  -> DTO-orientierter Lesezugriff
```

Infrastructure Tests helfen zu prüfen, ob Entities, Value Objects, Conversions und Queries korrekt mit der Datenbank funktionieren.

In der projektbasierten Struktur prüfen diese Tests hauptsächlich Code aus:

```text
CampusLibraryApi_4_Infrastructure
```

zusammen mit Domain-Typen und Ports aus:

```text
CampusLibraryApi_3_Core_Readers
CampusLibraryApi_2_BuildingBlocks
```

## 5. Controller- / End-to-End-Tests

Controller Tests verwenden:

```text
WebApplicationFactory<Program>
TestBaseFactory
TestBaseEndToEnd
TestAuthHandler
```

Diese Tests starten die ASP.NET-Core-Anwendung in einem Testhost und rufen die API über HTTP auf.

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

Die aktuellen Reader Controller Tests decken folgende Endpunkte ab:

```text
GET    /camplib/v1/readers
GET    /camplib/v1/readers/{id}
GET    /camplib/v1/readers/email?email=...
POST   /camplib/v1/readers
PUT    /camplib/v1/readers/{id}
DELETE /camplib/v1/readers/{id}
```

Diese Tests sind am nächsten an der realen API-Nutzung.

In Teil 2 prüfen Controller-/End-to-End-Tests außerdem, ob die getrennten Projekte korrekt durch das ausführbare API-Projekt verdrahtet werden.

Sie prüfen daher nicht nur Controller-Verhalten, sondern auch die Zusammensetzung von:

```text
Web
Readers-Modul
Infrastructure
BuildingBlocks
```

## Testdatenbank

Die Tests verwenden SQLite über die Testinfrastruktur.

Die Testdatenbank wird erzeugt durch:

```text
TestDatabase
TestBaseFactory
```

Die Factory ersetzt ausgewählte Produktivdienste:

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

Tests sollten Seed-Daten gegenüber manuell konstruierten Ad-hoc-Daten bevorzugen.

Dadurch bleiben Beispiele konsistent und für Studierende leichter verständlich.

## Tests für partielle Updates

`ReaderUpdateDto` unterstützt partielle Updates:

```csharp
public sealed record ReaderUpdateDto(
   string? Lastname,
   string? Email,
   AddressDto? AddressDto
);
```

Die Bedeutung von `null` lautet:

```text
Lastname = null   -> aktuellen Nachnamen beibehalten
Email = null      -> aktuelle Email beibehalten
AddressDto = null -> aktuelle Adresse beibehalten
```

Nur angegebene Werte werden geändert.

Ein leerer oder nur aus Leerzeichen bestehender Nachname ist nicht dasselbe wie `null`.

```text
null       -> keine Änderung
""         -> ungültiger Wert
"   "      -> ungültiger Wert
"Meier"   -> gültige Änderung
```

Diese Unterscheidung ist für die Semantik partieller Updates wichtig.

Die Tests sollten daher beide Fälle abdecken:

```text
Feld fehlt oder ist null     -> keine Änderung
Feld ist angegeben, aber ungültig -> Validierungsfehler
```

## Warum unterschiedliche Testarten?

Jede Testart beantwortet eine andere Frage.

```text
Domain Test:
Funktioniert die fachliche Regel?

UseCase Mock Test:
Ruft der Application Workflow die richtigen Ports auf und behandelt er Fehler korrekt?

Integration Test:
Funktioniert der UseCase mit echter Persistenz?

Infrastructure Test:
Speichert und lädt EF Core die Daten korrekt?

Controller-/E2E-Test:
Verhält sich die API von außen korrekt?
```

Zusammen bilden diese Tests eine lehrorientierte Teststrategie.

## Warum die Tests in Teil 2 wichtig sind

Teil 2 ist hauptsächlich ein architektonisches Refactoring.

Die Anwendung wurde von einem Ein-Projekt-Monolithen in einen projektbasierten modularen Monolithen überführt.

Das erwartete Ergebnis lautet:

```text
Die Struktur ändert sich.
Das fachliche Verhalten bleibt gleich.
```

Die Testsuite ist das Sicherheitsnetz für dieses Refactoring.

Wenn nach der Projektaufteilung alle Tests grün bleiben, gibt das Vertrauen, dass das Refactoring das Verhalten des Readers-Moduls nicht versehentlich verändert hat.

Der aktuelle Stand lautet:

```text
66 Tests
0 failed
```

## Empfohlener Workflow

Während der Entwicklung:

```bash
dotnet test
```

Bei API-Änderungen zusätzlich die Anwendung starten und Swagger prüfen:

```bash
dotnet run --project CampusLibraryApi
```

Swagger ist im Development-Modus erreichbar unter:

```text
https://localhost:8010/swagger
```

## Version

Die aktuelle Version gehört zu Teil 2:

```text
Branch: part-2/readers-modular-monolith
Tag:    v2-readers-modular-monolith
```

Teil 1 bleibt verfügbar als:

```text
Tag: v1-readers-monolith
```

## Didaktische Ziele

Die Testsuite soll Studierenden helfen, folgende Themen zu verstehen:

```text
Trennung von Testebenen
Domain Testing ohne Infrastructure
mockbasiertes Testen von UseCases
Integration Testing mit SQLite
Controller Testing über HTTP
Wiederverwendung von Testdaten über Seed-Objekte
Nutzen von Fake Clocks
Testen partieller Updates
Tests als Schutz bei Architektur-Refactorings
End-to-End-Tests trotz modularen Monolithen
```

Die Tests sind daher nicht nur ein Sicherheitsnetz, sondern auch Teil des Lernmaterials.

## Didaktische Faustregel

Jede Testebene hat ihren eigenen Zweck:

```text
Domain Tests schützen fachliche Regeln.
UseCase Tests schützen Application Workflows.
Infrastructure Tests schützen Persistenzverhalten.
Controller Tests schützen die HTTP API.
End-to-End-Tests schützen die Gesamtkomposition.
```

Für Teil 2 ist der wichtigste Lehrpunkt:

```text
Ein modulares Refactoring ist erfolgreich, wenn sich die Struktur ändert,
die Tests aber weiterhin dasselbe Verhalten nachweisen.
```
