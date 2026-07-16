# Architektur: CampusLibrary Teil 5 — Client ohne echte AuthN

Dieses Dokument beschreibt die aktuelle Architektur des Branches `part-5/client-noauth`.

Englische Version: [2Architecture.md](2Architecture.md)

## Architekturziel

Teil 5 soll fachlich bereits möglichst nah an Teil 6 liegen, ohne eine echte Authentifizierung über den IdentityAccessServer zu benötigen.

Die zentrale Trennung lautet:

```text
fachliche Identitätsnutzung bleibt gleich
technische Identitätsquelle ist unterschiedlich
```

```text
Teil 5:
appsettings -> DevIdentityGateway -> IIdentityGateway

Teil 6:
Access Token -> Claims/HttpContext-Adapter -> IIdentityGateway
```

Dadurch können Subject-basierte Use Cases und `/me`-Endpunkte schon in Teil 5 entwickelt und getestet werden.

## Solution-Sicht

```text
CampusLibraryApi
  Composition Root, Konfiguration, Hosting

CampusLibraryApi_1_Web
  Controller
  ProblemDetails-Abbildung
  API-seitige DevIdentity-Options und -Adapter

CampusLibraryApi_2_BuildingBlocks
  Result / Result<T>
  DomainError und gemeinsame Fehler
  IIdentityGateway
  IClock und IUnitOfWork
  echte BC-to-BC-Contracts und deren kleine DTOs

CampusLibraryApi_3_Core_Readers
  Reader-Aggregat
  Reader-Use-Cases
  Reader-ReadModel-Ports
  Reader-HTTP-DTOs

CampusLibraryApi_3_Core_Catalog
  Book und BookItem
  Catalog-Use-Cases
  Catalog-ReadModel-Ports
  Catalog-HTTP-DTOs

CampusLibraryApi_3_Core_Loan
  Loan-Aggregat
  Loan-Use-Cases
  Loan-ReadModel-Ports
  Loan-HTTP-DTOs

CampusLibraryApi_4_Infrastructure
  EF Core
  Repositories
  ReadModels
  modulübergreifende Contract-Adapter

CampusLibraryClient
  Blazor SSR
  UI-Perspektive über DevCurrentUserProvider
  eigene HTTP-Clients und Transport-DTOs

IdentityAccessServer
  vorbereitet, in Teil 5 nicht aktiv beteiligt
```

## Dependency Rule

Die Abhängigkeiten zeigen nach innen:

```text
Composition Root
   ↓
Web / Infrastructure
   ↓
Core-Module / BuildingBlocks
```

Core-Code kennt nicht:

```text
HttpContext
ClaimsPrincipal
JWT-Bibliotheken
IConfiguration
Blazor
EF-Core-Implementierungen
```

Der Client kennt nicht:

```text
API-Core-Projekte
Domain-Aggregate
EF-Core-Entities
Repository-Implementierungen
```

## Composition Root und Web-Modul

`CampusLibraryApi` ist das ausführbare Projekt und darf auf `CampusLibraryApi_1_Web` zugreifen.

Das Web-Modul darf nicht zurück auf den Composition Root zugreifen. Deshalb liegen die technischen Options-Klassen beim Adapter im Web-Projekt:

```text
CampusLibraryApi_1_Web/_1_Web/Security
├─ DevIdentityOptions.cs
├─ DevIdentityGateway.cs
└─ DevIdentityExtension.cs
```

Die Konfigurationswerte bleiben im ausführbaren Projekt:

```text
CampusLibraryApi/appsettings.json
```

Die Registrierung erfolgt vom Composition Root aus über eine Web-Extension:

```text
builder.Services.AddDevIdentityGateway(builder.Configuration)
```

Damit entsteht keine zyklische Projektabhängigkeit.

## API-seitige DevIdentity

Die API simuliert eine technische Identität aus ihrer eigenen Konfiguration.

```text
appsettings.json
   ↓ bind
DevIdentityOptions
   ↓ lesen
DevIdentityGateway
   ↓ implementiert
IIdentityGateway
```

Der Adapter stellt bereit:

```text
Subject
Username
CreatedAt
AdminRights
IsAuthenticated
IsReader
IsEmployee
```

Die API übernimmt keine `ReaderId` aus dem Client. Für Reader-Self-Service gilt:

```text
Subject
   ↓
ReaderRepository.FindBySubjectAsync(...)
   ↓
fachlicher Reader
```

## IIdentityGateway als stabiler Port

`IIdentityGateway` liegt in BuildingBlocks, weil Use Cases die technische Identität benötigen, aber nicht wissen sollen, wie sie gewonnen wurde.

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

`AdminRights` bleibt für die Kompatibilität mit dem späteren IA-Server-Token enthalten. CampusLibrary wertet diese Bitmaske nicht aus. Teil 5 setzt den Wert auf `0`.

## IdentitySubject

Die gemeinsame Anwendungslogik prüft die Identität über:

```text
IdentitySubject.Check(IIdentityGateway)
```

Prüfschritte:

