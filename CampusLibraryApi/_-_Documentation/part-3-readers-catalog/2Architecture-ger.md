# Architektur: CampusLibrary Teil 3 — Readers + Catalog Modular Monolith

Dieses Dokument beschreibt die Architektur der aktuellen CampusLibraryApi.

Die Anwendung ist ein projektbasierter modularer Monolith mit zwei fachlichen Modulen: Readers und Catalog. Sie wird als eine ASP.NET-Core-Anwendung deployt und verwendet eine Datenbank.

Finales Testergebnis:

```text
139 Tests
0 fehlgeschlagen
0 übersprungen
```

## Architekturziel

Die Architektur macht folgende Konzepte für den Unterricht sichtbar:

* projektbasierter modularer Monolith
* Modulgrenzen über Projektreferenzen
* unabhängige Core-Module
* gemeinsame BuildingBlocks
* Aggregates, Entities und Value Objects
* 1:n-Beziehung innerhalb eines Aggregates
* schreibende Use Cases und lesende ReadModels
* Repositories zum Laden von Aggregaten
* Infrastructure als Implementierung von Outbound Ports
* HTTP-API als Adapter
* automatisierte Tests über relevante Schichten hinweg

## Aktuelle Projektstruktur

```text
CampusLibraryApi
├─ Program.cs
├─ appsettings.json
└─ Configure

CampusLibraryApi_1_Web
└─ Controllers
   ├─ ReadersController.cs
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

CampusLibraryApi_3_Core_Readers
├─ _1_Ports
├─ _2_Application
└─ _3_Domain

CampusLibraryApi_3_Core_Catalog
├─ _1_Ports
│  └─ Outbound
│     ├─ IBookRepository.cs
│     ├─ IBookReadModel.cs
│     └─ ICatalogDbContext.cs
├─ _2_Application
│  ├─ Dtos
│  ├─ Enums
│  ├─ Mappings
│  └─ UseCases
└─ _3_Domain
   ├─ Entities
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
   ├─ Configurations
   ├─ Database
   ├─ ReadModels
   ├─ Repositories
   └─ Seed.cs

CampusLibraryApiTest
```

## Modularer Monolith

Die Anwendung besitzt eine deploybare Einheit, eine Datenbank und einen Runtime-Prozess. Der Code ist modular, weil fachliche Fähigkeiten in Projekte und Module gegliedert sind.

```text
Readers enthält reader-spezifischen Code.
Catalog enthält catalog-spezifischen Code.
BuildingBlocks enthält wiederverwendbare Architekturtypen.
Infrastructure implementiert technische Adapter.
Web stellt die HTTP-API bereit.
```

## Projektverantwortlichkeiten

## CampusLibraryApi

Das ausführbare Anwendungsprojekt. Es ist der Composition Root und verdrahtet alle Module.

Aufgaben:

* Host und Middleware konfigurieren
* Controller registrieren
* Swagger/OpenAPI registrieren
* API-Versionierung registrieren
* Core-Module und Infrastructure registrieren
* Anwendung starten

## CampusLibraryApi_1_Web

Die HTTP-Adapter-Schicht.

Aufgaben:

* Routen definieren
* Request-DTOs entgegennehmen
* ReadModels für GET-Requests aufrufen
* Use Cases für schreibende Requests aufrufen
* `Result<T>` in HTTP-Antworten übersetzen
* DTOs oder ProblemDetails zurückgeben
* API für Swagger/OpenAPI dokumentieren

## CampusLibraryApi_2_BuildingBlocks

Wiederverwendbare Architekturtypen:

* Result
* DomainError
* Entity
* AggregateRoot
* IClock
* IUnitOfWork

BuildingBlocks sind unabhängig von konkreten fachlichen Modulen.

## CampusLibraryApi_3_Core_Readers

Das fachliche Readers-Modul.

Es enthält Reader-Domänenmodell, Application Use Cases, DTOs, Mappings und Ports.

## CampusLibraryApi_3_Core_Catalog

Das fachliche Catalog-Modul.

Es enthält Book-Domänenmodell, Application Use Cases, DTOs, Mappings und Ports.

Das Catalog-Core-Modul ist unabhängig von HTTP, EF Core, SQLite und Swagger.

## CampusLibraryApi_4_Infrastructure

