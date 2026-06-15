# Architektur: CampusLibrary Teil 2 — Readers Modular Monolith

Dieses Dokument beschreibt die Architektur von Teil 2 der CampusLibraryApi.

Teil 2 überführt den abgeschlossenen Readers-Monolithen aus Teil 1 in einen projektbasierten modularen Monolithen. Der fachliche Funktionsumfang bleibt gleich: Die Anwendung enthält weiterhin nur das Readers-Modul. Das Hauptziel dieses Teils ist nicht, neue Fachlichkeit hinzuzufügen, sondern stärkere architektonische Grenzen durch getrennte Projekte sichtbar zu machen.

Teil 1 verwendete bereits eine saubere interne Struktur innerhalb eines Projekts:

```text
_1_Web
_2_BuildingBlocks
_3_Core
_4_Infrastructure
```

Teil 2 verschiebt diese architektonischen Bereiche in getrennte Projekte.

Das bedeutet:

* eine deploybare Anwendung
* mehrere Projekte
* eine Datenbank
* ein erstes fachliches Modul: Readers
* stärkere technische Grenzen durch Projektverweise
* unverändertes fachliches Verhalten
* bestehende Tests bleiben grün

Der aktuelle Teststand lautet:

```text
66 Tests
0 failed
```

## Architektonisches Ziel

Die Architektur von Teil 2 soll für die Lehre folgende Konzepte sichtbar machen:

* wie ein strukturierter Monolith in einen modularen Monolithen überführt wird
* wie Projekte als architektonische Grenzen genutzt werden
* wie Web/API, BuildingBlocks, Core-Modul, Infrastructure und Tests getrennt werden
* wie das Domain Model unabhängig von technischen Persistenzdetails bleibt
* wie Abhängigkeitsregeln durch Projektverweise technisch sichtbar werden
* wie bestehendes Verhalten während eines Architektur-Refactorings stabil bleibt
* wie Tests als Sicherheitsnetz für strukturelle Änderungen dienen
* wie die Lösung auf zukünftige Module wie Catalog und Loans vorbereitet wird

Teil 2 beantwortet damit diese Frage:

```text
Wie kann ein sauber strukturierter Ein-Projekt-Monolith
in einen projektbasierten modularen Monolithen überführt werden,
ohne sein fachliches Verhalten zu verändern?
```

## Aktuelle Projektstruktur

Aktueller Stand mit dem ersten Modul Readers:

```text
CampusLibraryApi
├─ Program.cs
├─ appsettings.json
└─ Configure
   ├─ DiSwagger.cs
   └─ weitere anwendungsweite Registrierungen

CampusLibraryApi_1_Web
└─ Controllers
   └─ ReadersController.cs

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
      └─ Error.cs

CampusLibraryApi_3_Core_Readers
├─ _1_Ports
│  ├─ IReaderRepository.cs
│  ├─ IReaderReadModel.cs
│  ├─ IReadersDbContext.cs
│  └─ IReaderUseCases.cs
│
├─ _2_Application
│  ├─ Dtos
│  │  ├─ AddressDto.cs
│  │  ├─ ReaderCreateDto.cs
│  │  ├─ ReaderUpdateDto.cs
│  │  └─ ReaderDto.cs
│  ├─ Mappings
│  └─ UseCases
│     ├─ ReaderUcCreate.cs
│     ├─ ReaderUcUpdate.cs
│     ├─ ReaderUcDelete.cs
│     └─ ReaderUseCases.cs
│
└─ _3_Domain
   ├─ Entities
   │  └─ Reader.cs
   ├─ ValueObjects
   │  ├─ EmailVo.cs
   │  └─ AddressVo.cs
   └─ Errors
      └─ ReaderErrors.cs

CampusLibraryApi_4_Infrastructure
└─ Persistence
   ├─ Configurations
   │  └─ ConfigReader.cs
   ├─ Database
   │  ├─ AppDbContext.cs
   │  └─ UnitOfWorkEf.cs
   ├─ ReadModels
   │  └─ ReaderReadModelEf.cs
   ├─ Repositories
   │  └─ ReaderRepositoryEf.cs
   └─ Seed.cs

CampusLibraryApiTest
└─ Tests für Domain, Value Objects, UseCases, Repositories, ReadModels
   und Controller-/End-to-End-Szenarien
```

