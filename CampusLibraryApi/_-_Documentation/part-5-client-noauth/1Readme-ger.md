# CampusLibrary — Teil 5: Client ohne echte AuthN

Lehrprojekt für eine modular aufgebaute, DDD-orientierte ASP.NET-Core-Web-API und einen Blazor-SSR-Client.

Englische Version: [1Readme.md](1Readme.md)

## Aktueller Stand

Teil 5 enthält bereits große Teile der fachlichen Struktur aus Teil 6. Der wesentliche Unterschied ist die Quelle der technischen Identität:

```text
Teil 5: API-eigene DevIdentity aus appsettings.json
Teil 6: validierte Claims aus einem Access Token des IdentityAccessServers
```

Der Client sendet in Teil 5 weder ein Bearer-Token noch eigene Identitätsheader an die API. Trotzdem können die Self-Service-Endpunkte unter `/me` verwendet werden, weil die API den aktuellen technischen Benutzer selbst simuliert.

Verifizierter Stand vom 15. Juli 2026:

```text
dotnet clean
Build succeeded

dotnet build
Build succeeded

dotnet test
212 total, 212 succeeded, 0 failed, 0 skipped
```

Die manuellen Tests in `Loan_Me.http` wurden ebenfalls erfolgreich ausgeführt:

```text
GET   /loans/me                    -> 200
POST  /loans/me                    -> 201
GET   /loans/me/{id}               -> 200
PATCH /loans/me/{id}/renew         -> 200
PATCH /loans/{id}/return-at-desk   -> 204
GET   /loans/me/{id} nach Rückgabe -> 404
```

## Branch

```text
part-5/client-noauth
```

Der Branch ist auf GitHub veröffentlicht und verfolgt:

```text
origin/part-5/client-noauth
```

## Ziel von Teil 5

Teil 5 zeigt, wie eine modulare CampusLibrary-API durch einen Blazor-SSR-Client verwendet wird und wie Self-Service-Endpunkte bereits ohne echten IdentityAccessServer vorbereitet werden können.

Aktiv in Teil 5:

```text
Blazor SSR Client
HTTP-Zugriff auf CampusLibraryApi
modulbezogene API-Clients
vereinheitlichte Transport-DTOs
Readers-, Catalog- und Loans-Modul
Reader-/Mitarbeiter-Perspektive im Client
API-seitige technische DevIdentity
Subject-basierte Reader-Zuordnung
Reader-Self-Service-Update über /readers/me/update
Loan-Self-Service über /loans/me
administrative Reader-, Catalog- und Loan-Endpunkte
ProblemDetails-basierte Fehlerbehandlung
Bootstrap-basiertes Layout
vorbereitete, aber deaktivierte AuthN/AuthZ-Infrastruktur
```

Nicht aktiv in Teil 5:

```text
echte Registrierung
echter Login gegen IdentityAccessServer
echte Logout-Session gegen IdentityAccessServer
Access-Token-Weitergabe an die API
JWT-Bearer-Authentifizierung der API
policybasierte API-Autorisierung
Reader-Provisionierung aus einem Access Token
geschützte API-Endpunkte
```

## Projektstruktur

```text
CampusLibraryApi                 ausführbares API-Projekt / Composition Root
CampusLibraryApi_1_Web           Controller, ProblemDetails, DevIdentity-Adapter
CampusLibraryApi_2_BuildingBlocks gemeinsame Ports, Result, Fehler, BC-Contracts
CampusLibraryApi_3_Core_Readers  Reader-Domäne und Reader-Anwendungsfälle
CampusLibraryApi_3_Core_Catalog  Catalog-Domäne und Catalog-Anwendungsfälle
CampusLibraryApi_3_Core_Loan     Loan-Domäne und Loan-Anwendungsfälle
CampusLibraryApi_4_Infrastructure EF Core, Repositories, ReadModels, Contract-Adapter
CampusLibraryApiTest             automatisierte API-Tests
CampusLibraryClient              Blazor-SSR-Client
IdentityAccessServer             vorbereitet, in Teil 5 nicht aktiv verwendet
Shared                           gemeinsam genutzte technische Hilfen
```

## Zentrale Architekturregel

Die fachlichen API-Module bleiben Eigentümer ihrer öffentlichen HTTP-DTOs:

```text
Readers  -> ReaderDtos.cs
Catalog  -> CatalogDtos.cs
Loans    -> LoanDtos.cs
```

Der Client referenziert keine Core-Projekte der API. Er besitzt eigene Transporttypen mit derselben JSON-Struktur:

```text
CampusLibraryClient/Api/Dtos/ReaderDtos.cs
CampusLibraryClient/Api/Dtos/CatalogDtos.cs
CampusLibraryClient/Api/Dtos/LoanDtos.cs
```

Nur echte modulübergreifende Verträge liegen in BuildingBlocks:

```text
_1_Ports/Contracts
_2_Application/Dtos
```

Beispiele:

