# CampusLibrary – Teil 6

Lehrprojekt für eine modular aufgebaute, DDD-orientierte ASP.NET-Core-Web-API mit Blazor-SSR-Client und eigenem IdentityAccessServer.

Englische Version: [1Readme.md](1Readme.md)

## Aktueller Stand

Teil 6 baut auf Teil 5 auf. Catalog, Readers, Loans, Blazor-Client, EF-Core-Persistenz, ReadModels, UseCases und die projektbasierte modulare Monolith-Struktur bleiben erhalten. Neu hinzu kommen echte technische Identitäten und ein Self-Service-Ablauf für Reader.

Der aktuelle Stand umfasst:

- ASP.NET Core Web API auf .NET 10
- Blazor-SSR-Client
- IdentityAccessServer für OIDC/OAuth 2.0
- JWT-Bearer-Authentifizierung der API
- Access-Token-Weitergabe durch den Client
- `IIdentityGateway` als Port zur technischen Identität
- Reader-Provisioning über den Token-Subject
- initiale Reader-Profilvervollständigung
- spätere Self-Service-Profiländerung
- Reader- und Loan-Endpunkte unter `/me`
- vereinheitlichte HTTP-DTOs pro fachlichem Modul
- BC-to-BC-Contracts in BuildingBlocks
- explizites Mapping von Domain Errors auf HTTP-Statuscodes
- SQLite und EF Core
- automatisierte Tests auf Domain-, Application-, Infrastructure- und API-Ebene
- manuelle `.http`-Skripte für Identity-, Reader-, Catalog- und Loan-Abläufe

Der zuletzt vollständig verifizierte Stand von Teil 6 war:

```text
238 Tests
0 fehlgeschlagen
0 übersprungen
```

Nach dem abschließenden Merge des DTO-Refactorings sollte dieser Wert noch einmal mit `dotnet test` bestätigt werden.

## Version

Offizieller Branch:

```text
part-6/authn-provision-profile
```

Geplanter finaler Tag:

```text
v6-authn-provision-profile
```

Die Versionsreihe lautet damit:

```text
v1-readers-monolith
v2-readers-modular-monolith
v3-readers-catalog
v4-readers-catalog-loans
v5-client-noauth
v6-authn-provision-profile
```

## Projektstruktur

```text
CampusLibraryApi
CampusLibraryApi_1_Web
CampusLibraryApi_2_BuildingBlocks
CampusLibraryApi_3_Core_Readers
CampusLibraryApi_3_Core_Catalog
CampusLibraryApi_3_Core_Loan
CampusLibraryApi_4_Infrastructure
CampusLibraryApiTest
CampusLibraryClient
IdentityAccessServer
```

## Ziel von Teil 6

Teil 5 simuliert die Benutzerperspektive ohne echte Anmeldung. Teil 6 ersetzt diese Simulation durch eine technische Identität aus dem IdentityAccessServer.

Die zentrale Trennung lautet:

```text
Authentifizierung:
Wer ist der technische Benutzer?

Provisioning:
Welcher fachliche Reader gehört zu diesem technischen Benutzer?
```

Der technische Benutzer wird durch ein stabiles Subject identifiziert. Die fachliche Reader-ID bleibt davon unabhängig.

```text
IdentityAccessServer subject
        ↓
Reader.Subject
        ↓
Reader.Id
```

Die E-Mail-Adresse ist kein stabiler Schlüssel. Sie darf später geändert werden. Der Zusammenhang zwischen technischer Identität und Reader bleibt deshalb über `Subject` erhalten.

## Authentifizierungsfluss

```text
Browser
  → Blazor SSR Client
  → OIDC Login beim IdentityAccessServer
  → Authentifizierungscookie im Client
  → Access Token
  → AccessTokenHandler
  → Authorization: Bearer <token>
  → CampusLibrary API
  → JWT-Bearer-Validierung
  → ClaimsPrincipal
  → IIdentityGateway
  → UseCase
```

Der Client sendet keine `ReaderId`, um den aktuellen Reader festzulegen. `/me`-UseCases verwenden den Token-Subject.

## Reader-Provisioning und Profil

Ein Identity-Benutzer ist nicht automatisch ein fachlicher Reader. Beim Provisioning wird ein Reader angelegt und dauerhaft mit dem Token-Subject verknüpft.

```text
POST /camplib/v1/readers/me/provision
```

Das Provisioning übernimmt vertrauenswürdige technische Werte aus dem Token:

- `sub` als stabilen Subject
- `preferred_username` beziehungsweise Username als initiale E-Mail
- Erstellungszeit der technischen Identität
- Account-Typ beziehungsweise Rolle

`AdminRights` bleibt aus Kompatibilitätsgründen im Identity-Port, wird von CampusLibrary aber fachlich nicht ausgewertet.

Nach dem Provisioning wird das fachliche Profil vervollständigt:

```text
PUT /camplib/v1/readers/me/profile
```

Die initiale Profilvervollständigung verwendet `ReaderProfileDto` mit Vorname, Nachname und Adresse. Die E-Mail kommt nicht aus einem frei manipulierbaren Profilformular, sondern initial aus der technischen Identität.

Spätere Änderungen erfolgen selektiv über:

```text
PUT /camplib/v1/readers/me/update
```

`ReaderUpdateDto` enthält optionale Werte. `null` bedeutet: bestehender Wert bleibt unverändert.

## Catalog und Loans

Der Catalog bleibt in Teil 6 bewusst weitgehend unabhängig von der Anmeldung sichtbar. Eine systematische Absicherung sämtlicher API-Operationen und feinere UseCase-Autorisierung sind Gegenstand eines späteren Teils.

Loans verwenden Self-Service-Endpunkte:

```text
GET   /camplib/v1/loans/me
POST  /camplib/v1/loans/me
GET   /camplib/v1/loans/me/{loanId}
PATCH /camplib/v1/loans/me/{loanId}/renew
```

Die Ausleihe wird dem aktuellen Reader über das Subject zugeordnet. Der Client sendet keine Reader-ID.

Die Rückgabe am Schalter erfolgt als Mitarbeiteroperation:

```text
PATCH /camplib/v1/loans/{loanId}/return-at-desk
```

Im aktuellen Modell wird der Loan bei erfolgreicher Rückgabe gelöscht. Ein anschließendes `GET` auf denselben Loan liefert deshalb `404 Not Found`.

## DTO-Regeln

Öffentliche HTTP-DTOs gehören dem jeweiligen fachlichen Modul:

```text
Readers/_2_Application/Dtos
Catalog/_2_Application/Dtos
Loans/_2_Application/Dtos
```

Der Client besitzt eigene Transporttypen unter:

```text
CampusLibraryClient/Api/Dtos
```

Der Client referenziert keine Core-Projekte.

Nur echte BC-to-BC-Schnittstellen liegen in BuildingBlocks:

```text
_1_Ports/Contracts
_2_Application/Dtos
```

Beispiele sind Reader-/Catalog-Informationen, die das Loan-Modul über explizite Modulgrenzen benötigt.

## Dokumentation

- [Architektur](2Architecture-ger.md)
- [API](3Api-ger.md)
- [Testing](4Testing-ger.md)
