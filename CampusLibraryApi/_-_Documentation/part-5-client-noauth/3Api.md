# API Usage from the Client Perspective — Part 5

This document describes which CampusLibraryApi endpoints are used by the Blazor SSR client in Part 5.

German version: [3Api-ger.md](3Api-ger.md)

## Basic idea

The client calls the API through typed API clients:

```text
IReaderClient -> ReaderClient
IBookClient   -> BookClient
ILoanClient   -> LoanClient
```

The API base address comes from `appsettings.json`:

```json
{
  "CampusLibraryApi": {
    "BaseUrl": "https://localhost:8010/"
  }
}
```

All business routes are located under:

```text
/camplib/v1
```

## Auth status in Part 5

Part 5 does not use real protected API calls.

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

The `AccessTokenHandler` is not attached to the CampusLibraryApi HttpClient while `ApiAccessTokenEnabled=false`.

`DevIdentity` only controls the UI perspective. It is not authentication and not authorization.

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

Displayed in the UI:

```text
Name
Email
Status
```

`Subject` is not displayed. It is a technical identity and becomes relevant in the later AuthN/AuthZ context.

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

The API client method may technically exist:

```text
IReaderClient.CreateAsync(dto)
```

But Part 5 intentionally does not expose a visible UI function `Create reader`.

Reason:

```text
Readers should later be provisioned from a technical user in IdentityAccessServer.
```

Part 5 uses readers from seed/test data.

### Update reader

The API client method may technically exist:

```text
IReaderClient.UpdateAsync(id, dto)
```

This is also not the central UI workflow in Part 5.

### Deactivate reader

If the UI uses this function, the client calls the existing API endpoint:

```text
IReaderClient.DeactivateAsync(id)
```

Current client call:

```http
DELETE /camplib/v1/readers/{id}
```

Business-wise this is deactivation, not physical deletion.

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

Employees load inactive books too:

```http
GET /camplib/v1/books?includeInactive=true
```

Response type:

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

The catalog table displays:

```text
Action | Title | Authors | ISBN | Items | Status
```

The `Title` column contains title and subtitle. The `Items` column shows borrowed / total.

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

```csharp
public sealed record BookCreateDto(
   string? AuthorsText,
   string? Title,
   string? Subtitle,
   string? Isbn,
   string? Id = null
);
```

Used by:

```text
/catalog/books/create
BookCreate.razor
```

This page is intended for employees.

### Add BookItem

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

```csharp
public sealed record BookItemAddDto(
   string? Id = null
);
```

Response type:

```csharp
public sealed record BookItemDto(
   Guid Id,
   Guid BookId,
   int Status
);
```

Used by:

```text
/catalog/books/{bookId}/items/add
BookItemAdd.razor
```

The API no longer has a separate `InventoryNumber`. The UI displays `BookItem.Id` as inventory number.

### Deactivate book

Client method:

```text
IBookClient.DeactivateAsync(bookId)
```

HTTP call:

```http
PATCH /camplib/v1/books/{bookId}/deactivate
```

Used by:

```text
/catalog/books/{bookId}/deactivate
BookDeactivate.razor
```

The client does not call a separate BookItem delete endpoint. Removing or blocking items is the responsibility of the API use case.

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
/my/loans
MyLoansList.razor
```

Response type:

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

`BookItemId` is displayed as inventory number in the UI.

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

Used by:

```text
/loans/{loanId}
LoanDetails.razor
```

The detail page shows book data, inventory number, reader data with email and loan data. Renew and return are started from there.

### Borrow BookItem

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

```csharp
public sealed record LoanCreateDto(
   Guid ReaderId,
   Guid BookItemId,
   string? Id = null
);
```

Used by:

```text
/catalog/books/{bookId}/borrow
BorrowBook.razor
```

The UI selects an actually available BookItem and sends its `BookItemId`.

### Return loan

Client method:

```text
ILoanClient.ReturnAtDeskAsync(id)
```

HTTP call:

```http
PATCH /camplib/v1/loans/{id}/return-at-desk
```

### Renew loan

Client method:

```text
ILoanClient.RenewAsync(id)
```

HTTP call:

```http
PATCH /camplib/v1/loans/{id}/renew
```

## Error handling

All API clients use:

```text
BaseApiClient<TClient>
```

Successful responses are returned as `Result<T>.Success`. Failed responses are returned as `Result<T>.Failure(ApiError)`.

The UI displays errors through:

```text
ErrorAlert.razor
```

## Later reader provisioning

The following planned endpoints are not part of Part 5, but are documented for the continuation.

```http
POST /camplib/v1/readers/me/provision
Authorization: Bearer <access_token>
```

The API reads `subject` and `email` from the token. The client does not send a subject in the body.

```http
POST /camplib/v1/readers/me/profile
Authorization: Bearer <access_token>
Content-Type: application/json
```

Example body:

```json
{
  "firstname": "Erika",
  "lastname": "Mustermann"
}
```