```text
IBookItemLoanContract
ILoanCatalogContract
ILoanReaderContract
IReaderLoanContract
BookItemLoanInfoDto
CurrentBookItemLoanInfoDto
ReaderLoanInfoDto
```

## Zwei getrennte DevIdentity-Verwendungen

Teil 5 besitzt eine DevIdentity im Client und eine DevIdentity in der API. Beide lesen ihre eigene `appsettings.json`; es erfolgt keine Übertragung zwischen den Anwendungen.

### Client

Der Client verwendet `DevCurrentUserProvider` für:

```text
sichtbare Navigation
Reader-/Mitarbeiter-Perspektive
DisplayName
ReaderId für UI-Zwecke
E-Mail-Anzeige
```

### API

Die API verwendet `DevIdentityGateway` als Implementierung von `IIdentityGateway` für:

```text
IsAuthenticated
Subject
AccountType -> IsReader / IsEmployee
Email -> Username
CreatedAt
AdminRights = 0
```

Die Anwendungen sollen dasselbe `ActiveProfile` verwenden, wenn Client und API gemeinsam getestet werden.

## Beispielkonfiguration

Ein gemeinsamer Profilaufbau in beiden Anwendungen verhindert unnötige Abweichungen. Nicht benötigte Felder werden vom jeweiligen Adapter ignoriert.

```json
{
  "Features": {
    "AuthNEnabled": false,
    "DevIdentityEnabled": true,
    "ApiAccessTokenEnabled": false,
    "AuthZEnabled": false
  },
  "DevIdentity": {
    "ActiveProfile": "ReaderRita",
    "Profiles": {
      "ReaderRita": {
        "IsAuthenticated": true,
        "Subject": "reader-099",
        "AccountType": "reader",
        "ReaderId": "00000099-0000-0000-0000-000000000000",
        "DisplayName": "Rita Reader",
        "Email": "r.reader@library.local",
        "CreatedAt": "2025-01-01T00:00:00Z",
        "AdminRights": 0
      },
      "EmployeeAdmin": {
        "IsAuthenticated": true,
        "Subject": "employee-admin",
        "AccountType": "employee",
        "ReaderId": null,
        "DisplayName": "Admin",
        "Email": "admin@mail.local",
        "CreatedAt": "2025-01-01T00:00:00Z",
        "AdminRights": 0
      }
    }
  }
}
```

Wichtig:

```text
DevIdentity.Subject muss exakt zu Reader.Subject in der Datenbank passen.
```

Für Rita gilt im manuellen Teststand:

```text
Subject  = reader-099
ReaderId = 00000099-0000-0000-0000-000000000000
```

Subject und ReaderId sind unterschiedliche Identifikatoren.

Die E-Mail darf später geändert werden. Deshalb wird der Reader nicht über die E-Mail, sondern über das stabile Subject ermittelt.

## API-seitige technische Identität

Der Datenfluss in Teil 5 lautet:

```text
CampusLibraryApi/appsettings.json
        ↓
DevIdentityOptions
        ↓
DevIdentityGateway
        ↓
IIdentityGateway
        ↓
IdentitySubject.Check(...)
        ↓
Reader über Subject laden
        ↓
/readers/me/update und /loans/me
```

`IdentitySubject.Check(...)` prüft:

```text
Benutzer ist als authentifiziert simuliert
Benutzer ist Reader
Subject ist vorhanden
Subject ist höchstens 200 Zeichen lang
Username ist vorhanden
CreatedAt ist gültig
```

`AdminRights` bleibt aus Kompatibilitätsgründen mit Teil 6 im Port erhalten, wird von CampusLibrary aber nicht fachlich ausgewertet und steht in Teil 5 auf `0`.

## CampusLibraryClient

Der Client ist eine Blazor-SSR-Anwendung mit interaktiven Server-Komponenten.

Wichtige Konzepte:

```text
Razor Components
Interactive Server Render Mode
modulbezogene API-Clients
Result<T> für Erfolgs-/Fehlerbehandlung
ProblemDetails-basierte Fehlermeldungen
Bootstrap-Utilities
ICurrentUserProvider als UI-Abstraktion
vorbereiteter AccessTokenHandler, in Teil 5 inaktiv
```

Wichtige Ordner:

```text
CampusLibraryClient
├─ Api
│  ├─ Auth
│  ├─ Clients
│  ├─ Contracts
│  ├─ Dtos
│  └─ Errors
├─ Core
├─ Extensions
├─ Security
├─ Shared
└─ Ui
   ├─ Components
   ├─ Controllers
   ├─ Models
   └─ Pages
```

## Sichtbare Seiten

```text
/                                      Startseite
/readers                               Reader-Liste für Mitarbeiter
/catalog/books                         Katalog
/catalog/books/create                  Buch hinzufügen
/catalog/books/{bookId}/items/add      Exemplar hinzufügen
/catalog/books/{bookId}/deactivate     Buch deaktivieren
/catalog/books/{bookId}/borrow         Buch ausleihen
/loans                                 aktuelle Ausleihen für Mitarbeiter
/loans/{loanId}                        Ausleihe-Details
/my/loans                              Ausleihen des aktuellen Readers
/logout                                Demo-/vorbereitete Logout-Seite
/access-denied                         vorbereitete Fehlerseite
/error                                 technische Fehlerseite
```

