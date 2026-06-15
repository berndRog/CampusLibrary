# Architektur: CampusLibrary Teil 3 — Readers + Catalog Modular Monolith

Dieses Dokument beschreibt die Architektur von Teil 3 der CampusLibraryApi.

Teil 3 erweitert den projektbasierten modularen Monolithen aus Teil 2 um ein zweites fachliches Modul: Catalog.

Teil 2 hatte die technische Modularisierung eingeführt. Die Anwendung bestand weiterhin nur aus dem Readers-Modul, war aber bereits in getrennte Projekte für Web/API, BuildingBlocks, Core, Infrastructure und Tests aufgeteilt.

Teil 3 behält diese modulare Struktur bei und ergänzt ein fachlich reichhaltigeres Domain Model mit Books, Authors, BookItems, einem ISBN Value Object, einer 1:n-Beziehung und einer m:n-Beziehung.

Das bedeutet:

* eine deploybare Anwendung
* mehrere Projekte
* eine Datenbank
* zwei fachliche Module: Readers und Catalog
* stärkere Modulgrenzen durch Projektstruktur
* ein fachlich reichhaltigeres Domain Model im Catalog-Modul
* unverändertes Verhalten des Readers-Moduls
* bestehende und neue Tests bleiben grün

Der aktuelle Teststand lautet:

```text
155 Tests
0 failed
0 skipped
```

## Architektonisches Ziel

Die Architektur von Teil 3 soll für die Lehre folgende Konzepte sichtbar machen:

* wie ein zweites fachliches Modul zu einem modularen Monolithen hinzugefügt wird
* wie bestehendes Verhalten stabil bleibt, während die Anwendung erweitert wird
* wie ein reichhaltigeres Domain Model mit Aggregates, Entities und Value Objects modelliert wird
* wie eine 1:n-Beziehung innerhalb eines Aggregates modelliert wird
* wie eine m:n-Beziehung modelliert wird, ohne die Join-Tabelle zur Domain Entity zu machen
* wie fachliche Beziehungen von technischen Persistenzdetails getrennt werden
* wie Core-Module unabhängig von EF Core und Datenbankkonfiguration bleiben
* wie schreibende UseCases von lesenden ReadModels getrennt werden
* wie Domain, Application, Infrastructure und API getestet werden
* wie die HTTP API mit Swagger/OpenAPI dokumentiert wird

Teil 3 beantwortet damit diese Frage:

```text
Wie kann ein zweites fachliches Modul mit reichhaltigeren Beziehungen
zu einem projektbasierten modularen Monolithen hinzugefügt werden,
ohne die bestehenden Architekturgrenzen aufzuweichen?
```

## Aktuelle Projektstruktur

Aktueller Stand mit den Modulen Readers und Catalog:

```text
CampusLibraryApi
├─ Program.cs
├─ appsettings.json
└─ Configure
   ├─ DiSwagger.cs
   └─ weitere anwendungsweite Registrierungen

CampusLibraryApi_1_Web
└─ Controllers
   ├─ ReadersController.cs
   ├─ AuthorsController.cs
   └─ BooksController.cs

CampusLibraryApi_2_BuildingBlocks
├─ Result.cs
├─ _1_Ports
│  ├─ IClock.cs
│  └─ IUnitOfWork.cs
└─ _3_Domain
   ├─ Entities
   │  ├─ Entity.cs
   │  └─ AggregateRoot.cs
   └─ Errors
      └─ DomainError.cs

CampusLibraryApi_3_Core_Readers
├─ _1_Ports
├─ _2_Application
└─ _3_Domain

CampusLibraryApi_3_Core_Catalog
├─ _1_Ports
│  └─ Outbound
│     ├─ IBookRepository.cs
│     ├─ IAuthorRepository.cs
│     ├─ IBookReadModel.cs
│     ├─ IAuthorReadModel.cs
│     └─ ICatalogDbContext.cs
│
├─ _2_Application
│  ├─ Dtos
│  │  ├─ AuthorCreateDto.cs
│  │  ├─ AuthorDto.cs
│  │  ├─ BookAssignAuthorDto.cs
│  │  ├─ BookCreateDto.cs
│  │  ├─ BookDetailDto.cs
│  │  ├─ BookDto.cs
│  │  ├─ BookItemAddDto.cs
│  │  ├─ BookItemDto.cs
│  │  ├─ BookListItemDto.cs
│  │  └─ BookSearchDto.cs
│  ├─ Mappings
│  └─ UseCases
│     ├─ AuthorUcCreate.cs
│     ├─ AuthorUcDeactivate.cs
│     ├─ AuthorUseCases.cs
│     ├─ BookUcCreate.cs
│     ├─ BookUcAddBookItem.cs
│     ├─ BookUcAssignAuthor.cs
│     ├─ BookUcDeactivate.cs
│     └─ BookUseCases.cs
│
└─ _3_Domain
   ├─ Entities
   │  ├─ Author.cs
   │  ├─ Book.cs
   │  └─ BookItem.cs
   ├─ Enums
   │  └─ BookItemStatus.cs
   ├─ Errors
   │  └─ CatalogErrors.cs
   └─ ValueObjects
      └─ IsbnVo.cs

CampusLibraryApi_4_Infrastructure
└─ Persistence
   ├─ Catalog
   │  └─ BookAuthorJoin.cs
   ├─ Configurations
   │  ├─ ConfigAuthor.cs
   │  ├─ ConfigBook.cs
   │  ├─ ConfigBookItem.cs
   │  └─ ConfigReader.cs
   ├─ Database
   │  ├─ AppDbContext.cs
   │  └─ UnitOfWorkEf.cs
   ├─ ReadModels
   │  ├─ ReaderReadModelEf.cs
   │  ├─ AuthorReadModelEf.cs
   │  └─ BookReadModelEf.cs
   ├─ Repositories
   │  ├─ ReaderRepositoryEf.cs
   │  ├─ AuthorRepositoryEf.cs
   │  └─ BookRepositoryEf.cs
   └─ Seed.cs

CampusLibraryApiTest
└─ Tests für Domain, Value Objects, UseCases, Repositories, ReadModels und Controller/API
```

