# Architektur – CampusLibrary Teil 6

Englische Version: [2Architecture.md](2Architecture.md)

Dieses Dokument beschreibt die Architektur des offiziellen Branches:

```text
part-6/authn-provision-profile
```

## Architekturziel

Teil 6 ergänzt die bestehende modulare Monolith-Architektur um echte Authentifizierung, ohne Core-Module an ASP.NET Core, `HttpContext`, Claims oder JWT-Bibliotheken zu koppeln.

Die zentrale Dependency-Regel bleibt erhalten:

```text
Core kennt weder Web noch Infrastructure noch IdentityAccessServer.
Web adaptiert HTTP und Claims.
Infrastructure implementiert Persistenz-Ports.
Der Composition Root verdrahtet alle Projekte.
```

## Projekte und Verantwortlichkeiten

### `CampusLibraryApi`

Ausführbares API-Projekt und Composition Root.

Es konfiguriert:

- Controller
- API-Versionierung
- Swagger/OpenAPI
- JWT-Bearer-Authentifizierung
- Policies und technische Authentifizierungsoptionen
- fachliche Module
- Infrastructure
- Datenbank

Das Projekt enthält keine fachliche Domain-Logik.

### `CampusLibraryApi_1_Web`

HTTP-Adapter der API.

Es enthält unter anderem:

- `ReadersController`
- `BooksController`
- `LoansController`
- Claim-/HttpContext-basierten Adapter für `IIdentityGateway`
- explizite Übersetzung von `Result` und `DomainError` in HTTP-Antworten
- `ProblemDetails`-Erzeugung

Die Web-Schicht kennt HTTP, Routing, Statuscodes, Claims und Swagger. UseCases kennen diese Details nicht.

### `CampusLibraryApi_2_BuildingBlocks`

Gemeinsame, fachmodulunabhängige Abstraktionen:

- `Result` und `Result<T>`
- `DomainError`
- `IClock`
- `IUnitOfWork`
- `IIdentityGateway`
- `IdentitySubject.Check(...)`
- echte BC-to-BC-Ports unter `_1_Ports/Contracts`
- BC-to-BC-Daten unter `_2_Application/Dtos`

`IdentitySubject.Check(...)` prüft eine authentifizierte Reader-Identität:

```text
IsAuthenticated
IsReader
Subject vorhanden und maximal 200 Zeichen
Username vorhanden
CreatedAt gültig
```

Der Subject wird als opaker Wert behandelt und nicht als GUID interpretiert.

### Core-Module

```text
CampusLibraryApi_3_Core_Readers
CampusLibraryApi_3_Core_Catalog
CampusLibraryApi_3_Core_Loan
```

Jedes Modul besitzt:

```text
_1_Ports
_2_Application
_3_Domain
```

Öffentliche HTTP-DTOs bleiben im jeweiligen Modul. UseCase-Fassaden enthalten Commands; ReadModel-Ports enthalten Queries.

### `CampusLibraryApi_4_Infrastructure`

Implementiert technische Details:

- EF-Core-DbContext
- SQLite
- Entity-Konfigurationen
- Repositories
- ReadModels
- Unit of Work
- BC-to-BC-Adapter

Infrastructure darf Core-Module referenzieren, weil es deren Outbound Ports implementiert. Core-Module referenzieren Infrastructure nicht.

### `CampusLibraryClient`

Blazor-SSR-Anwendung.

Der Client enthält:

- OIDC-Anmeldung und Abmeldung
- Cookie-basierte Client-Session
- Tokenzugriff
- `AccessTokenHandler`
- API-Clients
- eigene Transport-DTOs
- Reader-Profilseiten
- Catalog- und Loan-Seiten

Der Client referenziert keine API-Core-Projekte. Seine DTOs spiegeln ausschließlich den öffentlichen HTTP-Vertrag.

### `IdentityAccessServer`

Technischer Identity Provider für Teil 6.

Er übernimmt:

- Benutzerregistrierung beziehungsweise Development-Benutzer
- Anmeldung
- OIDC/OAuth-Flow
- Ausstellung von Identity- und Access-Tokens
- technische Claims

CampusLibrary verwaltet keine Passwörter.

## Dependency-Richtung

```text
IdentityAccessServer
        │  Tokens
        v
CampusLibraryClient
        │  Bearer Token
        v
CampusLibraryApi / Web
        │
        v
IIdentityGateway  ← BuildingBlocks
        │
        v
Readers / Loans UseCases
        │
        v
Repositories und ReadModels
        │
        v
Infrastructure / EF Core / SQLite
```

## Technische Identität als Port

```csharp
public interface IIdentityGateway {
   string Subject { get; }
   string Username { get; }
   DateTime CreatedAt { get; }
   int AdminRights { get; }
   bool IsAuthenticated { get; }
   bool IsReader { get; }
   bool IsEmployee { get; }
}
```

Der Port verhindert, dass Application-Code direkt auf folgende Typen zugreifen muss:

```text
HttpContext
ClaimsPrincipal
ClaimsIdentity
JwtSecurityToken
ASP.NET-Core-Authentifizierungsbibliotheken
```

`AdminRights` ist Teil des technischen Vertrags zum IdentityAccessServer, wird von CampusLibrary jedoch nicht als fachliche Berechtigung verwendet.

## Authentifizierung und Provisioning

Authentifizierung und Provisioning sind getrennte Workflows.

### Authentifizierung

```text
IdentityAccessServer
→ Access Token
→ JWT-Bearer-Validierung
→ ClaimsPrincipal
→ IIdentityGateway
```

### Provisioning

```text
POST /readers/me/provision
→ IdentitySubject.Check(...)
→ Reader anhand Subject suchen
→ neuen fachlichen Reader anlegen
→ Subject dauerhaft speichern
```

Provisioning vertraut dem Token, nicht einem Clientformular.

## Profilzustände

Ein provisionierter Reader kann ein unvollständiges Profil besitzen.

```text
Identity vorhanden
→ Reader provisioniert
→ IsProfileCompleted = false
→ Profil vervollständigen
→ IsProfileCompleted = true
```

Die initiale Profilvervollständigung und die spätere selektive Aktualisierung sind absichtlich getrennt:

```text
PUT /readers/me/profile
PUT /readers/me/update
```

`ReaderProfileDto` enthält die initial notwendigen fachlichen Werte. `ReaderUpdateDto` enthält optionale Werte für spätere Änderungen.

## Self-Service über `/me`

Self-Service-Endpunkte akzeptieren keine Reader-ID zur Auswahl des aktuellen Benutzers.

```text
HTTP Request
→ IIdentityGateway.Subject
→ ReaderReadModel/Repository nach Subject
→ fachliche Reader.Id
→ Operation
```

Damit kann ein Client nicht durch Manipulation einer Reader-ID auf einen anderen Reader wechseln.

## Modulkommunikation

Loan benötigt Informationen aus Readers und Catalog, darf aber nicht direkt auf deren Tabellen oder EF-Entities zugreifen.

Deshalb verwendet das Loan-Modul BC-to-BC-Ports:

```text
Loan UseCase
→ IReaderLoanContract
→ ReaderLoanInfoDto

Loan UseCase
→ ILoanCatalogContract
→ BookItemLoanInfoDto
```

Die Schnittstellen liegen in BuildingBlocks unter `_1_Ports/Contracts`, die zugehörigen Datentransferobjekte unter `_2_Application/Dtos`.

Jedes Modul bleibt Eigentümer seiner Daten. Nur der jeweilige Adapter darf auf die eigene Tabelle zugreifen.

## DTO-Eigentum

```text
HTTP-DTO eines Moduls
→ im Modul

Client-Transport-DTO
→ im Client

BC-to-BC-DTO
→ BuildingBlocks/_2_Application/Dtos
```

Ein gemeinsames `CampusLibrary.Contracts`-Projekt wird bewusst nicht verwendet. Es würde Ownership verwischen und Client sowie Module unnötig koppeln.

## Catalog-Modell

Catalog verwendet vereinheitlichte DTOs:

```text
BookDto
BookCreateDto
BookItemDto
BookItemAddDto
BookDeactivationInfoDto
BookLoanInfoDto
```

Autoren werden als `AuthorsText` am Book transportiert. Die Suche nach `AuthorLastName` zerlegt den kommagetrennten Text und vergleicht gezielt Nachnamen.

`BookItem` repräsentiert ein physisches Exemplar. Seine Identität ist die `Id`; eine zusätzliche `InventoryNumber` wird im aktuellen Modell nicht benötigt.

## Loan-Modell

Ein Loan repräsentiert eine aktuell laufende Ausleihe.

- Borrow erzeugt einen Loan.
- Renew verändert Fälligkeitsdatum und Verlängerungszähler.
- Return at desk entfernt den Loan.

Das Modell besitzt deshalb keinen dauerhaften Returned-Status als Historie. Eine historische Ausleihverwaltung wäre ein separates späteres Konzept.

## Autorisierung in Teil 6

Teil 6 fokussiert auf AuthN, Reader-Provisioning, Profil und grundlegende Rollenprüfung an Identity-nahen Workflows.

Nicht alle Catalog- und Administrationsendpunkte sind bereits systematisch durch Policies geschützt. Eine umfassende AuthZ-Diskussion mit Scopes, Policies und UseCase-Guards ist für einen späteren Teil vorgesehen.

## Fehlerabbildung

UseCases liefern `Result` oder `Result<T>`. Controller übersetzen Fehler explizit:

```text
Validation / BadRequest  → 400
Unauthenticated          → 401
AccessNotAllowed         → 403
NotFound                 → 404
Conflict                 → 409
```

`DomainProblemDetailsFactory` erzeugt ausschließlich `ProblemDetails`. Der konkrete HTTP-Status bleibt in jeder Controller-Action sichtbar.
