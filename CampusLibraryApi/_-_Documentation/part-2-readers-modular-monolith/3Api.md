# API Documentation: CampusLibrary Part 2

This document describes the public HTTP API of **Part 2 – Readers Modular Monolith**.

Swagger/OpenAPI is the authoritative technical API description. This document provides a didactic overview for students.

## Base URL

In development, the API is typically available at:

```text
https://localhost:8010
http://localhost:8012
```

The current API prefix is:

```text
/camplib/v1
```

Example:

```http
GET /camplib/v1/readers
```

## Swagger

Swagger UI is available in development mode:

```text
https://localhost:8010/swagger
```

Swagger documents:

- routes
- request bodies
- response bodies
- status codes
- ProblemDetails responses
- DTO schemas

## Module Scope

Part 2 contains only the **Readers** module.

It does not contain Catalog, Books, BookItems or Loans.

A Reader represents a business library reader of the CampusLibrary domain. A Reader is not the same as a technical login account.

The connection to a technical identity is represented through:

```text
Subject
```

## Reader Behavior

Readers are not physically deleted.

A Reader has an `IsActive` flag.

Normal query endpoints return only active readers. Additional endpoints can include deactivated readers.

The public API still uses HTTP semantics. Therefore the endpoint:

```http
DELETE /camplib/v1/readers/{id}
```

means:

```text
Deactivate the reader.
```

It does not physically remove the database row.

## Reader Routes

### Get all active readers

```http
GET /camplib/v1/readers
```

Returns all active readers.

Successful response:

```http
200 OK
```

Response body:

```json
[
  {
    "id": "10000000-0000-0000-0000-000000000000",
    "subject": "70000000-0007-0000-0000-000000000000",
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

### Get all readers including inactive readers

```http
GET /camplib/v1/readers/with-inactive
```

Returns active and deactivated readers.

This endpoint is useful for administration, tests and teaching the difference between normal queries and explicit queries including inactive data.

Successful response:

```http
200 OK
```

### Get one active reader by id

```http
GET /camplib/v1/readers/{id}
```

Example:

```http
GET /camplib/v1/readers/10000000-0000-0000-0000-000000000000
```

Returns the reader only if the reader is active.

Successful response:

```http
200 OK
```

Possible error responses:

```http
400 Bad Request
404 Not Found
```

### Get one reader by id including inactive readers

```http
GET /camplib/v1/readers/{id}/with-inactive
```

Returns the reader even if the reader is deactivated.

Successful response:

```http
200 OK
```

Possible error responses:

```http
400 Bad Request
404 Not Found
```

### Get one active reader by email

```http
GET /camplib/v1/readers/email?email={email}
```

Example:

```http
GET /camplib/v1/readers/email?email=erika.mustermann@t-online.de
```

Returns the reader only if the reader is active.

Successful response:

```http
200 OK
```

Possible error responses:

```http
400 Bad Request
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
  "id": null
}
```

Successful response:

```http
201 Created
```

Possible error responses:

```http
400 Bad Request
409 Conflict
```

### Update a reader

```http
PUT /camplib/v1/readers/{id}
```

Example:

```http
PUT /camplib/v1/readers/10000000-0000-0000-0000-000000000000
```

Request body for changing only the lastname:

```json
{
  "firstname": null,
  "lastname": "Meier",
  "email": null,
  "addressDto": null
}
```

Request body for changing only the email:

```json
{
  "firstname": null,
  "lastname": null,
  "email": "e.meier@gmx.de",
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
404 Not Found
409 Conflict
```

### Deactivate a reader

```http
DELETE /camplib/v1/readers/{id}
```

Example:

```http
DELETE /camplib/v1/readers/10000000-0000-0000-0000-000000000000
```

This endpoint deactivates the reader.

It does not physically delete the database row.

Successful response:

```http
204 No Content
```

Possible error responses:

```http
400 Bad Request
404 Not Found
409 Conflict
```

A deactivated reader no longer appears in normal read queries:

```http
GET /camplib/v1/readers
GET /camplib/v1/readers/{id}
```

A deactivated reader can still be inspected through endpoints that explicitly include inactive readers:

```http
GET /camplib/v1/readers/with-inactive
GET /camplib/v1/readers/{id}/with-inactive
```

## DTOs

### AddressDto

```csharp
public sealed record AddressDto(
   string Street,
   string PostalCode,
   string City,
   string? Country
);
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

`Id` is optional. In normal API usage, the use case can create a new id. In tests and deterministic seed scenarios, an id may be supplied explicitly.

### ReaderUpdateDto

```csharp
public sealed record ReaderUpdateDto(
   string? Firstname,
   string? Lastname,
   string? Email,
   AddressDto? AddressDto
);
```

`ReaderUpdateDto` supports partial updates. Fields with `null` are not changed.

### ReaderDto

```csharp
public sealed record ReaderDto(
   Guid Id,
   string Subject,
   string Firstname,
   string Lastname,
   string Email,
   AddressDto AddressDto
);
```

## ProblemDetails

Business and validation errors are mapped to HTTP responses through ProblemDetails.

Typical mappings are:

```text
400 Bad Request -> invalid input
404 Not Found   -> reader not found
409 Conflict    -> duplicate data or invalid state transition
```

The controller does not throw business exceptions for normal domain errors. Instead, use cases and read models return `Result<T>` values that are translated into HTTP responses.

## Teaching Notes

Part 2 is intentionally small.

The API is used to show:

- how a controller calls command use cases
- how a controller calls read models for queries
- how domain errors become HTTP ProblemDetails
- how soft-deactivation differs from physical deletion
- how a modular monolith can still expose a simple HTTP API
