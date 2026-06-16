# API Documentation

This document describes the public HTTP API of the current `CampusLibraryApi`.

Swagger/OpenAPI is the authoritative technical API description. This document provides an additional didactic overview for students.

## Base URL

In development, the API listens on:

```text
https://localhost:8010
http://localhost:8012
```

The current API prefix is:

```text
/camplib/v1
```

The version is part of the URL.

Example:

```http
GET /camplib/v1/readers
```

## Swagger

Swagger UI is available in development mode:

```text
https://localhost:8010/swagger
```

The generated OpenAPI document describes:

```text
routes
request bodies
response bodies
status codes
ProblemDetails responses
DTO schemas
```

The current API contains three endpoint groups:

```text
Readers
Authors
Books
```

## Manual HTTP Files

For manual API tests, reset or delete the database first.

Then execute the HTTP files in this order:

```text
1. Authors.http
2. Books.http
3. Readers.http
```

`Seed.cs` defines the stable ids.

The `.http` files create these records through the public API.

```text
Authors.http creates the Authors.
Books.http creates the Books, uses the existing Authors, assigns Authors to Books and adds BookItems.
Readers.http creates or verifies Reader data.
```

This keeps manual API tests reproducible and avoids hidden database state.

## Modules

The current API contains two functional modules:

```text
Readers module
Catalog module
```

The Readers module manages library readers.

The Catalog module manages books, authors and physical book items.

# Readers Module

A Reader represents a domain-level library user of the CampusLibrary.

A Reader is not the same as a technical user account.

The technical identity reference is represented by:

```text
Subject
```

## Reader Routes

### Get all readers

```http
GET /camplib/v1/readers
```

Returns all readers.

Successful response:

```http
200 OK
```

Example response body:

```json
[
  {
    "id": "00000001-0000-0000-0000-000000000000",
    "subject": "a00090ad-d9df-486a-8757-4a649e26a54e",
    "firstname": "Erika",
    "lastname": "Mustermann",
    "email": "erika.mustermann@t-online.de",
    "addressDto": {
      "street": "Hauptstr. 23",
      "postalCode": "29556",
      "city": "Suderburg",
      "country": "DE"
    }
  }
]
```

Possible error responses:

```http
400 Bad Request
401 Unauthorized
403 Forbidden
```

### Get one reader by id

```http
GET /camplib/v1/readers/{id}
```

Example:

```http
GET /camplib/v1/readers/00000001-0000-0000-0000-000000000000
```

Successful response:

```http
200 OK
```

Possible error responses:

```http
401 Unauthorized
403 Forbidden
404 Not Found
```

### Get one reader by email

```http
GET /camplib/v1/readers/email?email={email}
```

Example:

```http
GET /camplib/v1/readers/email?email=erika.mustermann@t-online.de
```

Successful response:

```http
200 OK
```

Possible error responses:

```http
400 Bad Request
401 Unauthorized
403 Forbidden
404 Not Found
```

### Create a reader

```http
POST /camplib/v1/readers
```

Request body:

```json
{
  "firstname": "Edgar",
  "lastname": "Engel",
  "email": "e.engel@freenet.de",
  "addressDto": {
    "street": "Am Markt 14",
    "postalCode": "04109",
    "city": "Leipzig",
    "country": "DE"
  },
  "subject": "70000000-0007-0000-0000-000000000000",
  "id": "00000007-0000-0000-0000-000000000000"
}
```

Successful response:

```http
201 Created
```

Possible error responses:

```http
400 Bad Request
401 Unauthorized
403 Forbidden
409 Conflict
```

### ReaderCreateDto

```csharp
public sealed record ReaderCreateDto(
   string Firstname,
   string Lastname,
   string Email,
   AddressDto AddressDto,
   string Subject,
   string? Id
);
```

`Id` is optional.

It may be omitted in normal API usage. The use case then creates a new id.

It may be provided for teaching, testing or deterministic seed data.

### Update a reader

```http
PUT /camplib/v1/readers/{id}
```

Example:

```http
PUT /camplib/v1/readers/00000001-0000-0000-0000-000000000000
```

Request body for changing only the lastname:

```json
{
  "lastname": "Meier",
  "email": null,
  "addressDto": null
}
```

Successful response:

```http
200 OK
```

Possible error responses:

```http
400 Bad Request
401 Unauthorized
403 Forbidden
404 Not Found
409 Conflict
```

### ReaderUpdateDto

```csharp
public sealed record ReaderUpdateDto(
   string? Lastname,
   string? Email,
   AddressDto? AddressDto
);
```

