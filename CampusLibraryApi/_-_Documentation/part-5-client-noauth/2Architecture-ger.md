# Architektur: CampusLibrary Teil 5 — Client ohne aktive Auth

Dieses Dokument beschreibt die Architektur von Teil 5 des Projekts `CampusLibrary`.

Teil 5 ergänzt die modulare CampusLibrary-API um einen Blazor-SSR-Client. Die API besteht weiterhin aus den Modulen Readers, Catalog und Loans aus Teil 4. Der Client verwendet die API über HTTP und referenziert keine API-Core-Projekte.

Englische Version: [2Architecture.md](2Architecture.md)

## Architekturziel

Teil 5 macht folgende Konzepte sichtbar:

```text
Backend-API wird von einem echten Web-Client verwendet
Frontend und Backend bleiben getrennt
API-Clients kapseln HTTP-Zugriffe
DTOs bilden die Transportgrenze
Result<T> und ErrorAlert kapseln Fehlerbehandlung
Bootstrap bildet das UI-Layout
DevIdentity simuliert UI-Perspektiven ohne echte AuthN/AuthZ
```

## Solution-Sicht

```text
CampusLibraryApi
├─ CampusLibraryApi_1_Web
├─ CampusLibraryApi_2_BuildingBlocks
├─ CampusLibraryApi_3_Core_Readers
├─ CampusLibraryApi_3_Core_Catalog
├─ CampusLibraryApi_3_Core_Loan
├─ CampusLibraryApi_4_Infrastructure
└─ CampusLibraryApiTest

CampusLibraryClient
```

Der Client ist bewusst ein eigenes Projekt.

```text
CampusLibraryClient -> HTTP -> CampusLibraryApi
CampusLibraryClient -/-> Core_Readers
CampusLibraryClient -/-> Core_Catalog
CampusLibraryClient -/-> Core_Loan
```

## Client-Architektur

```text
CampusLibraryClient
├─ Api
│  ├─ Clients        konkrete HTTP-Clients
│  ├─ Contracts      Client-Interfaces
│  ├─ Dtos           Transportmodelle
│  ├─ Errors         ApiError
│  └─ Auth           vorbereitete Token-Infrastruktur
├─ Core              Result<T>, FeatureFlags, Common
├─ Extensions        DI-Registrierung
├─ Security          CurrentUserProvider, Rollen, Policies
├─ Shared            gemeinsame Hilfstypen
└─ Ui
   ├─ Components     Layout, Navigation, ErrorAlert
   ├─ Controllers    vorbereitete Auth-Controller
   ├─ Models         UI-Formularmodelle
   └─ Pages          Razor Pages / Components
```

## Dependency Rule

Der Client kennt die API nur über HTTP.

```text
UI Page
  -> IBookClient / IReaderClient / ILoanClient
    -> BookClient / ReaderClient / LoanClient
      -> HttpClient
        -> CampusLibraryApi
```

Die fachlichen Regeln bleiben in der API. Der Client prüft nur UI-nahe Dinge, zum Beispiel ob ein Button angezeigt werden soll oder ob eine Eingabe lokal vollständig ist.

## Render-Modell

Interaktive Seiten verwenden:

```razor
@rendermode InteractiveServer
```

Dadurch können Buttons, Formulare und Ladezustände in Razor-Komponenten verwendet werden, ohne für Teil 5 ein separates JavaScript-Frontend einzuführen.

## API-Client-Schicht

Die drei fachlichen Clientadapter sind:

```text
ReaderClient
BookClient
LoanClient
```

Alle verwenden den benannten HttpClient:

```text
Common.CampusLibraryApiClientName
```

Die BaseUrl kommt aus:

```json
{
  "CampusLibraryApi": {
    "BaseUrl": "https://localhost:8010/"
  }
}
```

## DTOs als Transportgrenze

Der Client definiert eigene DTOs passend zur HTTP-API. Diese DTOs sind keine Domain-Objekte.

Wichtige aktuelle DTO-Entscheidungen:

```text
BookItemDto enthält Id, BookId und Status.
BookItemDto enthält keine InventoryNumber mehr.
BookItemAddDto enthält nur noch eine optionale Id.
LoanListItemDto enthält BookItemId, aber keine InventoryNumber.
LoanDetailDto enthält Reader-Email und BookItemId, aber keine InventoryNumber.
```

Die UI darf `BookItemId` als `Inventarnummer` beschriften. Der Code bleibt aber bei `BookItemId`.

## CurrentUserProvider

Teil 5 trennt die aktuelle Benutzerperspektive über ein Interface:

```text
ICurrentUserProvider
```

Implementierungen:

```text
DevCurrentUserProvider       aktive Teil-5-Simulation
ClaimsCurrentUserProvider    vorbereitet für echte AuthN
AnonymousCurrentUserProvider Fallback/No-User-Fall
```

`DevCurrentUserProvider` liest das aktive Profil aus `appsettings.json`.

Beispiele:

```text
ReaderRita      AccountType=reader, ReaderId gesetzt
EmployeeAdmin   AccountType=employee, ReaderId=null
```

Diese Information steuert nur die UI-Perspektive. Sie ersetzt keine echte Autorisierung.

