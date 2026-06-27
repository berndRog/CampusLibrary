# Teststrategie — Teil 3

Dieses Dokument beschreibt die Teststrategie in Teil 3 des Projekts `CampusLibrary`.

Ziel ist nicht nur die Prüfung der Korrektheit, sondern auch die Sichtbarkeit der unterschiedlichen Testebenen für den Unterricht.

Teil 3 prüft das Readers-Modul und das Catalog-Modul.

Finales automatisiertes Testergebnis:

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
Statuswerte
UTC-Zeitstempel
```

## 2. Use-Case-Mock-Tests

Use-Case-Mock-Tests prüfen die Orchestrierung von Application Workflows ohne echte Datenbank.

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

Diese Tests prüfen:

```text
Repository-Aufrufe
ReadModel-Prüfungen
UnitOfWork-Aufrufe
Fehlerweitergabe
Mapping von Aggregate zu DTO
```

## 3. Use-Case-Integrationstests

Use-Case-Integrationstests führen Use Cases mit echter Infrastructure-Verdrahtung und In-Memory-Datenbank aus.

Sie prüfen:

```text
Use Cases persistieren Änderungen korrekt
Repositories und UnitOfWork arbeiten zusammen
ReadModels können persistierte Änderungen sehen
fachliche Konflikte werden erkannt
```

## 4. Repository-Integrationstests

Repository-Integrationstests prüfen das Laden und Speichern von Aggregates über EF Core.

Repositories geben Aggregate zurück, keine DTOs.

Beispiele:

```text
IReaderRepository
IBookRepository
```

## 5. ReadModel-Integrationstests

ReadModel-Tests prüfen lesende Projektionen.

ReadModels geben DTOs zurück und können inaktive Datensätze aus normalen Abfragen ausblenden.

Beispiele:

```text
IReaderReadModel
IBookReadModel
```

Wichtiges Verhalten:

```text
normale Reader-Abfragen liefern nur aktive Reader
with-inactive Reader-Abfragen beziehen inaktive Reader mit ein
normale Book-Abfragen liefern nur aktive Books
Book-Suche ignoriert inaktive Books
```

## 6. Controller/API-End-to-End-Tests

Controller/API-Tests verwenden `WebApplicationFactory` und `HttpClient`.

Sie prüfen das HTTP-Verhalten der öffentlichen API:

```text
Statuscodes
JSON-Antwortkörper
Created-Antworten und Location Header
Routing
Validierungsfehler
Konfliktfehler
Not-Found-Fehler
```

Beispiele:

```text
ReadersControllerE2eT
BooksControllerE2eT
```

## 7. Manuelle HTTP-Dateien

Manuelle HTTP-Dateien werden für Demonstration und exploratives Testen verwendet.

Manueller Ablauf in Teil 3:

```text
1. Datenbank zurücksetzen/löschen
2. Readers.http ausführen
3. Books.http ausführen
```

Empfohlene Verbesserung für größere Lehreinheiten:

```text
01_Seed_Readers.http
02_Seed_Books.http
11_Readers_Api.http
12_Books_Api.http
91_Readers_Destructive.http
92_Books_Destructive.http
```

Dadurch werden Setup und eigentliche Tests getrennt.

## Didaktischer Wert

Die Testsuite zeigt, dass verschiedene Testarten verschiedene Fragen beantworten:

```text
Domain-Tests: Ist die Regel korrekt?
Use-Case-Tests: Ist der Workflow korrekt?
Repository-Tests: Funktioniert die Persistenz?
ReadModel-Tests: Ist die lesende Projektion korrekt?
API-Tests: Ist der HTTP-Vertrag korrekt?
Manuelle HTTP-Dateien: Können Studierende die API selbst erkunden?
```
