# API Documentation

This document describes the public HTTP API of the current `CampusLibraryApi`.

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

The current API contains two endpoint groups:

```text
Readers
Books
```

Swagger UI is available in development mode:

```text
https://localhost:8010/swagger
```

## Manual HTTP files

For manual API tests, reset or delete the database first.

Execution order:

```text
1. Books.http
2. Readers.http
```

# Readers API

A Reader represents a domain-level library user.

The technical identity reference is represented by `Subject`.

## Get all active readers

```http
GET /camplib/v1/readers
```

Successful response:

```http
200 OK
```

## Get all readers including inactive readers

```http
GET /camplib/v1/readers/with-inactive
```

Successful response:

```http
200 OK
```

## Get one active reader by id

```http
GET /camplib/v1/readers/{id}
```

Example:

```http
GET /camplib/v1/readers/10000000-0000-0000-0000-000000000000
```

Possible responses:

```http
200 OK
401 Unauthorized
403 Forbidden
404 Not Found
```

## Get one reader by id including inactive readers

```http
GET /camplib/v1/readers/{id}/with-inactive
```

Possible responses:

```http
200 OK
400 Bad Request
401 Unauthorized
403 Forbidden
404 Not Found
```

## Get one reader by email

```http
GET /camplib/v1/readers/email?email={email}
```

Example:

```http
GET /camplib/v1/readers/email?email=e.mustermann@t-online.de
```

Possible responses:

```http
200 OK
400 Bad Request
401 Unauthorized
403 Forbidden
404 Not Found
```

## Create a reader

```http
POST /camplib/v1/readers
```

Request body:

```json
{
  "firstname": "Erika",
  "lastname": "Mustermann",
  "email": "e.mustermann@t-online.de",
  "addressDto": {
    "street": "Hauptstr. 12",
    "postalCode": "29556",
    "city": "Suderburg",
    "country": "DE"
  },
  "subject": "reader-001",
  "id": "10000000-0000-0000-0000-000000000000"
}
```

Possible responses:

```http
201 Created
400 Bad Request
401 Unauthorized
403 Forbidden
409 Conflict
```

## Update a reader

```http
PUT /camplib/v1/readers/{id}
```

Request body:

```json
{
  "lastname": "Meier",
  "email": "e.meier@gmx.de",
  "addressDto": {
    "street": "Neue Straße 5",
    "postalCode": "30123",
    "city": "Hannover",
    "country": "DE"
  }
}
```

Possible responses:

```http
200 OK
400 Bad Request
401 Unauthorized
403 Forbidden
404 Not Found
409 Conflict
```

## Deactivate a reader

```http
DELETE /camplib/v1/readers/{id}
```

Successful response:

```http
204 No Content
```

A deactivated reader is hidden from normal reader queries but remains visible through `with-inactive` endpoints.

# Catalog API

The Catalog API manages books and physical book items.

## Get all active books

```http
GET /camplib/v1/books
```

Example response:

```json
[
  {
    "id": "b0000001-0000-0000-0000-000000000000",
    "authorsText": "Robert C. Martin",
    "title": "Clean Code",
    "subtitle": "A Handbook of Agile Software Craftsmanship",
    "isbn": "9780132350884",
    "totalBookItems": 2,
    "availableBookItems": 2
  }
]
```

## Get one active book by id

```http
GET /camplib/v1/books/{id}
```

Example response:

```json
{
  "id": "b0000001-0000-0000-0000-000000000000",
  "authorsText": "Robert C. Martin",
  "title": "Clean Code",
  "subtitle": "A Handbook of Agile Software Craftsmanship",
  "isbn": "9780132350884",
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

Supported search fields:

```text
Title
AuthorLastName
Isbn
```

`AuthorLastName` searches the author text by lastname rule.

```text
Robert C. Martin -> Martin
Martin Fowler -> Fowler
Kent Beck -> Beck
```

## Create a book

```http
POST /camplib/v1/books
```

Request body:

```json
{
  "authorsText": "Robert C. Martin",
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

## Add a physical book item

```http
POST /camplib/v1/books/{bookId}/items
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

A new book item starts with status `Available`.

## Deactivate a book

```http
PATCH /camplib/v1/books/{bookId}/deactivate
```

Successful response:

```http
200 OK
```

A deactivated book is hidden from normal book read endpoints and searches.

# DTO overview

## BookCreateDto

```csharp
public sealed record BookCreateDto(
   string? AuthorsText,
   string? Title,
   string? Subtitle,
   string? Isbn,
   string? Id
);
```

## BookSearchField

```csharp
public enum BookSearchField {
   Title = 1,
   AuthorLastName = 2,
   Isbn = 3
}
```

## Error handling

The API returns errors as `ProblemDetails`.

Typical status codes:

```text
400 Bad Request
401 Unauthorized
403 Forbidden
404 Not Found
409 Conflict
```
