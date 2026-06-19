# Architektur: CampusLibrary Teil 1 — Readers Monolith

Dieses Dokument beschreibt die Architektur von **Teil 1** der `CampusLibraryApi`.

Teil 1 implementiert das erste fachliche Modul `Readers` innerhalb eines einzigen ASP.NET Core Web API-Projekts. Die Anwendung bleibt bewusst ein **Ein-Projekt-Monolith**. Innerhalb dieses Monolithen wird der Code aber bereits nach klaren architektonischen Grenzen strukturiert.

Das bedeutet:

```text
eine deploybare Anwendung
ein Projekt
eine Datenbank
ein erstes fachliches Modul: Readers
```

Das Ziel von Teil 1 ist noch nicht die Aufteilung der Lösung in mehrere Projekte. Diese Aufteilung folgt in Teil 2. Teil 1 führt zunächst die interne Struktur ein, damit die spätere Projektaufteilung nachvollziehbar wird.

## Architektonisches Ziel

Die Architektur von Teil 1 soll in der Lehre folgende Konzepte sichtbar machen:

- wie ein Web API-Monolith intern strukturiert werden kann
- wie Web-, BuildingBlocks-, Core- und Infrastructure-Code getrennt werden
- wie ein erstes fachliches Modul modelliert wird
- wie schreibende Use Cases von lesenden ReadModels unterschieden werden
- wie Domainlogik aus Controllern herausgehalten wird
- wie DDD-Grundlagen wie Entity, Aggregate Root, Value Object und Domain Error verwendet werden
- wie EF Core als technische Persistenz eingesetzt wird
- wie Ports den Core von der Infrastructure entkoppeln
- wie Soft Delete als fachliche Deaktivierung modelliert wird
- wie die Codebasis auf eine spätere Modular-Monolith-Aufteilung vorbereitet wird

Teil 1 beantwortet damit diese Frage:

> Wie kann ein kleiner Web API-Monolith bereits sauber und modulorientiert strukturiert werden?

## Aktuelle Projektstruktur

Aktueller Stand mit dem ersten Modul `Readers`:

```text
CampusLibraryApi
├─ _0_Documentation
│  └─ part-1-readers-monolith
│     ├─ README.md
│     ├─ ARCHITECTURE.md
│     ├─ API.md
│     └─ TESTING.md
│
├─ _1_Web
│  └─ Controllers
│     └─ ReadersController.cs
│
├─ _2_BuildingBlocks
│  ├─ Result.cs
│  ├─ _1_Ports
│  │  ├─ IClock.cs
│  │  └─ IUnitOfWork.cs
│  └─ _3_Domain
│     ├─ Entities
│     │  ├─ Entity.cs
│     │  └─ AggregateRoot.cs
│     └─ Errors
│        └─ Error.cs
│
├─ _3_Core
│  └─ Readers
│     ├─ _1_Ports
│     │  ├─ IReaderRepository.cs
│     │  ├─ IReaderReadModel.cs
│     │  ├─ IReadersDbContext.cs
│     │  └─ IReaderUseCases.cs
│     │
│     ├─ _2_Application
│     │  ├─ Dtos
│     │  │  ├─ AddressDto.cs
│     │  │  ├─ ReaderCreateDto.cs
│     │  │  ├─ ReaderUpdateDto.cs
│     │  │  └─ ReaderDto.cs
│     │  ├─ Mappings
│     │  └─ UseCases
│     │     ├─ ReaderUcCreate.cs
│     │     ├─ ReaderUcUpdate.cs
│     │     ├─ ReaderUcDeactivate.cs
│     │     └─ ReaderUseCases.cs
│     │
│     └─ _3_Domain
│        ├─ Entities
│        │  └─ Reader.cs
│        ├─ ValueObjects
│        │  ├─ EmailVo.cs
│        │  └─ AddressVo.cs
│        └─ Errors
│           └─ ReaderErrors.cs
│
├─ _4_Infrastructure
│  └─ Persistence
│     ├─ Configurations
│     │  └─ ConfigReader.cs
│     ├─ Database
│     │  ├─ AppDbContext.cs
│     │  └─ UnitOfWorkEf.cs
│     ├─ ReadModels
│     │  └─ ReaderReadModelEf.cs
│     ├─ Repositories
│     │  └─ ReaderRepositoryEf.cs
│     └─ Seed.cs
│
├─ Configure
│  ├─ DiReaders.cs
│  ├─ DiInfrastructure.cs
│  └─ DiSwagger.cs
│
└─ Program.cs
```

## Warum dies noch ein Monolith ist

Teil 1 ist ein Monolith, weil der gesamte Anwendungscode in einem Projekt liegt:

```text
CampusLibraryApi
```

Es gibt noch keine separaten Projekte für:

```text
CampusLibrary.Api
CampusLibrary.Readers
CampusLibrary.Infrastructure
CampusLibrary.BuildingBlocks
```

Diese Projektaufteilung wird bewusst auf Teil 2 verschoben.