```text
1. IsAuthenticated muss true sein.
2. IsReader muss true sein.
3. Subject muss vorhanden sein.
4. Subject darf höchstens 200 Zeichen lang sein.
5. Username muss vorhanden sein.
6. CreatedAt darf nicht default sein.
7. Subject wird als opaker Wert zurückgegeben.
```

Die Klasse interpretiert das Subject nicht. Ein Subject kann eine GUID, ein anderer Identifier oder beispielsweise `reader-099` sein.

## Warum Subject und nicht E-Mail?

Die technische Zuordnung darf nicht von einer veränderbaren E-Mail-Adresse abhängen.

```text
Subject:
- stabil
- opak
- Identitätsanker

E-Mail:
- initial Username
- kann fachlich geändert werden
- nicht als dauerhafte Zuordnung geeignet
```

Nach einem Reader-Update können deshalb unterschiedliche Werte existieren:

```text
IIdentityGateway.Username = r.reader@library.local
Reader.Email              = e.meier@gmx.de
```

Die Zuordnung bleibt über `Subject` erhalten.

## API- und Client-Konfiguration

Client und API besitzen jeweils eine eigene `DevIdentity`-Sektion.

```text
Client appsettings -> DevCurrentUserProvider
API appsettings    -> DevIdentityGateway
```

Es gibt keine automatische Synchronisation und keine Übertragung per HTTP.

Für gemeinsame Szenarien müssen folgende Werte zusammenpassen:

```text
ActiveProfile
Subject des API-Profils und Reader.Subject in der Datenbank
```

Ein identischer Profilaufbau in beiden Konfigurationen reduziert Fehler. Die Adapter lesen jedoch unterschiedliche Teilmengen:

```text
Client liest:
IsAuthenticated, AccountType, ReaderId, DisplayName, Email

API liest:
IsAuthenticated, Subject, AccountType, Email, CreatedAt, AdminRights
```

## Client-Architektur

```text
Razor Page / Component
        ↓
IReaderClient / IBookClient / ILoanClient
        ↓
ReaderClient / BookClient / LoanClient
        ↓
BaseApiClient
        ↓
HttpClient
        ↓
CampusLibraryApi
```

Die Client-API-Schicht wandelt HTTP-Ergebnisse in `Result<T>` um und verarbeitet `ProblemDetails` zentral.

## CurrentUserProvider

Die UI hängt ausschließlich von `ICurrentUserProvider` ab.

Implementierungen:

```text
DevCurrentUserProvider       Teil 5
ClaimsCurrentUserProvider    vorbereitet für Teil 6
AnonymousCurrentUserProvider Fallback
```

Der Client wählt die Implementierung über Feature Flags:

```text
AuthNEnabled
DevIdentityEnabled
ApiAccessTokenEnabled
AuthZEnabled
```

Teil-5-Konfiguration:

```text
AuthNEnabled          = false
DevIdentityEnabled    = true
ApiAccessTokenEnabled = false
AuthZEnabled          = false
```

## Keine Identitätsübertragung vom Client

Teil 5 verwendet bewusst weder:

```text
Authorization: Bearer ...
X-Dev-Subject
X-Dev-Username
X-Dev-Account-Type
```

Der Client steuert nur seine UI-Perspektive. Die API bestimmt ihre technische Identität unabhängig aus der eigenen Konfiguration.

Das ermöglicht auch direkte `.http`-Tests ohne Client und ohne IdentityAccessServer.

## DTOs als Transportgrenze

Öffentliche HTTP-DTOs gehören dem jeweiligen Modul:

```text
Readers/_2_Application/Dtos/ReaderDtos.cs
Catalog/_2_Application/Dtos/CatalogDtos.cs
Loans/_2_Application/Dtos/LoanDtos.cs
```

Der Client besitzt strukturell passende Kopien:

```text
CampusLibraryClient/Api/Dtos
```

Es gibt kein gemeinsames `CampusLibrary.Contracts`-Projekt. Dadurch wird verhindert, dass alle Module und der Client an ein zentrales DTO-Paket gekoppelt werden.

## BC-to-BC-Contracts

Nur fachliche Kommunikation zwischen Modulen liegt in BuildingBlocks.

```text
Catalog -> Loans:
IBookItemLoanContract
BookItemLoanInfoDto

Loans -> Catalog:
ILoanCatalogContract
CurrentBookItemLoanInfoDto

Readers -> Loans:
IReaderLoanContract
ReaderLoanInfoDto

Loans -> Readers:
ILoanReaderContract
```

Ein Modul erhält nur die Informationen, die es tatsächlich benötigt. Es greift nie direkt auf Tabellen oder Aggregate eines anderen Moduls zu.

## Reader-Architektur

Query-Seite:

```text
IReaderReadModel
- SelectAllAsync
- FindByIdAsync
- FindByEmailAsync
- FindMeAsync für interne Self-Service-Auflösung
```

Command-Seite:

```text
IReaderUseCases
- CreateAsync
- UpdateMeAsync
- DeactivateAsync
```

Das Self-Service-Update läuft so:

```text
PUT /readers/me/update
        ↓
ReaderController
        ↓
IReaderUseCases.UpdateMeAsync
        ↓
ReaderUcUpdateMe
        ↓
IdentitySubject.Check
        ↓
Reader per Subject laden
        ↓
optionale Werte validieren
        ↓
Reader.UpdateProfile
        ↓
IUnitOfWork.SaveAllChangesAsync
```

Der Client sendet keine ReaderId. Dadurch kann der Aufrufer nicht auswählen, welcher Reader geändert wird.

## Catalog-Architektur

`Book` ist das Aggregate für Buchdaten und seine Exemplare.

```text
Book
├─ bibliografische Daten
├─ IsActive
└─ BookItems
```

`BookItem` besitzt eine Guid-Identität und einen Status:

```text
Available
Unavailable
Lost
Damaged
```

Eine `InventoryNumber` ist nicht mehr Teil des aktuellen DTO- oder UI-Vertrags.

Die List- und Detailprojektion wurde in `BookDto` vereinheitlicht. Dadurch entfallen alte Typen wie:

```text
BookListItemDto
BookDetailDto
BookSearchDto
```

## Deaktivieren eines Buchs

Vor dem Deaktivieren fragt Catalog über `ILoanCatalogContract`, ob aktuelle Loans für BookItems bestehen.

```text
Catalog
   ↓ ILoanCatalogContract
Loans
```

Die Deaktivierungsansicht erhält eine kleine Projektion mit:

```text
BookItemId
ReaderEmail
DueDate
```

Catalog erhält keine Loan-Entities und keinen direkten Zugriff auf die Loans-Tabelle.

## Loan-Architektur

Ein Loan repräsentiert im aktuellen Modell eine laufende Ausleihe.

```text
Loan vorhanden  = aktuell ausgeliehen
Loan gelöscht   = zurückgegeben
```

Daraus folgen:

```text
kein Loan.Status
kein Loan.ReturnedAt
kein historischer Returned-Loan im aktuellen Aggregate
```

`LoanDto` ist für Listen- und Detailansicht vereinheitlicht. Alte Typen wie `LoanListItemDto` und `LoanDetailDto` entfallen.

## Administrative und Self-Service-Loans

Administrative Abläufe verwenden explizite Reader- oder Loan-IDs:

```text
GET   /loans
GET   /loans/{id}
POST  /loans
PATCH /loans/{id}/renew
PATCH /loans/{id}/return-at-desk
```

Reader-Self-Service verwendet die technische Identität:

```text
GET   /loans/me
GET   /loans/me/{id}
POST  /loans/me
PATCH /loans/me/{id}/renew
```

Bei `POST /loans/me` enthält `LoanBorrowMeDto` nur:

```text
BookItemId
optionale Id für deterministische Tests
```

Die ReaderId wird serverseitig aus dem Subject bestimmt.

## Fehlerarchitektur

Domain- und Anwendungscode liefern `Result` oder `Result<T>` mit `DomainError`.

Die Web-Schicht erstellt `ProblemDetails` und wählt den Statuscode explizit:

```text
WebErrorStatus.BadRequest   -> 400
WebErrorStatus.Unauthorized -> 401
WebErrorStatus.Forbidden    -> 403
WebErrorStatus.NotFound     -> 404
WebErrorStatus.Conflict     -> 409
```

`DomainProblemDetailsFactory` erzeugt nur die ProblemDetails-Daten. Der Controller bleibt Eigentümer der konkreten HTTP-Antwort.

## Auth-Vorbereitung ohne Aktivierung

Der Client enthält bereits vorbereitete Bausteine:

```text
ClaimsCurrentUserProvider
AccessTokenHandler
IdentityController
EntryController
ConfigureAuthN
ConfigureAuthZ
```

Sie bleiben in Teil 5 durch Feature Flags inaktiv.

Der IdentityAccessServer kann Teil der Solution bleiben, ist aber für den Part-5-Lauf nicht erforderlich.

## Übergang zu Teil 6

Teil 6 tauscht primär den Adapter aus:

```text
Teil 5: DevIdentityGateway
Teil 6: IdentityGateway aus Claims/HttpContext
```

Gleich bleiben:

```text
IIdentityGateway
IdentitySubject
ReaderUcUpdateMe
Loan-/me-Use-Cases
Subject-basierte Reader-Auflösung
Modulgrenzen
DTO-Eigentum
```

Neu aktiv werden:

```text
OIDC
Cookie-Authentifizierung des SSR-Clients
Access Token
Bearer-Token-Handler
JWT-Validierung
Claims-basierte Rollen- und Subject-Auswertung
```

## Didaktischer Kern

Teil 5 zeigt gleichzeitig:

```text
- Client und API bleiben getrennte Anwendungen.
- UI-Perspektive ist nicht dasselbe wie API-Sicherheit.
- Eine technische Identität kann hinter einem Port simuliert werden.
- Subject ist stabiler als eine veränderbare E-Mail.
- /me-Endpunkte vermeiden vom Client wählbare fachliche IDs.
- Modulübergreifende Kommunikation erfolgt über kleine Contracts.
- HTTP-DTOs bleiben Eigentum ihrer Module.
- Teil 6 kann die Identitätsquelle austauschen, ohne die Fachlogik neu zu schreiben.
```
