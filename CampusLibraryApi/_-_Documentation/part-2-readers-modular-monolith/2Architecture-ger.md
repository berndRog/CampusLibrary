# Architektur: CampusLibrary Teil 2 — Readers Modular Monolith

Dieses Dokument beschreibt die Architektur von Teil 2 des Lehrprojekts `CampusLibraryApi`.

Teil 2 überführt die Readers-Anwendung aus Teil 1 in einen **projektbasierten modularen Monolithen**. Die Anwendung wird weiterhin als eine Anwendung deployt und verwendet eine Datenbank. Die architektonischen Bereiche sind nun aber in getrennte Projekte aufgeteilt.

Der fachliche Umfang bleibt bewusst auf ein Modul begrenzt:

```text
Readers
```

Das Ziel von Teil 2 ist nicht, neue Fachlichkeit hinzuzufügen. Das Ziel ist, Architekturgrenzen über Projektverweise sichtbar zu machen und die Lösung auf spätere Module wie Catalog und Loans vorzubereiten.

Der aktuelle Teststand lautet:

```text
Test summary: total: 70, failed: 0, succeeded: 70, skipped: 0
```

## Architektonisches Ziel

Teil 2 zeigt für die Lehre folgende Punkte:

- wie ein strukturierter Ein-Projekt-Monolith in einen modularen Monolithen überführt wird
- wie Projekte als Architekturgrenzen genutzt werden
- wie Abhängigkeitsrichtungen durch Projektverweise sichtbar werden
- wie das Domain Model unabhängig von EF Core und ASP.NET Core bleibt
- wie Command-UseCases und Query-ReadModels getrennt werden können
- wie Tests Verhalten während eines strukturellen Refactorings absichern
- wie eine Lösung auf zusätzliche Module vorbereitet wird, ohne diese schon einzuführen

Die zentrale Frage dieses Teils lautet:

```text
Wie kann ein sauber strukturierter Ein-Projekt-Monolith
in einen projektbasierten modularen Monolithen überführt werden,
ohne das sichtbare fachliche Verhalten zu verändern?
```

## Aktuelle Projektstruktur

```text
CampusLibraryApi
├─ Program.cs
├─ appsettings.json
└─ Configure

CampusLibraryApi_1_Web
└─ _1_Web
   └─ Controllers
      └─ ReadersController.cs

CampusLibraryApi_2_BuildingBlocks
└─ _2_BuildingBlocks
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
└─ _3_Core
   └─ Readers
      ├─ DiReaderModule.cs
      ├─ DiReaders.cs
      ├─ _1_Ports
      │  ├─ Inbound
      │  │  └─ IReaderUseCases.cs
      │  └─ Outbound
      │     ├─ IReaderDbContext.cs
      │     ├─ IReaderReadModel.cs
      │     └─ IReaderRepository.cs
      ├─ _2_Application
      │  ├─ Dtos
      │  ├─ Mappings
      │  └─ UseCases
      │     ├─ ReaderUcCreate.cs
      │     ├─ ReaderUcUpdate.cs
      │     ├─ ReaderUcDeactivate.cs
      │     └─ ReaderUseCases.cs
      └─ _3_Domain
         ├─ Entities
         │  └─ Reader.cs
         ├─ Errors
         │  └─ ReaderErrors.cs
         └─ ValueObjects
            ├─ AddressVo.cs
            └─ EmailVo.cs

CampusLibraryApi_4_Infrastructure
└─ _4_Infrastructure
   └─ Persistence
      ├─ Converters
      │  └─ UtcDateTimeConverter.cs
      ├─ Database
      │  ├─ AppDbContext.cs
      │  └─ UnitOfWorkEf.cs
      └─ Readers
         ├─ ConfigReader.cs
         ├─ ReaderDbContextEf.cs
         ├─ ReaderReadModelEf.cs
         └─ ReaderRepositoryEf.cs

CampusLibraryApiTest
└─ Tests für Domain, Application, Infrastructure und API
```

## Warum dies ein modularer Monolith ist

Die Anwendung ist weiterhin ein Monolith, weil sie als eine Anwendung deployt wird:

```text
eine deploybare Anwendung
ein Prozess
eine Datenbank
```

Sie ist modular, weil der Code in Projekte mit expliziten Abhängigkeitsregeln aufgeteilt ist:

```text
Web/API
BuildingBlocks
Core_Readers
Infrastructure
Tests
```

Der wichtige Unterschied zu Teil 1 lautet:

```text
Teil 1: Architekturgrenzen werden durch Ordner dargestellt.
Teil 2: Architekturgrenzen werden durch Projekte dargestellt.
```

Dadurch werden ungewollte Abhängigkeiten schwerer einzuführen.

## Verantwortlichkeiten der Projekte

### CampusLibraryApi

`CampusLibraryApi` ist das ausführbare Anwendungsprojekt und bildet den Composition Root.

Es ist verantwortlich für:

- Hosting der ASP.NET-Core-Anwendung
- Laden der Konfiguration
- Middleware-Konfiguration
- API-Versionierung und Swagger
- Zusammensetzen von Web, Core und Infrastructure

### CampusLibraryApi_1_Web

`CampusLibraryApi_1_Web` enthält die HTTP-API-Schicht.

In Teil 2 enthält sie den `ReadersController`.

Der Controller ist verantwortlich für:

- Routing
- Model Binding
- HTTP-Statuscodes
- ProblemDetails-Mapping
- Aufruf von UseCases für Commands
- Aufruf von ReadModels für Queries

Der Controller enthält keine fachlichen Regeln.

### CampusLibraryApi_2_BuildingBlocks