## Warum dies weiterhin ein modularer Monolith ist

Teil 3 ist weiterhin ein Monolith, weil die Anwendung als eine Anwendung deployt wird.

Es gibt weiterhin:

```text
eine deploybare Anwendung
eine Datenbank
einen Runtime-Prozess
```

Die Anwendung ist aber modular, weil die Lösung in getrennte Projekte und fachliche Module aufgeteilt ist.

Der wichtige Unterschied zu Teil 2 lautet:

```text
Teil 2: ein fachliches Modul, Readers.
Teil 3: zwei fachliche Module, Readers und Catalog.
```

Mit zwei Modulen wird die modulare Struktur fachlich relevanter. Die Architektur ist nicht mehr nur Vorbereitung auf spätere Module, sondern muss nun echte Modultrennung unterstützen.

## Verantwortlichkeiten der Projekte

Teil 3 verwendet folgende Hauptprojekte:

```text
CampusLibraryApi
CampusLibraryApi_1_Web
CampusLibraryApi_2_BuildingBlocks
CampusLibraryApi_3_Core_Readers
CampusLibraryApi_3_Core_Catalog
CampusLibraryApi_4_Infrastructure
CampusLibraryApiTest
```

Jedes Projekt hat eine klare Verantwortung.

## CampusLibraryApi

`CampusLibraryApi` ist das ausführbare Anwendungsprojekt.

Es enthält den Composition Root der Anwendung.

Typische Aufgaben sind:

* Anwendungshost konfigurieren
* Konfiguration laden
* Controller registrieren
* Swagger/OpenAPI registrieren
* API-Versionierung registrieren
* Module registrieren
* Infrastructure registrieren
* Anwendung bauen und starten

`CampusLibraryApi` verdrahtet die Anwendung.

Dieses Projekt darf alle anderen Produktionsprojekte referenzieren, weil es die laufende Anwendung zusammensetzt.

Es darf keine Domainlogik enthalten.

## CampusLibraryApi_1_Web

`CampusLibraryApi_1_Web` enthält die HTTP API.

In Teil 3 sind das:

```text
ReadersController
AuthorsController
BooksController
```

Das Web-Projekt übersetzt HTTP Requests in Application-Aufrufe.

Typische Aufgaben sind:

* Routen definieren
* DTOs entgegennehmen
* ReadModels für GET Requests aufrufen
* UseCases für schreibende Requests aufrufen
* Result-Fehler in HTTP-Antworten übersetzen
* DTOs oder ProblemDetails zurückgeben
* Swagger/OpenAPI-Metadaten bereitstellen

Das Web-Projekt enthält keine Fachlogik.

Der Controller entscheidet zum Beispiel nicht, ob eine ISBN gültig ist. Das ist Aufgabe des Catalog Domain Models.

## CampusLibraryApi_2_BuildingBlocks