## UI-Perspektiven

### Reader

Reader können:

```text
Katalog anzeigen
Bücher suchen
Buch ausleihen, wenn ein Exemplar tatsächlich verfügbar ist
eigene Ausleihen anzeigen
Ausleihe-Details öffnen
```

### Mitarbeiter

Mitarbeiter können:

```text
Reader-Liste anzeigen
Katalog anzeigen, inklusive inaktiver Bücher
Buch hinzufügen
Exemplar zu aktivem Buch hinzufügen
aktives Buch deaktivieren
Ausleihen anzeigen
Ausleihe-Details öffnen
Ausleihe verlängern
Ausleihe zurückgeben
```

## Warum Reader-Erzeugung nicht im Client liegt

Das Erzeugen eines Readers ist in Teil 5 bewusst nicht als Mitarbeiterfunktion umgesetzt.

Die fachliche Zielarchitektur für spätere Teile lautet:

```text
technischer Benutzer im IdentityAccessServer
  -> Subject und Email
  -> Reader-Provisionierung in CampusLibraryApi
  -> Reader ergänzt Vorname und Nachname
```

Ein Teil-5-Formular `Reader hinzufügen` würde einen falschen Ablauf zeigen. Deshalb bleiben Reader in Teil 5 Seed-/Testdaten.

## Katalog-Architektur

Der Katalog verwendet:

```text
BooksList.razor        Liste, Suche, rollenabhängige Aktionen
BookCreate.razor       Buch hinzufügen
BookItemAdd.razor      Exemplar hinzufügen
BookDeactivate.razor   Buch deaktivieren
BorrowBook.razor       Buch ausleihen aus Reader-Perspektive
```

Die Katalogtabelle ist fachlich so aufgebaut:

```text
Aktion | Titel | Autorinnen/Autoren | ISBN | Exemplare | Status
```

Die Aktion steht vorne, weil sie bei schmaleren Fenstern nicht abgeschnitten werden soll. Titel und Untertitel stehen gemeinsam in einer Spalte, weil der Untertitel den Titel präzisiert.

## Exemplar-Identität

Die separate Inventarnummer wurde entfernt.

```text
BookItem.Id ist eindeutig.
```

Die UI bezeichnet diese Id weiterhin als Inventarnummer:

```text
BookItemId -> Inventarnummer in der Oberfläche
```

Das vermeidet eine doppelte Identität und hält das Modell einfacher.

## Ausleih-Architektur

`BorrowBook.razor` lädt:

```text
BookDetailDto mit BookItems
aktuell ausgeliehene Loans
```

Daraus berechnet die UI, welche Exemplare wirklich verfügbar sind:

```text
BookItem.Status == Available
und
BookItem.Id ist nicht in den aktuell ausgeliehenen BookItemIds
```

Der Borrow-Request sendet:

```text
ReaderId aus CurrentUserProvider
BookItemId der ausgewählten Inventarnummer
```

## Loan-Details

Die Übersichtsseiten führen zur Detailseite:

```text
/loans/{loanId}
```

Renew und Return gehören in die Detailansicht. Dadurch bleibt die Übersicht einfacher und die fachliche Entscheidung wird vor der Aktion sichtbar.

## Auth-Vorbereitung ohne Aktivierung

Teil 5 enthält vorbereitete Klassen, aktiviert sie aber nicht:

```text
AccessTokenHandler
AuthenticationExtensions
AuthorizationExtensions
IdentityController
EntryController
ClaimsCurrentUserProvider
```

Feature-Flags:

```json
{
  "Features": {
    "AuthNEnabled": false,
    "DevIdentityEnabled": true,
    "ApiAccessTokenEnabled": false,
    "AuthZEnabled": false
  }
}
```

Bedeutung:

```text
AuthNEnabled=false          -> kein echter Login-/Logout-Flow
DevIdentityEnabled=true     -> simulierte UI-Perspektive
ApiAccessTokenEnabled=false -> keine Access-Token-Weitergabe
AuthZEnabled=false          -> keine echte Policy-Autorisierung
```

## Geplante Auth-Architektur

Spätere Zielarchitektur:

```text
Teil 6: Client meldet Benutzer am IdentityAccessServer an.
Teil 7: CampusLibraryApi validiert Bearer Tokens.
Teil 8: Client sendet Access Token an geschützte API-Endpunkte.
```

Reader-Provisionierung später:

```text
POST /camplib/v1/readers/me/provision
Authorization: Bearer <access_token>

API liest Subject und Email aus dem Token.
```

Profilpflege später:

```text
POST /camplib/v1/readers/me/profile
Authorization: Bearer <access_token>

Body enthält nur Vorname und Nachname.
```

## Didaktischer Kern

Teil 5 soll zeigen:

```text
Ein modularer API-Backend-Stand kann von einem echten Web-Client genutzt werden.
Der Client bleibt technisch getrennt vom Domain-Core.
UI-Perspektiven können für die Lehre simuliert werden, ohne echte Sicherheit vorwegzunehmen.
Reader-Provisionierung gehört erst in den AuthN/AuthZ-Teil.
```
