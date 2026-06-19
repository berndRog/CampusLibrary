# Architektur: CampusLibrary Teil 2 — Readers Modular Monolith

Dieses Dokument beschreibt die Architektur von **Teil 2** der `CampusLibraryApi`.

Teil 2 setzt das `Readers`-Modul aus Teil 1 fort und verschiebt die internen Architekturgrenzen in separate Projekte. Die Anwendung bleibt weiterhin ein deploybarer modularer Monolith, aber Projektstruktur, Abhängigkeiten und Modulgrenzen werden deutlicher sichtbar.

```text
eine deploybare Anwendung
mehrere Projekte
eine Datenbank
ein erstes fachliches Modul: Readers
```

## Architektonisches Ziel

Teil 2 soll in der Lehre folgende Konzepte sichtbar machen:

- wie aus einem Ein-Projekt-Monolithen ein projektbasierter modularer Monolith wird
- wie Web, BuildingBlocks, Core, Infrastructure und Tests in Projekte getrennt werden
- wie das Domänenmodell unabhängig von technischer Infrastructure bleibt
- wie ein erstes fachliches Modul mit expliziten Ports modelliert wird
- wie schreibende Use Cases von lesenden ReadModels unterschieden werden
- wie Soft Delete über eine fachliche Domänenoperation modelliert wird
- wie die Codebasis auf weitere Module wie Catalog, Loans und AuthN/AuthZ vorbereitet wird

## Solution-Struktur

```text
CampusLibraryApi
CampusLibraryApi_1_Web
CampusLibraryApi_2_BuildingBlocks
CampusLibraryApi_3_Core_Readers
CampusLibraryApi_4_Infrastructure
CampusLibraryApiTest
```

Das wichtige Abhängigkeitsprinzip lautet:

```text
Core definiert Ports.
Infrastructure implementiert Ports.
Web ruft Ports auf.
Domain hängt nicht von Web oder Infrastructure ab.
```

## Projektverantwortlichkeiten

### CampusLibraryApi

Einstiegspunkt der Anwendung. Dieses Projekt konfiguriert und startet die Anwendung. Es enthält keine Domainlogik.

### CampusLibraryApi_1_Web

Enthält HTTP-Controller. `ReadersController` übersetzt HTTP-Anfragen in Aufrufe an `IReaderReadModel` und `IReaderUseCases`.

### CampusLibraryApi_2_BuildingBlocks

Enthält wiederverwendbare Bausteine wie `Result`, `DomainError`, `Entity`, `AggregateRoot`, `IClock` und `IUnitOfWork`.

### CampusLibraryApi_3_Core_Readers

Enthält den Core des Readers-Moduls:

```text
_1_Ports
_2_Application
_3_Domain
```

Der Core definiert Abstraktionen wie `IReaderRepository`, `IReaderReadModel`, `IReaderUseCases` und `IReaderDbContext`. Außerdem enthält er das Domänenmodell: `Reader`, `EmailVo`, `AddressVo` und `ReaderErrors`.

### CampusLibraryApi_4_Infrastructure

Enthält technische Implementierungen wie `AppDbContext`, `UnitOfWorkEf`, `ReaderRepositoryEf`, `ReaderReadModelEf`, EF-Core-Konfigurationen, Migrations und Seed-Daten.

### CampusLibraryApiTest

Enthält Tests für Domainverhalten, UseCase-Orchestrierung, Repository-Verhalten, ReadModel-Verhalten und Controller-/End-to-End-Verhalten.

## Das Readers-Modul

Das Modul enthält:

- `Reader` als Aggregate Root
- `EmailVo` und `AddressVo` als Value Objects
- `ReaderErrors` als Domain Errors
- `ReaderUcCreate`, `ReaderUcUpdate`, `ReaderUcDeactivate`
- `ReaderUseCases` als Write-Side-Fassade
- `IReaderRepository` für die Write-Seite
- `IReaderReadModel` für die Read-Seite
- `ReaderRepositoryEf` und `ReaderReadModelEf` als EF-Core-Implementierungen

## Aktuelle HTTP API

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

`Reader` schützt die Konsistenzregeln der Reader-Profildaten und des Reader-Lebenszyklus.

Das Aggregate wird über fachliche Methoden erzeugt und verändert:

```text
Reader.Create(...)
Reader.UpdateProfile(...)
Reader.Deactivate(...)
```

> Domänenzustand soll über ausdrückliche Domänenmethoden geändert werden, nicht durch Setzen von Properties von außen.

## Use Cases und ReadModels

```text
Use Case  = schreibender Anwendungsablauf
ReadModel = lesende DB-zu-DTO-Projektion
```

```text
GET                  -> ReadModel
POST / PUT / DELETE  -> Use Case
```

## Write-Seite

```text
Controller
→ Use Case
→ Domain / Aggregate
→ Repository
→ EF Core
→ UnitOfWork
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

Ein Reader wird nicht physisch aus der Datenbank gelöscht. Stattdessen setzt `Reader.Deactivate(...)` die Eigenschaft `IsActive` auf `false`.

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

ReadModels projizieren Datenbankdaten direkt in DTOs und verwenden typischerweise `.AsNoTracking()`.

Normale Read-Methoden liefern nur aktive Reader. Methoden mit `WithInactive` im Namen liefern aktive und inaktive Reader.

## Migrations

Teil 2 verwendet EF-Core-Migrations im Infrastructure-Projekt.

Da der Reader-Lebenszyklus jetzt `IsActive` enthält, müssen Datenbankmodell und Migrations diese Property abbilden.

> Wenn das Domänenmodell persistenten Zustand ändert, müssen Datenbankschema und Migrations angepasst werden.

## Testarchitektur

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

Aktueller bekannter Teststatus:

```text
72 Tests
0 fehlgeschlagen
```

## Architekturegeln

1. Die Anwendung ist weiterhin ein deploybarer Monolith.
2. Die Solution ist in separate Projekte aufgeteilt.
3. Web übersetzt HTTP und enthält keine Domainlogik.
4. Core enthält Domain- und Application-Logik.
5. Domain kennt weder Web, Infrastructure, EF Core noch Swagger.
6. Core definiert Ports.
7. Infrastructure implementiert Core-Ports.
8. Use Cases schreiben Domänenzustand.
9. Deaktivierung ist ein schreibender Use Case und ändert Domänenzustand.
10. Normale ReadModel-Abfragen liefern nur aktive Reader.
11. `WithInactive`-ReadModel-Abfragen liefern aktive und inaktive Reader.
12. EF-Core-Konfiguration und Migrations gehören in die Infrastructure.

## Didaktischer Merksatz

```text
Use Cases write.
ReadModels read.
Deactivate ändert den Zustand.
ReadModels entscheiden über die Sichtbarkeit.
```
