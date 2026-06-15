# CampusLibrary

Lehrprojekt für eine modular aufgebaute, DDD-orientierte ASP.NET-Core-Web-API.

Das Projekt zeigt, wie ein kleiner modularer Monolith in getrennte Projekte für Web/API, Building Blocks, Core-Module, Infrastructure und Tests aufgeteilt werden kann, ohne dass das Domain Model von technischen Persistenzdetails abhängig wird.

English version: [1readme.md](1readme.md)

## Aktueller Stand

Das Projekt enthält aktuell zwei funktionsfähige Module:

* Readers-Modul
* Catalog-Modul
* ASP.NET Core Web API
* API-Versionierung
* Swagger/OpenAPI-Dokumentation
* SQLite-Persistenz mit EF Core
* Repository- und ReadModel-Infrastruktur
* UseCases für schreibende Workflows
* ReadModels für lesende Projektionen
* Controller/API-Tests mit echter SQLite-Testdatenbank

Der ursprüngliche Monolith wurde in einen projektbasierten modularen Monolithen überführt. Gemeinsame Abstraktionen und Basistypen liegen in `BuildingBlocks`. Die Module `Readers` und `Catalog` sind eigenständige Core-Module. Technische Persistenzdetails werden im Infrastructure-Projekt implementiert.

Der aktuelle Teststand ist:

```text
155 Tests
0 failed
0 skipped
```

## Versionen

* `v1-readers-monolith`
  Erste abgeschlossene Version mit dem Readers-Modul in einer einfachen monolithischen Projektstruktur.

* `v2-readers-modular-monolith`
  Refactoring in eine projektbasierte modulare Monolith-Struktur.

* `v3-readers-catalog`
  Ergänzt das Catalog-Modul mit Books, Authors, BookItems, ISBN Value Object, ReadModels, UseCases, Repositories, Controllern und Swagger-Dokumentation.

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

Die Web/API-Schicht stellt die HTTP-Endpunkte bereit.

Die Core-Module enthalten Domain Model, Application UseCases, DTOs und Ports eines Fachmoduls.

Das BuildingBlocks-Projekt enthält gemeinsame Abstraktionen, die nicht zu einem bestimmten Fachmodul gehören.

Das Infrastructure-Projekt implementiert technische Details wie EF-Core-Persistenz, Repositories, ReadModels und Datenbankkonfiguration.

Das Testprojekt prüft das Verhalten über Domain-, Application-, Infrastructure- und API-Grenzen hinweg.

Die wichtigste Abhängigkeitsregel lautet:

```text
Core-Module hängen nicht von Web/API oder Infrastructure ab.
Infrastructure hängt von Core-Modulen ab, weil sie deren Outbound Ports implementiert.
Das API-Projekt ist Composition Root und verdrahtet die Module.
```

## Module

## Readers-Modul

Das Readers-Modul verwaltet Leserinnen und Leser der Bibliothek.

Es enthält:

* Reader Aggregate
* Reader Value Objects
* Reader UseCases
* Reader Repository Port
* Reader ReadModel Port
* Reader Controller
* Reader Tests

Typische Operationen sind:

* Reader anlegen
* Reader-Profildaten ändern
* Reader löschen
* Reader abfragen
* Reader nach Id oder Email suchen

Das Readers-Modul ist bewusst einfach und bildet den Einstieg in die Architektur.

## Catalog-Modul

Das Catalog-Modul verwaltet den Bibliothekskatalog.

Es enthält:

* Book Aggregate
* Author Aggregate
* BookItem Entity
* ISBN Value Object
* Book- und Author-UseCases
* Book- und Author-ReadModels
* Book- und Author-Repositories
* Book- und Author-Controller
* Catalog Tests

Das Catalog-Modul führt im Vergleich zum Readers-Modul ein fachlich reichhaltigeres Domain Model ein.

## Domain Model im Catalog

### Book

`Book` ist ein Aggregate Root.

Ein Book beschreibt das bibliografische Werk und enthält:

* Title
* optional Subtitle
* ISBN
* Authors
* BookItems
* aktiven Zustand

Ein Book kann mehrere Authors haben.

Ein Book kann mehrere physische BookItems haben.

### Author

`Author` ist ein Aggregate Root.

Ein Author enthält:

* Firstname
* Lastname
* DisplayName
* aktiven Zustand

Authors werden im Catalog-Modul nicht physisch gelöscht. Sie werden deaktiviert, indem `IsActive` auf `false` gesetzt wird.

### BookItem

`BookItem` ist eine Entity innerhalb des `Book`-Aggregates.

Ein BookItem beschreibt ein physisches Exemplar eines Buches.

Es enthält:

* InventoryNumber
* Status

Der Status wird durch ein Enum modelliert:

```csharp
public enum BookItemStatus {
   Available = 1,
   Unavailable = 2,
   Lost = 3,
   Damaged = 4
}
```

Der Enum-Wert wird in der Datenbank als Zahl gespeichert. Das ist kompakt und technisch stabil. Die fachliche Bedeutung bleibt im Code über die Enum-Namen sichtbar.

### ISBN Value Object

`IsbnVo` ist ein Value Object.

Es schützt die fachliche Regel, dass ein Book eine gültige ISBN benötigt. Die Domain arbeitet dadurch nicht mit beliebigen Strings, wenn ein Wert eine konkrete fachliche Bedeutung hat.

## Beziehungen

### Book zu BookItem

Die Beziehung zwischen `Book` und `BookItem` ist eine 1:n-Beziehung.

```text
Book 1 --- n BookItem
```

Ein `BookItem` gehört zu einem `Book`. Es wird über das `Book`-Aggregate hinzugefügt.

### Book zu Author

Die Beziehung zwischen `Book` und `Author` ist eine m:n-Beziehung.

```text
Book n --- m Author
```

Die Domain zeigt diese Beziehung über `Book.Authors`.

Die Datenbank speichert die Beziehung über eine Join-Tabelle in der Infrastructure.

```text
BookAuthorJoin ist ein Infrastrukturdetail.
BookAuthorJoin ist keine Domain Entity.
BookAuthorJoin verwendet den zusammengesetzten Schlüssel BookId + AuthorId.
```

## Commands und Queries

Das Projekt trennt schreibende Commands von lesenden Queries.

### UseCases

UseCases verändern den Zustand des Systems.

Beispiele:

```text
ReaderUseCases
- CreateAsync
- UpdateAsync
- DeleteAsync

BookUseCases
- CreateAsync
- AddBookItemAsync
- AssignAuthorAsync
- DeactivateAsync

AuthorUseCases
- CreateAsync
- DeactivateAsync
```

UseCases arbeiten mit Repositories, Domain-Objekten und UnitOfWork.

### ReadModels

ReadModels liefern Daten für Anzeige, Suche und Auswahl.

Beispiele:

```text
ReaderReadModel
- FindByIdAsync
- FindByEmailAsync
- SelectAllAsync

BookReadModel
- FindByIdAsync
- SelectAllAsync
- SearchAsync
- SelectByAuthorIdAsync

AuthorReadModel
- FindByIdAsync
- SelectAllAsync
- SearchAsync
```

ReadModels liefern DTOs, keine Domain-Objekte.

Die zentrale Unterscheidung lautet:

```text
UseCases verändern Zustand.
ReadModels lesen und projizieren Daten.
Repositories laden Aggregate.
Controller übersetzen HTTP-Anfragen und HTTP-Antworten.
```

## Deactivate statt Delete

Im Catalog-Modul werden Books und Authors nicht physisch gelöscht.

Stattdessen werden sie deaktiviert:

```text
IsActive = false
```

Repositories können das Aggregate weiterhin laden.

ReadModels entscheiden, was in normalen Abfragen sichtbar ist.

Normale Listen und Suchen liefern nur aktive Books und Authors.

## API-Endpunkte

