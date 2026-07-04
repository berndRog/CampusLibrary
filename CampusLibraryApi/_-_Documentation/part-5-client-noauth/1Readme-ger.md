# CampusLibrary — Teil 5: Client ohne aktive Auth

Lehrprojekt für eine modular aufgebaute, DDD-orientierte ASP.NET-Core-Web-API und einen Blazor-SSR-Client.

Englische Version: [1Readme.md](1Readme.md)

## Aktueller Stand

Teil 5 ergänzt die modulare CampusLibrary-API aus Teil 4 um einen echten Web-Client.

Teil 5 enthält damit:

```text
Readers
Catalog
Loans
CampusLibraryClient
```

Die fachlichen API-Module bleiben Eigentümer der Regeln. Der Client referenziert keine Core-Projekte der API, sondern verwendet HTTP-Clients und DTOs.

Bekannter Prüfstand nach der BookItem-Identity-Umstellung:

```text
dotnet build
Build succeeded

dotnet test
196 total, 0 failed, 0 skipped
```

Wichtig: Die automatisierten Tests prüfen im aktuellen Stand im Wesentlichen die API. Reine Client-Layout- und Navigationsänderungen werden durch `dotnet build` und manuelle Browsertests geprüft.

## Branch

```text
part-5/client-noauth
```

## Ziel von Teil 5

Teil 5 zeigt, wie eine bestehende modulare API von einem Blazor-SSR-Client verwendet wird.

Nicht Ziel von Teil 5 ist eine echte Anmeldung oder eine echte API-Autorisierung.

Aktiv in Teil 5:

```text
Blazor SSR Client
HTTP-Zugriff auf CampusLibraryApi
Bootstrap-basiertes Layout
Readers-Liste
Katalogsuche
Buch ausleihen aus Reader-Perspektive
Bücher hinzufügen aus Mitarbeiter-Perspektive
Exemplare zu aktiven Büchern hinzufügen
Bücher deaktivieren
Ausleihen anzeigen
Ausleihe-Details anzeigen
Ausleihen verlängern und zurückgeben
zentrale Fehleranzeige
DevIdentity als simulierte UI-Perspektive
```

Nicht aktiv in Teil 5:

```text
echte Registrierung
echter Login
echte Logout-Session gegen IdentityAccessServer
Access-Token-Weitergabe an die API
geschützte API-Aufrufe
policybasierte API-Autorisierung
Reader-Provisionierung aus einem Token
Reader-Erzeugung in der UI
```

## Warum keine Reader-Erzeugung in Teil 5?

Ein Reader soll später nicht einfach durch ein Mitarbeiterformular im Client entstehen. Der fachlich korrekte Ablauf beginnt mit einem technischen Benutzer im IdentityAccessServer.

Geplanter Ablauf für spätere Teile:

```text
1. Reader registriert sich im IdentityAccessServer.
2. Email ist initial Username.
3. IdentityAccessServer erzeugt einen technischen Benutzer mit Subject.
4. CampusLibraryApi provisioniert daraus einen fachlichen Reader.
5. Reader ergänzt fachliche Profildaten wie Vorname und Nachname.
```

Deshalb bleibt `Reader erstellen` in Teil 5 bewusst weg. Reader stammen in Teil 5 aus Seed- und Testdaten.

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
```

## CampusLibraryClient

Der Client ist eine Blazor-Server-Side-Rendering-Anwendung.

Wichtige Konzepte:

```text
Razor Components
Interactive Server Render Mode für interaktive Seiten
modulbezogene API-Clients
DTOs für den API-Transport
Result<T> für clientseitige Erfolgs-/Fehlerbehandlung
ProblemDetails-basierte Fehleranzeige
Bootstrap-Utilities statt eigener Layout-CSS-Regeln
DevIdentity für Reader-/Mitarbeiter-Perspektive
vorbereitete, aber inaktive AuthN/AuthZ-Infrastruktur
```

Wichtige Client-Ordner:

```text
CampusLibraryClient
├─ Api
│  ├─ Clients
│  ├─ Contracts
│  ├─ Dtos
│  ├─ Errors
│  └─ Auth
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
/readers                                Reader-Liste
/catalog/books                          Katalog
/catalog/books/create                   Buch hinzufügen
/catalog/books/{bookId}/items/add       Exemplar hinzufügen
/catalog/books/{bookId}/deactivate      Buch deaktivieren
/catalog/books/{bookId}/borrow          Buch ausleihen
/loans                                  Ausleihen aus Mitarbeiter-Perspektive
/loans/{loanId}                         Ausleihe-Details
/my/loans                               Ausleihen des aktuellen Readers
/logout                                 Demo-Logout-Seite
/access-denied                          vorbereitete Fehlerseite
/error                                  technische Fehlerseite
```

## DevIdentity in Teil 5

Teil 5 verwendet keine echte Authentifizierung. Damit die UI trotzdem zwischen Reader- und Mitarbeiter-Perspektive unterscheiden kann, verwendet der Client eine DevIdentity.

Beispiel:

```json
{
  "Features": {
    "AuthNEnabled": false,
    "DevIdentityEnabled": true,
    "ApiAccessTokenEnabled": false,
    "AuthZEnabled": false
  },
  "DevIdentity": {
    "ActiveProfile": "EmployeeAdmin",
    "Profiles": {
      "ReaderRita": {
        "IsAuthenticated": true,
        "AccountType": "reader",
        "ReaderId": "00000099-0000-0000-0000-000000000000",
        "DisplayName": "Rita Reader",
        "Email": "r.reader@library.local"
      },
      "EmployeeAdmin": {
        "IsAuthenticated": true,
        "AccountType": "employee",
        "ReaderId": null,
        "DisplayName": "Admin",
        "Email": "admin@mail.local"
      }
    }
  }
}
```

Die DevIdentity ist keine Sicherheit. Sie ist nur ein didaktisches Hilfsmittel für die UI.

## Navigation und Layout

Der Client verwendet ein horizontales Bootstrap-Menü. Der aktive Menüpunkt wird über Bootstrap-Nav-Links hervorgehoben.

Die Menüpunkte hängen von der simulierten Perspektive ab:

```text
Reader:
Home | Katalog | Ausleihen | Logout

