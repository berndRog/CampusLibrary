# CampusLibrary — Teil 5: Client ohne aktive Auth

Lehrprojekt für eine modular aufgebaute, DDD-orientierte ASP.NET-Core-Web-API und einen Blazor-Server-Side-Rendering-Client.

Englische Version: [1Readme.md](1Readme.md)

## Aktueller Stand

Diese Version ergänzt die modulare CampusLibrary-API um einen echten Web-Client.

Teil 5 baut auf Teil 4 auf:

* Readers
* Catalog
* Loans
* CampusLibraryClient

Die fachlichen API-Module behalten ihre Verantwortung. Der neue Client verwendet die vorhandene HTTP-API über modulbezogene API-Clients.

Bekanntes Build-Ergebnis für den aktuellen Startstand von Teil 5:

```text
dotnet build
Build succeeded
```

Teil 5 führt keine aktive Authentifizierung und keine aktive Autorisierung ein. Der Client verwendet die CampusLibraryApi anonym. AuthN/AuthZ-Vorbereitungen dürfen im Code bleiben, sind aber über Feature-Flags deaktiviert.

## Aktueller Branch

```text
part-5/client-noauth
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
```

## Architekturidee

Teil 4 zeigte die modulare API als projektbasierten modularen Monolithen.

Teil 5 ergänzt einen separaten Blazor-SSR-Client. Der Client gehört nicht zum Domain-Core. Er ist eine externe Benutzerschnittstelle und greift über HTTP auf die API zu.

Zentrale Abhängigkeitsidee:

```text
CampusLibraryClient referenziert keine API-Core-Module.
CampusLibraryClient verwendet HTTP-Clients und DTOs.
CampusLibraryApi bleibt Eigentümer der fachlichen Regeln.
Der Client zeigt Daten an und startet API-Workflows.
```

So bleibt die Grenze zwischen Web-UI und Backend-Modulen sichtbar.

## CampusLibraryClient

Der Client ist eine Blazor-Server-Side-Rendering-Anwendung.

Wichtige Konzepte:

```text
Blazor SSR
Razor Components
modulbezogene API-Clients
DTOs für den API-Transport
Result<T> für clientseitige Erfolgs-/Fehlerbehandlung
ProblemDetails-basierte Fehleranzeige
einfache Navigation
vorbereitete, aber inaktive AuthN/AuthZ
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

Aktuell sichtbare Seiten sind:

```text
/
/readers
/catalog/books
/loans
```

Der Client enthält bereits Infrastruktur für spätere Command-Seiten, zum Beispiel Create-/Update-Models und API-Client-Methoden. Der erste Fokus von Teil 5 ist der vertikale Durchstich von Navigation über Listen-Seiten bis zur API-Fehleranzeige.

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

## Fehlerbehandlung

API-Fehler werden zentral in der Client-Infrastruktur behandelt.

Wichtige Typen und Komponenten:

```text
BaseApiClient<TClient>
ApiError
Result<T>
ErrorAlert.razor
```

Die API liefert Fehler als `ProblemDetails`. Der Client bildet diese auf `ApiError` ab und zeigt sie über `ErrorAlert` an.

Netzwerkfehler, ungültige JSON-Antworten und spätere Autorisierungsfehler werden ebenfalls in clientseitige Fehler übersetzt.

## Auth-Status in Teil 5

Teil 5 ist bewusst ein No-Auth-Client-Teil.

Aktiv in Teil 5:

```text
anonyme API-Aufrufe
einfache Navigation
Listen-Seiten
Fehleranzeige
vorbereitete Konfiguration
```

Inaktiv in Teil 5:

```text
Login
Logout
AuthorizeView
[Authorize]
Access-Token-Weitergabe
rollenbasierte UI-Entscheidungen
policybasierte Autorisierung
geschützte API-Aufrufe
```

Feature-Flags im Client machen das explizit:

```json
{
  "Features": {
    "AuthNEnabled": false,
    "ApiAccessTokenEnabled": false,
    "AuthZEnabled": false
  }
}
```

Vorbereitete AuthN/AuthZ-Klassen dürfen im Projekt bleiben, weil sie für die nächsten Teile nützlich sind.

Geplante Fortsetzung:

```text
Teil 6: Client-AuthN mit Login/Logout
Teil 7: AuthN/AuthZ in CampusLibraryApi
Teil 8: geschützter API-Zugriff aus dem Client
```

## Vom Client verwendete Module

## Readers

Die Readers-Seite zeigt Reader an, die von der API geliefert werden.

Wichtige Client-Konzepte:

```text
ReaderDto
IReaderClient
ReaderClient
ReadersList.razor
```

Typischer Client-Aufruf:

```text
GET /camplib/v1/readers?includeInactive=false
```

## Catalog

Die Catalog-Seite zeigt Bücher an und unterstützt Suche.

Wichtige Client-Konzepte:

```text
BookListItemDto
BookSearchField
IBookClient
BookClient
BooksList.razor
```

Typische Client-Aufrufe:

```text
GET /camplib/v1/books?includeInactive=false
GET /camplib/v1/books/search?searchField=Title&searchText=Clean%20Code&includeInactive=false
```

## Loans

Die Loans-Seite zeigt ausgeliehene Loans an und ermöglicht Verlängerung und Rückgabe.

Wichtige Client-Konzepte:

```text
LoanListItemDto
LoanDto
ILoanClient
LoanClient
LoansList.razor
```

Typische Client-Aufrufe:

```text
GET   /camplib/v1/loans
PATCH /camplib/v1/loans/{id}/renew
PATCH /camplib/v1/loans/{id}/return-at-desk
```

## Lokal starten

Zuerst die CampusLibraryApi starten.

Dann den Client starten:

```bash
dotnet run --project CampusLibraryClient
```

Der Client ruft die API-BaseUrl aus `CampusLibraryClient/appsettings.json` auf.

## Didaktisches Ziel

Teil 5 zeigt, dass eine modulare API nicht nur über HTTP-Dateien oder automatisierte Tests geprüft wird, sondern von einem echten Web-Client verwendet werden kann.

Die Studierenden sehen:

```text
wie ein Blazor-SSR-Client eine Backend-API aufruft
wie clientseitige API-Wrapper pro Modul strukturiert werden
wie DTOs die Transportgrenze definieren
wie API-Fehler in der UI angezeigt werden
wie Navigation Seiten mit API-Workflows verbindet
wie AuthN/AuthZ vorbereitet werden kann, ohne es zu früh zu aktivieren
```
