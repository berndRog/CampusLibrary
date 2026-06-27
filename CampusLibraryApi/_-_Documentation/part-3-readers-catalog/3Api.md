# API Documentation — Part 3

This document describes the public HTTP API of Part 3 of `CampusLibraryApi`.

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
```

# Readers API

A Reader represents a domain-level library user.

The technical identity reference is represented by `Subject`.

## Get all active readers

```http
GET /camplib/v1/readers
```

Response:

```http
200 OK
```

## Get all readers including inactive readers

```http
GET /camplib/v1/readers/with-inactive
```

Response:

```http
200 OK
```

## Get one active reader by id

```http
GET /camplib/v1/readers/{id}
```

Possible responses:

```http
200 OK
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
404 Not Found
```

## Get one reader by email

```http
GET /camplib/v1/readers/email?email={email}
```

Possible responses:

```http
200 OK
400 Bad Request
404 Not Found
```

## Create a reader

```http
POST /camplib/v1/readers
Content-Type: application/json
```

Example request:

```json
{
  "firstname": "Erika",
  "lastname": "Mustermann",
  "email": "e.mustermann@example.com",
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
409 Conflict
```

## Update a reader

```http
PUT /camplib/v1/readers/{id}
Content-Type: application/json
```

Possible responses:

```http
200 OK
400 Bad Request
404 Not Found
409 Conflict
```

## Deactivate a reader

```http
DELETE /camplib/v1/readers/{id}
```

The endpoint deactivates the reader. It does not physically delete the row.

Possible responses:

```http
204 No Content
404 Not Found
409 Conflict
```

# Books API

A Book represents a bibliographic work. A BookItem represents one physical copy.

There is no Author API in Part 3. Authors are stored in `authorsText`.

## Get all active books

```http
GET /camplib/v1/books
```

Response:

```http
200 OK
```

## Get one active book by id

```http
GET /camplib/v1/books/{id}
```

Possible responses:

```http
200 OK
404 Not Found
```

## Search books

```http
GET /camplib/v1/books/search?searchField=Title&searchText=Clean
GET /camplib/v1/books/search?searchField=Isbn&searchText=9780132350884
GET /camplib/v1/books/search?searchField=AuthorLastName&searchText=Martin
```

Supported search fields:

```text
Title
Isbn
AuthorLastName
```

The API accepts one search field at a time. If no book matches, the response is `200 OK` with an empty list.

## Create a book

```http
POST /camplib/v1/books
Content-Type: application/json
```

Example request:

```json
{
  "authorsText": "Robert C. Martin",
  "title": "Clean Code",
  "subtitle": "A Handbook of Agile Software Craftsmanship",
  "isbn": "9780132350884",
  "id": "b0000001-0000-0000-0000-000000000000"
}
```

Possible responses:

```http
201 Created
400 Bad Request
409 Conflict
```

## Add a BookItem

```http
POST /camplib/v1/books/{bookId}/items
Content-Type: application/json
```

Example request:

```json
{
  "inventoryNumber": "CL-BOOK-0001",
  "id": "be000001-0000-0000-0000-000000000000"
}
```

Possible responses:

```http
200 OK
400 Bad Request
404 Not Found
409 Conflict
```

## Deactivate a book

```http
PATCH /camplib/v1/books/{bookId}/deactivate
```

Possible responses:

```http
200 OK
404 Not Found
409 Conflict
```

Deactivated books are hidden from normal book queries and search results.

## Status and deactivation concepts

```text
Reader and Book use IsActive.
BookItem uses BookItemStatus.
```

This distinction prepares the model for Part 4, where Loan will also use a status instead of `IsActive`.