`CampusLibraryApi_2_BuildingBlocks` enthält wiederverwendbare architektonische Bausteine.

Typische Inhalte sind:

* Result
* DomainError
* WebErrorStatus
* Entity
* AggregateRoot
* IClock
* IUnitOfWork

Diese Typen sind nicht spezifisch für Readers oder Catalog.

Sie sind gemeinsame Konzepte für aktuelle und zukünftige Module.

Die wichtige Regel lautet:

```text
BuildingBlocks dürfen nicht von einem konkreten Fachmodul abhängen.
```

BuildingBlocks sind allgemeine Architekturbausteine. Sie sind nicht der Ort für reader-spezifische, catalog-spezifische oder loan-spezifische Fachlogik.

## CampusLibraryApi_3_Core_Readers

`CampusLibraryApi_3_Core_Readers` ist das erste fachliche Modul.

Es enthält das readerspezifische Domain Model, Application UseCases, DTOs, Mappings und Ports.

Das Readers-Modul bleibt in Teil 3 stabil.

Die wichtige Regel lautet:

```text
Das Hinzufügen von Catalog darf keine Änderung am Readers Domain Model erzwingen.
```

Readers ist weiterhin nur für Readers verantwortlich.

Das Modul kennt keine Books, Authors, BookItems oder Catalog-Persistenzdetails.

## CampusLibraryApi_3_Core_Catalog

`CampusLibraryApi_3_Core_Catalog` ist das zweite fachliche Modul.

Es enthält das catalog-spezifische Domain Model, Application UseCases, DTOs, Mappings und Ports.

Das Catalog-Modul ist intern gegliedert in:

```text
_1_Ports
_2_Application
_3_Domain
```

Die wichtige Regel lautet:

```text
Das Catalog Core Modul hängt nicht von Web oder Infrastructure ab.
```

Damit bleibt das Catalog-Modul unabhängig von HTTP, EF Core, SQLite und Swagger.

## Catalog Domain

Der Domain-Bereich des Catalog-Moduls enthält:

* Book
* Author
* BookItem
* IsbnVo
* BookItemStatus
* CatalogErrors

Die Catalog Domain enthält fachliche Regeln und fachliche Validierung.

Sie kennt nicht:

* Controller
* EF Core
* HTTP
* Swagger
* Datenbankdetails
* Dependency Injection

Das Domain Model soll verständlich sein, ohne wissen zu müssen, wie Daten gespeichert oder HTTP Requests empfangen werden.

## Book als Aggregate Root

`Book` ist ein Aggregate Root.

Es beschreibt das bibliografische Werk.

Ein Book besitzt:

* Title
* optionalen Subtitle
* ISBN
* Authors
* BookItems
* aktiven Zustand

Das Aggregate wird über eine Factory-Methode erzeugt:

```csharp
Book.Create(...)
```

Es wird über explizite Domain-Methoden geändert, zum Beispiel:

```csharp
Book.AddBookItem(...)
Book.AssignAuthor(...)
Book.Deactivate(...)
```

Dadurch werden unkontrollierte Änderungen über öffentliche Setter vermieden.

Die didaktische Regel lautet:

```text
Domain-Zustand sollte über explizite Domain-Methoden verändert werden,
nicht durch Setzen von Properties von außen.
```

## Author als Aggregate Root

`Author` ist ein Aggregate Root.

Ein Author beschreibt eine Person, die Books zugeordnet werden kann.

Ein Author besitzt:

* Firstname
* Lastname
* DisplayName
* aktiven Zustand

Authors werden im Catalog-Modul nicht physisch gelöscht.

Sie werden über eine Domain-Methode deaktiviert:

```csharp
Author.Deactivate(...)
```

Damit wird der fachliche Zustand geändert, statt die Datenbankzeile zu löschen.

## BookItem als Entity

`BookItem` ist eine Entity innerhalb des Book-Aggregates.

Es beschreibt ein physisches Exemplar eines Buches.

Ein BookItem besitzt:

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

Der Enum wird in der Datenbank als Zahl gespeichert.

Das hält die Datenbank kompakt und stabil, während der Code die fachliche Bedeutung durch die Enum-Namen ausdrückt.

## ISBN als Value Object

`IsbnVo` ist ein Value Object.

Es kapselt Validierungs- und Normalisierungsregeln für ISBN-Werte.

Ziel ist, ISBN-Validierung nicht über Controller, UseCases und Repositories zu verteilen.

Die didaktische Regel lautet:

```text
Wenn ein Wert fachliche Bedeutung und Regeln besitzt,
sollte er als Value Object modelliert werden.
```

## Beziehungen im Catalog

Teil 3 führt zwei wichtige Beziehungstypen ein.

## Book zu BookItem: 1:n

Die Beziehung zwischen `Book` und `BookItem` ist eine 1:n-Beziehung.

```text
Book 1 --- n BookItem
```

Diese Beziehung gehört in das Book-Aggregate.

Ein BookItem wird über das Book-Aggregate hinzugefügt:

```csharp
Book.AddBookItem(...)
```

Die didaktische Bedeutung lautet:

```text
Book ist für die Konsistenz innerhalb seiner Aggregate-Grenze verantwortlich.
BookItem gehört zu Book.
BookItem wird nicht unabhängig über eine eigene UseCase-Fassade verwaltet.
```

## Book zu Author: m:n

Die Beziehung zwischen `Book` und `Author` ist eine m:n-Beziehung.

```text
Book n --- m Author
```

Die Domain zeigt diese Beziehung über:

```csharp
Book.Authors
```

Ein Author wird einem Book zugeordnet über:

```csharp
Book.AssignAuthor(...)
```

Die Datenbank speichert die Beziehung über eine Join-Tabelle.

Der Join-Typ wird in der Infrastructure implementiert:

```text
BookAuthorJoin
```

Die wichtige Architekturentscheidung lautet:

```text
BookAuthorJoin ist ein Infrastrukturdetail.
BookAuthorJoin ist keine Domain Entity.
BookAuthorJoin besitzt keine eigene fachliche Identität.
```

Die Datenbankbeziehung verwendet den zusammengesetzten Schlüssel:

```text
BookId + AuthorId
```

Damit wird das Persistenzmodell sichtbar, ohne das Domain Model mit einer technischen Join-Entity zu belasten.

## Warum BookAuthorJoin keine Domain Entity ist

Eine Domain Entity sollte eine eigene fachliche Identität und fachliche Bedeutung besitzen.

In diesem Projekt ist die Beziehung zwischen Book und Author wichtig. Die Join-Zeile selbst besitzt aber keinen eigenen fachlichen Lebenszyklus.

Es gibt kein eigenes Fachkonzept wie `AuthorshipAssignment` mit eigenen Attributen und Regeln.

Daher gilt:

```text
Book und Author sind Domain-Konzepte.
Die Book-Author-Beziehung ist eine fachliche Beziehung.
Die Join-Tabelle ist ein Persistenzmechanismus.
```

Die Infrastrukturklasse existiert, weil EF Core die m:n-Tabelle explizit abbilden soll.

## Deactivate statt Delete

Im Catalog-Modul werden Books und Authors nicht physisch gelöscht.

Stattdessen werden sie deaktiviert:

```text
IsActive = false
```

Das hat zwei Konsequenzen:

```text
Repositories können das Aggregate weiterhin laden.
ReadModels entscheiden, was in normalen Abfragen sichtbar ist.
```

Normale Listen und Suchen liefern nur aktive Books und Authors.

Diese Unterscheidung ist wichtig:

```text
Deactivate ändert fachlichen Zustand.
Delete entfernt Daten physisch.
```

Das Catalog-Modul verwendet Deactivate, um historische und referenzielle Informationen zu erhalten.

## Repositories und ReadModels

Teil 3 hält die Trennung zwischen Repositories und ReadModels klar ein.

## Repositories

Repositories werden auf der Schreibseite verwendet.

Sie laden Aggregates für UseCases.

Beispiele:

```text
IBookRepository
IAuthorRepository
```

Typische Repository-Aufgaben sind:

* Aggregate hinzufügen
* mehrere Aggregate für Seed oder Tests hinzufügen
* Aggregate anhand der Id laden
* Eindeutigkeitsregeln prüfen
* EF-Core-Tracking für Schreibworkflows ermöglichen

Repositories geben Domain-Objekte zurück.

Sie sind nicht für Anzeigeoptimierung zuständig.

## ReadModels

ReadModels werden auf der Leseseite verwendet.

Sie liefern DTOs für Anzeige, Suche und Auswahl.

Beispiele:

```text
IBookReadModel
IAuthorReadModel
```

Typische ReadModel-Aufgaben sind:

* aktive Books abfragen
* aktive Authors abfragen
* Books suchen
* Authors suchen
* Datenbankdaten in DTOs projizieren
* AsNoTracking für Nur-Lese-Abfragen verwenden

