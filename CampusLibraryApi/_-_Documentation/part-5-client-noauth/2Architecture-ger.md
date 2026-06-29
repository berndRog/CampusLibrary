# Architektur: CampusLibrary Teil 5 — Client ohne aktive Auth

Dieses Dokument beschreibt die Architektur von Teil 5 des Projekts `CampusLibrary`.

Teil 5 ergänzt die modulare CampusLibrary-API um einen Blazor-Server-Side-Rendering-Client. Die API besteht weiterhin aus den Modulen Readers, Catalog und Loans aus Teil 4. Der Client verwendet die API über HTTP und referenziert keine API-Core-Projekte.

Bekanntes Build-Ergebnis:

```text
dotnet build
Build succeeded
```

## Architekturziel

Teil 5 macht folgende Konzepte für den Unterricht sichtbar:

* eine modulare Backend-API wird von einem echten Web-Client verwendet
* Trennung zwischen Backend-Modulen und Frontend-Client
* API-Clients als typisierte clientseitige Adapter
* DTOs als Transportmodelle an der HTTP-Grenze
* clientseitige Result- und Fehlerbehandlung
* Blazor-SSR-Seiten und Komponenten
* einfache Navigation über Backend-Module hinweg
* vorbereitete, aber inaktive AuthN/AuthZ-Infrastruktur

## Lösungsstruktur

```text
CampusLibrary
├─ CampusLibraryApi
├─ CampusLibraryApi_1_Web
├─ CampusLibraryApi_2_BuildingBlocks
├─ CampusLibraryApi_3_Core_Readers
├─ CampusLibraryApi_3_Core_Catalog
├─ CampusLibraryApi_3_Core_Loan
├─ CampusLibraryApi_4_Infrastructure
├─ CampusLibraryApiTest
└─ CampusLibraryClient
```

## Backend-Architektur

Das Backend bleibt ein projektbasierter modularer Monolith.

Die API wird als eine ASP.NET-Core-Anwendung deployt. Intern ist der Code nach Verantwortung getrennt:

```text
Web/API-Projekt       -> HTTP-Controller
BuildingBlocks        -> gemeinsame Abstraktionen und modulübergreifende Contracts
Core_Readers          -> Readers-Modul
Core_Catalog          -> Catalog-Modul
Core_Loan             -> Loans-Modul
Infrastructure        -> EF Core, Repositories, ReadModels, Contract-Implementierungen
CampusLibraryApi      -> ausführbare Anwendung und Composition Root
```

Core-Module hängen nicht vom Client ab. Der Client ist eine separate Anwendung und kommuniziert über HTTP.

## Client-Architektur

Das Projekt `CampusLibraryClient` ist eine Blazor-SSR-Anwendung.

Wichtige Struktur:

```text
CampusLibraryClient
├─ Api
│  ├─ Auth
│  ├─ Clients
│  ├─ Contracts
│  ├─ Dtos
│  └─ Errors
├─ Core
│  ├─ FeatureFlags.cs
│  ├─ Result.cs
│  └─ Utils
├─ Extensions
├─ Security
├─ Shared
│  └─ Logging
└─ Ui
   ├─ Components
   ├─ Controllers
   ├─ Models
   └─ Pages
```

Die Client-Architektur folgt derselben Grundidee wie die Backend-Module: Verantwortlichkeiten trennen und Grenzen sichtbar halten.

## Clientseitige API-Adapter-Schicht

Der Client besitzt einen typisierten API-Adapter pro Backend-Bereich:

```text
IReaderClient -> ReaderClient
IBookClient   -> BookClient
ILoanClient   -> LoanClient
```

Diese Clients sind keine Domain Services. Sie sind HTTP-Adapter auf Clientseite.

Sie sind verantwortlich für:

```text
URLs aufbauen
Request-DTOs serialisieren
Response-DTOs deserialisieren
ProblemDetails auf ApiError abbilden
Result<T> zurückgeben
ausgehende Aufrufe und Fehler loggen
```

Das Basisverhalten liegt in:

```text
BaseApiClient<TClient>
```

So wird HTTP-Erfolgs- und Fehlerbehandlung nicht in jedem konkreten Client dupliziert.

## DTO-Grenze

Der Client verwendet eigene DTOs unter:

```text
CampusLibraryClient/Api/Dtos
```

Diese DTOs spiegeln den HTTP-Vertrag der CampusLibraryApi. Sie sind Transportmodelle, keine Domain Entities.

Beispiele:

```text
ReaderDto
BookListItemDto
BookDetailDto
BookCreateDto
BookItemDto
LoanListItemDto
LoanDetailDto
LoanCreateDto
```

