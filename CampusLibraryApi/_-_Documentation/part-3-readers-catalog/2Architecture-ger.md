# Architektur: CampusLibrary Teil 3 — Readers + Catalog Modular Monolith

Dieses Dokument beschreibt die Architektur von Teil 3 der `CampusLibraryApi`.

Die Anwendung ist ein projektbasierter modularer Monolith mit zwei fachlichen Modulen: Readers und Catalog. Sie wird als eine ASP.NET-Core-Anwendung deployt und verwendet eine Datenbank.

Finales automatisiertes Testergebnis:

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

## Abhängigkeitsrichtung

```text
Web/API
  -> Core-Module
  -> BuildingBlocks

Infrastructure
  -> Core-Module
  -> BuildingBlocks

Core-Module
  -> BuildingBlocks
```

Core-Module referenzieren weder Web noch Infrastructure.

## Ports und Adapter

Die Core-Module definieren Ports. Infrastructure implementiert Outbound Ports.

Beispiele:

```text
IReaderRepository    -> ReaderRepositoryEf
IReaderReadModel     -> ReaderReadModelEf
IBookRepository      -> BookRepositoryEf
IBookReadModel       -> BookReadModelEf
IReaderDbContext     -> AppDbContext
ICatalogDbContext    -> AppDbContext
```

Controller rufen Use-Case-Fassaden oder ReadModels auf. Sie greifen nicht direkt auf EF Core zu.

## Write Model und Read Model

Das Projekt trennt bewusst schreibendes Verhalten und lesende Projektionen.

Schreibseite:

```text
Controller -> UseCase -> Repository -> Aggregate -> UnitOfWork
```

Leseseite:

```text
Controller -> ReadModel -> DTO-Projektion
```

Repositories geben Aggregate zurück. ReadModels geben DTOs zurück.

## Readers-Modul

Readers besitzt das Reader-Aggregate und die zugehörigen Value Objects.

Reader verwendet `IsActive`, um Deaktivierung zu unterstützen. Normale ReadModel-Abfragen liefern nur aktive Reader. Zusätzliche `with-inactive`-Abfragen beziehen inaktive Reader mit ein.

Typische Use Cases:

```text
ReaderUcCreate
ReaderUcUpdate
ReaderUcDeactivate
```

## Catalog-Modul

Catalog besitzt Books und BookItems.

Book ist ein Aggregate Root. BookItem ist eine Entity innerhalb des Book-Aggregates.

Wichtige Modellierungsentscheidungen:

```text
Book verwendet IsActive.
BookItem verwendet BookItemStatus.
Es gibt kein Author-Aggregate.
Autoren werden als Book.AuthorsText abgebildet.
```

Damit bleibt Teil 3 auf ein Aggregate mit einer 1:n-Kind-Entity fokussiert.

## IsActive versus Status

Das Projekt verwendet zwei unterschiedliche Modellierungskonzepte:

```text
Reader / Book:
- IsActive
- Deaktivierung blendet Datensätze aus normalen ReadModels aus

BookItem:
- Status
- beschreibt den Zustand eines physischen Exemplars
```

Diese Unterscheidung wird wichtig, wenn in Teil 4 Loans eingeführt werden.

## Infrastructure

Infrastructure implementiert Persistenz und technische Adapter.

Sie enthält:

* EF-Core-Konfigurationen
* `AppDbContext`
* Repositories
* ReadModels
* Unit of Work
* Clock-Implementierung
* Seed-Daten

Alle Module verwenden dieselbe Datenbank. Die fachliche Besitzregel wird aber über Ports und Codegrenzen ausgedrückt.

## Composition Root

Das ausführbare Projekt `CampusLibraryApi` verdrahtet alles.

Es registriert:

* Controller
* API-Versionierung
* Swagger/OpenAPI
* Core-Module
* Infrastructure
* EF Core und SQLite

## Didaktischer Fokus von Teil 3

Teil 3 zeigt, wie ein zweites Modul in den modularen Monolithen eingeführt wird, ohne verteilte Systemkomplexität zu erzeugen.

Studierende sehen:

* zwei unabhängige Core-Module
* eine gemeinsame Runtime
* eine gemeinsame Datenbank
* klare Modulverantwortung
* Aggregate-Grenzen
* Trennung von Lesen und Schreiben
* API- und Integrationstests