## Warum dies ein modularer Monolith ist

Teil 2 ist weiterhin ein Monolith, weil die Anwendung als eine Anwendung deployt wird.

Es gibt weiterhin:

```text
eine deploybare Anwendung
eine Datenbank
einen Runtime-Prozess
```

Die Anwendung ist aber nun modular, weil die Lösung in getrennte Projekte mit expliziten Abhängigkeitsregeln aufgeteilt ist.

Der wichtige Unterschied zu Teil 1 lautet:

```text
Teil 1: Architekturgrenzen werden durch Ordner dargestellt.
Teil 2: Architekturgrenzen werden durch Projekte dargestellt.
```

Dadurch wird die Architektur expliziter und schwerer versehentlich zu verletzen.

## Verantwortlichkeiten der Projekte

Teil 2 verwendet folgende Hauptprojekte:

```text
CampusLibraryApi
CampusLibraryApi_1_Web
CampusLibraryApi_2_BuildingBlocks
CampusLibraryApi_3_Core_Readers
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

Dieses Projekt darf alle anderen Produktionsprojekte referenzieren, weil es für das Zusammensetzen der laufenden Anwendung verantwortlich ist.

Es darf keine Domainlogik enthalten.

## CampusLibraryApi_1_Web

`CampusLibraryApi_1_Web` enthält die HTTP-API-Oberfläche.

In Teil 2 ist das hauptsächlich:

```text
ReadersController
```

Das Web-Projekt ist dafür verantwortlich, HTTP Requests in Application-Aufrufe zu übersetzen.

Typische Aufgaben sind:

* Routen definieren
* DTOs entgegennehmen
* ReadModels für GET Requests aufrufen
* UseCases für schreibende Requests aufrufen
* Result-Fehler in HTTP-Antworten übersetzen
* DTOs oder ProblemDetails zurückgeben

Das Web-Projekt enthält keine Fachregeln.

Der Controller entscheidet zum Beispiel nicht, ob eine Email-Adresse gültig ist. Das gehört in das Readers Domain Model.

## CampusLibraryApi_2_BuildingBlocks

`CampusLibraryApi_2_BuildingBlocks` enthält wiederverwendbare architektonische Bausteine.

Typische Inhalte sind:

* Result
* Error
* Entity
* AggregateRoot
* IClock
* IUnitOfWork

Diese Typen sind nicht spezifisch für Readers.

Sie sind wiederverwendbare Konzepte für alle aktuellen und zukünftigen Module.

Die wichtige Regel lautet:

```text
BuildingBlocks dürfen nicht von einem konkreten Fachmodul abhängen.
```

BuildingBlocks sind allgemeine architektonische Elemente. Sie sind nicht der Ort für reader-spezifische, catalog-spezifische oder loan-spezifische Fachlogik.

## CampusLibraryApi_3_Core_Readers

`CampusLibraryApi_3_Core_Readers` ist das erste fachliche Modul.

Es enthält das readerspezifische Domain Model, Application UseCases, DTOs, Mappings und Ports.

Das Readers-Modul ist intern gegliedert in:

```text
_1_Ports
_2_Application
_3_Domain
```

Diese Struktur bleibt dieselbe wie in Teil 1, lebt jetzt aber in einem eigenen Projekt.

Die wichtige Regel lautet:

```text
Das Readers Core Modul hängt nicht von Web oder Infrastructure ab.
```

Dadurch bleibt das fachliche Modul unabhängig von HTTP, EF Core, SQLite und anderen technischen Details.

## Readers Domain

Der Domain-Bereich des Readers-Moduls enthält:

* Reader
* EmailVo
* AddressVo
* ReaderErrors

Die Domain-Schicht enthält Fachregeln und fachliche Validierung.

Sie kennt nicht:

* Controller
* EF Core
* HTTP
* Swagger
* Datenbankdetails
* Dependency Injection

Das Domain Model soll verständlich sein, ohne wissen zu müssen, wie Daten gespeichert oder HTTP Requests empfangen werden.

## Reader als Aggregate Root

`Reader` ist der Aggregate Root des Readers-Moduls.

Er besitzt die Konsistenzregeln für Reader-Profildaten.

Das Aggregate wird über eine Factory-Methode erzeugt:

```csharp
Reader.Create(...)
```

Es wird über Domain-Methoden geändert, zum Beispiel:

```csharp
Reader.UpdateProfile(...)
```

Dadurch werden unkontrollierte Änderungen über öffentliche Setter vermieden.

Die didaktische Regel lautet:

```text
Domain-Zustand sollte über explizite Domain-Methoden verändert werden,
nicht durch Setzen von Properties von außen.
```

## Value Objects

Das Readers-Modul verwendet aktuell zwei Value Objects:

* EmailVo
* AddressVo

Value Objects kapseln Validierungs- und Normalisierungsregeln.

`EmailVo` ist zum Beispiel dafür verantwortlich, eine Email-Adresse zu prüfen und zu normalisieren.

Ziel ist, Validierungslogik nicht über Controller, UseCases und Repositories zu verteilen.

## Domain Errors

Domain Errors werden explizit modelliert.

Beispiele:

```text
ReaderErrors.InvalidEmail
ReaderErrors.EmailAlreadyInUse
ReaderErrors.ReaderNotFound
```

Erwartete fachliche Fehler werden über `Result` zurückgegeben und nicht als Exceptions geworfen.

Dadurch werden Erfolgs- und Fehlerpfade im Code sichtbar und einfach testbar.

## Readers Application Layer

Der Application-Bereich des Readers-Moduls koordiniert UseCases.

Er enthält:

* DTOs
* UseCases
* Mapping-Helfer
* UseCase-Fassade

Beispiele:

* ReaderUcCreate
* ReaderUcUpdate
* ReaderUcDelete
* ReaderUseCases

UseCases sind für Workflows verantwortlich.

Typische Aufgaben eines UseCases sind:

* grundlegende Eingaben prüfen
* Aggregates laden
* Value Objects erzeugen
* Eindeutigkeitsregeln über Repositories prüfen
* Domain-Methoden aufrufen
* Änderungen über IUnitOfWork speichern
* DTOs zurückgeben

UseCases sollten keine detaillierten Domain-Regeln enthalten, wenn diese Regeln in das Domain Model gehören.

## Readers Ports

Ports sind Interfaces, die vom Readers Core Modul benötigt werden.

Das Readers-Modul definiert aktuell:

* IReaderRepository
* IReaderReadModel
* IReadersDbContext
* IReaderUseCases

Ports erlauben dem Core-Modul, von Abstraktionen statt von konkreter Infrastructure abzuhängen.

Das Core-Modul kann sagen:

```text
Ich benötige ein Reader Repository.
```

Es muss aber nicht wissen:

```text
Dieses Repository wird mit EF Core und SQLite implementiert.
```

Dieses Wissen gehört in die Infrastructure.

## CampusLibraryApi_4_Infrastructure

`CampusLibraryApi_4_Infrastructure` enthält technische Implementierungen.

Dazu gehören:

* EF-Core-Konfigurationen
* AppDbContext
* Repositories
* ReadModels
* UnitOfWorkEf
* Seed-Daten
* später Security oder externe Systemimplementierungen

Das Infrastructure-Projekt darf EF Core kennen.

Das Core-Modul darf EF Core nicht kennen.

Die Abhängigkeitsrichtung ist entscheidend:

```text
Core definiert Ports.
Infrastructure implementiert Ports.
```

## Repository-Implementierung

Die Repository-Implementierung gehört in die Infrastructure.

Beispiel:

```text
ReaderRepositoryEf
```

Sie implementiert:

```text
IReaderRepository
```

Das Repository wird von schreibenden UseCases verwendet.

Es arbeitet mit Aggregates und unterstützt Operationen wie:

* Reader hinzufügen
* Reader anhand der Id laden
* Reader anhand der Email laden
* Subject-Eindeutigkeit prüfen
* Reader entfernen

## ReadModel-Implementierung

Die ReadModel-Implementierung gehört ebenfalls in die Infrastructure.

Beispiel:

```text
ReaderReadModelEf
```

Sie implementiert:

```text
IReaderReadModel
```

Das ReadModel wird von GET-Endpunkten verwendet.

Es gibt direkt DTOs zurück und sollte kein Domain-Verhalten enthalten.

ReadModels verwenden typischerweise:

```csharp
.AsNoTracking()
.Select(...)
```

Die Leseseite lädt nicht das Aggregate, um eine Liste von DTOs zurückzugeben. Sie projiziert Datenbankdaten direkt in DTOs.

Dadurch bleiben Leseoperationen einfach und effizient.

## DbContext-Zugriff

Es gibt eine gemeinsame technische Datenbank und einen gemeinsamen EF-Core-DbContext.

Um den Modulzugriff einzuschränken, definiert das Readers-Modul seinen eigenen DbContext-Port:

```csharp
public interface IReadersDbContext {
   DbSet<Reader> Readers { get; }
   Task<int> SaveChangesAsync(CancellationToken ct);
}
```

`AppDbContext` implementiert dieses Interface.

Dadurch hängt das Readers-Modul nur von dem Teil des DbContext ab, den es benötigt.

Die didaktische Idee lautet:

```text
Auch mit einem physischen DbContext können Module ihre eigene logische Sicht
auf die Datenbank definieren.
```

## Abhängigkeitsregeln

Die wichtigsten Projektabhängigkeitsregeln lauten:

```text
BuildingBlocks hängt von keinem Fachmodul ab.