Mitarbeiter:
Home | Katalog | Leser | Ausleihen | Logout
```

Die Startseite enthält eine normale Überschrift und aktuelle Meldungen. Das Layout verwendet Bootstrap-Klassen wie `container-fluid`, `px-4`, `navbar`, `nav-pills`, `table`, `card`, `row`, `col-*`, `badge` und `btn`.

Eigene CSS-Regeln bleiben auf Blazor-spezifische Validierungs- und Fehlerdarstellung begrenzt.

## API-Clients pro Modul

Der Client verwendet eine API-Client-Abstraktion pro fachlichem Bereich:

```text
IReaderClient -> ReaderClient
IBookClient   -> BookClient
ILoanClient   -> LoanClient
```

Diese Clients rufen die vorhandenen CampusLibraryApi-Routen auf:

```text
/camplib/v1/readers
/camplib/v1/books
/camplib/v1/loans
```

Die API-Clients werden registriert über:

```text
AddCampusLibraryClients(...)
```

Die BaseUrl steht in `appsettings.json`:

```json
{
  "CampusLibraryApi": {
    "BaseUrl": "https://localhost:8010/"
  }
}
```

## Readers

Die Readers-Seite zeigt Reader an, die von der API geliefert werden.

Aktuell angezeigt:

```text
Name
Email
Status
```

`Subject` wird in der UI nicht angezeigt. Es ist eine technische Identität und gehört später zum AuthN/AuthZ-Thema.

Reader werden in Teil 5 nicht in der UI angelegt. Das ist eine bewusste didaktische Entscheidung, weil der spätere korrekte Ablauf über IdentityAccessServer, Subject und Provisionierung läuft.

## Catalog

Der Katalog ist für Reader und Mitarbeiter sichtbar.

Die Katalogtabelle verwendet diese fachliche Struktur:

```text
Aktion | Titel | Autorinnen/Autoren | ISBN | Exemplare | Status
```

Die Spalte `Titel` enthält Titel und Untertitel direkt zusammen. Der Untertitel präzisiert den Titel und wird daher nicht als entfernte eigene Spalte geführt.

Die Spalte `Exemplare` zeigt:

```text
ausgeliehen / gesamt
```

Mitarbeiterfunktionen im Katalog:

```text
Buch hinzufügen
Exemplar zu aktivem Buch hinzufügen
aktives Buch deaktivieren
aktive und inaktive Bücher anzeigen
```

Readerfunktionen im Katalog:

```text
Bücher suchen
Buch ausleihen, wenn mindestens ein Exemplar tatsächlich verfügbar ist
```

## BookItem und Inventarnummer

Die API hat keine separate `InventoryNumber`-Property mehr.

Technisch gilt:

```text
BookItem.Id identifiziert ein Exemplar eindeutig.
```

In der UI wird diese Id weiterhin als fachlicher Begriff `Inventarnummer` angezeigt.

Damit gilt:

```text
technisch:  BookItemId
fachlich/UI: Inventarnummer
```

## Loans

Die Loans-Seiten zeigen Ausleihen und Details.

Wichtige Seiten:

```text
/loans          Mitarbeiterperspektive: ausgeliehene Loans
/my/loans       Readerperspektive: eigene Loans
/loans/{id}     Detailansicht einer Ausleihe
```

Renew und Return werden in der Detailansicht ausgeführt, nicht direkt in der Übersicht. Die Detailansicht zeigt Buchdaten, Readerdaten und Ausleihdaten.

`BookIsActive` und `IsAvailableForLoan` können im DTO vorhanden sein, werden aber in der normalen Ausleihe-Detailseite nicht als zentrale fachliche Information angezeigt. Eine bestehende Ausleihe soll nicht durch technische Verfügbarkeitsflags irritieren.

## Fehlerbehandlung

API-Fehler werden zentral behandelt.

Wichtige Typen und Komponenten:

```text
BaseApiClient<TClient>
ApiError
Result<T>
ErrorAlert.razor
```

Die API liefert Fehler als `ProblemDetails`. Der Client bildet diese auf `ApiError` ab und zeigt sie über `ErrorAlert` an.

## Geplante Fortsetzung

```text
Teil 6: Client-AuthN mit Login/Logout gegen IdentityAccessServer
Teil 7: AuthN/AuthZ in CampusLibraryApi, Reader-Provisionierung über Token
Teil 8: geschützter API-Zugriff aus dem Client mit Access Token
```

Der geplante Reader-Ablauf:

```text
POST /camplib/v1/readers/me/provision
- API liest Subject und Email aus dem Access Token.
- Kein Subject im Body.

POST /camplib/v1/readers/me/profile
- Client sendet nur fachliche Profildaten wie Vorname und Nachname.
```
