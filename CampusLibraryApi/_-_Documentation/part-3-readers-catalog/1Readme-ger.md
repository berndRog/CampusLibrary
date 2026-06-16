# CampusLibrary

Lehrprojekt für eine modular aufgebaute, DDD-orientierte ASP.NET-Core-Web-API.

Das Projekt zeigt, wie ein kleiner modularer Monolith in getrennte Projekte für Web/API, Building Blocks, Core-Module, Infrastructure und Tests aufgeteilt werden kann, ohne dass das Domain Model von technischen Persistenzdetails abhängig wird.

Englische Version: [1readme.md](1readme.md)

## Aktueller Stand

Das Projekt enthält aktuell zwei fachliche Module:

* Readers-Modul
* Catalog-Modul
* ASP.NET Core Web API
* API-Versionierung
* Swagger/OpenAPI-Dokumentation
* SQLite-Persistenz mit EF Core
* Repository- und ReadModel-Infrastruktur
* UseCases für schreibende Workflows
* ReadModels für lesende Projektionen
* Controller-/API-Tests mit `WebApplicationFactory` und `HttpClient`
* Manuelle `.http`-Dateien für didaktische API-Tests

Der ursprüngliche Monolith wurde in einen projektbasierten modularen Monolithen umgebaut. Gemeinsame Abstraktionen und Basistypen liegen in `BuildingBlocks`. Die Module `Readers` und `Catalog` sind unabhängige Core-Module, während technische Persistenzdetails im Infrastructure-Projekt implementiert werden.

Die finale Testanzahl für Teil 3 sollte nach dem letzten Testlauf aktualisiert werden:

```bash
dotnet test
```

Dieser Platzhalter wird danach durch das finale Ergebnis ersetzt:

```text
<finale Testanzahl> Tests
0 fehlgeschlagen
0 übersprungen
```

## Versionen

* `v1-readers-monolith`  
  Erste abgeschlossene Version mit dem Readers-Modul in einer einzelnen monolithischen Projektstruktur.

* `v2-readers-modular-monolith`  
  Umgebaute Version mit projektbasierter modularer Monolith-Struktur.

* `v3-readers-catalog`  
  Ergänzt das Catalog-Modul mit Books, Authors, BookItems, ISBN Value Object, ReadModels, UseCases, Repositories, Controllern, Swagger-Dokumentation und Catalog-Tests.

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

Die Core-Module enthalten das Domain Model, Application UseCases, DTOs und Ports eines fachlichen Moduls.

Das BuildingBlocks-Projekt enthält gemeinsame Abstraktionen, die unabhängig von einem konkreten fachlichen Modul sind.

Das Infrastructure-Projekt implementiert technische Details wie EF-Core-Persistenz, Repositories, ReadModels und Datenbankkonfiguration.

Das Testprojekt prüft das Verhalten über Domain-, Application-, Infrastructure- und API-Grenzen hinweg.

Die wichtigste Dependency-Regel lautet:

```text
Core-Module hängen nicht von Web/API oder Infrastructure ab.
Infrastructure hängt von Core-Modulen ab, weil es deren Outbound Ports implementiert.
Das API-Projekt ist die Composition Root und verdrahtet alle Module.
```

## Module

## Readers-Modul

Das Readers-Modul verwaltet Bibliotheksnutzer.

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
* Reader-Profildaten aktualisieren
* Reader löschen
* Reader abfragen
* Reader anhand von ID oder Email finden

Das Readers-Modul ist bewusst einfach gehalten und dient als Einstiegspunkt in die Architektur.

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

Das Catalog-Modul führt im Vergleich zum Readers-Modul ein reichhaltigeres Domain Modeling ein.

## Catalog Domain Model

### Book

`Book` ist ein Aggregate Root.

Ein Book repräsentiert das bibliografische Werk und enthält:

* Titel
* optionalen Untertitel
* ISBN
* Authors
* BookItems
* Aktivstatus

Ein Book kann mehrere Authors haben.

Ein Book kann mehrere physische BookItems besitzen.

### Author

`Author` ist ein Aggregate Root.

Ein Author enthält:

* Firstname
* Lastname
* DisplayName
* Aktivstatus

Authors werden im Catalog-Modul nicht physisch gelöscht. Sie werden durch `IsActive = false` deaktiviert.

### BookItem

`BookItem` ist eine Entity innerhalb des `Book`-Aggregates.

Ein BookItem repräsentiert ein physisches Exemplar eines Books.

Es enthält:

* InventoryNumber
* Status

Der BookItem-Status wird als Enum modelliert:

```csharp
public enum BookItemStatus {
   Available = 1,
   Unavailable = 2,
   Lost = 3,
   Damaged = 4
}
```

Das Enum kann in der Datenbank als Integer gespeichert werden. Dadurch bleibt die Persistenz kompakt und stabil, während der Code die Bedeutung weiterhin über die Enum-Namen ausdrückt.

In der JSON-API können Enum-Werte als Strings serialisiert werden, wenn Enum-String-Serialisierung aktiviert ist.

Beispiel:

```json
{
  "status": "Available"
}
```

### ISBN Value Object

`IsbnVo` ist ein Value Object.

Es schützt die Domain-Regel, dass ein Book eine gültige ISBN haben muss. Die Domain sollte nicht mit beliebigen Strings arbeiten, wenn ein Wert eine konkrete fachliche Bedeutung besitzt.

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

Die Datenbank speichert die Beziehung über eine Join-Tabelle auf Infrastructure-Ebene.

