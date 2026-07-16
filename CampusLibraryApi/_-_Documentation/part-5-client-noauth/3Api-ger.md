# API-Nutzung aus Sicht des Clients — Teil 5

Dieses Dokument beschreibt die aktuell verwendeten HTTP-Endpunkte und Transporttypen des Branches `part-5/client-noauth`.

Englische Version: [3Api.md](3Api.md)

## Grundidee

Der Blazor-SSR-Client verwendet modulbezogene HTTP-Clients:

```text
IReaderClient -> ReaderClient
IBookClient   -> BookClient
ILoanClient   -> LoanClient
```

Gemeinsame Basisadresse:

```text
https://localhost:8010/camplib/v1
```

Der Client referenziert keine Core-Projekte der API. Er besitzt eigene DTOs mit derselben JSON-Struktur wie die öffentlichen DTOs der Module.

## Auth-Status in Teil 5

Teil 5 verwendet keine echte API-Authentifizierung.

```text
kein Bearer-Token
kein Authorization-Header
keine DevIdentity-Header vom Client
kein laufender IdentityAccessServer erforderlich
```

Die API liest ihre technische Identität aus:

```text
CampusLibraryApi/appsettings.json
```

Datenfluss für `/me`:

```text
DevIdentity -> DevIdentityGateway -> IIdentityGateway
            -> IdentitySubject.Check -> Reader.Subject
```

Für manuelle HTTP-Tests kann die API deshalb direkt aufgerufen werden.

## DevIdentity-Voraussetzungen

Beispiel für das Reader-Profil:

```json
{
  "DevIdentity": {
    "ActiveProfile": "ReaderRita",
    "Profiles": {
      "ReaderRita": {
        "IsAuthenticated": true,
        "Subject": "reader-099",
        "AccountType": "reader",
        "Email": "r.reader@library.local",
        "CreatedAt": "2025-01-01T00:00:00Z",
        "AdminRights": 0
      }
    }
  }
}
```

Die wichtigste Bedingung ist:

```text
DevIdentity.Subject == Reader.Subject in der Datenbank
```

Die E-Mail ist dafür nicht geeignet, weil `Reader.Email` über das Self-Service-Update geändert werden kann.

## Gemeinsame Fehlerantworten

Controller übersetzen fachliche Fehler in `ProblemDetails`.

Typische Statuscodes:

```text
400 Bad Request   ungültige Eingabe
401 Unauthorized  technische Identität nicht als authentifiziert markiert
403 Forbidden     Identität ist kein Reader oder Zugriff nicht erlaubt
404 Not Found     Ressource oder aktueller Reader nicht gefunden
409 Conflict      Eindeutigkeits- oder Zustandskonflikt
```

## Readers API

### ReaderDto

```csharp
public sealed record ReaderDto(
   Guid Id,
   string? Firstname,
   string? Lastname,
   string Email,
   AddressDto? AddressDto,
   bool IsActive,
   string Subject
);
```

### AddressDto

```csharp
public sealed record AddressDto(
   string Street,
   string PostalCode,
   string City,
   string? Country
);
```

### Readers laden

Clientmethode:

```text
IReaderClient.GetAllAsync(includeInactive=false)
```

HTTP:

```http
GET /camplib/v1/readers?includeInactive=false
```

Antwort:

```text
200 OK + ReaderDto[]
```

Mit inaktiven Readern:

```http
GET /camplib/v1/readers?includeInactive=true
```

### Reader per Id laden

Clientmethode:

```text
IReaderClient.GetByIdAsync(id, includeInactive=false)
```

HTTP:

```http
GET /camplib/v1/readers/{id}?includeInactive=false
```

Antworten:

```text
200 OK
404 Not Found
```

### Reader per E-Mail laden

Clientmethode:

```text
IReaderClient.GetByEmailAsync(email, includeInactive=false)
```

HTTP:

```http
GET /camplib/v1/readers/email?email={email}&includeInactive=false
```

Die E-Mail-Suche ist eine normale Query. Sie wird nicht für die technische `/me`-Zuordnung verwendet.

### Reader administrativ anlegen

Clientmethode:

```text
IReaderClient.CreateAsync(ReaderCreateDto dto)
```

HTTP:

```http
POST /camplib/v1/readers
Content-Type: application/json
```

Request:

```csharp
public sealed record ReaderCreateDto(
   string Firstname,
   string Lastname,
   string Email,
   AddressDto AddressDto,
   string Subject,
   string? Id = null
);
```

Beispiel:

```json
{
  "firstname": "Rita",
  "lastname": "Reader",
  "email": "r.reader@library.local",
  "addressDto": {
    "street": "Bibliotheksweg 99",
    "postalCode": "29556",
    "city": "Suderburg",
    "country": "DE"
  },
  "subject": "reader-099",
  "id": "00000099-0000-0000-0000-000000000000"
}
```

Antwort:

```text
201 Created
Location: /camplib/v1/readers/{id}
```

Die optionale Id unterstützt deterministische Tests und HTTP-Skripte.

### Aktuellen Reader aktualisieren

Der frühere administrative Updatepfad wurde durch Self-Service ersetzt.

Clientmethode:

```text
IReaderClient.UpdateMeAsync(ReaderUpdateDto dto)
```

HTTP:

```http
PUT /camplib/v1/readers/me/update
Content-Type: application/json
```

Request:

```csharp
public sealed record ReaderUpdateDto(
   string? Lastname,
   string? Email,
   AddressDto? AddressDto
);
```

Beispiel:

```json
{
  "lastname": "Meier",
  "email": "e.meier@gmx.de",
  "addressDto": {
    "street": "Neue Straße 1",
    "postalCode": "29556",
    "city": "Suderburg",
    "country": "DE"
  }
}
```

Semantik:

```text
null -> bisherigen Wert unverändert lassen
```

Die API bestimmt den Reader über `IIdentityGateway.Subject`. Der Request enthält keine ReaderId.

Mögliche Antworten:

```text
200 OK       Reader aktualisiert
400          ungültige Profilwerte
401          DevIdentity nicht authentifiziert
403          aktives Profil ist kein Reader
404          kein Reader mit dem Subject gefunden
409          neue E-Mail wird bereits verwendet
```

Hinweis: Ein eigener `GET /readers/me`-Endpunkt ist im aktuellen Part-5-Controller nicht öffentlich vorhanden. Die interne Auflösung des aktuellen Readers wird für Update- und Loan-Self-Service genutzt.

### Reader deaktivieren

Clientmethode:

```text
IReaderClient.DeactivateAsync(id)
```

HTTP:

```http
DELETE /camplib/v1/readers/{id}
```

Antwort:

```text
204 No Content
```

Der Reader wird fachlich deaktiviert und bleibt gespeichert. Eine Deaktivierung kann abgelehnt werden, wenn aktuelle Loans bestehen.

## Catalog API

### BookDto

Liste und Detailansicht verwenden denselben Typ:

```csharp
public sealed record BookDto(
   Guid Id,
   string AuthorsText,
   string Title,
   string? Subtitle,
   string Isbn,
   IReadOnlyList<BookItemDto> BookItems,
   int TotalItems,
   int AvailableItems,
   bool IsActive
);
```

### BookItemDto

```csharp
public sealed record BookItemDto(
   Guid Id,
   Guid BookId,
   int Status
);
```

Statuswerte:

```text
1 Available
2 Unavailable
3 Lost
4 Damaged
```

Es gibt im aktuellen Transportvertrag keine `InventoryNumber`.

### Bücher laden

Clientmethode:

```text
IBookClient.GetAllAsync(includeInactive=false)
```

HTTP:

```http
GET /camplib/v1/books?includeInactive=false
```

