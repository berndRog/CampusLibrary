# Teststrategie

Dieses Dokument beschreibt die Teststrategie im Projekt `CampusLibrary`.

Ziel ist nicht nur die Prüfung der Korrektheit, sondern auch die Sichtbarkeit der unterschiedlichen Testebenen für den Unterricht.

Die aktuelle Testsuite prüft das Readers-Modul und das Catalog-Modul.

Finales Testergebnis:

```text
Test summary: total: 139, failed: 0, succeeded: 139, skipped: 0
Build succeeded
```

## Testprojekt

```text
CampusLibraryApiTest
```

Produktionsprojekte:

```text
CampusLibraryApi
CampusLibraryApi_1_Web
CampusLibraryApi_2_BuildingBlocks
CampusLibraryApi_3_Core_Readers
CampusLibraryApi_3_Core_Catalog
CampusLibraryApi_4_Infrastructure
```

## Testebenen

Die Testsuite deckt ab:

```text
Domain-Tests
Value-Object-Tests
Use-Case-Mock-Tests
Use-Case-Integrationstests
Repository-Integrationstests
ReadModel-Integrationstests
Controller/API-End-to-End-Tests
Manuelle HTTP-Dateien
```

Alle Tests ausführen:

```bash
dotnet test
```

## 1. Domain-Tests

Domain-Tests prüfen Domänenobjekte ohne Infrastructure.

Readers-Beispiele:

```text
Reader.Create(...)
Reader.UpdateProfile(...)
Reader.Deactivate(...)
EmailVo.Create(...)
AddressVo.Create(...)
```

Catalog-Beispiele:

```text
Book.Create(...)
Book.AddBookItem(...)
Book.Deactivate(...)
BookItem.Create(...)
IsbnVo.Create(...)
```

Domain-Tests konzentrieren sich auf:

```text
Pflichtwerte
Normalisierung
ungültige Eingaben
Domain-Fehler
Aggregate-Invarianten
Value-Object-Validierung
Aktiv/Inaktiv-Zustand
UTC-Zeitstempel
```

## Catalog-Domain-Tests

Catalog-Domain-Tests prüfen:

```text
Book kann mit gültigem AuthorsText, Titel und ISBN erzeugt werden
Book kann nicht ohne gültigen AuthorsText erzeugt werden
Book kann nicht mit ungültiger ISBN erzeugt werden
AuthorsText wird normalisiert
BookItem kann zu Book hinzugefügt werden
BookItem startet mit Status Available
doppelte Inventarnummern werden abgelehnt
Book kann deaktiviert werden
CreatedAt und UpdatedAt verwenden UTC-Zeitstempel
```

## 2. Use-Case-Mock-Tests

Use-Case-Mock-Tests prüfen die Orchestrierung von Application Workflows.

Readers-Beispiele:

```text
ReaderUcCreate
ReaderUcUpdate
ReaderUcDeactivate
```

Catalog-Beispiele:

```text
BookUcCreate
BookUcAddBookItem
BookUcDeactivate
```

Typische gemockte Ports:

```text
IReaderRepository
IBookRepository
IUnitOfWork
IClock
ILogger<T>
```

Use-Case-Mock-Tests prüfen:

```text
Eingabevalidierung
optionale Ids
Repository-Aufrufe
Eindeutigkeitsprüfungen
Domänenmethoden-Aufrufe
UnitOfWork-Aufrufe
zurückgegebene DTOs
Fehlerergebnisse
```

## 3. Use-Case-Integrationstests

Use-Case-Integrationstests prüfen Use Cases mit echten Persistenzadaptern.

Sie verwenden:

```text
echte Repository-Implementierung
echte UnitOfWork
SQLite-Testdatenbank
EF-Core-Tracking
echte EF-Core-Mappings
```

Catalog-Integrationsbeispiele:

```text
das Erzeugen eines Books persistiert Book und ISBN
das Erzeugen eines Books persistiert AuthorsText
das Erzeugen eines Books ohne AuthorsText schlägt fehl
das Hinzufügen eines BookItems persistiert das BookItem
eine doppelte Inventarnummer schlägt fehl
das Deaktivieren eines Books aktualisiert IsActive
```

## 4. Infrastructure-Tests

