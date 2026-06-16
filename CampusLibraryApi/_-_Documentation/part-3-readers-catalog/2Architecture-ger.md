# Architektur: CampusLibrary Teil 3 — Readers + Catalog Modularer Monolith

Dieses Dokument beschreibt die Architektur von Teil 3 der `CampusLibraryApi`.

Teil 3 erweitert den projektbasierten modularen Monolithen aus Teil 2 um ein zweites fachliches Modul: `Catalog`.

Teil 2 hatte die Architekturgrenzen gestärkt, indem der strukturierte Readers-Monolith in getrennte Projekte für Web/API, BuildingBlocks, Core, Infrastructure und Tests aufgeteilt wurde.

Teil 3 behält diese modulare Struktur bei und ergänzt ein reichhaltigeres Domain Model mit Books, Authors, physischen BookItems, einem ISBN Value Object, einer 1:n-Beziehung und einer m:n-Beziehung.

Das bedeutet:

* eine deploybare Anwendung
* mehrere Projekte
* eine Datenbank
* zwei fachliche Module: Readers und Catalog
* stärkere Modulgrenzen durch Projektgrenzen
* ein reichhaltigeres Domain Model im Catalog-Modul
* unverändertes Readers-Verhalten
* bestehende und neue Tests bleiben nach dem finalen Testlauf grün

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

## Architekturziel

Die Architektur von Teil 3 soll folgende Konzepte für die Lehre sichtbar machen:

* wie ein zweites fachliches Modul zu einem modularen Monolithen hinzugefügt wird
* wie bestehendes Modulverhalten stabil bleibt, während das System erweitert wird
* wie ein reichhaltigeres Domain Model mit Aggregates, Entities und Value Objects modelliert wird
* wie eine 1:n-Beziehung innerhalb eines Aggregates modelliert wird
* wie eine m:n-Beziehung modelliert wird, ohne die Join-Tabelle zur Domain Entity zu machen
* wie Domain-Beziehungen von Persistenzdetails getrennt werden
* wie Core-Module unabhängig von EF Core und Datenbankkonfiguration bleiben
* wie schreibende UseCases von lesenden ReadModels getrennt werden
* wie Domain, Application, Infrastructure und API über Modulgrenzen hinweg getestet werden
* wie die HTTP-API mit Swagger/OpenAPI dokumentiert wird

Teil 3 beantwortet damit diese Frage:

```text
Wie kann ein zweites fachliches Modul mit reichhaltigeren Domain-Beziehungen zu einem projektbasierten modularen Monolithen hinzugefügt werden, ohne die bestehende Architektur zu schwächen?
```

## Aktuelle Projektstruktur

Aktueller Zustand mit den Modulen Readers und Catalog:

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
│  ├─ Mappings
│  └─ UseCases
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
   ├─ Database
   ├─ ReadModels
   ├─ Repositories
   └─ Seed.cs