Mitarbeiter können auch inaktive Bücher laden:

```http
GET /camplib/v1/books?includeInactive=true
```

### Buch per Id laden

Clientmethode:

```text
IBookClient.GetByIdAsync(id, includeInactive=false)
```

HTTP:

```http
GET /camplib/v1/books/{id}?includeInactive=false
```

### Bücher suchen

Clientmethode:

```text
IBookClient.SearchAsync(searchField, searchText, includeInactive=false)
```

HTTP:

```http
GET /camplib/v1/books/search?searchField=Title&searchText=Clean%20Code&includeInactive=false
```

Suchfelder:

```text
Title
AuthorLastName
Isbn
```

Antwort ist ebenfalls `BookDto[]`. Separate Typen wie `BookSearchDto`, `BookListItemDto` oder `BookDetailDto` werden nicht mehr verwendet.

### Buch anlegen

Clientmethode:

```text
IBookClient.CreateAsync(BookCreateDto dto)
```

HTTP:

```http
POST /camplib/v1/books
Content-Type: application/json
```

Request:

```csharp
public sealed record BookCreateDto(
   string AuthorsText,
   string Title,
   string? Subtitle,
   string Isbn,
   string? Id = null
);
```

Antwort:

```text
201 Created + BookDto
```

### BookItem hinzufügen

Clientmethode:

```text
IBookClient.AddBookItemAsync(bookId, BookItemAddDto dto)
```

HTTP:

```http
POST /camplib/v1/books/{bookId}/items
Content-Type: application/json
```

Request:

```csharp
public sealed record BookItemAddDto(
   string? Id = null
);
```

Antwort:

```text
201 Created + BookItemDto
```

### Deaktivierungsinformationen laden

Clientmethode:

```text
IBookClient.GetDeactivationInfoAsync(bookId)
```

HTTP:

```http
GET /camplib/v1/books/{bookId}/deactivation-info
```

Antwort:

```csharp
public sealed record BookDeactivationInfoDto(
   Guid BookId,
   int TotalItems,
   int BorrowedItems,
   IReadOnlyList<BookLoanInfoDto> CurrentLoans
);
```

### Buch deaktivieren

Clientmethode:

```text
IBookClient.DeactivateAsync(bookId)
```

HTTP:

```http
PATCH /camplib/v1/books/{bookId}/deactivate
```

Antwort:

```text
200 OK + BookDto
```

Bei aktuellen Ausleihen kann die Deaktivierung mit einem Konflikt abgelehnt werden.

## Loans API

### LoanDto

Liste und Detailansicht verwenden denselben Typ:

```csharp
public sealed record LoanDto(
   Guid Id,
   Guid ReaderId,
   string Firstname,
   string Lastname,
   string Email,
   Guid BookItemId,
   Guid BookId,
   string AuthorsText,
   string Title,
   string? Subtitle,
   string Isbn,
   bool BookIsActive,
   bool IsAvailableForLoan,
   DateTime LoanDate,
   DateTime DueDate,
   int RenewalCount,
   bool IsOverdue,
   bool CanRenew
);
```

`LoanDto` besitzt bewusst keine Felder `Status` und `ReturnedAt`. Ein gespeicherter Loan ist eine aktuelle Ausleihe.

### Alle aktuellen Loans laden

Clientmethode:

```text
ILoanClient.GetBorrowedAsync()
```

HTTP:

```http
GET /camplib/v1/loans
```

Antwort:

```text
200 OK + LoanDto[]
```

### Eigene aktuelle Loans laden

Clientmethode:

```text
ILoanClient.GetMyBorrowedAsync()
```

HTTP:

```http
GET /camplib/v1/loans/me
```

Die API löst den Reader über Subject auf. Bei einem Reader ohne Loans wird eine leere Liste mit `200 OK` geliefert.

### Loan administrativ per Id laden

```http
GET /camplib/v1/loans/{id}
```

Clientmethode:

```text
ILoanClient.GetByIdAsync(id)
```

### Eigenen Loan per Id laden

```http
GET /camplib/v1/loans/me/{id}
```

Clientmethode:

```text
ILoanClient.GetMyByIdAsync(id)
```

Der Endpunkt prüft, ob der Loan dem über Subject ermittelten Reader gehört.

### BookItem administrativ ausleihen

Clientmethode:

```text
ILoanClient.BorrowAsync(LoanCreateDto dto)
```

HTTP:

```http
POST /camplib/v1/loans
Content-Type: application/json
```

Request:

```csharp
public sealed record LoanCreateDto(
   Guid ReaderId,
   Guid BookItemId,
   string? Id = null
);
```

### BookItem als aktueller Reader ausleihen

Clientmethode:

```text
ILoanClient.BorrowMyAsync(LoanBorrowMeDto dto)
```

HTTP:

```http
POST /camplib/v1/loans/me
Content-Type: application/json
```

Request:

```csharp
public sealed record LoanBorrowMeDto(
   Guid BookItemId,
   string? Id = null
);
```

Beispiel:

```json
{
  "bookItemId": "00000002-0000-0000-0000-000000000000",
  "id": "00000099-0000-0001-0000-000000000000"
}
```

Antwort:

```text
201 Created + LoanDto
Location: /camplib/v1/loans/me/{id}
```

### Loan administrativ verlängern

Clientmethode:

```text
ILoanClient.RenewAsync(id)
```

HTTP:

```http
PATCH /camplib/v1/loans/{id}/renew
```

Antwort:

```text
200 OK + LoanDto
```

### Eigenen Loan verlängern

Clientmethode:

```text
ILoanClient.RenewMyAsync(id)
```

HTTP:

```http
PATCH /camplib/v1/loans/me/{id}/renew
```

Zusätzlich zur normalen Verlängerungslogik wird die Zugehörigkeit zum aktuellen Reader geprüft.

### Loan zurückgeben

Clientmethode:

```text
ILoanClient.ReturnAtDeskAsync(id)
```

HTTP:

```http
PATCH /camplib/v1/loans/{id}/return-at-desk
```

Antwort:

```text
204 No Content
```

Die Rückgabe löscht den Loan. Daher ist danach korrekt:

```http
GET /camplib/v1/loans/me/{id}
```

```text
404 Not Found
```

## Manueller `/me`-Ablauf

Datei:

```text
CampusLibraryApi/_5_ApiTest/Loan_Me.http
```

Voraussetzungen:

```text
API läuft auf https://localhost:8010
ActiveProfile der API ist ReaderRita
Subject stimmt mit dem Reader in der Datenbank überein
BookItem ist vorhanden und verfügbar
LoanId ist noch nicht vorhanden
```

Erwartete Reihenfolge:

```text
1. GET /loans/me                         -> 200
2. POST /loans/me                        -> 201
3. GET /loans/me/{id}                    -> 200
4. PATCH /loans/me/{id}/renew            -> 200
5. PATCH /loans/{id}/return-at-desk      -> 204
6. GET /loans/me/{id}                    -> 404
```

## ProblemDetails im Client

`BaseApiClient` verarbeitet erfolgreiche Antworten und Fehler zentral.

Client-Aufrufe liefern:

```text
Result<T>.Success(value)
Result<T>.Failure(error)
```

UI-Komponenten müssen deshalb nicht selbst JSON-ProblemDetails parsen.

## Abgrenzung zu Teil 6

In Teil 6 bleiben die fachlichen Routen und DTOs weitgehend erhalten. Anders ist die technische Identitätsquelle:

```text
Teil 5: API DevIdentity aus appsettings
Teil 6: validiertes Access Token
```

Der Client aktiviert dann `AccessTokenHandler`; die API liest `sub`, Username, Rolle, CreatedAt und AdminRights aus Claims statt aus Konfiguration.
