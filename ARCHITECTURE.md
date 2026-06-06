# Architektur: CampusLibrary

Dieses Dokument beschreibt die aktuelle Architektur der `CampusLibraryApi` und die Regeln für die Erweiterung um weitere fachliche Module.

Die Anwendung ist als **modularer Monolith** aufgebaut: Es gibt eine gemeinsame API-Anwendung, eine gemeinsame Infrastructure und eine gemeinsame Datenbank. Die Fachlichkeit wird jedoch in klar abgegrenzte Core-Module geschnitten.

## Ziel der Architektur

Die Architektur soll in der Lehre folgende Konzepte sichtbar machen:

- fachliche Module mit eigener Domain und Application-Schicht
- Unterschied zwischen schreibenden Use Cases und lesenden ReadModels
- Trennung von Core und Infrastructure
- DDD-Grundbegriffe wie Entity, Aggregate Root, Value Object und Domain Error
- EF Core als technische Persistenz in der Infrastructure
- modulbezogene Ports und interne Implementierungen
- spätere Erweiterbarkeit um AuthN/AuthZ und weitere Module

## Aktuelle Projektstruktur

Aktueller Stand mit dem ersten Modul `Readers`:

```text
CampusLibraryApi
├─ _1_Web
│  └─ Controllers
│     └─ ReadersController.cs
│
├─ _2_Shared
│  ├─ Result.cs
│  ├─ _1_Ports
│  │  └─ IUnitOfWork.cs
│  └─ _3_Domain
│     ├─ Entities
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
│     │  │  ├─ ReaderCreateDto.cs
│     │  │  └─ ReaderDto.cs
│     │  └─ UseCases
│     │     └─ ReaderUcCreate.cs
│     │
│     └─ _3_Domain
│        ├─ Entities
│        │  └─ Reader.cs
│        ├─ ValueObjects
│        │  └─ EmailVo.cs
│        └─ Errors
│           └─ ReaderErrors.cs
│
├─ _4_Infrastructure
│  └─ Persistence
│     ├─ Configurations
│     │  └─ ConfigReader.cs
│     ├─ Database
│     │  ├─ LibraryDbContext.cs
│     │  └─ UnitOfWorkEf.cs
│     ├─ ReadModels
│     │  └─ ReaderReadModelEf.cs
│     └─ Repositories
│        └─ ReaderRepositoryEf.cs
│
├─ Configure
│  └─ DiReaders.cs
│
└─ Program.cs
```

## Geplante fachliche Module

Die Vorlesungsdomäne wird fachlich in drei Module gegliedert:

```text
Readers
Catalog
Loans
```

### Readers

Das Modul `Readers` verwaltet den fachlichen Bibliotheksnutzer.

Aktuell enthält es:

- `Reader` als Aggregate Root
- `EmailVo` als Value Object
- `ReaderErrors` als fachliche Fehler
- `ReaderUcCreate` als schreibenden Use Case
- `IReaderRepository` für die Write-Seite
- `IReaderReadModel` für die Read-Seite
- `ReaderReadModelEf` als DB-zu-DTO-Projektion
- `ReaderRepositoryEf` als EF-Core-Repository

In einer späteren Ausbaustufe wird der manuelle Create-Use-Case durch tokenbasiertes Provisioning ergänzt:

```text
ReaderUcProvisionMe
```

Dann gilt:

```text
technischer Benutzer im IdentityServer ≠ fachlicher Reader in der CampusLibrary
```

### Catalog

Das Modul `Catalog` wird später Bücher und Autoren verwalten.

Geplante fachliche Bestandteile:

- `Book`
- `Author`
- `BookAuthor`
- `IsbnVo`
- `BookStatus`

Das Modul zeigt insbesondere die m:n-Beziehung:

```text
Book 1:n BookAuthor n:1 Author
```

### Loans

Das Modul `Loans` wird die Ausleihe als eigenes fachliches Objekt modellieren.

Geplante fachliche Beziehung:

```text
Reader 1:n Loan n:1 Book
```

Wichtig ist die fachliche Aussage:

> Eine Ausleihe ist nicht nur eine technische Verbindung zwischen Reader und Book. Sie ist ein eigenes fachliches Objekt mit eigenen Daten und Regeln.

## Schichten innerhalb eines Core-Moduls

Jedes Core-Modul folgt derselben inneren Struktur.

```text
_1_Ports
_2_Application
_3_Domain
```

### _3_Domain

Die Domain enthält die fachlichen Bausteine:

- Entities
- Aggregate Roots
- Value Objects
- Domain Errors
- fachliche Methoden und Regeln

Die Domain kennt keine EF-Core-Klassen, keine Controller und keine Datenbankdetails.

Beispiel:

```text
_3_Core/Readers/_3_Domain/Entities/Reader.cs
_3_Core/Readers/_3_Domain/ValueObjects/EmailVo.cs
_3_Core/Readers/_3_Domain/Errors/ReaderErrors.cs
```

### _2_Application

Die Application-Schicht koordiniert fachliche Abläufe.

Sie enthält:

- DTOs
- Use Cases
- Mapping-Hilfen, falls erforderlich

Use Cases verändern fachlichen Zustand und verwenden Ports, um mit der Außenwelt zu sprechen.

Beispiel:

```text
ReaderUcCreate
```

### _1_Ports

Ports sind Schnittstellen, die der Core benötigt.

