# API Documentation

This document describes the public HTTP API of the current `CampusLibraryApi`.

Swagger/OpenAPI is the authoritative technical API description. This file provides an additional didactic overview for students.

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

## Reader Module

The current API contains the Reader module.

A Reader represents a domain-level library user of the CampusLibrary domain.

It is not the same as a technical user account.

The technical identity reference is represented by:

```text
Subject
```

## Reader Routes

### Get all active readers

```http
GET /camplib/v1/readers
```

Returns all active readers.

Inactive readers are not returned by this endpoint.

Successful response:

```http
200 OK
```

Response body:

```json
[
  {
    "id": "10000000-0000-0000-0000-000000000000",
    "subject": "a00090ad-d9df-486a-8757-4a649e26a54e",
    "firstname": "Erika",
    "lastname": "Mustermann",
    "email": "erika.mustermann@t-online.de",
    "addressDto": {
      "street": "Hauptstr. 23",
      "postalCode": "29556",
      "city": "Suderburg",
      "country": "DE"
    },
    "isActive": true
  }
]
```

### Get all readers including inactive readers

```http
GET /camplib/v1/readers/with-inactive
```

Returns all readers, including inactive readers.

This endpoint is intended for administrative or internal views.

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

A deactivated reader is treated as not found in this normal reader view.

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

### Get one reader by id including inactive readers

```http
GET /camplib/v1/readers/{id}/with-inactive
```

Example:

```http
GET /camplib/v1/readers/10000000-0000-0000-0000-000000000000/with-inactive
```

Returns the reader even if the reader is inactive.

This endpoint is intended for administrative or internal views.

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

Response body:

```json
{
  "id": "generated-or-provided-id",
  "subject": "70000000-0007-0000-0000-000000000000",
  "firstname": "Edgar",
  "lastname": "Engel",
  "email": "e.engel@freenet.de",
  "addressDto": {
    "street": "Am Markt 14",
    "postalCode": "04109",
    "city": "Leipzig",
    "country": "DE"
  },
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

## ReaderCreateDto

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

This is intentional.

The id may be omitted in normal API usage. The use case then creates a new id.

The id may be provided for teaching, testing, or deterministic seed data.

Therefore, `Id` is both technically and conceptually nullable.

## Update a reader

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
  "lastname": "Meier",
  "email": null,
  "addressDto": null
}
```

Request body for changing only the email:

```json
{
  "lastname": null,
  "email": "e.meier@gmx.de",
  "addressDto": null
}
```

Request body for changing only the address:

```json
{
  "lastname": null,
  "email": null,
  "addressDto": {
    "street": "Schillerstr. 1",
    "postalCode": "30123",
    "city": "Hannover",
    "country": "DE"
  }
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

## ReaderUpdateDto

```csharp
public sealed record ReaderUpdateDto(
   string? Lastname,
   string? Email,
   AddressDto? AddressDto
);
```

All properties are nullable by design.

This is required for partial updates.

The meaning of `null` is:

```text
Lastname = null   -> no change
Email = null      -> no change
AddressDto = null -> no change
```

This is technically and conceptually required.

`Firstname` is intentionally not part of the update DTO.

`Subject` is also intentionally not part of the update DTO.

The technical identity reference is not changed by normal profile updates.

## Deactivate a reader

```http
DELETE /camplib/v1/readers/{id}
```

Example:

```http
DELETE /camplib/v1/readers/30000000-0000-0000-0000-000000000000
```

Successful response:

```http
204 No Content
```

This endpoint does not physically delete the reader from the database.

It triggers a soft delete:

```text
Reader.Deactivate(...)
IsActive = false
```

After deactivation:

```text
GET /camplib/v1/readers/{id}               -> 404 Not Found
GET /camplib/v1/readers/{id}/with-inactive -> 200 OK
GET /camplib/v1/readers                    -> reader is not included
GET /camplib/v1/readers/with-inactive      -> reader is included
```

Possible error responses:

```http
400 Bad Request
401 Unauthorized
403 Forbidden
404 Not Found
409 Conflict
```

`409 Conflict` is returned when a reader is already deactivated.

## AddressDto

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

## Error Responses

The API uses `ProblemDetails` for error responses.

Example structure:

```json
{
  "type": "about:blank",
  "title": "Bad Request",
  "status": 400,
  "detail": "Invalid email.",
  "code": "Reader.InvalidEmail",
  "traceId": "..."
}
```

The exact `code` depends on the domain or application error.

Typical categories:

```text
400 Bad Request   -> invalid input
401 Unauthorized  -> authentication required
403 Forbidden     -> access denied
404 Not Found     -> resource not found or inactive in a normal active-reader view
409 Conflict      -> duplicate email, duplicate subject, or already deactivated reader
```

## Read Side and Write Side

The Reader controller uses two different application ports:

```text
IReaderReadModel
IReaderUseCases
```

The read side is used for queries:

```text
GET /readers
GET /readers/with-inactive
GET /readers/{id}
GET /readers/{id}/with-inactive
GET /readers/email
```

The write side is used for commands:

```text
POST /readers
PUT /readers/{id}
DELETE /readers/{id}
```

This separation supports a clean teaching model:

```text
queries read DTOs
commands change aggregates
repositories work with domain objects
read models return DTOs directly
```

## Didactic Goals

This API is designed to demonstrate:

```text
REST-style endpoints
API versioning
Swagger/OpenAPI documentation
DTOs as API contracts
Result-based error handling
ProblemDetails responses
partial update semantics
soft delete through deactivation
separation of read and write paths
controller tests with WebApplicationFactory
```

Swagger should be used for technical exploration.

This document should be used for conceptual explanation.