Die API besitzt bereits `PUT /readers/me/update`; eine vollständige Reader-Profilseite im Client ist in diesem Teil noch nicht sichtbar umgesetzt.

## API-Clients pro Modul

```text
IReaderClient -> ReaderClient
IBookClient   -> BookClient
ILoanClient   -> LoanClient
```

Die BaseUrl steht im Client:

```json
{
  "CampusLibraryApi": {
    "BaseUrl": "https://localhost:8010/"
  }
}
```

Teil 5 ruft die API ohne Bearer-Token auf.

## Readers

Der Readers-Bereich bietet:

```text
Reader-Liste
Reader per Id
Reader per E-Mail
administrative Reader-Erzeugung
Self-Service-Update des aktuellen Readers
Soft-Delete / Deaktivierung
```

Der alte Update-Endpunkt mit expliziter ReaderId wurde ersetzt:

```text
alt: PUT /readers/{id}
neu: PUT /readers/me/update
```

Clientmethode:

```text
UpdateMeAsync(ReaderUpdateDto dto)
```

`ReaderUpdateDto` enthält optionale Werte:

```text
Lastname
Email
AddressDto
```

`null` bedeutet: Wert nicht ändern.

## Catalog

Catalog verwendet einen vereinheitlichten `BookDto` für Liste und Detailansicht:

```text
Id
AuthorsText
Title
Subtitle
Isbn
BookItems
TotalItems
AvailableItems
IsActive
```

`BookItemDto` enthält:

```text
Id
BookId
Status
```

Eine zusätzliche `InventoryNumber` ist im aktuellen Modell nicht mehr Bestandteil des Transportvertrags. Die BookItem-Identität ist die `Guid Id`.

BookItem-Statuswerte:

```text
1 = Available
2 = Unavailable
3 = Lost
4 = Damaged
```

## Loans

Loans verwendet einen vereinheitlichten `LoanDto` für Liste und Detailansicht.

Ein gespeicherter Loan repräsentiert immer eine aktuell bestehende Ausleihe. Deshalb besitzt der aktuelle Loan-Vertrag keine Felder `Status` oder `ReturnedAt`.

Bei der Rückgabe gilt:

```text
PATCH /loans/{id}/return-at-desk
        ↓
Loan wird gelöscht
        ↓
späteres GET liefert 404
```

Reader-Self-Service:

```text
GET   /loans/me
GET   /loans/me/{id}
POST  /loans/me
PATCH /loans/me/{id}/renew
```

Administrative Endpunkte:

```text
GET   /loans
GET   /loans/{id}
POST  /loans
PATCH /loans/{id}/renew
PATCH /loans/{id}/return-at-desk
```

Bei `/loans/me` sendet der Client keine ReaderId. Die API ermittelt den Reader über `IIdentityGateway.Subject`.

## Fehlerbehandlung

Use Cases und ReadModels liefern `Result` beziehungsweise `Result<T>`.

Controller übersetzen `DomainError.Status` explizit in HTTP-Antworten:

```text
BadRequest   -> 400
Unauthorized -> 401
Forbidden    -> 403
NotFound     -> 404
Conflict     -> 409
```

Fehler werden als `ProblemDetails` zurückgegeben. Der Client verarbeitet diese zentral im `BaseApiClient`.

## Manuelle HTTP-Tests ohne Client und IA-Server

Für `/me`-Tests reicht die laufende API:

```bash
dotnet run --project CampusLibraryApi
```

Es werden keine Header benötigt:

```http
GET https://localhost:8010/camplib/v1/loans/me
Accept: application/json
```

Voraussetzungen:

```text
DevIdentity:ActiveProfile = ReaderRita
ReaderRita.Subject stimmt mit Reader.Subject in der Datenbank überein
benötigte Reader-, Book- und BookItem-Testdaten existieren
```

## Starten

API:

```bash
dotnet run --project CampusLibraryApi
```

Client:

```bash
dotnet run --project CampusLibraryClient
```

Prüfung:

```bash
dotnet clean
dotnet build
dotnet test
```

## Fortsetzung in Teil 6

Teil 6 ersetzt die technische Identitätsquelle:

```text
DevIdentityGateway
        ↓ wird ersetzt durch
Claim-/HttpContext-basiertes IIdentityGateway
```

Gleich bleiben können:

```text
IIdentityGateway
IdentitySubject.Check(...)
Subject-basierte Reader-Zuordnung
ReaderUcUpdateMe
/me-Endpunkte
fachliche Use Cases
DTO-Grenzen
```

Neu hinzukommen:

```text
OIDC-Login im Client
Cookie-Session im SSR-Client
Access Token
Bearer-Token-Weitergabe
JWT-Validierung in der API
echte Claims statt Appsettings-Werte
```