CampusLibraryApiTest
└─ Tests für Domain, Value Objects, UseCases, Repositories, ReadModels und Controller/API-Szenarien
```

## Warum das weiterhin ein modularer Monolith ist

Teil 3 ist weiterhin ein Monolith, weil die Anwendung als eine Anwendung deployt wird.

Es gibt weiterhin:

```text
eine deploybare Anwendung
eine Datenbank
einen Runtime-Prozess
```

Gleichzeitig ist die Anwendung modular, weil die Solution in getrennte Projekte und fachliche Module mit expliziten Dependency-Regeln aufgeteilt ist.

Der wichtige Unterschied zu Teil 2 lautet:

```text
Teil 2: ein fachliches Modul, Readers.
Teil 3: zwei fachliche Module, Readers und Catalog.
```

Mit zwei Modulen wird die modulare Struktur fachlich bedeutsamer. Die Architektur ist nicht mehr nur Vorbereitung auf spätere Module, sondern muss echte Modultrennung tragen.

## Projektverantwortlichkeiten

Teil 3 verwendet diese Hauptprojekte:

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

Es enthält die Composition Root der Anwendung.

Typische Verantwortlichkeiten sind:

* Anwendungshost konfigurieren
* Konfiguration laden
* Controller registrieren
* Swagger/OpenAPI registrieren
* API-Versionierung registrieren
* Module registrieren
* Infrastructure registrieren
* Anwendung bauen und starten

`CampusLibraryApi` verdrahtet die Anwendung.

Es darf alle anderen Produktionsprojekte referenzieren, weil es die laufende Anwendung zusammensetzt.

Es enthält keine Domain-Logik.

## CampusLibraryApi_1_Web

`CampusLibraryApi_1_Web` enthält die HTTP-API-Oberfläche.

In Teil 3 sind das:

```text
ReadersController
AuthorsController
BooksController
```

Das Web-Projekt übersetzt HTTP Requests in Application-Aufrufe.

Typische Verantwortlichkeiten sind:

* Routen definieren
* DTOs, Route-Parameter und Query-Parameter entgegennehmen
* ReadModels für GET-Requests aufrufen
* UseCases für schreibende Requests aufrufen
* Result-Fehler in HTTP Responses übersetzen
* DTOs oder ProblemDetails zurückgeben
* Erfolgs- und Fehlerantworten für Swagger/OpenAPI dokumentieren

Das Web-Projekt enthält keine fachlichen Regeln.

Ein Controller entscheidet zum Beispiel nicht, ob eine ISBN gültig ist. Diese Regel gehört in das Catalog Domain Model.

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

Sie sind allgemeine Konzepte für aktuelle und zukünftige Module.

Die wichtige Regel lautet:

```text
BuildingBlocks hängt nicht von einem konkreten fachlichen Modul ab.
```

BuildingBlocks sind allgemeine architektonische Elemente. Sie sind nicht der Ort für reader-spezifische, catalog-spezifische oder loan-spezifische Fachlogik.

## CampusLibraryApi_3_Core_Readers

`CampusLibraryApi_3_Core_Readers` ist das erste fachliche Modul.

Es enthält das reader-spezifische Domain Model, Application UseCases, DTOs, Mappings und Ports.

Das Readers-Modul bleibt in Teil 3 stabil.

Die wichtige Regel lautet:

```text
Das Ergänzen von Catalog darf keine Änderung am Readers Domain Model erzwingen.
```

Readers ist weiterhin nur für Readers verantwortlich.

Es kennt keine Books, Authors, BookItems oder Catalog-Persistenzdetails.

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

Dadurch bleibt das Catalog-Modul unabhängig von HTTP, EF Core, SQLite und Swagger.

## Catalog Domain

Der Domain-Teil des Catalog-Moduls enthält:

* Book
* Author
* BookItem
* IsbnVo
* BookItemStatus
* CatalogErrors

Die Catalog Domain enthält fachliche Regeln und Domain-Validierung.

Sie kennt nicht:

* Controller
* EF Core
* HTTP
* Swagger
* Datenbankdetails
* Dependency Injection

Das Domain Model soll verständlich sein, ohne zu wissen, wie Daten gespeichert werden oder wie HTTP Requests empfangen werden.

## Book als Aggregate Root

`Book` ist ein Aggregate Root.

Es repräsentiert das bibliografische Werk.

Ein Book hat:

* Titel
* optionalen Untertitel
* ISBN
* Authors
* BookItems
* Aktivstatus

Das Aggregate wird über eine Factory-Methode erzeugt:

```csharp
Book.Create(...)
```

Es wird über explizite Domain-Methoden verändert, zum Beispiel:

```csharp
Book.AddBookItem(...)
Book.AssignAuthor(...)
Book.Deactivate(...)
```

Dadurch werden unkontrollierte Änderungen über öffentliche Setter vermieden.

Die didaktische Regel lautet:

```text
Domain-Zustand sollte über explizite Domain-Methoden geändert werden, nicht durch Setzen von Properties von außen.
```

## Author als Aggregate Root

`Author` ist ein Aggregate Root.

Es repräsentiert eine Person, die Books zugeordnet werden kann.

Ein Author hat:

* Firstname
* Lastname
* DisplayName
* Aktivstatus

Authors werden im Catalog-Modul nicht physisch gelöscht.

Sie werden über eine Domain-Methode deaktiviert:

```csharp
Author.Deactivate(...)
```

Dadurch wird der fachliche Zustand geändert, statt die Datenbankzeile zu löschen.

## BookItem als Entity

`BookItem` ist eine Entity innerhalb des Book-Aggregates.

Es repräsentiert ein physisches Exemplar eines Books.

Ein BookItem hat:

* InventoryNumber
* Status

Der aktuelle Status wird durch ein Enum dargestellt:

```csharp
public enum BookItemStatus {
   Available = 1,
   Unavailable = 2,
   Lost = 3,
   Damaged = 4
}
```

Das Enum kann in der Datenbank als Integer gespeichert werden.

Dadurch bleibt die Datenbank kompakt und stabil, während der Code die Bedeutung über die Enum-Namen ausdrückt.

In der JSON-API können Enum-Werte als Strings serialisiert werden, wenn Enum-String-Serialisierung aktiviert ist.

Beispiel:

```json
{
  "status": "Available"
}
```

## ISBN als Value Object

`IsbnVo` ist ein Value Object.

Es kapselt Validierungs- und Normalisierungsregeln für ISBN-Werte.

Ziel ist, ISBN-Validierung nicht über Controller, UseCases und Repositories zu verteilen.

Die didaktische Regel lautet:

```text
Wenn ein Wert fachliche Bedeutung und Regeln besitzt, modelliere ihn als Value Object.
```

## Catalog-Beziehungen

Teil 3 führt zwei wichtige Beziehungstypen ein.

## Book zu BookItem: 1:n

Die Beziehung zwischen `Book` und `BookItem` ist eine 1:n-Beziehung.

```text
Book 1 --- n BookItem
```

Diese Beziehung gehört innerhalb des Book-Aggregates.

Ein BookItem wird über das Book-Aggregate hinzugefügt:

```csharp
Book.AddBookItem(...)
```

Die didaktische Bedeutung lautet:

```text
Book ist für Konsistenz innerhalb seiner Aggregate-Grenze verantwortlich.
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