Die technische Adapter-Schicht.

Aufgaben:

* EF-Core-DbContext
* EF-Core-Konfigurationen
* Repository-Implementierungen
* ReadModel-Implementierungen
* UnitOfWork-Implementierung
* Migrations
* Seed-Daten

Infrastructure hängt von Core-Modulen ab, weil sie deren Ports implementiert.

## Domänenmodell

## Reader

`Reader` ist ein Aggregate Root.

Ein Reader hat:

* Vorname
* Nachname
* E-Mail
* Adresse
* Subject
* Aktiv-Status
* Erstellzeitpunkt
* Änderungszeitpunkt

E-Mail und Adresse werden mit Value Objects modelliert.

## Book

`Book` ist ein Aggregate Root.

Ein Book hat:

* Autorentext
* Titel
* optionalen Untertitel
* ISBN
* BookItems
* Aktiv-Status
* Erstellzeitpunkt
* Änderungszeitpunkt

Zustandsänderungen erfolgen über Domänenmethoden:

```csharp
Book.Create(...)
Book.AddBookItem(...)
Book.Deactivate(...)
```

## AuthorsText

`AuthorsText` speichert Autorennamen als Text.

Beispiele:

```text
Robert C. Martin
Martin Fowler, Kent Beck
Erich Gamma, Richard Helm, Ralph Johnson, John Vlissides
```

Für die Suche wird der Text nach einer Nachnamenregel interpretiert:

```text
An Kommata trennen.
Jeden Autoren-Token an Leerzeichen trennen.
Das letzte Wort als Nachname verwenden.
```

Beispiele:

```text
Robert C. Martin -> Martin
Martin Fowler -> Fowler
Kent Beck -> Beck
```

## BookItem

`BookItem` ist eine Entity innerhalb des Book-Aggregates.

Es repräsentiert ein physisches Exemplar eines Buchs.

Ein neues BookItem startet mit Status `Available`.

## IsbnVo

`IsbnVo` ist ein Value Object, das ISBN-Werte validiert und normalisiert.

## Beziehung: Book zu BookItem

```text
Book 1 --- n BookItem
```

Das Book-Aggregate schützt die Konsistenz seiner BookItems.

## Deaktivierung

Reader und Books besitzen einen Aktiv-Zustand.

```text
IsActive = false
```

Repositories können Aggregate nach Id laden. ReadModels entscheiden, was in normalen Abfragen sichtbar ist.

## Repositories und ReadModels

Repositories werden auf der Schreibseite verwendet. Sie laden Aggregate und behalten EF-Core-Tracking für Workflows.

ReadModels werden auf der Leseseite verwendet. Sie projizieren Datenbankdaten in DTOs und verwenden normalerweise kein Tracking.

```text
Repository -> domänenorientierter Schreibzugriff
ReadModel  -> DTO-orientierter Lesezugriff
```

## Use Cases und ReadModels

```text
GET-Requests                 -> ReadModel
POST / PUT / PATCH / DELETE  -> Use Case
```

Catalog-Beispiele:

```text
GET /camplib/v1/books
-> IBookReadModel.SelectAllAsync

POST /camplib/v1/books
-> IBookUseCases.CreateAsync

POST /camplib/v1/books/{bookId}/items
-> IBookUseCases.AddBookItemAsync

PATCH /camplib/v1/books/{bookId}/deactivate
-> IBookUseCases.DeactivateAsync
```

## Datenbankmodell

Aktuelle Tabellen:

```text
Readers
Books
BookItems
```

Books-Spalten:

```text
Id
Authors
Title
Subtitle
Isbn
IsActive
CreatedAt
UpdatedAt
```

Die Spalte `Authors` speichert `Book.AuthorsText`.

BookItems-Spalten:

```text
Id
InventoryNumber
Status
BookId
```

## Abhängigkeitsregeln

```text
BuildingBlocks hängt von keinem fachlichen Modul ab.
Readers hängt von BuildingBlocks ab.
Catalog hängt von BuildingBlocks ab.
Infrastructure hängt von BuildingBlocks, Readers und Catalog ab.
Web hängt von Readers, Catalog und BuildingBlocks ab.
Das ausführbare API-Projekt verdrahtet alle Projekte.
Tests dürfen alle benötigten Projekte referenzieren.
```
