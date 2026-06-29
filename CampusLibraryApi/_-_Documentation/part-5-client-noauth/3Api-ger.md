# API und Client-Vertrag — Teil 5

Dieses Dokument beschreibt die API-Oberfläche, die der `CampusLibraryClient` in Teil 5 verwendet.

Teil 5 ergänzt vor allem keine neuen Backend-API-Endpunkte. Teil 5 ergänzt einen Blazor-SSR-Client, der die vorhandenen CampusLibraryApi-Endpunkte aus Teil 4 verwendet.

Englische Version: [3Api.md](3Api.md)

## Base URLs

CampusLibraryApi:

```text
https://localhost:8010/
```

CampusLibraryClient:

```text
konfiguriert über die Launch Settings des Clients
```

Der Client konfiguriert die Backend-API-URL in `CampusLibraryClient/appsettings.json`:

```json
{
  "CampusLibraryApi": {
    "BaseUrl": "https://localhost:8010/"
  }
}
```

## Registrierung der API-Clients

Der Client registriert alle Modul-Clients über:

```text
AddCampusLibraryClients(configuration, useAccessToken)
```

In Teil 5 gilt:

```text
useAccessToken = false
```

Requests werden daher ohne Bearer Token gesendet.

## Client-API-Struktur

Der Client verwendet drei modulbezogene API-Client-Abstraktionen:

```text
IReaderClient
IBookClient
ILoanClient
```

Die konkreten Implementierungen sind:

```text
ReaderClient
BookClient
LoanClient
```

Alle drei Clients verwenden den benannten HttpClient:

```text
Common.CampusLibraryApiClientName
```

## Fehlervertrag

Das Backend liefert Fehler als `ProblemDetails`.

Der Client bildet Backend-Fehler ab auf:

```text
ApiError
```

Der gemeinsame Result-Typ ist:

```text
Result<T>
```

Typische Client-Behandlung:

```text
Result<T>.IsSuccess -> Daten anzeigen
Result<T>.IsFailure -> ErrorAlert anzeigen
```

## Auth-Status

Teil 5 verwendet anonyme API-Aufrufe.

Feature-Flags:

```json
{
  "Features": {
    "AuthNEnabled": false,
    "ApiAccessTokenEnabled": false,
    "AuthZEnabled": false
  }
}
```

Vorbereitet, aber inaktiv:

```text
AccessTokenHandler
AuthenticationExtensions
AuthorizationExtensions
IdentityController
EntryController
```

Der `AccessTokenHandler` wird nicht an den CampusLibraryApi-HttpClient gehängt, solange `ApiAccessTokenEnabled=false` ist.

## Readers API aus Sicht des Clients

### Readers laden

Client-Methode:

```text
IReaderClient.GetAllAsync(includeInactive=false)
```

HTTP-Aufruf:

```http
GET /camplib/v1/readers?includeInactive=false
```

Verwendet von:

```text
/readers
ReadersList.razor
```

Response-Typ:

```text
IEnumerable<ReaderDto>
```

Angezeigte Felder:

```text
Firstname
Lastname
Email
Subject
IsActive
```

### Reader per id laden

Client-Methode:

```text
IReaderClient.GetByIdAsync(id, includeInactive=false)
```

HTTP-Aufruf:

```http
GET /camplib/v1/readers/{id}?includeInactive=false
```

### Reader per E-Mail laden

Client-Methode:

```text
IReaderClient.GetByEmailAsync(email, includeInactive=false)
```

HTTP-Aufruf:

```http
GET /camplib/v1/readers/email?email={email}&includeInactive=false
```

### Reader anlegen

Client-Methode:

```text
IReaderClient.CreateAsync(dto)
```

HTTP-Aufruf:

```http
POST /camplib/v1/readers
Content-Type: application/json
```

Request-Typ:

```text
ReaderCreateDto
```

### Reader aktualisieren

Client-Methode:

```text
IReaderClient.UpdateAsync(id, dto)
```

HTTP-Aufruf:

```http
PUT /camplib/v1/readers/{id}
Content-Type: application/json
```

Request-Typ:

```text
ReaderUpdateDto
```

### Reader deaktivieren

Client-Methode:

```text
IReaderClient.DeactivateAsync(id)
```

Aktueller Client-Aufruf:

```http
DELETE /camplib/v1/readers/{id}
```

Diesen Punkt bitte gegen die aktuelle API-Route prüfen. Falls die finale API einen expliziten Deactivate-Endpunkt verwendet, sollte der Client entsprechend angepasst werden, zum Beispiel:

```http
PATCH /camplib/v1/readers/{id}/deactivate
```

## Catalog API aus Sicht des Clients

### Bücher laden

Client-Methode:

```text
IBookClient.GetAllAsync(includeInactive=false)
```

HTTP-Aufruf:

```http
GET /camplib/v1/books?includeInactive=false
```

Verwendet von:

```text
/catalog/books
BooksList.razor
```

Response-Typ:

```text
IEnumerable<BookListItemDto>
```

Angezeigte Felder:

```text
Title
Subtitle
AuthorsText
Isbn
AvailableItems
TotalItems
IsActive
```

### Buch per id laden

Client-Methode:

```text
IBookClient.GetByIdAsync(id, includeInactive=false)
```

HTTP-Aufruf:

```http
GET /camplib/v1/books/{id}?includeInactive=false
```

Response-Typ:

```text
BookDetailDto
```

### Bücher suchen

Client-Methode:

```text
IBookClient.SearchAsync(searchField, searchText, includeInactive=false)
```

HTTP-Aufruf:

```http
GET /camplib/v1/books/search?searchField=Title&searchText=Clean%20Code&includeInactive=false
```

Suchfelder:

```text
Title
AuthorLastName
Isbn
```

### Buch anlegen

Client-Methode:

```text
IBookClient.CreateAsync(dto)
```

HTTP-Aufruf:

```http
POST /camplib/v1/books
Content-Type: application/json
```

Request-Typ:

```text
BookCreateDto
```

### BookItem hinzufügen

Client-Methode:

```text
IBookClient.AddBookItemAsync(bookId, dto)
```

HTTP-Aufruf:

```http
POST /camplib/v1/books/{bookId}/items
Content-Type: application/json
```

Request-Typ:

```text
BookItemAddDto
```

## Loans API aus Sicht des Clients

### Ausgeliehene Loans laden

Client-Methode:

```text
ILoanClient.GetBorrowedAsync()
```

HTTP-Aufruf:

```http
GET /camplib/v1/loans
```

Verwendet von:

```text
/loans
LoansList.razor
```

Response-Typ:

```text
IEnumerable<LoanListItemDto>
```

Angezeigte Felder:

```text
Reader firstname/lastname
Book title
Inventory number
Loan date
Due date
Status
IsOverdue
```

### Loan per id laden

Client-Methode:

```text
ILoanClient.GetByIdAsync(id)
```

HTTP-Aufruf:

```http
GET /camplib/v1/loans/{id}
```

Response-Typ:

```text
LoanDetailDto
```

### BookItem ausleihen

Client-Methode:

```text
ILoanClient.BorrowAsync(dto)
```

HTTP-Aufruf:

```http
POST /camplib/v1/loans
Content-Type: application/json
```

Request-Typ:

```text
LoanCreateDto
```

### Loan verlängern

Client-Methode:

```text
ILoanClient.RenewAsync(id)
```

HTTP-Aufruf:

```http
PATCH /camplib/v1/loans/{id}/renew
```

Verwendet durch den Action-Button in `LoansList.razor`.

### Loan an der Theke zurückgeben

Client-Methode:

```text
ILoanClient.ReturnAtDeskAsync(id)
```

HTTP-Aufruf:

```http
PATCH /camplib/v1/loans/{id}/return-at-desk
```

Verwendet durch den Action-Button in `LoansList.razor`.

## Typischer manueller UI-Ablauf in Teil 5

```text
1. CampusLibraryApi starten.
2. CampusLibraryClient starten.
3. Client-Startseite öffnen.
4. Zu Readers navigieren.
5. Zu Catalog / Books navigieren.
6. Buch nach Titel, Autorennachname oder ISBN suchen.
7. Zu Loans navigieren.
8. Eine ausgeliehene Loan verlängern oder zurückgeben.
9. CampusLibraryApi stoppen und Seite neu laden, um ErrorAlert zu beobachten.
```

## Was Teil 5 bewusst nicht macht

```text
Kein Login-Endpunkt ist für die sichtbare UI erforderlich.
Kein Access Token wird an CampusLibraryApi gesendet.
Kein API-Endpunkt wird clientseitig geschützt.
Keine Rollen- oder Policy-Entscheidung ist in der UI aktiv.
```

Die vorbereiteten AuthN/AuthZ-Elemente werden dokumentiert, weil sie den Übergang zu späteren Teilen erleichtern.