Der Join-Typ ist in Infrastructure implementiert:

```text
BookAuthorJoin
```

Die wichtige Designentscheidung lautet:

```text
BookAuthorJoin ist ein Infrastructure-Detail.
BookAuthorJoin ist keine Domain Entity.
BookAuthorJoin besitzt keine eigene fachliche Identität.
```

Die Datenbankbeziehung verwendet den zusammengesetzten Schlüssel:

```text
BookId + AuthorId
```

So bleibt das Persistenzmodell explizit, ohne das Domain Model mit einer technischen Join Entity zu verunreinigen.

## Warum BookAuthorJoin keine Domain Entity ist

Eine Domain Entity sollte eine eigene fachliche Identität und fachliche Bedeutung besitzen.

In diesem Projekt ist die Beziehung zwischen Book und Author wichtig, aber die Join-Zeile selbst hat keinen eigenen fachlichen Lebenszyklus.

Es gibt kein separates Fachkonzept wie `AuthorshipAssignment` mit eigenen Attributen und Regeln.

Daher gilt:

```text
Book und Author sind Domain-Konzepte.
Die Book-Author-Beziehung ist eine Domain-Beziehung.
Die Join-Tabelle ist ein Persistenzmechanismus.
```

Die Infrastructure-Join-Klasse existiert nur, weil EF Core die m:n-Tabelle explizit abbilden muss.

## Deactivate statt Delete

Im Catalog-Modul werden Books und Authors nicht physisch gelöscht.

Sie werden deaktiviert:

```text
IsActive = false
```

Das hat zwei Folgen:

```text
Repositories können das Aggregate weiterhin laden.
ReadModels entscheiden, was in normalen Queries sichtbar ist.
```

Normale Listen und Suchen liefern nur aktive Books und Authors.

Diese Unterscheidung ist wichtig:

```text
Deactivate ändert fachlichen Zustand.
Delete entfernt Daten physisch.
```

Das Catalog-Modul verwendet Deactivate, um historische und referenzielle Informationen zu erhalten.

## Repositories und ReadModels

Teil 3 behält die klare Trennung zwischen Repositories und ReadModels bei.

## Repositories

Repositories werden auf der Schreibseite verwendet.

Sie laden Aggregates für UseCases.

Beispiele:

```text
IBookRepository
IAuthorRepository
```

Typische Repository-Verantwortlichkeiten sind:

* Aggregate hinzufügen
* mehrere Aggregates für Seed oder Tests hinzufügen
* Aggregate anhand der ID finden
* Eindeutigkeitsregeln prüfen
* EF-Core-Tracking für Schreib-Workflows erhalten

Repositories liefern Domain-Objekte zurück.

Sie sind nicht für optimierte Anzeige-Projektionen verantwortlich.

## ReadModels