ReadModels geben DTOs zurück, keine Domain-Objekte.

Die didaktische Regel lautet:

```text
Repositories laden Aggregates für Änderungen.
ReadModels liefern DTOs für Abfragen.
```

## UseCases und ReadModels

Teil 3 behält die Schreib-/Lese-Trennung aus Teil 2 bei.

```text
UseCase   = schreibender Application Workflow
ReadModel = lesende DB-zu-DTO-Projektion
```

Daher gilt:

```text
GET Requests                → ReadModel
POST / PUT / PATCH / DELETE → UseCase
```

Für Catalog bedeutet das zum Beispiel:

```text
GET /camplib/v1/books
→ BooksController
→ IBookReadModel.SelectAllAsync

POST /camplib/v1/books
→ BooksController
→ IBookUseCases.CreateAsync

POST /camplib/v1/books/{bookId}/items
→ BooksController
→ IBookUseCases.AddBookItemAsync

POST /camplib/v1/books/{bookId}/authors
→ BooksController
→ IBookUseCases.AssignAuthorAsync

PATCH /camplib/v1/books/{bookId}/deactivate
→ BooksController
→ IBookUseCases.DeactivateAsync
```

Diese Unterscheidung ist für die Lehre wichtig.

GET Requests sollen nicht versehentlich zu Domain Workflows werden. Sie fragen Daten ab und liefern DTOs.

Schreibende Requests schützen fachliche Konsistenz.

## Application Layer im Catalog

Der Application-Bereich des Catalog-Moduls koordiniert UseCases.

Er enthält:

* DTOs
* UseCases
* Mapping-Helfer
* UseCase-Fassaden

Beispiele:

* BookUcCreate
* BookUcAddBookItem
* BookUcAssignAuthor
* BookUcDeactivate
* BookUseCases
* AuthorUcCreate
* AuthorUcDeactivate
* AuthorUseCases

UseCases sind für Workflows verantwortlich.

Typische Aufgaben eines UseCases sind:

* grundlegende Eingaben prüfen
* optionale Ids auflösen
* Aggregates laden
* Value Objects erzeugen
* Eindeutigkeitsregeln über Repositories prüfen
* Domain-Methoden aufrufen
* Änderungen über IUnitOfWork speichern
* DTOs zurückgeben

UseCases sollten keine detaillierten Domain-Regeln enthalten, wenn diese Regeln in das Domain Model gehören.

## UseCase-Fassaden

Das Modul stellt Fassadeninterfaces für schreibende UseCases bereit:

```text
IBookUseCases
IAuthorUseCases
```

Diese Interfaces enthalten nur schreibende Operationen.

Sie enthalten keine Query-Operationen.

Die Regel lautet:

```text
Commands gehören in UseCases.
Queries gehören in ReadModels.
```

Dadurch wird verhindert, dass die UseCase-Fassade zu einem allgemeinen Service-Interface für alles wird, was der Controller braucht.

Ein Controller darf daher von beidem abhängen:

```text
IBookUseCases für Commands.
IBookReadModel für Queries.
```

Das ist Absicht.

Die didaktische Regel lautet:

```text
Nicht alles, was der Controller braucht, ist ein UseCase.
```

## DTOs im Catalog

Das Catalog-Modul verwendet unterschiedliche DTOs für unterschiedliche Zwecke.

Beispiele:

```text
BookCreateDto       → Eingabe zum Erzeugen eines Books
BookDto             → Ergebnis schreibender Operationen
BookDetailDto       → ausführliches Ergebnis der Detailanzeige
BookListItemDto     → Ergebnis für Listen und Suchtreffer
BookItemAddDto      → Eingabe zum Hinzufügen eines physischen Exemplars
BookItemDto         → Darstellung eines physischen Exemplars
BookAssignAuthorDto → Eingabe zum Zuordnen eines Authors zu einem Book
AuthorCreateDto     → Eingabe zum Erzeugen eines Authors
AuthorDto           → Darstellung eines Authors
```

Der wichtige Punkt ist:

```text
DTOs werden für UseCases und Queries zugeschnitten.
Sie müssen das Domain Model nicht exakt spiegeln.
```

`BookDetailDto` enthält zum Beispiel Authors, BookItems und berechnete Zähler.

`BookListItemDto` enthält kompakte Daten für Listen und Suchergebnisse.

## BookAssignAuthorDto

Der Endpoint für die Zuordnung eines Authors zu einem Book lautet:

```text
POST /camplib/v1/books/{bookId}/authors
```