```text
BookAuthorJoin ist ein Infrastructure-Detail.
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

UseCases arbeiten mit Repositories, Domain-Objekten und der Unit of Work.

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
Repositories laden Aggregates.
Controller übersetzen HTTP Requests und Responses.
```

## Katalogsuche

Books können über ein explizites Suchfeld gesucht werden:

```text
Title
AuthorLastName
Isbn
```

`AuthorLastName` sucht ausschließlich im Nachnamen der zugeordneten Authors.

Der Vorname wird nicht durchsucht. Dadurch werden zufällige Treffer vermieden.

Beispiel:

```text
AuthorLastName = Martin -> Clean Code
AuthorLastName = Fowler -> Refactoring und Design Patterns
```

Auch die Author-Suche verwendet den Nachnamen des Authors als fachlich relevantes Suchkriterium.

## Deactivate statt Delete

Im Catalog-Modul werden Books und Authors nicht physisch gelöscht.

Stattdessen werden sie deaktiviert:

```text
IsActive = false
```

Repositories können das Aggregate weiterhin laden.

ReadModels entscheiden, was in normalen Queries sichtbar ist.

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
GET   /camplib/v1/books/search?searchField=AuthorLastName&searchText=...
GET   /camplib/v1/books/search?searchField=Isbn&searchText=...
GET   /camplib/v1/books/by-author/{authorId}
POST  /camplib/v1/books
POST  /camplib/v1/books/{bookId}/items
POST  /camplib/v1/books/{bookId}/authors
PATCH /camplib/v1/books/{bookId}/deactivate
```

## Manuelle HTTP-Dateien

Für manuelle API-Tests wird die Datenbank zuerst zurückgesetzt oder gelöscht.

Danach werden die HTTP-Dateien in dieser Reihenfolge ausgeführt:

```text
1. Authors.http
2. Books.http
3. Readers.http
```

`Seed.cs` definiert die stabilen IDs.

Die `.http`-Dateien erzeugen die entsprechenden Daten über die öffentliche API.

```text
Authors.http erzeugt die Authors.
Books.http erzeugt die Books, verwendet die vorhandenen Authors, ordnet Authors zu Books zu und fügt BookItems hinzu.
Readers.http erzeugt oder prüft Reader-Daten.
```

Dadurch bleiben manuelle API-Tests reproduzierbar und hängen nicht von verstecktem Datenbankzustand ab.

## Swagger und Fehlerbehandlung

Die Controller enthalten XML-Kommentare und Swagger Response Annotations.

Die API dokumentiert Erfolgs- und Fehlerantworten ausdrücklich.

Typische Fehlerantworten sind:

* `400 Bad Request`
* `401 Unauthorized`
* `403 Forbidden`
* `404 Not Found`
* `409 Conflict`

Fehler werden als `ProblemDetails` zurückgegeben.

Die Controller mappen Domain Errors bewusst explizit auf HTTP Responses. Dadurch wird für die Lehre sichtbar, welcher Domain Error zu welchem HTTP-Statuscode führt.

## Testing

Alle automatisierten Tests ausführen:

```bash
dotnet test
```

Das finale Testergebnis für Teil 3 sollte nach dem letzten Testlauf eingetragen werden:

```text
<finale Testanzahl> Tests
0 fehlgeschlagen
0 übersprungen
```

Die Testsuite deckt ab:

* Domain Tests
* Value Object Tests
* UseCase Mock Tests
* UseCase Integration Tests
* Repository Integration Tests
* ReadModel Integration Tests
* Controller-/API-Tests mit `WebApplicationFactory` und `HttpClient`
* Manuelle `.http`-Dateien für didaktische API-Tests

Controller-Mock-Tests werden bewusst nicht als breite zusätzliche Testebene verwendet.

Die Controller sind dünne HTTP-Adapter. Der Application Workflow wird in UseCase Tests geprüft. Der öffentliche HTTP-Vertrag wird über `WebApplicationFactory` und `HttpClient` geprüft.

Die didaktische Unterscheidung lautet:

```text
Domain Tests schützen fachliche Regeln.
UseCase Mock Tests schützen Application Workflows.
Repository- und ReadModel-Tests schützen Persistenz und Projektionen.
Controller-/API-Tests schützen den öffentlichen HTTP-Vertrag.
Manuelle HTTP-Dateien machen API-Verhalten für Studierende sichtbar.
```

## Anwendung starten

```bash
dotnet run --project CampusLibraryApi
```

## Migrations

Migration erstellen:

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
Repositories laden Aggregates.
Domain-Objekte schützen fachliche Regeln.
DTOs überschreiten Application-Grenzen.
Infrastructure implementiert technische Details.
```

Wichtige Regeln:

```text
Core-Module hängen nicht von Infrastructure ab.
Queries gehören in ReadModels.
Commands gehören in UseCases.
Deactivate ist nicht Delete.
Die Domain zeigt die fachliche Beziehung.
Infrastructure zeigt den Persistenzmechanismus.
AuthorLastName sucht anhand des Author-Nachnamens.
Controller-Mock-Tests sind für dünne Controller nicht erforderlich.
```

## Nächster Schritt

Das nächste geplante Modul ist das Loans-Modul.

Das wichtigste fachliche Ziel lautet:

```text
Ein Reader leiht ein BookItem aus.
```

Dadurch entstehen Beziehungen zwischen Modulen und neue Designfragen:

* Soll Loan ein eigenes Aggregate sein?
* Wie referenziert ein Modul Daten aus einem anderen Modul?
* Welche Daten werden direkt referenziert?
* Welche Daten sollten als Snapshot gespeichert werden?
* Wie werden modulübergreifende Regeln geprüft?