ReadModels werden auf der Leseseite verwendet.

Sie liefern DTOs für Anzeige, Suche und Auswahl.

Beispiele:

```text
IBookReadModel
IAuthorReadModel
```

Typische ReadModel-Verantwortlichkeiten sind:

* aktive Books abfragen
* aktive Authors abfragen
* Books suchen
* Authors suchen
* Datenbankdaten in DTOs projizieren
* `AsNoTracking` für reine Leseabfragen verwenden

ReadModels liefern DTOs, keine Domain-Objekte.

Die didaktische Regel lautet:

```text
Repositories laden Aggregates für Änderungen.
ReadModels liefern DTOs für Queries.
```

## Catalog-Suche

Das Catalog-Modul verwendet explizite Suchkriterien für Books.

Die unterstützten Book-Suchfelder sind:

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

Dadurch ist das Suchverhalten explizit und ein allgemeiner Begriff wie "AuthorName" wird nicht irrtümlich als Firstname plus Lastname interpretiert.

## UseCases und ReadModels

Teil 3 behält die Schreib-/Lese-Trennung aus Teil 2 bei.

```text
UseCase   = schreibender Application Workflow
ReadModel = lesende DB-zu-DTO-Projektion
```

Daher gilt:

```text
GET Requests                 → ReadModel
POST / PUT / PATCH / DELETE  → UseCase
```

Für Catalog:

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

Schreibende Requests schützen die Domain-Konsistenz.

## Application Layer im Catalog

Der Application-Teil des Catalog-Moduls koordiniert UseCases.

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

Typische Verantwortlichkeiten eines UseCases sind:

* grundlegende Eingaben validieren
* optionale IDs auflösen
* Aggregates laden
* Value Objects erzeugen
* Eindeutigkeitsregeln über Repositories prüfen
* Domain-Methoden aufrufen
* Änderungen über IUnitOfWork speichern
* DTOs zurückgeben

UseCases sollten keine detaillierten Domain-Regeln enthalten, wenn diese Regeln ins Domain Model gehören.

## UseCase-Fassaden

Das Modul veröffentlicht Fassadeninterfaces für Command UseCases:

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

Dadurch wird vermieden, dass die UseCase-Fassade zu einem allgemeinen Service-Interface für alles wird, was ein Controller benötigt.

Der Controller darf von beiden abhängig sein:

```text
IBookUseCases für Commands.
IBookReadModel für Queries.
```

Das ist bewusst so gewählt.

Die didaktische Regel lautet:

```text
Nicht alles, was ein Controller braucht, ist ein UseCase.
```

## DTOs im Catalog

Das Catalog-Modul verwendet verschiedene DTOs für unterschiedliche Anwendungsfälle.

Beispiele:

```text
BookCreateDto       → Eingabe zum Erzeugen eines Books
BookDto             → Ergebnis schreibender Operationen
BookDetailDto       → detailliertes ReadModel-Ergebnis
BookListItemDto     → Listen- und Suchergebnis
BookItemAddDto      → Eingabe zum Hinzufügen eines physischen BookItems
BookItemDto         → Darstellung eines physischen BookItems
BookAssignAuthorDto → Eingabe zum Zuordnen eines Authors zu einem Book
AuthorCreateDto     → Eingabe zum Erzeugen eines Authors
AuthorDto           → Author-Darstellung
```

Wichtig ist:

```text
DTOs werden nach UseCases und Queries geformt.
Sie müssen nicht exakt das Domain Model spiegeln.
```

`BookDetailDto` enthält zum Beispiel Authors, BookItems und berechnete Zähler.

`BookListItemDto` enthält kompakte Daten für Listen und Suchergebnisse.

## BookAssignAuthorDto

Der Endpunkt für die Zuordnung eines Authors zu einem Book ist:

```text
POST /camplib/v1/books/{bookId}/authors
```

Die Book-ID kommt aus der Route.

Der Request Body benötigt deshalb nur die Author-ID:

```csharp
public sealed record BookAssignAuthorDto(
   Guid AuthorId
);
```

Es gibt keine BookAuthor-ID.

Die Join-Tabelle ist ein Infrastructure-Detail und hat keine API-Identität.

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

