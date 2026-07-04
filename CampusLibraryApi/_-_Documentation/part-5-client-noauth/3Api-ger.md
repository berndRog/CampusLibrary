# API-Nutzung aus Sicht des Clients — Teil 5

Dieses Dokument beschreibt, welche CampusLibraryApi-Endpunkte der Blazor-SSR-Client in Teil 5 verwendet.

Englische Version: [3Api.md](3Api.md)

## Grundidee

Der Client ruft die API über typisierte API-Clients auf:

```text
IReaderClient -> ReaderClient
IBookClient   -> BookClient
ILoanClient   -> LoanClient
```

Die API-Basisadresse kommt aus `appsettings.json`:

```json
{
  "CampusLibraryApi": {
    "BaseUrl": "https://localhost:8010/"
  }
}
```

Alle fachlichen Routen liegen unter:

```text
/camplib/v1
```

## Auth-Status in Teil 5

Teil 5 verwendet keine echten geschützten API-Aufrufe.

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

Der `AccessTokenHandler` wird nicht an den CampusLibraryApi-HttpClient gehängt, solange `ApiAccessTokenEnabled=false` ist.

`DevIdentity` steuert nur die UI-Perspektive. Es ersetzt keine Authentifizierung und keine Autorisierung.

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

```csharp
public sealed record ReaderDto(
   Guid Id,
   string? Firstname,
   string? Lastname,
   string? Email,
   AddressDto? AddressDto,
   bool IsActive,
   string? Subject
);
```

In der UI angezeigt:

```text
Name
Email
Status
```

`Subject` wird nicht angezeigt. Es ist eine technische Identität und wird erst im AuthN/AuthZ-Kontext fachlich wichtig.

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

Die API-Client-Methode kann technisch vorhanden sein:

```text
IReaderClient.CreateAsync(dto)
```

In Teil 5 gibt es aber bewusst keine sichtbare UI-Funktion `Reader anlegen`.

Grund:

```text
Reader sollen später aus einem technischen Benutzer im IdentityAccessServer provisioniert werden.
```

Teil 5 verwendet Reader aus Seed-/Testdaten.

### Reader aktualisieren

Die API-Client-Methode kann technisch vorhanden sein:

```text
IReaderClient.UpdateAsync(id, dto)
```

Auch das ist in Teil 5 nicht der zentrale UI-Workflow.

### Reader deaktivieren

Falls die UI diese Funktion nutzt, ruft der Client den bestehenden API-Endpunkt auf:

```text
IReaderClient.DeactivateAsync(id)
```

Aktueller Client-Aufruf:

```http
DELETE /camplib/v1/readers/{id}
```

Fachlich ist das ein Deaktivieren, kein physisches Löschen.

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

Mitarbeiter laden auch inaktive Bücher:

```http
GET /camplib/v1/books?includeInactive=true
```

Response-Typ:

```csharp
public sealed record BookListItemDto(
   Guid Id,
   string? AuthorsText,
   string? Title,
   string? Subtitle,
   string? Isbn,
   int TotalItems,
   int AvailableItems,
   bool IsActive
);
```

Die Katalogtabelle zeigt:

```text
Aktion | Titel | Autorinnen/Autoren | ISBN | Exemplare | Status
```

Die Spalte `Titel` enthält Titel und Untertitel. Die Spalte `Exemplare` zeigt ausgeliehen / gesamt.

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