Trotzdem verwendet Teil 1 bereits eine klare interne Struktur:

```text
_1_Web
_2_BuildingBlocks
_3_Core
_4_Infrastructure
```

Dadurch wird der Übergang zu Teil 2 leichter. Studierende lernen zunächst die architektonischen Grenzen innerhalb eines Projekts. Später können dieselben Grenzen in separate Projekte verschoben werden.

## Das erste fachliche Modul: Readers

Das erste implementierte fachliche Modul ist `Readers`.

Das Modul `Readers` verwaltet das fachliche Konzept eines Bibliotheks-Readers. Ein Reader ist die fachliche Repräsentation einer Person, die die Bibliothek nutzen kann.

Das Modul enthält aktuell:

- `Reader` als Aggregate Root
- `EmailVo` als Value Object
- `AddressVo` als Value Object
- `ReaderErrors` als Domain Errors
- `ReaderCreateDto`
- `ReaderUpdateDto`
- `ReaderDto`
- `ReaderUcCreate` als schreibender Use Case
- `ReaderUcUpdate` als schreibender Use Case für partielle Änderungen
- `ReaderUcDeactivate` als schreibender Use Case für Soft Delete
- `ReaderUseCases` als Fassade für schreibende Use Cases
- `IReaderRepository` für die Write-Seite
- `IReaderReadModel` für die Read-Seite
- `IReadersDbContext` als eingeschränkter DbContext-Port
- `ReaderRepositoryEf` als EF-Core-Repository
- `ReaderReadModelEf` als EF-Core-ReadModel

Die aktuelle HTTP API unterstützt:

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

`DELETE /camplib/v1/readers/{id}` löst eine Deaktivierung aus. Der Reader wird nicht physisch aus der Datenbank entfernt.

## Reader als Aggregate Root

`Reader` ist die Aggregate Root des Readers-Moduls.

Das Aggregate schützt die Konsistenzregeln der Reader-Profildaten und des Reader-Lebenszyklus.

Das Aggregate wird über eine Factory-Methode erzeugt:

```text
Reader.Create(...)
```

Es wird über fachliche Methoden verändert, zum Beispiel:

```text
Reader.UpdateProfile(...)
Reader.Deactivate(...)
```

Dadurch werden unkontrollierte Änderungen über öffentliche Setter vermieden.

Die didaktische Regel lautet:

> Domänenzustand soll über ausdrückliche Domänenmethoden geändert werden, nicht durch Setzen von Properties von außen.

## Domain Errors

Domain Errors werden ausdrücklich modelliert.

Beispiele:

```text
ReaderErrors.InvalidEmail
ReaderErrors.EmailAlreadyInUse
ReaderErrors.ReaderNotFound
ReaderErrors.IsAlreadyDeactivated
```

Erwartbare fachliche Fehler werden über `Result` zurückgegeben, nicht als Exceptions geworfen.

Dadurch werden Erfolgs- und Fehlerpfade im Code sichtbar und gut testbar.

## Use Cases und ReadModels

Teil 1 trennt bewusst schreibende und lesende Zugriffe.

```text
Use Case  = schreibender Anwendungsablauf
ReadModel = lesende DB-zu-DTO-Projektion
```

Deshalb gilt:

```text
GET                  → ReadModel
POST / PUT / DELETE  → Use Case
```

GET-Anfragen sollen nicht versehentlich zu fachlichen Workflows werden. Sie fragen Daten ab und liefern DTOs.

Schreibende Anfragen müssen dagegen die fachliche Konsistenz schützen.

## Write-Seite

Schreibende Abläufe gehen über Use Cases.

```text
Controller
→ Use Case
→ Domain / Aggregate
→ Repository
→ EF Core
→ UnitOfWork
```

Beispiel für Create:

```text
POST /camplib/v1/readers
→ ReadersController
→ ReaderUseCases.CreateAsync
→ ReaderUcCreate
→ EmailVo.Create(...)
→ AddressVo.Create(...)
→ Reader.Create(...)
→ IReaderRepository
→ ReaderRepositoryEf
→ UnitOfWorkEf
```

Beispiel für Update:

```text
PUT /camplib/v1/readers/{id}
→ ReadersController
→ ReaderUseCases.UpdateAsync
→ ReaderUcUpdate
→ Reader.UpdateProfile(...)
→ IReaderRepository
→ ReaderRepositoryEf
→ UnitOfWorkEf
```

Beispiel für Deactivate:

```text
DELETE /camplib/v1/readers/{id}
→ ReadersController
→ ReaderUseCases.DeactivateAsync
→ ReaderUcDeactivate
→ Reader.Deactivate(...)
→ IReaderRepository
→ ReaderRepositoryEf
→ UnitOfWorkEf
```

## Soft Delete und `IsActive`

Der Reader-Lebenszyklus verwendet Soft Delete.