Infrastructure-Tests prüfen Persistenzadapter.

Typische Bereiche:

```text
ReaderRepositoryEf
ReaderReadModelEf
BookRepositoryEf
BookReadModelEf
AppDbContext
EF-Core-Mappings
SQLite-Verhalten
```

Repositories gehören zur Schreibseite und liefern Domänenobjekte.

ReadModels gehören zur Leseseite und liefern DTOs.

```text
Repository -> aggregate-orientierter Schreibzugriff
ReadModel  -> DTO-orientierter Lesezugriff
```

## Repository-Tests

Readers-Repository-Tests prüfen:

```text
Reader hinzufügen
Reader nach Id finden
Reader nach E-Mail finden
Subject-Eindeutigkeit prüfen
deaktivierten Reader als Aggregate laden
```

Catalog-Repository-Tests prüfen:

```text
Book hinzufügen
Book nach Id finden
ISBN-Eindeutigkeit prüfen
Inventarnummer-Eindeutigkeit prüfen
Book mit BookItems laden
deaktiviertes Book als Aggregate laden
```

## ReadModel-Tests

Reader-ReadModel-Tests prüfen:

```text
alle aktiven Reader auswählen
alle Reader inklusive inaktiver Reader auswählen
aktiven Reader nach Id finden
Reader nach Id inklusive inaktiver Reader finden
Reader nach E-Mail finden
```

Catalog-ReadModel-Tests prüfen:

```text
alle aktiven Books auswählen
aktives Book nach Id finden
aktive Books nach Titel suchen
aktive Books nach Autoren-Nachname suchen
aktive Books nach ISBN suchen
inaktive Books aus normalen Queries ausblenden
```

## Catalog-Suchtests

Book-Suche unterstützt:

```text
Title
AuthorLastName
Isbn
```

`AuthorLastName` verwendet die Nachnamenregel für AuthorsText.

Beispiele:

```text
Robert C. Martin -> Martin
Martin Fowler -> Fowler
Kent Beck -> Beck
```

Ein Regressionstest prüft, dass eine Suche nach `Martin` `Clean Code` liefert, weil der Autorentext `Robert C. Martin` enthält. `Refactoring` wird dabei nicht geliefert, weil dort `Martin Fowler` enthalten ist und `Fowler` der Nachname ist.

## 5. Controller/API-End-to-End-Tests

Controller/API-End-to-End-Tests verwenden:

```text
WebApplicationFactory<Program>
TestBaseFactory
TestBaseEndToEnd
TestAuthHandler
HttpClient
```

Sie prüfen:

```text
Routing
Model Binding
Controller Actions
Statuscodes
JSON-Serialisierung
ProblemDetails-Mapping
Dependency Injection
Datenbankintegration
HTTP-Vertrag von außen
```

Reader-API-Tests decken ab:

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

Book-API-Tests decken ab:

```text
GET   /camplib/v1/books
GET   /camplib/v1/books/{id}
GET   /camplib/v1/books/search?searchField=Title&searchText=...
GET   /camplib/v1/books/search?searchField=AuthorLastName&searchText=...
GET   /camplib/v1/books/search?searchField=Isbn&searchText=...
POST  /camplib/v1/books
POST  /camplib/v1/books/{bookId}/items
PATCH /camplib/v1/books/{bookId}/deactivate
```

## Manuelle HTTP-Dateien

Manuelle HTTP-Dateien machen API-Verhalten für Studierende sichtbar.

Reihenfolge nach Datenbank-Reset:

```text
1. Books.http
2. Readers.http
```

## Testdatenbank

Automatisierte Tests verwenden SQLite über die Testinfrastruktur.

Die Test-Factory ersetzt ausgewählte Services:

```text
AppDbContext
IUnitOfWork
IClock
TestSeed
Authentication
```

Eine Fake Clock macht Zeitstempel deterministisch.

## Test Seed

Der Test Seed stellt stabile Demo- und Testdaten bereit.

Typische Catalog-Daten:

```text
Book1
Book2
Book3
Book4
BookItems für Books
```

Stabile Seed-Daten halten Beispiele über Domain-Tests, Integrationstests, API-Tests und manuelle HTTP-Dateien hinweg konsistent.