Damit wird die HTTP-Grenze explizit:

```text
Domain-Objekte leben in den Backend-Core-Modulen.
DTOs werden über HTTP ausgetauscht.
Der Client manipuliert keine Backend-Aggregates direkt.
```

## Seiten und Komponenten

Die sichtbare UI in Teil 5 ist bewusst einfach.

Seiten:

```text
Home.razor
ReadersList.razor
BooksList.razor
LoansList.razor
Error.razor
AccessDenied.razor
```

Gemeinsame UI-Komponenten:

```text
MainLayout.razor
NavMenu.razor
TopMenu.razor
ErrorAlert.razor
```

Gemeinsames Seitenverhalten liegt in:

```text
BasePage.cs
```

Die ersten vertikalen Durchstiche sind:

```text
Navigation -> ReadersList -> IReaderClient -> CampusLibraryApi
Navigation -> BooksList   -> IBookClient   -> CampusLibraryApi
Navigation -> LoansList   -> ILoanClient   -> CampusLibraryApi
```

## Architektur der Fehlerbehandlung

Der Client verwendet ein einheitliches Result-Modell:

```text
Result<T>
```

Erfolgreiche API-Aufrufe liefern einen Wert. Fehlgeschlagene API-Aufrufe liefern einen `ApiError`.

Die Fehler-Pipeline ist:

```text
CampusLibraryApi liefert ProblemDetails
BaseApiClient liest ProblemDetails
BaseApiClient bildet sie auf ApiError ab
Page speichert Error
ErrorAlert zeigt den Fehler an
```

So werden API-Fehler in der UI sichtbar, ohne Exceptions direkt in Razor-Komponenten zu werfen.

## Konfiguration

Die API-BaseUrl wird konfiguriert über:

```json
{
  "CampusLibraryApi": {
    "BaseUrl": "https://localhost:8010/"
  }
}
```

Die API-Clients werden registriert in:

```text
CampusLibraryClientExtensions.AddCampusLibraryClients(...)
```

## Auth-Vorbereitung ohne Aktivierung

Teil 5 aktiviert bewusst keine Authentifizierung und keine Autorisierung.

Der Client enthält aber bereits Vorbereitung für spätere Teile:

```text
Api/Auth/AccessTokenHandler.cs
Extensions/AuthenticationExtensions.cs
Extensions/AuthorizationExtensions.cs
Security/CampusLibraryRoles.cs
Security/CampusLibraryPolicies.cs
Ui/Controllers/IdentityController.cs
Ui/Controllers/EntryController.cs
```

Feature-Flags steuern die Aktivierung:

```json
{
  "Features": {
    "AuthNEnabled": false,
    "ApiAccessTokenEnabled": false,
    "AuthZEnabled": false
  }
}
```

In Teil 5 gilt:

```text
AuthNEnabled=false           -> kein Login-/Logout-Flow
ApiAccessTokenEnabled=false  -> API-Aufrufe bleiben anonym
AuthZEnabled=false           -> keine rollen-/policybasierte UI-Einschränkung
```

Das ermöglicht einen glatten Übergang zu den nächsten Teilen:

```text
Teil 6: Client-AuthN aktivieren
Teil 7: AuthN/AuthZ in CampusLibraryApi ergänzen
Teil 8: Token-Weitergabe und geschützten API-Zugriff aktivieren
```

## Abhängigkeitsregeln

Teil 5 ergänzt eine neue Abhängigkeitsrichtung:

```text
Browser/User -> CampusLibraryClient -> HTTP -> CampusLibraryApi
```

Es entstehen aber keine Projektreferenzen vom Client auf Backend-Core-Module.

Regeln:

```text
CampusLibraryClient darf von ASP.NET-Core-Blazor-Paketen abhängen.
CampusLibraryClient darf DTOs verwenden, die zum API-Vertrag passen.
CampusLibraryClient darf Core_Readers, Core_Catalog oder Core_Loan nicht referenzieren.
CampusLibraryApi darf nicht vom CampusLibraryClient abhängen.
Fachliche Regeln bleiben in den API-Modulen.
```

## Didaktische Zusammenfassung

Teil 5 verschiebt die Perspektive von der Backend-Implementierung zur Backend-Verwendung.

Die Studierenden sehen jetzt:

```text
Die API ist eine wiederverwendbare Grenze.
Der Client ist ein weiterer Adapter.
HTTP-Verträge sind wichtig.
Fehlerverhalten ist Teil der User Experience.
Auth kann vorbereitet werden, ohne die erste Client-Einheit zu dominieren.
```