All properties are nullable by design.

The meaning of `null` is:

```text
Lastname = null   -> no change
Email = null      -> no change
AddressDto = null -> no change
```

`Firstname` is intentionally not part of the update DTO.

`Subject` is also intentionally not part of the update DTO.

The technical identity reference is not changed by normal profile updates.

### Delete a reader

```http
DELETE /camplib/v1/readers/{id}
```

Example:

```http
DELETE /camplib/v1/readers/00000003-0000-0000-0000-000000000000
```

Successful response:

```http
204 No Content
```

Possible error responses:

```http
400 Bad Request
401 Unauthorized
403 Forbidden
404 Not Found
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

The address is converted into an `AddressVo` in the application layer.

The value object performs its own validation.

# Catalog Module

The Catalog module manages books, authors and physical book items.

It contains:

```text
Book
Author
BookItem
IsbnVo
```

A `Book` represents the bibliographic work.

An `Author` represents a person who can be assigned to books.

A `BookItem` represents a physical item or copy of a book.

An `IsbnVo` protects the ISBN validation rules.

# Author Routes

## Get all active authors

```http
GET /camplib/v1/authors
```

Returns all active authors.

Successful response:

```http
200 OK
```

Example response body:

```json
[
  {
    "id": "a0000001-0000-0000-0000-000000000000",
    "firstname": "Robert C.",
    "lastname": "Martin",
    "displayName": "Robert C. Martin",
    "isActive": true
  }
]
```

Possible error responses:

```http
400 Bad Request
401 Unauthorized
403 Forbidden
```

## Get one active author by id

```http
GET /camplib/v1/authors/{id}
```

Example:

```http
GET /camplib/v1/authors/a0000001-0000-0000-0000-000000000000
```

Successful response:

```http
200 OK
```

Possible error responses:

```http
400 Bad Request
401 Unauthorized
403 Forbidden
404 Not Found
```

Inactive authors are not returned by this normal read endpoint.

## Search active authors

```http
GET /camplib/v1/authors/search?searchText={searchText}
```

Example:

```http
GET /camplib/v1/authors/search?searchText=Martin
```

Searches active authors by lastname.

Successful response:

```http
200 OK
```

Example response body:

```json
[
  {
    "id": "a0000001-0000-0000-0000-000000000000",
    "firstname": "Robert C.",
    "lastname": "Martin",
    "displayName": "Robert C. Martin",
    "isActive": true
  }
]
```

Possible error responses:

```http
400 Bad Request
401 Unauthorized
403 Forbidden
```

If no author matches the search text, the endpoint returns an empty list.

## Create an author

```http
POST /camplib/v1/authors
```

Request body:

```json
{
  "firstname": "Robert C.",
  "lastname": "Martin",
  "id": "a0000001-0000-0000-0000-000000000000"
}
```

Successful response:

```http
201 Created
```

Example response body:

```json
{
  "id": "a0000001-0000-0000-0000-000000000000",
  "firstname": "Robert C.",
  "lastname": "Martin",
  "displayName": "Robert C. Martin",
  "isActive": true
}
```

Possible error responses:

```http
400 Bad Request
401 Unauthorized
403 Forbidden
409 Conflict
```

## AuthorCreateDto

```csharp
public sealed record AuthorCreateDto(
   string Firstname,
   string Lastname,
   string? Id
);
```

`Id` is optional.

It may be omitted in normal API usage. The use case then creates a new id.

It may be provided for teaching, testing or deterministic seed data.

## Deactivate an author

```http
PATCH /camplib/v1/authors/{id}/deactivate
```

Example:

```http
PATCH /camplib/v1/authors/a0000005-0000-0000-0000-000000000000/deactivate
```

Successful response:

```http
200 OK
```

Example response body:

```json
{
  "id": "a0000005-0000-0000-0000-000000000000",
  "firstname": "Kent",
  "lastname": "Beck",
  "displayName": "Kent Beck",
  "isActive": false
}
```

Possible error responses:

```http
400 Bad Request
401 Unauthorized
403 Forbidden
404 Not Found
```

Deactivate is not the same as delete.

The author remains stored in the database.

Normal read models decide whether inactive authors are visible.

## AuthorDto

```csharp
public sealed record AuthorDto(
   Guid Id,
   string Firstname,
   string Lastname,
   string DisplayName,
   bool IsActive
);
```

`AuthorDto` is used both for read responses and write use case responses.

# Book Routes

## Get all active books

```http
GET /camplib/v1/books
```

Returns all active books as compact list items.

Successful response:

```http
200 OK
```

Example response body:

```json
[
  {
    "id": "b0000001-0000-0000-0000-000000000000",
    "title": "Clean Code",
    "subtitle": "A Handbook of Agile Software Craftsmanship",
    "isbn": "9780132350884",
    "authors": [
      "Robert C. Martin"
    ],
    "totalBookItems": 2,
    "availableBookItems": 2
  }
]
```

Possible error responses:

```http
400 Bad Request
401 Unauthorized
403 Forbidden
```

## Get one active book by id

```http
GET /camplib/v1/books/{id}
```

Example:

```http
GET /camplib/v1/books/b0000001-0000-0000-0000-000000000000
```

Returns a detailed book representation.

Successful response:

```http
200 OK
```

Example response body:

```json
{
  "id": "b0000001-0000-0000-0000-000000000000",
  "title": "Clean Code",
  "subtitle": "A Handbook of Agile Software Craftsmanship",
  "isbn": "9780132350884",
  "authors": [
    {
      "id": "a0000001-0000-0000-0000-000000000000",
      "firstname": "Robert C.",
      "lastname": "Martin",
      "displayName": "Robert C. Martin",
      "isActive": true
    }
  ],
  "bookItems": [
    {
      "id": "be000001-0000-0000-0000-000000000000",
      "bookId": "b0000001-0000-0000-0000-000000000000",
      "inventoryNumber": "CL-BOOK-0001",
      "status": "Available"
    }
  ],
  "totalBookItems": 2,
  "availableBookItems": 2,
  "isActive": true,
  "createdAt": "2025-01-01T00:00:00Z",
  "updatedAt": "2025-01-01T00:00:00Z"
}
```

Possible error responses:

```http
400 Bad Request
401 Unauthorized
403 Forbidden
404 Not Found
```

Inactive books are not returned by this normal read endpoint.

## Search active books

```http
GET /camplib/v1/books/search?searchField={searchField}&searchText={searchText}
```

Examples:

```http
GET /camplib/v1/books/search?searchField=Title&searchText=Clean
GET /camplib/v1/books/search?searchField=AuthorLastName&searchText=Martin
GET /camplib/v1/books/search?searchField=Isbn&searchText=9780132350884
```

Searches active books by one search criterion.

Supported search fields:

```text
Title
AuthorLastName
Isbn
```

`AuthorLastName` searches only the lastname of assigned authors.

The firstname is not searched. This avoids accidental matches, for example `Martin` matching `Martin Fowler` when the user actually searches for the lastname `Martin`.

Successful response:

```http
200 OK
```

Example response body:

```json
[
  {
    "id": "b0000001-0000-0000-0000-000000000000",
    "title": "Clean Code",
    "subtitle": "A Handbook of Agile Software Craftsmanship",
    "isbn": "9780132350884",
    "authors": [
      "Robert C. Martin"
    ],
    "totalBookItems": 2,
    "availableBookItems": 2
  }
]
```

Possible error responses:

```http
400 Bad Request
401 Unauthorized
403 Forbidden
```

If no book matches the search criterion, the endpoint returns an empty list.

## Get active books by author id

```http
GET /camplib/v1/books/by-author/{authorId}
```

Example:

```http
GET /camplib/v1/books/by-author/a0000001-0000-0000-0000-000000000000
```

Returns all active books assigned to one author.

Successful response:

```http
200 OK
```

Example response body:

```json
[
  {
    "id": "b0000001-0000-0000-0000-000000000000",
    "title": "Clean Code",
    "subtitle": "A Handbook of Agile Software Craftsmanship",
    "isbn": "9780132350884",
    "authors": [
      "Robert C. Martin"
    ],
    "totalBookItems": 2,
    "availableBookItems": 2
  }
]
```

Possible error responses:

```http
400 Bad Request
401 Unauthorized
403 Forbidden
```

If no active book is assigned to the author, the endpoint returns an empty list.

## Create a book

```http
POST /camplib/v1/books
```

Request body:

```json
{
  "title": "Clean Code",
  "subtitle": "A Handbook of Agile Software Craftsmanship",
  "isbn": "9780132350884",
  "id": "b0000001-0000-0000-0000-000000000000"
}
```

Successful response:

```http
201 Created
```

Example response body:

```json
{
  "id": "b0000001-0000-0000-0000-000000000000",
  "title": "Clean Code",
  "subtitle": "A Handbook of Agile Software Craftsmanship",
  "isbn": "9780132350884",
  "bookItemCount": 0,
  "isActive": true
}
```

Possible error responses:

```http
400 Bad Request
401 Unauthorized
403 Forbidden
409 Conflict
```

## BookCreateDto

```csharp
public sealed record BookCreateDto(
   string Title,
   string? Subtitle,
   string Isbn,
   string? Id
);
```

`Id` is optional.

It may be omitted in normal API usage. The use case then creates a new id.

It may be provided for teaching, testing or deterministic seed data.

The ISBN is validated by `IsbnVo`.

## Add a physical book item

```http
POST /camplib/v1/books/{bookId}/items
```

Example:

```http
POST /camplib/v1/books/b0000001-0000-0000-0000-000000000000/items
```

Request body:

```json
{
  "inventoryNumber": "CL-BOOK-0001",
  "id": "be000001-0000-0000-0000-000000000000"
}
```

Successful response:

```http
200 OK
```

Example response body:

```json
{
  "id": "be000001-0000-0000-0000-000000000000",
  "bookId": "b0000001-0000-0000-0000-000000000000",
  "inventoryNumber": "CL-BOOK-0001",
  "status": "Available"
}
```

Possible error responses:

```http
400 Bad Request
401 Unauthorized
403 Forbidden
404 Not Found
409 Conflict
```

A newly added book item starts with the status:

```text
Available
```

The enum may still be stored as an integer in the database.

In the JSON API it is serialized as a string, because the API uses enum string serialization.

## BookItemAddDto

```csharp
public sealed record BookItemAddDto(
   string InventoryNumber,
   string? Id
);
```

`InventoryNumber` must be unique.

`Id` is optional.

It may be omitted in normal API usage. The use case then creates a new id.

It may be provided for teaching, testing or deterministic seed data.

## Assign an author to a book

```http
POST /camplib/v1/books/{bookId}/authors
```

Example:

```http
POST /camplib/v1/books/b0000001-0000-0000-0000-000000000000/authors
```

Request body:

```json
{
  "authorId": "a0000001-0000-0000-0000-000000000000"
}
```

Successful response:

```http
200 OK
```

Example response body:

```json
{
  "id": "b0000001-0000-0000-0000-000000000000",
  "title": "Clean Code",
  "subtitle": "A Handbook of Agile Software Craftsmanship",
  "isbn": "9780132350884",
  "bookItemCount": 0,
  "isActive": true
}
```

Possible error responses:

```http
400 Bad Request
401 Unauthorized
403 Forbidden
404 Not Found
409 Conflict
```

The `bookId` comes from the route.

The `authorId` comes from the request body.

There is no `BookAuthorId`.

The join table is an infrastructure detail.

## BookAssignAuthorDto

```csharp
public sealed record BookAssignAuthorDto(
   Guid AuthorId
);
```

This DTO contains only the author id because the book id is already part of the route.

## Deactivate a book

```http
PATCH /camplib/v1/books/{bookId}/deactivate
```

Example:

```http
PATCH /camplib/v1/books/b0000004-0000-0000-0000-000000000000/deactivate
```

Successful response:

```http
200 OK
```

Example response body:

```json
{
  "id": "b0000004-0000-0000-0000-000000000000",
  "title": "Design Patterns",
  "subtitle": "Elements of Reusable Object-Oriented Software",
  "isbn": "9780201633610",
  "bookItemCount": 2,
  "isActive": false
}
```

Possible error responses:

```http
400 Bad Request
401 Unauthorized
403 Forbidden
404 Not Found
```

Deactivate is not the same as delete.

The book remains stored in the database.

Normal read models decide whether inactive books are visible.

## BookSearchField

```csharp
public enum BookSearchField {
   Title = 1,
   AuthorLastName = 2,
   Isbn = 3
}
```

`AuthorLastName` is the catalog-oriented author search field.

It searches books by the lastname of assigned authors.

## BookDto

```csharp
public sealed record BookDto(
   Guid Id,
   string Title,
   string? Subtitle,
   string Isbn,
   int BookItemCount,
   bool IsActive
);
```

`BookDto` is mainly used as the result of write-side book use cases.

## BookListItemDto

```csharp
public sealed record BookListItemDto(
   Guid Id,
   string Title,
   string? Subtitle,
   string Isbn,
   IReadOnlyList<string> Authors,
   int TotalBookItems,
   int AvailableBookItems
);
```

`BookListItemDto` is used for lists and search results.

It is optimized for catalog overview screens.

## BookDetailDto

```csharp
public sealed record BookDetailDto(
   Guid Id,
   string Title,
   string? Subtitle,
   string Isbn,
   IReadOnlyList<AuthorDto> Authors,
   IReadOnlyList<BookItemDto> BookItems,
   int TotalBookItems,
   int AvailableBookItems,
   bool IsActive,
   DateTime CreatedAt,
   DateTime UpdatedAt
);
```

`BookDetailDto` is used for the detail view of one book.

It contains more information than `BookListItemDto`.

## BookItemDto

```csharp
public sealed record BookItemDto(
   Guid Id,
   Guid BookId,
   string InventoryNumber,
   BookItemStatus Status
);
```

`BookItemDto` represents a physical book item.

## BookItemStatus

```csharp
public enum BookItemStatus {
   Available = 1,
   Unavailable = 2,
   Lost = 3,
   Damaged = 4
}
```

The semantic meaning remains visible through the enum names.

The database may store the enum as an integer.

The JSON API serializes enum values as strings.

Example:

```json
{
  "status": "Available"
}
```

# Error Responses

The API uses `ProblemDetails` for error responses.

Example structure:

```json
{
  "type": "https://httpstatuses.com/404",
  "title": "Catalog: Author NotFound",
  "status": 404,
  "detail": "The author was not found.",
  "instance": "/camplib/v1/books/b0000001-0000-0000-0000-000000000000/authors",
  "traceId": "..."
}
```

Typical categories:

```text
400 Bad Request   -> invalid input
401 Unauthorized  -> authentication required
403 Forbidden     -> access denied
404 Not Found     -> resource not found
409 Conflict      -> duplicate or conflicting resource
```

Examples:

```text
409 Conflict -> duplicate reader email
409 Conflict -> duplicate reader subject
409 Conflict -> duplicate book ISBN
409 Conflict -> duplicate author name
409 Conflict -> duplicate inventory number
409 Conflict -> author already assigned to book
```

# Read Side and Write Side

The controllers use different application ports for read and write behavior.

Readers:

```text
IReaderReadModel
IReaderUseCases
```

Authors:

```text
IAuthorReadModel
IAuthorUseCases
```

Books:

```text
IBookReadModel
IBookUseCases
```

The read side is used for queries:

```text
GET /readers
GET /readers/{id}
GET /readers/email

