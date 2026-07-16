# API Usage from the Client Perspective — Part 5

This document describes the currently used HTTP endpoints and transport types of branch `part-5/client-noauth`.

German version: [3Api-ger.md](3Api-ger.md)

## Basic idea

The Blazor SSR client uses module-specific HTTP clients:

```text
IReaderClient -> ReaderClient
IBookClient   -> BookClient
ILoanClient   -> LoanClient
```

Shared base address:

```text
https://localhost:8010/camplib/v1
```

The client references no API core project. It owns DTOs with the same JSON shape as the public DTOs of the modules.

## Auth status in Part 5

Part 5 uses no real API authentication.

```text
no Bearer token
no Authorization header
no DevIdentity headers from the client
no running IdentityAccessServer required
```

The API reads its technical identity from:

```text
CampusLibraryApi/appsettings.json
```

Data flow for `/me`:

```text
DevIdentity -> DevIdentityGateway -> IIdentityGateway
            -> IdentitySubject.Check -> Reader.Subject
```

The API can therefore be called directly from manual HTTP tests.

## DevIdentity preconditions

Example Reader profile:

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

The most important condition is:

```text
DevIdentity.Subject == Reader.Subject in the database
```

Email is unsuitable for this association because `Reader.Email` can be changed through self-service update.

## Shared error responses

Controllers translate business errors to `ProblemDetails`.

Typical status codes:

```text
400 Bad Request   invalid input
401 Unauthorized  technical identity is not marked authenticated
403 Forbidden     identity is not a Reader or access is not allowed
404 Not Found     resource or current Reader was not found
409 Conflict      uniqueness or state conflict
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

### Get Readers

Client method:

```text
IReaderClient.GetAllAsync(includeInactive=false)
```

HTTP:

```http
GET /camplib/v1/readers?includeInactive=false
```

Response:

```text
200 OK + ReaderDto[]
```

Including inactive Readers:

```http
GET /camplib/v1/readers?includeInactive=true
```

### Get Reader by id

Client method:

```text
IReaderClient.GetByIdAsync(id, includeInactive=false)
```

HTTP:

```http
GET /camplib/v1/readers/{id}?includeInactive=false
```

Responses:

```text
200 OK
404 Not Found
```

### Get Reader by email

Client method:

```text
IReaderClient.GetByEmailAsync(email, includeInactive=false)
```

HTTP:

```http
GET /camplib/v1/readers/email?email={email}&includeInactive=false
```

Email search is an ordinary query. It is not used for technical `/me` association.

### Create Reader administratively

Client method:

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

Example:

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

Response:

```text
201 Created
Location: /camplib/v1/readers/{id}
```

The optional id supports deterministic tests and HTTP scripts.

### Update the current Reader

The former administrative update path was replaced by self-service.

Client method:

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

Example:

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

Semantics:

```text
null -> leave current value unchanged
```

The API resolves the Reader through `IIdentityGateway.Subject`. The request contains no ReaderId.

Possible responses:

```text
200 OK       Reader updated
400          invalid profile values
401          DevIdentity not authenticated
403          active profile is not a Reader
404          no Reader with the subject was found
409          new email is already used
```

Note: The current Part 5 controller exposes no public `GET /readers/me`. Current-Reader resolution is used internally for update and Loan self-service.

### Deactivate Reader

Client method:

```text
IReaderClient.DeactivateAsync(id)
```

HTTP:

```http
DELETE /camplib/v1/readers/{id}
```

Response:

```text
204 No Content
```

The Reader is deactivated and remains stored. Deactivation may be rejected while current Loans exist.

## Catalog API

### BookDto

List and detail views use the same type:

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

Status values:

```text
1 Available
2 Unavailable
3 Lost
4 Damaged
```

The current transport contract has no `InventoryNumber`.

### Get books

Client method:

```text
IBookClient.GetAllAsync(includeInactive=false)
```

HTTP:

```http
GET /camplib/v1/books?includeInactive=false
```

Employees can include inactive books:

```http
GET /camplib/v1/books?includeInactive=true
```

### Get book by id

Client method:

```text
IBookClient.GetByIdAsync(id, includeInactive=false)
```

HTTP:

```http
GET /camplib/v1/books/{id}?includeInactive=false
```

### Search books

Client method:

```text
IBookClient.SearchAsync(searchField, searchText, includeInactive=false)
```

HTTP:

```http
GET /camplib/v1/books/search?searchField=Title&searchText=Clean%20Code&includeInactive=false
```

Search fields:

```text
Title
AuthorLastName
Isbn
```

The response is also `BookDto[]`. Separate types such as `BookSearchDto`, `BookListItemDto` or `BookDetailDto` are no longer used.

### Create book

Client method:

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

Response:

```text
201 Created + BookDto
```

### Add BookItem

Client method:

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

Response:

```text
201 Created + BookItemDto
```

### Get deactivation information

Client method:

```text
IBookClient.GetDeactivationInfoAsync(bookId)
```

HTTP:

```http
GET /camplib/v1/books/{bookId}/deactivation-info
```

Response:

```csharp
public sealed record BookDeactivationInfoDto(
   Guid BookId,
   int TotalItems,
   int BorrowedItems,
   IReadOnlyList<BookLoanInfoDto> CurrentLoans
);
```

### Deactivate book

Client method:

```text
IBookClient.DeactivateAsync(bookId)
```

HTTP:

```http
PATCH /camplib/v1/books/{bookId}/deactivate
```

Response:

```text
200 OK + BookDto
```

Deactivation may return a conflict while current Loans exist.

## Loans API

### LoanDto

List and detail views use the same type:

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

`LoanDto` intentionally has no `Status` or `ReturnedAt` fields. A stored Loan is a current borrowing process.

### Get all current Loans

Client method:

```text
ILoanClient.GetBorrowedAsync()
```

HTTP:

```http
GET /camplib/v1/loans
```

Response:

```text
200 OK + LoanDto[]
```

### Get the current Reader's Loans

Client method:

```text
ILoanClient.GetMyBorrowedAsync()
```

HTTP:

```http
GET /camplib/v1/loans/me
```

The API resolves the Reader by subject. A Reader without Loans receives an empty list with `200 OK`.

### Get Loan administratively by id

```http
GET /camplib/v1/loans/{id}
```

Client method:

```text
ILoanClient.GetByIdAsync(id)
```

### Get own Loan by id

```http
GET /camplib/v1/loans/me/{id}
```

Client method:

```text
ILoanClient.GetMyByIdAsync(id)
```

The endpoint verifies that the Loan belongs to the Reader resolved from the subject.

### Borrow BookItem administratively

Client method:

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

### Borrow BookItem as current Reader

Client method:

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

Example:

```json
{
  "bookItemId": "00000002-0000-0000-0000-000000000000",
  "id": "00000099-0000-0001-0000-000000000000"
}
```

Response:

```text
201 Created + LoanDto
Location: /camplib/v1/loans/me/{id}
```

### Renew Loan administratively

Client method:

```text
ILoanClient.RenewAsync(id)
```

HTTP:

```http
PATCH /camplib/v1/loans/{id}/renew
```

Response:

```text
200 OK + LoanDto
```

### Renew own Loan

Client method:

```text
ILoanClient.RenewMyAsync(id)
```

HTTP:

```http
PATCH /camplib/v1/loans/me/{id}/renew
```

Ownership is checked in addition to ordinary renewal rules.

### Return Loan

Client method:

```text
ILoanClient.ReturnAtDeskAsync(id)
```

HTTP:

```http
PATCH /camplib/v1/loans/{id}/return-at-desk
```

Response:

```text
204 No Content
```

Return deletes the Loan. The following response is therefore correct afterwards:

```http
GET /camplib/v1/loans/me/{id}
```

```text
404 Not Found
```

## Manual `/me` workflow

File:

```text
CampusLibraryApi/_5_ApiTest/Loan_Me.http
```

Preconditions:

```text
API runs at https://localhost:8010
API ActiveProfile is ReaderRita
Subject matches the Reader in the database
BookItem exists and is available
LoanId does not already exist
```

Expected sequence:

```text
1. GET /loans/me                         -> 200
2. POST /loans/me                        -> 201
3. GET /loans/me/{id}                    -> 200
4. PATCH /loans/me/{id}/renew            -> 200
5. PATCH /loans/{id}/return-at-desk      -> 204
6. GET /loans/me/{id}                    -> 404
```

## ProblemDetails in the client

`BaseApiClient` centrally handles successful responses and errors.

Client calls return:

```text
Result<T>.Success(value)
Result<T>.Failure(error)
```

UI components therefore do not parse JSON ProblemDetails themselves.

## Boundary to Part 6

In Part 6 the business routes and DTOs can largely remain unchanged. The technical identity source changes:

```text
Part 5: API DevIdentity from appsettings
Part 6: validated access token
```

The client then activates `AccessTokenHandler`; the API reads `sub`, username, role, CreatedAt and AdminRights from claims instead of configuration.