Die Dependency-Richtung bleibt:

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
Auch mit einem physischen DbContext können Module ihre eigene logische Sicht auf die Datenbank definieren.
```

## EF-Core-Konfiguration

EF-Core-Konfiguration gehört in Infrastructure.

Beispiele:

```text
ConfigReader
ConfigAuthor
ConfigBook
ConfigBookItem
```

Das Domain Model soll keine EF-Core-spezifische Konfiguration enthalten.

Die m:n-Beziehung zwischen Book und Author wird in Infrastructure über `BookAuthorJoin` konfiguriert.

Die Beziehung von Book zu BookItem wird als 1:n-Beziehung konfiguriert.

Der BookItemStatus kann in der Datenbank als Integer gespeichert werden.

Dadurch bleibt die Datenbank kompakt, während der Code ausdrucksstark bleibt.

## Dependency-Regeln

Die wichtigsten Projekt-Dependency-Regeln lauten:

```text
BuildingBlocks hängt von keinem fachlichen Modul ab.

Readers hängt von BuildingBlocks ab.

Catalog hängt von BuildingBlocks ab.

Infrastructure hängt von BuildingBlocks, Readers und Catalog ab.

Web hängt von Readers, Catalog und BuildingBlocks ab.

Das ausführbare API-Projekt verdrahtet alle Projekte.

Tests dürfen alle Projekte referenzieren, die zum Testen erforderlich sind.
```

Eine vereinfachte Dependency-Richtung ist:

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

Beispiel für Book-Suche nach Titel:

```text
GET /camplib/v1/books/search?searchField=Title&searchText=clean
→ BooksController
→ IBookReadModel.SearchAsync
→ BookReadModelEf
→ AppDbContext
→ BookListItemDto
```

Beispiel für Book-Suche nach Author-Nachname:

```text
GET /camplib/v1/books/search?searchField=AuthorLastName&searchText=Martin
→ BooksController
→ IBookReadModel.SearchAsync
→ BookReadModelEf
→ AppDbContext
→ BookListItemDto
```

Beispiel für Author-Suche:

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

Die aktuelle HTTP-API enthält Endpunkte für:

* Readers
* Authors
* Books

Swagger/OpenAPI ist für Dokumentation und manuelle Tests konfiguriert.

Die Controller enthalten XML-Kommentare und Response Annotations.

Swagger dokumentiert:

* erfolgreiche Responses
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

## Aktuelle HTTP-API

Die aktuelle HTTP-API unterstützt diese Endpoint-Gruppen.

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
GET   /camplib/v1/books/search?searchField=AuthorLastName&searchText=...
GET   /camplib/v1/books/search?searchField=Isbn&searchText=...
GET   /camplib/v1/books/by-author/{authorId}
POST  /camplib/v1/books
POST  /camplib/v1/books/{bookId}/items
POST  /camplib/v1/books/{bookId}/authors
PATCH /camplib/v1/books/{bookId}/deactivate
```

## Fehlerbehandlung

Erwartete fachliche Fehler werden über `Result` zurückgegeben.

Controller übersetzen fehlgeschlagene Results in HTTP Responses.

Fehler werden als `ProblemDetails` zurückgegeben.

Beispiel-Mapping:

```text
BadRequest   → 400
Unauthorized → 401
Forbidden    → 403
NotFound     → 404
Conflict     → 409
```

Diese explizite Übersetzung ist für die Lehre bewusst sichtbar.

Studierende sollen erkennen können, welcher Domain Error zu welchem HTTP-Statuscode führt.

## Testing-Architektur

Teil 3 behält die bestehende Teststrategie bei und erweitert sie.

Typische Testgruppen sind:

* Domain Tests
* Value Object Tests
* UseCase Mock Tests
* UseCase Integration Tests
* Repository Integration Tests
* ReadModel Integration Tests
* Controller-/API-Tests mit `WebApplicationFactory` und `HttpClient`
* Manuelle `.http`-Dateien für didaktische API-Tests

Die aktuelle Testsuite prüft:

* Reader-Domain-Verhalten
* Catalog-Domain-Verhalten
* Email- und Address-Validierung
* ISBN-Validierung
* Create UseCases
* Update UseCases
* Deactivate UseCases
* Book-Author-Zuordnung
* BookItem-Erzeugung
* Repository-Verhalten
* ReadModel-Projektionen
* Ausblenden inaktiver Daten auf der Leseseite
* HTTP-Controller-Verhalten über `WebApplicationFactory` und `HttpClient`
* Swagger-dokumentiertes API-Verhalten
* manuelle API-Workflows über `.http`-Dateien