Ein Reader wird nicht physisch aus der Datenbank gelöscht. Stattdessen setzt `Reader.Deactivate(...)` die Eigenschaft `IsActive` auf `false`.

Das bedeutet:

```text
Deactivate = fachliche Operation
Inactive   = Zustand nach der Deaktivierung
DELETE     = HTTP-Verb zum Auslösen der Operation
```

Normale ReadModel-Abfragen liefern nur aktive Reader.

Spezielle ReadModel-Abfragen liefern auch inaktive Reader:

```text
FindByIdWithInactiveAsync
SelectAllWithInactiveAsync
```

So bleiben historische Informationen für spätere Module wie `Loans` erhalten, während normale Clients weiterhin eine saubere Sicht auf aktive Reader erhalten.

## Read-Seite

Lesende Abläufe gehen über ReadModels.

```text
Controller
→ ReadModel
→ DbContext
→ DTO
```

ReadModels verwenden typischerweise:

```csharp
.AsNoTracking()
```

Beispiel:

```text
GET /camplib/v1/readers
→ ReadersController
→ IReaderReadModel.SelectAllAsync
→ ReaderReadModelEf
→ AppDbContext
→ ReaderDto
```

Die Read-Seite lädt kein Aggregate, um eine DTO-Liste zurückzugeben. Sie projiziert Datenbankdaten direkt in DTOs.

Normale Read-Methoden liefern nur aktive Reader. Methoden mit `WithInactive` im Namen liefern aktive und inaktive Reader.

## Repository-Implementierung

Die Repository-Implementierung gehört zur Infrastructure.

Beispiel:

```text
ReaderRepositoryEf
```

Sie implementiert:

```text
IReaderRepository
```

Das Repository wird von schreibenden Use Cases verwendet.

Es arbeitet mit Aggregates und unterstützt Operationen wie:

- Reader hinzufügen
- Reader per Id finden
- Reader per E-Mail finden
- Subject-Eindeutigkeit prüfen

Das Repository entfernt Reader im normalen Lebenszyklus nicht mehr. Deaktivierung bedeutet, dass der Zustand des Aggregates geändert und über die UnitOfWork gespeichert wird.

## ReadModel-Implementierung

Die ReadModel-Implementierung gehört ebenfalls zur Infrastructure.

Beispiel:

```text
ReaderReadModelEf
```

Sie implementiert:

```text
IReaderReadModel
```

Das ReadModel wird von GET-Endpunkten verwendet.

Es liefert DTOs direkt zurück und enthält keine Domainlogik.

Es enthält getrennte Methoden für normale aktive Reader-Sichten und Sichten inklusive inaktiver Reader.

## Testarchitektur

Teil 1 enthält Tests auf mehreren Ebenen.

Typische Testgruppen sind:

```text
Domain tests
Use case tests with mocks
Use case integration tests
Repository integration tests
Read model integration tests
Controller / end-to-end tests
```

Die aktuelle Testsuite prüft:

```text
Reader-Domainverhalten
E-Mail- und Address-Validierung
Create-UseCase
Update-UseCase
Deactivate-UseCase
Soft-Delete-Verhalten über IsActive
Repository-Verhalten
ReadModel-Projektionen
HTTP-Controller-Verhalten
```

Der aktuelle bekannte Teststatus für Teil 1 ist:

```text
72 Tests
0 fehlgeschlagen
```

## Architekturegeln

1. Die Anwendung ist in Teil 1 ein Projekt.
2. Die interne Struktur folgt bereits architektonischen Grenzen.
3. Web übersetzt HTTP und enthält keine Domainlogik.
4. Core enthält Domain- und Application-Logik.
5. Domain kennt weder Web, Infrastructure, EF Core noch Swagger.
6. Use Cases schreiben Domänenzustand.
7. Deaktivierung ist ein schreibender Use Case und ändert Domänenzustand.
8. ReadModels lesen Daten direkt als DTO-Projektionen.
9. Normale ReadModel-Abfragen liefern nur aktive Reader.
10. `WithInactive`-ReadModel-Abfragen liefern aktive und inaktive Reader.
11. Repositories werden auf der Write-Seite verwendet.
12. ReadModels werden auf der Read-Seite verwendet.
13. Infrastructure implementiert Core-Ports.
14. EF-Core-Konfiguration gehört in die Infrastructure.
15. `Program.cs` verbindet Module, enthält aber keine Domainlogik.
16. Zusätzliche Module sollen dieselbe Struktur wie `Readers` verwenden.
17. AuthN/AuthZ wird später ergänzt, ohne die Grundstruktur zu verändern.

## Didaktischer Merksatz

> Use Cases schützen fachliche Regeln auf der Write-Seite. ReadModels liefern einfache DTOs auf der Read-Seite.

Kürzer:

```text
Use Cases write.
ReadModels read.
```

Für den aktuellen Reader-Lebenszyklus gilt zusätzlich:

```text
Deactivate ändert den Zustand.
ReadModels entscheiden über die Sichtbarkeit.
```