`CampusLibraryApi_2_BuildingBlocks` enthält gemeinsame Abstraktionen, die unabhängig von einem konkreten Modul sind.

Beispiele:

```text
Result<T>
IClock
IUnitOfWork
Entity
AggregateRoot
Domain Errors
```

BuildingBlocks sollen klein und stabil bleiben. Dort gehören nur Konzepte hinein, die wirklich modulübergreifend benötigt werden.

### CampusLibraryApi_3_Core_Readers

`CampusLibraryApi_3_Core_Readers` enthält das Readers-Modul.

Es besitzt das Reader-Domain-Model und definiert die Ports, die das Modul benötigt.

Das Modul enthält:

- Reader-Aggregate
- Email- und Address-Value-Objects
- Reader-DTOs
- Mappings
- Command-UseCases
- Inbound-Port `IReaderUseCases`
- Outbound-Ports `IReaderRepository`, `IReaderReadModel`, `IReaderDbContext`

Das Core-Projekt hängt nicht von Infrastructure oder Web ab.

### CampusLibraryApi_4_Infrastructure

`CampusLibraryApi_4_Infrastructure` implementiert technische Details.

Für das Readers-Modul stellt es bereit:

- EF-Core-Konfiguration für Reader
- EF-Core-DbContext-Integration
- Reader-Repository-Implementierung
- Reader-ReadModel-Implementierung
- UnitOfWork-Implementierung
- UTC-DateTime-Konvertierung

Infrastructure hängt von Core ab, weil es die dort definierten Outbound Ports implementiert.

## Abhängigkeitsrichtung

Die beabsichtigte Abhängigkeitsrichtung lautet:

```text
CampusLibraryApi
   ├─ CampusLibraryApi_1_Web
   ├─ CampusLibraryApi_2_BuildingBlocks
   ├─ CampusLibraryApi_3_Core_Readers
   └─ CampusLibraryApi_4_Infrastructure

CampusLibraryApi_1_Web
   ├─ CampusLibraryApi_2_BuildingBlocks
   └─ CampusLibraryApi_3_Core_Readers

CampusLibraryApi_3_Core_Readers
   └─ CampusLibraryApi_2_BuildingBlocks

CampusLibraryApi_4_Infrastructure
   ├─ CampusLibraryApi_2_BuildingBlocks
   └─ CampusLibraryApi_3_Core_Readers
```

Die wichtigste Regel lautet:

```text
Core definiert Ports.
Infrastructure implementiert Ports.
Web ruft die öffentlichen Application- und ReadModel-Ports auf.
```

## Reader-Domain-Model

`Reader` ist ein Aggregate Root.

Ein Reader repräsentiert einen fachlichen Bibliotheksnutzer, nicht einen technischen Benutzeraccount.

Der Reader speichert Profildaten und referenziert die technische Identität über einen Subject-Wert.

Ein Reader besitzt ein `IsActive`-Flag.

Dieses wird für fachliches Deaktivieren verwendet:

```text
aktiver Reader       -> erscheint in normalen Leseabfragen
deaktivierter Reader -> bleibt gespeichert, wird aber in normalen Leseabfragen ausgeblendet
```

Reader werden in Teil 2 nicht physisch gelöscht.

## Deactivate statt Delete

Das frühere Delete-Verhalten wurde durch ein fachliches Deactivate-Verhalten ersetzt.

Das ist didaktisch wichtig, weil dadurch HTTP-Verb und fachliche Bedeutung getrennt werden:

```text
HTTP DELETE /readers/{id}
```

kann weiterhin als öffentliche API-Operation verwendet werden, ruft intern aber auf:

```text
ReaderUcDeactivate
Reader.Deactivate(...)
```

Der Datenbanksatz bleibt erhalten. Das ist realistischer für Nachvollziehbarkeit, spätere Referenzen und fachliche Historie.

## UseCases und ReadModels

Teil 2 trennt Command-Verhalten von Query-Verhalten.

Command-UseCases:

```text
ReaderUcCreate
ReaderUcUpdate
ReaderUcDeactivate
ReaderUseCases
```

ReadModels:

```text
ReaderReadModelEf
IReaderReadModel
```

Die Regel lautet:

```text
UseCases ändern Zustand.
ReadModels beantworten Abfragen.
```

`IReaderUseCases` ist daher ein Inbound-Command-Port.

`IReaderReadModel` ist ein Outbound-Query-Port, der von Infrastructure implementiert wird.

## Repository und ReadModel

Das Repository arbeitet mit Aggregates und gehört zur Schreibseite:

```text
ReaderRepositoryEf -> Reader-Aggregate
```

Das ReadModel liefert DTOs und gehört zur Leseseite:

```text
ReaderReadModelEf -> ReaderDto
```

Dadurch bleiben Schreibverhalten und Leseprojektionen getrennt.

## Persistenz

Teil 2 verwendet EF Core und SQLite.

Die Reader-Tabelle speichert den Zustand des Reader-Aggregates, unter anderem:

- Id
- Subject
- Firstname
- Lastname
- Email
- Address-Daten
- IsActive
- CreatedAt
- UpdatedAt

UTC-DateTime-Behandlung wird zentral über `UtcDateTimeConverter` umgesetzt.

## Was später kommt

Teil 2 führt bewusst noch keine weiteren Module ein.

Spätere Teile ergänzen:

```text
Teil 3: Catalog mit Book und BookItem
Teil 4: Loans und modulübergreifende Zusammenarbeit
Teil 5: Authentication und Authorization
```

Teil 2 bereitet die Architektur auf diese Entwicklung vor, ohne diese Themen schon zu vermischen.