Typische Ports sind:

- Repository-Interfaces für schreibende Zugriffe
- ReadModel-Interfaces für lesende Zugriffe
- DbContext-Interfaces zur begrenzten Infrastructure-Sicht
- spätere Gateways, z. B. `IIdentityGateway`

Beispiele:

```text
IReaderRepository
IReaderReadModel
IReadersDbContext
```

## Use Cases und ReadModels

In diesem Projekt werden die Begriffe bewusst eng verwendet.

```text
Use Case  = schreibender fachlicher Anwendungsfall
ReadModel = lesender Zugriff DB → DTO
```

Daraus folgt:

```text
GET              → ReadModel
POST / PUT / DELETE → Use Case
```

### Write-Seite

Schreibende Abläufe laufen über Use Cases:

```text
Controller
→ Use Case
→ Domain / Aggregate
→ Repository
→ EF Core
→ SaveChanges
```

Beispiel:

```text
POST /library/v1/readers
→ ReaderUcCreate
→ Reader.Create(...)
→ IReaderRepository
→ ReaderRepositoryEf
→ UnitOfWorkEf
```

### Read-Seite

Lesende Abläufe laufen über ReadModels:

```text
Controller
→ ReadModel
→ DbContext
→ DTO
```

ReadModels verwenden typischerweise:

```csharp
.AsNoTracking()
.Select(...)
```

Beispiel:

```text
GET /library/v1/readers
→ IReaderReadModel.SelectAllAsync
→ ReaderReadModelEf
→ DB → ReaderDto
```

## Infrastructure

Die Infrastructure enthält technische Implementierungen.

Dazu gehören:

- EF-Core-Konfigurationen
- DbContext
- Repositories
- ReadModels
- UnitOfWork
- später Security-Implementierungen

Die Infrastructure darf EF Core kennen. Der Core darf EF Core nicht kennen.

## Sichtbarkeit und `internal`

Konkrete Implementierungsklassen sollen `internal` sein.

Typische interne Klassen:

```text
ReaderRepositoryEf
ReaderReadModelEf
ConfigReader
UnitOfWorkEf
```

Öffentlich sichtbar bleiben nur die benötigten Ports, DTOs, Use Cases und DI-Erweiterungsmethoden.

Dadurch bleibt die öffentliche Oberfläche klein.

## Dependency Injection

Die DI-Registrierung ist der technische Verbindungspunkt zwischen Core-Port und Infrastructure-Implementierung.

Beispiel:

```text
IReaderRepository → ReaderRepositoryEf
IReaderReadModel  → ReaderReadModelEf
ReaderUcCreate
```

Die konkrete Implementierung bleibt `internal`. `Program.cs` soll möglichst nur die Modulregistrierung kennen, z. B.:

```csharp
builder.Services.AddReadersModule();
```

## DbContext-Zugriff

Es gibt eine gemeinsame technische Datenbank und einen gemeinsamen EF-Core-DbContext.

Der Zugriff soll fachlich begrenzt werden. Dafür definiert jedes Modul ein eigenes DbContext-Interface.

Beispiel:

```csharp
public interface IReadersDbContext {
   DbSet<Reader> Readers { get; }
   Task<int> SaveChangesAsync(CancellationToken ct);
}
```

`LibraryDbContext` implementiert dieses Interface.

So kann das Readers-Modul nur auf die Tabellen zugreifen, die es benötigt.

## Regeln für neue Module

Für jedes neue fachliche Modul gelten dieselben Regeln.

### Core

Ein neues Modul erhält einen eigenen Core-Bereich:

```text
_3_Core/<ModuleName>
├─ _1_Ports
├─ _2_Application
└─ _3_Domain
```

### Infrastructure

Die Implementierungen liegen in der Infrastructure. Aktuell ist Infrastructure technisch nach Repositories, ReadModels und Configurations gruppiert. Für größere Gruppen kann sie zusätzlich fachlich gruppiert werden, z. B.:

```text
_4_Infrastructure/Persistence/Readers
_4_Infrastructure/Persistence/Catalog
_4_Infrastructure/Persistence/Loans
```

Wichtig ist nicht der konkrete Ordnername, sondern die Regel:

> Die Infrastructure implementiert die Ports des jeweiligen Core-Moduls.

### Web

Controller liegen in `_1_Web/Controllers`.

Controller enthalten keine Fachlogik. Sie übersetzen HTTP in Aufrufe an Use Cases oder ReadModels.

## Architekturregeln

1. Core kennt keine Infrastructure.
2. Domain kennt keine Application, Infrastructure oder Web-Schicht.
3. Use Cases schreiben fachlichen Zustand.
4. ReadModels lesen Daten direkt als DTO-Projektion.
5. Controller enthalten keine Fachlogik.
6. Repository-Implementierungen sind technische Details und bleiben `internal`.
7. ReadModel-Implementierungen sind technische Details und bleiben `internal`.
8. EF-Core-Konfigurationen liegen in der Infrastructure.
9. Neue Module folgen der gleichen Struktur wie `Readers`.
10. AuthN/AuthZ wird später ergänzt, ohne die Grundstruktur der Module zu verändern.

## Didaktischer Merksatz

> Use Cases schützen fachliche Regeln auf der Write-Seite. ReadModels liefern einfache DTOs auf der Read-Seite.

Oder kurz:

```text
Use Cases schreiben.
ReadModels lesen.
```
