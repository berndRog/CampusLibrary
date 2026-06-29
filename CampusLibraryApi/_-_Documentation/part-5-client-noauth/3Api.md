# API and Client Contract — Part 5

This document describes the API surface used by `CampusLibraryClient` in Part 5.

Part 5 does not primarily add new backend API endpoints. It adds a Blazor SSR client that consumes the existing CampusLibraryApi endpoints from Part 4.

German version: [3Api-ger.md](3Api-ger.md)

## Base URLs

CampusLibraryApi:

```text
https://localhost:8010/
```

CampusLibraryClient:

```text
configured by the client launch settings
```

The client configures the backend API URL in `CampusLibraryClient/appsettings.json`:

```json
{
  "CampusLibraryApi": {
    "BaseUrl": "https://localhost:8010/"
  }
}
```

## API client registration

The client registers all module clients through:

```text
AddCampusLibraryClients(configuration, useAccessToken)
```

In Part 5:

```text
useAccessToken = false
```

Therefore requests are sent without a Bearer token.

## Client API structure

The client uses three module-specific API client abstractions:

```text
IReaderClient
IBookClient
ILoanClient
```

The concrete implementations are:

```text
ReaderClient
BookClient
LoanClient
```

All three clients use the named HttpClient:

```text
Common.CampusLibraryApiClientName
```

## Error contract

The backend returns errors as `ProblemDetails`.

The client maps backend errors to:

```text
ApiError
```

The common result type is:

```text
Result<T>
```

Typical client handling:

```text
Result<T>.IsSuccess -> display data
Result<T>.IsFailure -> display ErrorAlert
```

## Auth status

Part 5 uses anonymous API calls.

Feature flags:

```json
{
  "Features": {
    "AuthNEnabled": false,
    "ApiAccessTokenEnabled": false,
    "AuthZEnabled": false
  }
}
```

Prepared but inactive:

```text
AccessTokenHandler
AuthenticationExtensions
AuthorizationExtensions
IdentityController
EntryController
```

The `AccessTokenHandler` is not attached to the CampusLibraryApi HttpClient while `ApiAccessTokenEnabled=false`.

## Readers API used by the client

### Get readers

Client method:

```text
IReaderClient.GetAllAsync(includeInactive=false)
```

HTTP call:

```http
GET /camplib/v1/readers?includeInactive=false
```

Used by:

```text
/readers
ReadersList.razor
```

Response type:

```text
IEnumerable<ReaderDto>
```

Displayed fields:

```text
Firstname
Lastname
Email
Subject
IsActive
```

### Get reader by id

Client method:

```text
IReaderClient.GetByIdAsync(id, includeInactive=false)
```

HTTP call:

```http
GET /camplib/v1/readers/{id}?includeInactive=false
```

### Get reader by email

Client method:

```text
IReaderClient.GetByEmailAsync(email, includeInactive=false)
```

HTTP call:

```http
GET /camplib/v1/readers/email?email={email}&includeInactive=false
```

### Create reader

Client method:

```text
IReaderClient.CreateAsync(dto)
```

HTTP call:

```http
POST /camplib/v1/readers
Content-Type: application/json
```

Request type:

```text
ReaderCreateDto
```

### Update reader

Client method:

```text
IReaderClient.UpdateAsync(id, dto)
```

HTTP call:

```http
PUT /camplib/v1/readers/{id}
Content-Type: application/json
```

Request type:

```text
ReaderUpdateDto
```

### Deactivate reader

Client method:

```text
IReaderClient.DeactivateAsync(id)
```

Current client call:

```http
DELETE /camplib/v1/readers/{id}
```

Check this against the current API route. If the final API uses an explicit deactivate route, the client should be adjusted accordingly, for example:

```http
PATCH /camplib/v1/readers/{id}/deactivate
```

## Catalog API used by the client

### Get books

Client method:

```text
IBookClient.GetAllAsync(includeInactive=false)
```

HTTP call:

```http
GET /camplib/v1/books?includeInactive=false
```

Used by:

```text
/catalog/books
BooksList.razor
```

Response type:

```text
IEnumerable<BookListItemDto>
```

Displayed fields:

```text
Title
Subtitle
AuthorsText
Isbn
AvailableItems
TotalItems
IsActive
```

### Get book by id

Client method:

```text
IBookClient.GetByIdAsync(id, includeInactive=false)
```

HTTP call:

```http
GET /camplib/v1/books/{id}?includeInactive=false
```

Response type:

```text
BookDetailDto
```

### Search books

Client method:

```text
IBookClient.SearchAsync(searchField, searchText, includeInactive=false)
```

HTTP call:

```http
GET /camplib/v1/books/search?searchField=Title&searchText=Clean%20Code&includeInactive=false
```

Search fields:

```text
Title
AuthorLastName
Isbn
```

### Create book

Client method:

```text
IBookClient.CreateAsync(dto)
```

HTTP call:

```http
POST /camplib/v1/books
Content-Type: application/json
```

Request type:

```text
BookCreateDto
```

### Add book item

Client method:

```text
IBookClient.AddBookItemAsync(bookId, dto)
```

HTTP call:

```http
POST /camplib/v1/books/{bookId}/items
Content-Type: application/json
```

Request type:

```text
BookItemAddDto
```

## Loans API used by the client

### Get borrowed loans

Client method:

```text
ILoanClient.GetBorrowedAsync()
```

HTTP call:

```http
GET /camplib/v1/loans
```

Used by:

```text
/loans
LoansList.razor
```

Response type:

```text
IEnumerable<LoanListItemDto>
```

Displayed fields:

```text
Reader firstname/lastname
Book title
Inventory number
Loan date
Due date
Status
IsOverdue
```

### Get loan by id

Client method:

```text
ILoanClient.GetByIdAsync(id)
```

HTTP call:

```http
GET /camplib/v1/loans/{id}
```

Response type:

```text
LoanDetailDto
```

### Borrow a book item

Client method:

```text
ILoanClient.BorrowAsync(dto)
```

HTTP call:

```http
POST /camplib/v1/loans
Content-Type: application/json
```

Request type:

```text
LoanCreateDto
```

### Renew a loan

Client method:

```text
ILoanClient.RenewAsync(id)
```

HTTP call:

```http
PATCH /camplib/v1/loans/{id}/renew
```

Used by the action button in `LoansList.razor`.

### Return a loan at the service desk

Client method:

```text
ILoanClient.ReturnAtDeskAsync(id)
```

HTTP call:

```http
PATCH /camplib/v1/loans/{id}/return-at-desk
```

Used by the action button in `LoansList.razor`.

## Typical Part 5 manual UI flow

```text
1. Start CampusLibraryApi.
2. Start CampusLibraryClient.
3. Open the client home page.
4. Navigate to Readers.
5. Navigate to Catalog / Books.
6. Search for a book by title, author last name or ISBN.
7. Navigate to Loans.
8. Renew or return a borrowed loan.
9. Stop CampusLibraryApi and reload a page to observe ErrorAlert.
```

## What Part 5 intentionally does not do

```text
No login endpoint is required for the visible UI.
No access token is sent to CampusLibraryApi.
No API endpoint is protected by the client.
No role or policy decision is active in the UI.
```

The prepared AuthN/AuthZ elements are documented because they reduce the transition effort for later parts.