Die API ist versioniert.

Aktuelle API-Version:

```text
v1
```

Basisroute:

```text
/camplib/v1
```

### Readers

```http
GET    /camplib/v1/readers
GET    /camplib/v1/readers/{id}
GET    /camplib/v1/readers/email?email=...
POST   /camplib/v1/readers
PUT    /camplib/v1/readers/{id}
DELETE /camplib/v1/readers/{id}
```

### Authors

```http
GET   /camplib/v1/authors
GET   /camplib/v1/authors/{id}
GET   /camplib/v1/authors/search?searchText=...
POST  /camplib/v1/authors
PATCH /camplib/v1/authors/{id}/deactivate
```

### Books

```http
GET   /camplib/v1/books
GET   /camplib/v1/books/{id}
GET   /camplib/v1/books/search?searchField=Title&searchText=...
GET   /camplib/v1/books/by-author/{authorId}
POST  /camplib/v1/books
POST  /camplib/v1/books/{bookId}/items
POST  /camplib/v1/books/{bookId}/authors
PATCH /camplib/v1/books/{bookId}/deactivate
```

## Swagger und Fehlerbehandlung

Die Controller enthalten XML-Kommentare und Swagger-Response-Annotationen.

Die API dokumentiert Erfolgs- und Fehlerantworten explizit.

Typische Fehlerantworten sind:

* `400 Bad Request`
* `401 Unauthorized`
* `403 Forbidden`
* `404 Not Found`
* `409 Conflict`

Fehler werden als `ProblemDetails` zurückgegeben.

Die Controller bilden Domain Errors bewusst explizit auf HTTP-Antworten ab. Dadurch wird für die Lehre sichtbar, welcher fachliche Fehler zu welchem HTTP-Status führt.

## Tests

Alle Tests ausführen:

```bash
dotnet test
```

Aktueller Teststand:

```text
155 Tests
0 failed
0 skipped
```

Die Testsuite umfasst:

* Domain Tests
* Value Object Tests
* UseCase Mock Tests
* UseCase Integration Tests
* Repository Integration Tests
* ReadModel Integration Tests
* Controller/API Tests

## Anwendung starten

```bash
dotnet run --project CampusLibraryApi
```

## Migrationen

Migration erzeugen:

```bash
dotnet ef migrations add <MigrationName> \
  --project CampusLibraryApi_4_Infrastructure/CampusLibraryApi_4_Infrastructure.csproj \
  --startup-project CampusLibraryApi/CampusLibraryApi.csproj
```

Datenbank aktualisieren:

```bash
dotnet ef database update \
  --project CampusLibraryApi_4_Infrastructure/CampusLibraryApi_4_Infrastructure.csproj \
  --startup-project CampusLibraryApi/CampusLibraryApi.csproj
```

## Zentrale Lernpunkte

```text
Controller sind HTTP-Adapter.
UseCases verändern Zustand.
ReadModels liefern Daten für Anzeige und Suche.
Repositories laden Aggregate.
Domain-Objekte schützen fachliche Regeln.
DTOs überschreiten Anwendungsschichtgrenzen.
Infrastructure implementiert technische Details.
```

Wichtige Regeln:

```text
Core-Module hängen nicht von Infrastructure ab.
Queries gehören in ReadModels.
Commands gehören in UseCases.
Deactivate ist kein Delete.
Die Domain zeigt die fachliche Beziehung.
Infrastructure zeigt die technische Persistenz.
```

## Nächster Schritt

Das nächste geplante Modul ist das Loans-Modul.

Das fachliche Ziel lautet:

```text
Ein Reader leiht ein BookItem aus.
```

Dadurch entstehen neue Architekturfragen:

* Ist Loan ein eigenes Aggregate?
* Wie referenziert ein Modul Daten eines anderen Moduls?
* Welche Daten werden direkt referenziert?
* Welche Daten werden als Snapshot übernommen?
* Wie werden modulübergreifende Regeln geprüft?
