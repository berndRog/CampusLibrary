# API Documentation — Part 4

This document describes the public HTTP API of Part 4 of `CampusLibraryApi`.

Swagger/OpenAPI is the authoritative technical API description. This document provides a didactic overview for students.

## Base URL

Development URLs:

```text
https://localhost:8010
http://localhost:8012
```

API prefix:

```text
/camplib/v1
```

Endpoint groups:

```text
Readers
Books
Loans
```

Swagger UI is available in development mode:

```text
https://localhost:8010/swagger
```

## Manual HTTP files

For manual API tests, reset or delete the database first.

Recommended order:

```text
1. Readers.http
2. Books.http
3. Loans.http
```

For larger exercises, separate seed files and behavior tests:

```text
01_Seed_Readers.http
02_Seed_Books.http
03_Seed_Loans.http
11_Readers_Api.http
12_Books_Api.http
13_Loans_Api.http
```

# Readers API

Readers API is unchanged from Part 3.

Important endpoints:

```http
GET    /camplib/v1/readers
GET    /camplib/v1/readers/with-inactive
GET    /camplib/v1/readers/{id}
GET    /camplib/v1/readers/{id}/with-inactive
GET    /camplib/v1/readers/email?email={email}
POST   /camplib/v1/readers
PUT    /camplib/v1/readers/{id}
DELETE /camplib/v1/readers/{id}
```

`DELETE /readers/{id}` deactivates a reader.

# Books API

Books API still uses Books and BookItems. A BookItem is now identified only by its unique `Id`. There is no separate `InventoryNumber` anymore.

Important endpoints:

```http
GET   /camplib/v1/books
GET   /camplib/v1/books/{id}
GET   /camplib/v1/books/search?searchField=Title&searchText=...
GET   /camplib/v1/books/search?searchField=Isbn&searchText=...
GET   /camplib/v1/books/search?searchField=AuthorLastName&searchText=...
POST  /camplib/v1/books
POST  /camplib/v1/books/{bookId}/items
PATCH /camplib/v1/books/{bookId}/deactivate
```

There is no Author API.

When adding a BookItem, the client may optionally provide an id. The inventory number is no longer transferred as a separate field.

```csharp
public sealed record BookItemAddDto(
   string? Id
);

public sealed record BookItemDto(
   Guid Id,
   Guid BookId,
   int Status
);
```

The UI may still display `BookItemId` as the inventory number.

# Loans API

Loans describe the borrowing lifecycle of concrete BookItems.

Loans use `LoanStatus`, not `IsActive`.

```text
Borrowed = 1
Returned = 2
Cancelled = 3
```

DTOs expose the status as a numeric API value. The domain still uses the `LoanStatus` enum internally.

Current DTO rule: `InventoryNumber` has been removed from the loan DTOs. The unique physical-copy identity is `BookItemId`. Reader-related loan DTOs now also contain `Email`.

```csharp
public sealed record ReaderLoanInfoDto(
   Guid Id,
   string Firstname,
   string Lastname,
   string Email,
   bool IsActive
);

public sealed record LoanListItemDto(
   Guid Id,

   Guid ReaderId,
   string Firstname,
   string Lastname,

   Guid BookItemId,

   string Title,
   string? Subtitle,

   DateTime LoanDate,
   DateTime DueDate,

   int Status,
   bool IsOverdue
);

public sealed record LoanDetailDto(
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
   DateTime? ReturnedAt,

   int Status,
   int RenewalCount,

   bool IsOverdue,
   bool CanRenew
);
```

## Get borrowed loans

```http
GET /camplib/v1/loans
```

Returns all currently borrowed loans.

Response type:

```text
IReadOnlyList<LoanListItemDto>
```

Successful response:

```http
200 OK
```

There is intentionally no `/loans/active` route.

## Get one loan by id

```http
GET /camplib/v1/loans/{id}
```

Returns one detailed loan projection enriched with reader and book item data.

Response type:

```text
LoanDetailDto
```

Possible responses:

```http
200 OK
400 Bad Request
404 Not Found
```

## Borrow a book item

```http
POST /camplib/v1/loans
Content-Type: application/json
```

Example request:

```json
{
  "id": "a1000001-0000-0000-0000-000000000000",
  "readerId": "10000000-0000-0000-0000-000000000000",
  "bookItemId": "be000001-0000-0000-0000-000000000000"
}
```

Possible responses:

```http
201 Created
400 Bad Request
404 Not Found
409 Conflict
```

The client does not provide loan duration. The due date is derived from `LoanRules.StandardLoanDays`.

## Renew a loan

```http
PATCH /camplib/v1/loans/{id}/renew
```

Possible responses:

```http
200 OK
400 Bad Request
404 Not Found
409 Conflict
```

A loan can be renewed only if it is borrowed, not overdue and below the maximum number of renewals.

## Return a loan at the service desk

```http
PATCH /camplib/v1/loans/{id}/return-at-desk
```

Possible responses:

```http
200 OK
400 Bad Request
404 Not Found
409 Conflict
```

The return timestamp is provided by the application service using `IClock`.

## Typical manual flow

```http
POST  /camplib/v1/loans
GET   /camplib/v1/loans/{id}
PATCH /camplib/v1/loans/{id}/renew
PATCH /camplib/v1/loans/{id}/return-at-desk
GET   /camplib/v1/loans
```

After a loan is returned, it no longer appears in `GET /loans` because that endpoint lists currently borrowed loans.