Das finale Testergebnis für Teil 3 sollte nach dem letzten Testlauf eingetragen werden:

```text
<finale Testanzahl> Tests
0 fehlgeschlagen
0 übersprungen
```

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

Manuelle HTTP-Dateien werden nach einem Datenbank-Reset in dieser Reihenfolge ausgeführt:

```text
1. Authors.http
2. Books.http
3. Readers.http
```

`Seed.cs` definiert die stabilen IDs. Die `.http`-Dateien erzeugen die entsprechenden Daten über die öffentliche API.

Das gewünschte Ergebnis lautet:

```text
Die Architektur wächst.
Bestehendes Verhalten bleibt stabil.
Neues Verhalten ist durch Tests abgesichert.
```

## Version

Teil 3 wird durch diesen Branch und das geplante Tag repräsentiert:

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

Teil 3 ist die Grundlage für den nächsten Lehrschritt.

Die geplante Entwicklung lautet:

```text
Teil 1: Readers, Ein-Projekt-Monolith
Teil 2: Readers, projektbasierter modularer Monolith
Teil 3: Readers + Catalog
Teil 4: Readers + Catalog + Loans
Teil 5: AuthN + AuthZ
```

Teil 4 ergänzt ein drittes fachliches Modul.

Dieser Schritt ist wichtig, weil die Architektur dann Beziehungen zwischen Modulen sichtbar machen muss.

## Regeln für Erweiterungen nach Teil 3

Neue fachliche Module sollten derselben Struktur folgen wie Readers und Catalog.

Ein neues Core-Modul sollte ein eigenes Projekt erhalten, zum Beispiel:

```text
CampusLibraryApi_3_Core_Loans
```

Die interne Struktur sollte demselben Muster folgen:

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

Controller enthalten keine Domain-Logik. Sie übersetzen HTTP Requests in Aufrufe an UseCases oder ReadModels.

## Architekturregeln

Die Anwendung ist eine deploybare Anwendung.

Die Solution ist in mehrere Projekte aufgeteilt.

Projektgrenzen repräsentieren Architekturgrenzen.

Fachliche Module werden als getrennte Core-Projekte dargestellt.

Web übersetzt HTTP und enthält keine Domain-Logik.

BuildingBlocks enthält wiederverwendbare architektonische Basistypen.

Core-Module enthalten Domain- und Application-Logik.

Domain kennt kein Web, keine Infrastructure, kein EF Core und kein Swagger.

UseCases schreiben Domain-Zustand.

ReadModels lesen Daten direkt als DTO-Projektionen.

Repositories werden auf der Schreibseite verwendet.

ReadModels werden auf der Leseseite verwendet.

Book-Suche verwendet explizite Suchfelder wie `Title`, `AuthorLastName` und `Isbn`.

Controller-Mock-Tests sind für dünne Controller nicht erforderlich.

Infrastructure implementiert Core Ports.

EF-Core-Konfiguration gehört in Infrastructure.

Join-Tabellen sind Persistenzdetails, solange sie keine eigene fachliche Identität besitzen.

Program.cs verdrahtet Module, enthält aber keine Domain-Logik.

Zusätzliche Module sollten derselben Struktur folgen wie Readers und Catalog.

AuthN/AuthZ wird später ergänzt, ohne die Grundstruktur zu verändern.

## Didaktische Faustregel

UseCases schützen fachliche Regeln auf der Schreibseite.

ReadModels liefern einfache DTOs auf der Leseseite.

Oder kürzer:

```text
UseCases schreiben.
ReadModels lesen.
```

Für Teil 3 ist eine weitere Regel wichtig:

```text
Die Domain zeigt die fachliche Beziehung.
Infrastructure zeigt den Persistenzmechanismus.
```

Für die Book-Author-Beziehung bedeutet das:

```text
Book.Authors ist Teil des Domain Models.
BookAuthorJoin ist Teil der Infrastructure.
```

Für die Katalogsuche gilt:

```text
AuthorLastName sucht anhand des Author-Nachnamens.
Firstname wird nicht durchsucht, weil dadurch zufällige Treffer entstehen würden.
```

Und für Modularisierung:

```text
Ein neues Modul soll die Architektur erweitern, nicht die Grenzen schwächen.
```