GET /authors
GET /authors/{id}
GET /authors/search

GET /books
GET /books/{id}
GET /books/search
GET /books/by-author/{authorId}
```

The write side is used for commands:

```text
POST   /readers
PUT    /readers/{id}
DELETE /readers/{id}

POST  /authors
PATCH /authors/{id}/deactivate

POST  /books
POST  /books/{bookId}/items
POST  /books/{bookId}/authors
PATCH /books/{bookId}/deactivate
```

This separation supports a clean teaching model:

```text
queries read DTOs
commands change aggregates
repositories work with domain objects
read models return DTOs directly
```

# Important Design Decisions

## Book to BookItem

A book can have multiple physical book items.

This is modeled as a one-to-many relationship:

```text
Book 1 --- n BookItem
```

A book item is added through the `Book` aggregate:

```text
POST /camplib/v1/books/{bookId}/items
```

## Book to Author

A book can have multiple authors.

An author can be assigned to multiple books.

This is modeled as a many-to-many relationship:

```text
Book n --- m Author
```

The API exposes the assignment as:

```text
POST /camplib/v1/books/{bookId}/authors
```

The request body only contains the `authorId`.

The technical join table is not exposed as an API resource.

## Catalog Search by Author Lastname

For catalog search, the lastname of the author is the fachlich relevant search criterion.

The firstname is not searched.

This avoids accidental search results.

Example:

```text
AuthorLastName = Martin -> Clean Code
AuthorLastName = Fowler -> Refactoring and Design Patterns
```

## Deactivate instead of Delete in Catalog

Books and authors are not deleted physically.

They are deactivated.

```text
IsActive = false
```

Repositories may still load them.

Read models decide whether inactive data is visible.

Normal catalog lists and searches return only active books and authors.

# Didactic Goals

This API is designed to demonstrate:

```text
REST-style endpoints
API versioning
Swagger/OpenAPI documentation
DTOs as API contracts
Result-based error handling
ProblemDetails responses
partial update semantics
separation of read and write paths
controller tests with WebApplicationFactory
one-to-many relationship in an aggregate
many-to-many relationship through infrastructure mapping
catalog search by author lastname
deactivate instead of delete
module-specific read models and use cases
```

Swagger should be used for technical exploration.

This document should be used for conceptual explanation.

```
```