Readers hängt von BuildingBlocks ab.

Infrastructure hängt von BuildingBlocks und Readers ab.

Web hängt von Readers und BuildingBlocks ab.

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
```

Die Web/API-Seite ruft das Readers-Modul über Ports und UseCases auf.

Die Infrastructure-Seite implementiert Outbound Ports, die vom Readers-Modul definiert werden.

Das Readers-Modul selbst bleibt unabhängig von Web und Infrastructure.

## UseCases und ReadModels

Teil 2 behält die Schreib-/Lese-Trennung aus Teil 1 bei.

```text
UseCase   = schreibender Application Workflow
ReadModel = lesende DB-zu-DTO-Projektion
```

Daher gilt:

```text
GET                  → ReadModel
POST / PUT / DELETE  → UseCase
```

Diese Unterscheidung ist für die Lehre wichtig.

GET Requests sollen nicht versehentlich zu Domain Workflows werden. Sie fragen Daten ab und geben DTOs zurück.

Schreibende Requests müssen dagegen fachliche Konsistenz schützen.

## Schreibseite

Schreibende Workflows laufen über UseCases.

```text
Controller
→ UseCase
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

Beispiel für Delete:

```text
DELETE /camplib/v1/readers/{id}
→ ReadersController
→ ReaderUseCases.DeleteAsync
→ ReaderUcDelete
→ IReaderRepository
→ ReaderRepositoryEf
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

Beispiel:

```text
GET /camplib/v1/readers
→ ReadersController
→ IReaderReadModel.SelectAllAsync
→ ReaderReadModelEf
→ AppDbContext
→ ReaderDto
```

Die Leseseite lädt nicht das Aggregate. Sie projiziert Datenbankdaten direkt in DTOs.

## Partielle Updates

Das `ReaderUpdateDto` ist bewusst nullable:

```csharp
public sealed record ReaderUpdateDto(
   string? Lastname,
   string? Email,
   AddressDto? AddressDto
);
```

Das ist technisch und konzeptionell für partielle Updates erforderlich.

Die Bedeutung lautet:

```text
Lastname = null   → aktuellen Nachnamen beibehalten
Email = null      → aktuelle Email-Adresse beibehalten
AddressDto = null → aktuelle Adresse beibehalten
```

`Firstname` ist bewusst nicht Teil des Update DTOs. Es wird durch den aktuellen Update-UseCase nicht geändert.

Die Domain validiert weiterhin Werte, wenn sie angegeben werden:

```text
null          → keine Änderung
"" / "   "    → ungültiger Wert, wenn das Feld angegeben wird
"Meier"      → gültige Änderung
```

`null` bedeutet in diesem DTO also nicht "ungültig", sondern "keine Änderung".

## Create DTO und optionale Id

Das `ReaderCreateDto` enthält eine optionale Id:

```csharp
public sealed record ReaderCreateDto(
   string Firstname,
   string Lastname,
   string Email,
   AddressDto AddressDto,
   string Subject,
   string? Id
);
```

`Id` ist bewusst nullable.

In normaler API-Nutzung kann die Id weggelassen werden. In diesem Fall erzeugt die Anwendung eine neue Id.

Für Lehre, Seed-Daten oder Tests kann die Id angegeben werden.

Daher ist `Id` sowohl technisch als auch konzeptionell optional.

Das unterscheidet sich von den erforderlichen fachlichen Daten:

* Firstname
* Lastname
* Email
* Address
* Subject

Diese Felder sind notwendig, um einen gültigen Reader zu erzeugen.

## Sichtbarkeit und internal

Konkrete Infrastructure-Klassen sollten möglichst `internal` sein.

Typische interne Klassen sind:

* ReaderRepositoryEf
* ReaderReadModelEf
* ConfigReader
* UnitOfWorkEf

Nur die benötigten Ports, DTOs, UseCases und DI-Erweiterungsmethoden bleiben öffentlich sichtbar.

Dadurch bleibt die öffentliche Oberfläche klein und Modulgrenzen werden klarer.

## Dependency Injection

Dependency Injection verbindet Ports mit Implementierungen.

Beispiele:

```text
IReaderRepository → ReaderRepositoryEf
IReaderReadModel  → ReaderReadModelEf
IReaderUseCases   → ReaderUseCases
IUnitOfWork       → UnitOfWorkEf
```

Die konkrete Implementierung bleibt in der Infrastructure.

Das ausführbare API-Projekt sollte nur übergeordnete Registrierungen kennen, zum Beispiel:

```csharp
builder.Services.AddReadersModule();
builder.Services.AddInfrastructureModule(builder.Configuration);
```

Ziel ist, den Startup-Code lesbar zu halten und von detaillierten Implementierungsregistrierungen freizuhalten.

## Program.cs

`Program.cs` gehört zum ausführbaren API-Projekt.

Die Aufgabe von `Program.cs` ist, die Anwendung zu konfigurieren und zu starten.

Typische Aufgaben sind:

* Builder erzeugen
* Controller registrieren
* Readers-Modul registrieren
* Infrastructure registrieren
* Swagger und API-Versionierung registrieren
* Anwendung bauen
* Swagger in Development aktivieren
* Controller mappen
* Anwendung starten

`Program.cs` ist kein Ort für Domainlogik.

## API-Versionierung und Swagger

Die API verwendet versionierte Routen.

Die aktuellen Reader-Routen verwenden:

```text
/camplib/v1/readers
```

Swagger/OpenAPI ist für Dokumentation und manuelle Tests konfiguriert.

Swagger ist nicht die Architektur selbst. Swagger dokumentiert die HTTP-Oberfläche der Anwendung.

Die Architekturregel bleibt:

```text
Swagger dokumentiert die API.
Controller übersetzen HTTP.
UseCases schreiben.
ReadModels lesen.
```

## Aktuelle HTTP API

Die aktuelle HTTP API unterstützt:

```text
GET    /camplib/v1/readers
GET    /camplib/v1/readers/{id}
GET    /camplib/v1/readers/email?email=...
POST   /camplib/v1/readers
PUT    /camplib/v1/readers/{id}
DELETE /camplib/v1/readers/{id}
```

## Testarchitektur

Teil 2 behält die bestehende Teststrategie aus Teil 1 bei.

Typische Testgruppen sind:

* Domain Tests
* Value Object Tests
* UseCase Tests
* UseCase Integration Tests
* Repository Integration Tests
* ReadModel Integration Tests
* Controller-/End-to-End-Tests

Die aktuelle Testsuite prüft:

* Reader Domain Verhalten
* Email- und Address-Validierung
* Create UseCase
* Update UseCase
* Delete UseCase
* Repository-Verhalten
* ReadModel-Projektionen
* HTTP Controller Verhalten

Der letzte bekannte Teststand für Teil 2 lautet:

```text
66 Tests
0 failed
```

Die Tests sind in Teil 2 besonders wichtig, weil die Hauptänderung strukturell ist.

Das beabsichtigte Ergebnis lautet:

```text
Die Architektur ändert sich.
Das fachliche Verhalten bleibt gleich.
```

## Version

Teil 2 wird durch folgenden Branch und Tag repräsentiert:

```text
Branch: part-2/readers-modular-monolith
Tag:    v2-readers-modular-monolith
```

Teil 1 bleibt verfügbar als:

```text
Tag: v1-readers-monolith
```

## Geplante Weiterentwicklung

Teil 2 ist die modulare Grundlage für die nächsten Lehrschritte.

Die geplante Entwicklung lautet:

```text
Teil 1: Readers, Ein-Projekt-Monolith
Teil 2: Readers, projektbasierter modularer Monolith
Teil 3: Readers + Catalog
Teil 4: Readers + Catalog + Loans
Teil 5: AuthN + AuthZ
```

Teil 3 wird ein zweites fachliches Modul hinzufügen.

Dieser Schritt ist wichtig, weil die Architektur dann deutlicher zeigt, warum Modulgrenzen relevant sind. Mit nur Readers ist die modulare Struktur bereits sichtbar. Mit Readers und Catalog wird die Trennung zwischen Modulen konkreter.

## Regeln für die Erweiterung von Teil 2

Neue fachliche Module sollten der gleichen Struktur wie Readers folgen.

Ein neues Core-Modul sollte ein eigenes Projekt besitzen, zum Beispiel:

```text
CampusLibraryApi_3_Core_Catalog
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

Program.cs verdrahtet Module, enthält aber keine Domainlogik.

Zusätzliche Module sollten der gleichen Struktur wie Readers folgen.

AuthN/AuthZ wird später ergänzt, ohne die Grundstruktur zu verändern.

## Didaktische Faustregel

UseCases schützen fachliche Regeln auf der Schreibseite.

ReadModels liefern einfache DTOs auf der Leseseite.

Kurz:

```text
UseCases schreiben.
ReadModels lesen.
```

Für Teil 2 ist eine weitere Regel wichtig:

```text
Zuerst die Grenzen innerhalb eines Projekts verstehen.
Dann die Grenzen in getrennte Projekte verschieben.
```

Teil 2 zeigt diesen zweiten Schritt:

```text
Ordner werden Projekte.
Konventionen werden technische Grenzen.
```
