# CampusLibrary

Lehrprojekt für eine modular aufgebaute, DDD-orientierte ASP.NET-Core-Web-API.

Englische Version: [1Readme.md](1Readme.md)

## Aktueller Stand

Die aktuelle Version enthält zwei fachliche Module:

* Readers-Modul
* Catalog-Modul

Die Anwendung bietet:

* ASP.NET Core Web API
* API-Versionierung
* Swagger/OpenAPI-Dokumentation
* SQLite-Persistenz mit EF Core
* Repository- und ReadModel-Infrastruktur
* Use Cases für schreibende Workflows
* ReadModels für lesende Projektionen
* Controller/API-Tests mit `WebApplicationFactory` und `HttpClient`
* manuelle `.http`-Dateien für didaktische API-Tests

Finales Testergebnis:

```text
139 Tests
0 fehlgeschlagen
0 übersprungen
```

## Aktueller Branch

```text
part-3/readers-catalog
```

## Projektstruktur

```text
CampusLibraryApi
CampusLibraryApi_1_Web
CampusLibraryApi_2_BuildingBlocks
CampusLibraryApi_3_Core_Readers
CampusLibraryApi_3_Core_Catalog
CampusLibraryApi_4_Infrastructure
CampusLibraryApiTest
```

## Architekturidee

Die Lösung ist ein projektbasierter modularer Monolith.

Die Web/API-Schicht stellt HTTP-Endpunkte bereit. Die Core-Module enthalten Domänenmodell, Use Cases, DTOs und Ports. BuildingBlocks enthält wiederverwendbare Abstraktionen. Infrastructure implementiert EF-Core-Persistenz, Repositories, ReadModels und Datenbankkonfiguration. Tests prüfen das Verhalten über Domain-, Application-, Infrastructure- und API-Grenzen hinweg.

Zentrale Abhängigkeitsregel:

```text
Core-Module hängen nicht von Web/API oder Infrastructure ab.
Infrastructure implementiert Outbound Ports der Core-Module.
Das ausführbare API-Projekt verdrahtet alle Module.
```

## Readers-Modul

Das Readers-Modul verwaltet Bibliotheksleserinnen und -leser.

Es enthält:

* Reader-Aggregate
* Reader-Value-Objects
* Reader-Use-Cases
* Reader-Repository-Port
* Reader-ReadModel-Port
* Reader-Controller
* Reader-Tests

Typische Operationen sind:

* Reader anlegen
* Reader-Profildaten ändern
* Reader deaktivieren
* aktive Reader abfragen
* Reader inklusive inaktiver Reader abfragen
* Reader nach Id oder E-Mail suchen

Ein Reader wird durch Änderung seines fachlichen Zustands deaktiviert. Normale Lese-Endpunkte zeigen aktive Reader. Spezielle `with-inactive`-Endpunkte beziehen inaktive Reader mit ein.

## Catalog-Modul

Das Catalog-Modul verwaltet den Bibliothekskatalog.

Es enthält:

* Book-Aggregate
* BookItem-Entity
* ISBN-Value-Object
* Book-Use-Cases
* Book-ReadModel
* Book-Repository
* Books-Controller
* Catalog-Tests

Ein Book repräsentiert das bibliografische Werk. Ein BookItem repräsentiert ein physisches Exemplar eines Buchs.

## Catalog-Domänenmodell

## Book

`Book` ist ein Aggregate Root.

Ein Book enthält:

* Autorentext
* Titel
* optionalen Untertitel
* ISBN
* physische Exemplare
* Aktiv-Status

Der Autorentext wird als ein String gespeichert.

Beispiele:

```text
Robert C. Martin
Martin Fowler, Kent Beck
Erich Gamma, Richard Helm, Ralph Johnson, John Vlissides
```

Die Domain validiert, dass mindestens ein Autorname angegeben ist.

## BookItem

`BookItem` ist eine Entity innerhalb des `Book`-Aggregates.

Es enthält:

* Inventarnummer
* Status

Der Status wird als Enum modelliert:

```csharp
public enum BookItemStatus {
   Available = 1,
   Unavailable = 2,
   Lost = 3,
   Damaged = 4
}
```

Ein neues BookItem startet mit Status `Available`.

## ISBN-Value-Object

`IsbnVo` schützt die Regel, dass ein Book eine gültige ISBN haben muss.

Die Domain soll nicht mit beliebigen Strings arbeiten, wenn ein Wert eine bestimmte fachliche Bedeutung hat.

## Beziehung: Book zu BookItem

Die Beziehung zwischen `Book` und `BookItem` ist eine 1:n-Beziehung.

```text
Book 1 --- n BookItem
```

Ein `BookItem` gehört zu einem `Book`. Es wird über das `Book`-Aggregate hinzugefügt.

## Commands und Queries

Use Cases verändern den Zustand des Systems.

```text
ReaderUseCases
- CreateAsync
- UpdateAsync
- DeactivateAsync

BookUseCases
- CreateAsync
- AddBookItemAsync
- DeactivateAsync
```

ReadModels liefern Daten für Anzeige, Suche und Auswahl.

```text
ReaderReadModel
- FindByIdAsync
- FindByEmailAsync
- SelectAllAsync
- FindByIdWithInactiveAsync
- SelectAllWithInactiveAsync

BookReadModel
- FindByIdAsync
- SelectAllAsync
- SearchAsync
```

Zentrale Unterscheidung:

```text
Use Cases verändern Zustand.
ReadModels lesen und projizieren Daten.
Repositories laden Aggregate.
Controller übersetzen HTTP-Requests und Responses.
```

## Catalog-Suche

Books können nach einem expliziten Suchfeld gesucht werden:

```text
Title
AuthorLastName
Isbn
```

`AuthorLastName` durchsucht den Autorentext nach der Nachnamenregel. Der Autorentext wird an Kommata getrennt. Jeder Autoren-Token wird an Leerzeichen getrennt. Das letzte Wort jedes Autoren-Tokens gilt als Nachname.

Beispiele:

```text
Robert C. Martin -> Martin
Martin Fowler -> Fowler
Kent Beck -> Beck
```

## API-Endpunkte

Basisroute:

```text
/camplib/v1
```

Readers:

```http
GET    /camplib/v1/readers
GET    /camplib/v1/readers/with-inactive
GET    /camplib/v1/readers/{id}
GET    /camplib/v1/readers/{id}/with-inactive
GET    /camplib/v1/readers/email?email=...
POST   /camplib/v1/readers
PUT    /camplib/v1/readers/{id}
DELETE /camplib/v1/readers/{id}
```

Books:

```http
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

Für manuelle API-Tests sollte die Datenbank zuerst zurückgesetzt oder gelöscht werden.

Reihenfolge:

```text
1. Books.http
2. Readers.http
```

## Tests

Alle automatisierten Tests ausführen:

```bash
dotnet test
```

Finales Ergebnis:

```text
139 Tests
0 fehlgeschlagen
0 übersprungen
```

Die Tests decken Domain, Value Objects, Use Cases, Repositories, ReadModels, Controller/API-Verhalten und manuelle HTTP-Szenarien ab.