Die Book-Id kommt aus der Route.

Der Request Body benötigt daher nur die Author-Id:

```csharp
public sealed record BookAssignAuthorDto(
   Guid AuthorId
);
```

Es gibt keine BookAuthor-Id.

Die Join-Tabelle ist ein Infrastrukturdetail und besitzt keine API-Identität.

## Infrastructure in Teil 3

`CampusLibraryApi_4_Infrastructure` implementiert technische Details für alle aktuellen Module.

Dazu gehören:

* EF-Core-Konfigurationen
* AppDbContext
* Repositories
* ReadModels
* UnitOfWorkEf
* Seed-Daten
* Join-Table-Mapping

Das Infrastructure-Projekt darf EF Core kennen.

Die Core-Module dürfen EF Core nicht kennen.

Die Abhängigkeitsrichtung bleibt:

```text
Core-Module definieren Ports.
Infrastructure implementiert Ports.
```

## DbContext-Zugriff

Es gibt eine gemeinsame technische Datenbank und einen gemeinsamen EF-Core-DbContext.

Jedes Modul definiert seinen eigenen logischen DbContext-Port.

Readers definiert:

```text
IReadersDbContext
```

Catalog definiert:

```text
ICatalogDbContext
```

`AppDbContext` implementiert beide Interfaces.

Dadurch hängt jedes Core-Modul nur von dem Teil des DbContext ab, den es benötigt.

Die didaktische Idee lautet:

```text
Auch mit einem physischen DbContext können Module ihre eigene logische Sicht
auf die Datenbank definieren.
```

## EF-Core-Konfiguration

EF-Core-Konfiguration gehört in die Infrastructure.

Beispiele:

```text
ConfigReader
ConfigAuthor
ConfigBook
ConfigBookItem
```

Das Domain Model soll keine EF-Core-spezifische Konfiguration enthalten.

Die m:n-Beziehung zwischen Book und Author wird in der Infrastructure über `BookAuthorJoin` konfiguriert.

Die Beziehung zwischen Book und BookItem wird als 1:n-Beziehung konfiguriert.

Der BookItem-Status wird als Integer gespeichert.

Dadurch bleibt die Datenbank kompakt, während der Code ausdrucksstark bleibt.

## Abhängigkeitsregeln

Die wichtigsten Projektabhängigkeitsregeln lauten:

```text
BuildingBlocks hängt von keinem Fachmodul ab.

Readers hängt von BuildingBlocks ab.

Catalog hängt von BuildingBlocks ab.

Infrastructure hängt von BuildingBlocks, Readers und Catalog ab.

Web hängt von Readers, Catalog und BuildingBlocks ab.

Das ausführbare API-Projekt verdrahtet alle Projekte.

Tests dürfen alle Projekte referenzieren, die für Tests benötigt werden.
```

Eine vereinfachte Abhängigkeitsrichtung ist:

```text
CampusLibraryApi_2_BuildingBlocks
        ↑
        │
CampusLibraryApi_3_Core_Readers
        ↑
        │
CampusLibraryApi_4_Infrastructure

CampusLibraryApi_2_BuildingBlocks
        ↑
        │
CampusLibraryApi_3_Core_Catalog
        ↑
        │
CampusLibraryApi_4_Infrastructure
```

Die Web/API-Seite ruft Module über Ports und UseCase-Fassaden auf.

Die Infrastructure-Seite implementiert Outbound Ports, die von den Modulen definiert werden.

Die Core-Module bleiben unabhängig von Web und Infrastructure.

## Schreibseite

Schreibende Workflows laufen über UseCases.

```text
Controller
→ UseCase-Fassade
→ konkreter UseCase
→ Domain / Aggregate
→ Repository
→ EF Core
→ UnitOfWork
```

Beispiel für das Erzeugen eines Books:

```text
POST /camplib/v1/books
→ BooksController
→ IBookUseCases.CreateAsync
→ BookUcCreate
→ IsbnVo.Create(...)
→ Book.Create(...)
→ IBookRepository
→ BookRepositoryEf
→ UnitOfWorkEf
```

Beispiel für das Zuordnen eines Authors:

```text
POST /camplib/v1/books/{bookId}/authors
→ BooksController
→ IBookUseCases.AssignAuthorAsync
→ BookUcAssignAuthor
→ IBookRepository.FindByIdAsync(...)
→ IAuthorRepository.FindByIdAsync(...)
→ Book.AssignAuthor(...)
→ UnitOfWorkEf
```

Beispiel für das Hinzufügen eines BookItems:

```text
POST /camplib/v1/books/{bookId}/items
→ BooksController
→ IBookUseCases.AddBookItemAsync
→ BookUcAddBookItem
→ IBookRepository.FindByIdAsync(...)
→ Book.AddBookItem(...)
→ UnitOfWorkEf
```

Beispiel für das Deaktivieren eines Books:

```text
PATCH /camplib/v1/books/{bookId}/deactivate
→ BooksController
→ IBookUseCases.DeactivateAsync
→ BookUcDeactivate
→ IBookRepository.FindByIdAsync(...)
→ Book.Deactivate(...)
→ UnitOfWorkEf
```

## Leseseite

Lesende Workflows laufen über ReadModels.

```text
Controller
→ ReadModel
→ DbContext
→ DTO
```

Beispiel für die Book-Suche:

```text
GET /camplib/v1/books/search?searchField=Title&searchText=clean
→ BooksController
→ IBookReadModel.SearchAsync
→ BookReadModelEf
→ AppDbContext
→ BookListItemDto
```

Beispiel für die Author-Suche:

```text
GET /camplib/v1/authors/search?searchText=Martin
→ AuthorsController
→ IAuthorReadModel.SearchAsync
→ AuthorReadModelEf
→ AppDbContext
→ AuthorDto
```

Die Leseseite lädt für normale Query-Antworten keine Aggregates.

Sie projiziert Datenbankdaten in DTOs.

## API-Versionierung und Swagger

Die API verwendet versionierte Routen.

Aktuelle Routen verwenden:

```text
/camplib/v1
```

Die aktuelle HTTP API enthält Endpunkte für:

* Readers
* Authors
* Books

Swagger/OpenAPI ist für Dokumentation und manuelle Tests konfiguriert.

Die Controller enthalten XML-Kommentare und Response-Annotationen.

Swagger dokumentiert:

* erfolgreiche Antworten
* ProblemDetails-Fehlerantworten
* 400 Bad Request
* 401 Unauthorized
* 403 Forbidden
* 404 Not Found
* 409 Conflict

Swagger ist nicht die Architektur selbst. Swagger dokumentiert die HTTP-Oberfläche der Anwendung.

Die Architekturregel bleibt:

```text
Swagger dokumentiert die API.
Controller übersetzen HTTP.
UseCases schreiben.
ReadModels lesen.
```

## Aktuelle HTTP API

Die aktuelle HTTP API unterstützt folgende Endpoint-Gruppen.

## Readers

```text
GET    /camplib/v1/readers
GET    /camplib/v1/readers/{id}
GET    /camplib/v1/readers/email?email=...
POST   /camplib/v1/readers
PUT    /camplib/v1/readers/{id}
DELETE /camplib/v1/readers/{id}
```

## Authors

```text
GET   /camplib/v1/authors
GET   /camplib/v1/authors/{id}
GET   /camplib/v1/authors/search?searchText=...
POST  /camplib/v1/authors
PATCH /camplib/v1/authors/{id}/deactivate
```

## Books

```text
GET   /camplib/v1/books
GET   /camplib/v1/books/{id}
GET   /camplib/v1/books/search?searchField=Title&searchText=...
GET   /camplib/v1/books/by-author/{authorId}
POST  /camplib/v1/books
POST  /camplib/v1/books/{bookId}/items
POST  /camplib/v1/books/{bookId}/authors
PATCH /camplib/v1/books/{bookId}/deactivate
```

## Fehlerbehandlung

Erwartete fachliche Fehler werden über `Result` zurückgegeben.

Controller übersetzen fehlgeschlagene Results in HTTP-Antworten.

Fehler werden als `ProblemDetails` zurückgegeben.

Die Entscheidung ist bewusst im Controller sichtbar.

Beispiel-Mapping:

```text
BadRequest   → 400
Unauthorized → 401
Forbidden    → 403
NotFound     → 404
Conflict     → 409
```

Diese Dopplung ist für die Lehre beabsichtigt.

Ziel ist, dass Studierende sehen können, welcher DomainError.Status zu welcher HTTP-Antwort führt.

## Testarchitektur

Teil 3 behält die bestehende Teststrategie bei und erweitert sie.

Typische Testgruppen sind:

* Domain Tests
* Value Object Tests
* UseCase Mock Tests
* UseCase Integration Tests
* Repository Integration Tests
* ReadModel Integration Tests
* Controller/API Tests

Die aktuelle Testsuite prüft:

* Reader Domain Verhalten
* Catalog Domain Verhalten
* Email- und Address-Validierung
* ISBN-Validierung
* Create UseCases
* Update UseCases
* Deactivate UseCases
* Book-Author-Zuordnung
* BookItem-Erzeugung
* Repository-Verhalten
* ReadModel-Projektionen
* Filterung inaktiver Daten auf der Leseseite
* HTTP Controller Verhalten
* Swagger-dokumentiertes API-Verhalten

Der aktuelle Teststand für Teil 3 lautet:

```text
155 Tests
0 failed
0 skipped
```

Das beabsichtigte Ergebnis ist:

```text
Die Architektur wächst.
Bestehendes Verhalten bleibt stabil.
Neues Verhalten ist durch Tests abgesichert.
```

## Version

Teil 3 wird durch folgenden Branch und geplanten Tag repräsentiert:

```text
Branch: part-3/readers-catalog
Tag:    v3-readers-catalog
```

Teil 2 bleibt verfügbar als:

```text
Tag: v2-readers-modular-monolith
```

Teil 1 bleibt verfügbar als:

```text
Tag: v1-readers-monolith
```

## Geplante Weiterentwicklung

Teil 3 bildet die Grundlage für den nächsten Lehrschritt.

Die geplante Entwicklung lautet:

```text
Teil 1: Readers, Ein-Projekt-Monolith
Teil 2: Readers, projektbasierter modularer Monolith
Teil 3: Readers + Catalog
Teil 4: Readers + Catalog + Loans
Teil 5: AuthN + AuthZ
```

Teil 4 wird ein drittes fachliches Modul hinzufügen.

Dieser Schritt ist wichtig, weil die Architektur dann Beziehungen zwischen Modulen zeigen muss.

## Regeln für die Erweiterung von Teil 3

Neue fachliche Module sollen der gleichen Struktur wie Readers und Catalog folgen.

Ein neues Core-Modul sollte ein eigenes Projekt besitzen, zum Beispiel:

```text
CampusLibraryApi_3_Core_Loans
```

Die interne Struktur sollte dem gleichen Muster folgen:

```text
_1_Ports
_2_Application
_3_Domain
```

Infrastructure implementiert die Ports der Core-Module.

Die wichtige Regel bleibt:

```text
Core-Module definieren Ports.
Infrastructure implementiert Ports.
Core-Module hängen nicht von Infrastructure ab.
```

Web-Controller liegen im Web-Projekt.

Controller enthalten keine Domainlogik. Sie übersetzen HTTP Requests in Aufrufe an UseCases oder ReadModels.

## Architekturregeln

Die Anwendung ist eine deploybare Anwendung.

Die Lösung ist in mehrere Projekte aufgeteilt.

Projektgrenzen repräsentieren Architekturgrenzen.

Fachliche Module werden als eigene Core-Projekte modelliert.

Web übersetzt HTTP und enthält keine Domainlogik.

BuildingBlocks enthält wiederverwendbare architektonische Basistypen.

Core-Module enthalten Domain- und Application-Logik.

Domain kennt kein Web, keine Infrastructure, kein EF Core und kein Swagger.

UseCases schreiben Domain-Zustand.

ReadModels lesen Daten direkt als DTO-Projektionen.

Repositories werden auf der Schreibseite verwendet.

ReadModels werden auf der Leseseite verwendet.

Infrastructure implementiert Core Ports.

EF-Core-Konfiguration gehört in die Infrastructure.

Join-Tabellen sind Persistenzdetails, solange sie keine eigene fachliche Identität besitzen.

Program.cs verdrahtet Module, enthält aber keine Domainlogik.

Zusätzliche Module sollen der gleichen Struktur wie Readers und Catalog folgen.

AuthN/AuthZ wird später ergänzt, ohne die Grundstruktur zu verändern.

## Didaktische Faustregel

UseCases schützen fachliche Regeln auf der Schreibseite.

ReadModels liefern einfache DTOs auf der Leseseite.

Kurz:

```text
UseCases schreiben.
ReadModels lesen.
```

Für Teil 3 ist zusätzlich wichtig:

```text
Die Domain zeigt die fachliche Beziehung.
Infrastructure zeigt den Persistenzmechanismus.
```

Für die Book-Author-Beziehung bedeutet das:

```text
Book.Authors gehört zum Domain Model.
BookAuthorJoin gehört zur Infrastructure.
```

Und für die Modularisierung:

```text
Ein neues Modul soll die Architektur erweitern,
nicht die bestehenden Grenzen aufweichen.
```