```csharp
public sealed record BookDetailDto(
   Guid Id,
   string? AuthorsText,
   string? Title,
   string? Subtitle,
   string? Isbn,
   IReadOnlyList<BookItemDto>? BookItems,
   int TotalItems,
   int AvailableItems,
   bool IsActive
);
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

```csharp
public sealed record BookCreateDto(
   string? AuthorsText,
   string? Title,
   string? Subtitle,
   string? Isbn,
   string? Id = null
);
```

Verwendet von:

```text
/catalog/books/create
BookCreate.razor
```

Diese Seite ist für Mitarbeiter vorgesehen.

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

```csharp
public sealed record BookItemAddDto(
   string? Id = null
);
```

Response-Typ:

```csharp
public sealed record BookItemDto(
   Guid Id,
   Guid BookId,
   int Status
);
```

Verwendet von:

```text
/catalog/books/{bookId}/items/add
BookItemAdd.razor
```

Die API hat keine separate `InventoryNumber` mehr. Die UI zeigt `BookItem.Id` als Inventarnummer an.

### Buch deaktivieren

Client-Methode:

```text
IBookClient.DeactivateAsync(bookId)
```

HTTP-Aufruf:

```http
PATCH /camplib/v1/books/{bookId}/deactivate
```

Verwendet von:

```text
/catalog/books/{bookId}/deactivate
BookDeactivate.razor
```

Der Client ruft keinen separaten BookItem-Delete-Endpunkt auf. Das Entfernen beziehungsweise Sperren der Exemplare ist Aufgabe des API-Use-Cases.

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
/my/loans
MyLoansList.razor
```

Response-Typ:

```csharp
public sealed record LoanListItemDto(
   Guid Id,
   Guid ReaderId,
   string? Firstname,
   string? Lastname,
   Guid BookItemId,
   string? Title,
   string? Subtitle,
   DateTime LoanDate,
   DateTime DueDate,
   int Status,
   bool IsOverdue
);
```

`BookItemId` wird in der UI als Inventarnummer angezeigt.

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

```csharp
public sealed record LoanDetailDto(
   Guid Id,
   Guid ReaderId,
   string? Firstname,
   string? Lastname,
   string? Email,
   Guid BookItemId,
   Guid BookId,
   string? AuthorsText,
   string? Title,
   string? Subtitle,
   string? Isbn,
   bool BookIsActive,
   bool IsAvailableForLoan,
   DateTime LoanDate,
   DateTime DueDate,
   DateTime? ReturnedAt,
   int Status,
   int RenewalCount,
   bool IsOverdue,
   bool CanRenew
);
```

Verwendet von:

```text
/loans/{loanId}
LoanDetails.razor
```

Die Detailseite zeigt Buchdaten, Inventarnummer, Readerdaten mit Email und Ausleihdaten. Renew und Return werden von dort gestartet.

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

```csharp
public sealed record LoanCreateDto(
   Guid ReaderId,
   Guid BookItemId,
   string? Id = null
);
```

Verwendet von:

```text
/catalog/books/{bookId}/borrow
BorrowBook.razor
```

Die UI wählt ein tatsächlich verfügbares BookItem aus und sendet dessen `BookItemId`.

### Loan zurückgeben

Client-Methode:

```text
ILoanClient.ReturnAtDeskAsync(id)
```

HTTP-Aufruf:

```http
PATCH /camplib/v1/loans/{id}/return-at-desk
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

## Fehlerbehandlung

Alle API-Clients verwenden:

```text
BaseApiClient<TClient>
```

Erfolgreiche Antworten werden als `Result<T>.Success` zurückgegeben. Fehlerhafte Antworten werden als `Result<T>.Failure(ApiError)` zurückgegeben.

Die UI zeigt Fehler über:

```text
ErrorAlert.razor
```

## Spätere Reader-Provisionierung

Die später geplanten Endpunkte gehören nicht zu Teil 5, werden aber für die Fortsetzung festgehalten.

```http
POST /camplib/v1/readers/me/provision
Authorization: Bearer <access_token>
```

Die API liest `subject` und `email` aus dem Token. Der Client sendet kein Subject im Body.

```http
POST /camplib/v1/readers/me/profile
Authorization: Bearer <access_token>
Content-Type: application/json
```

Beispielbody:

```json
{
  "firstname": "Erika",
  "lastname": "Mustermann"
}
```
