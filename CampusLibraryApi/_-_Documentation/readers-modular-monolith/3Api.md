# API Documentation

This document describes the public HTTP API of the current `CampusLibraryApi`.

Swagger/OpenAPI is the authoritative technical API description. This file provides an additional didactic overview for students.

## Base URL

```text
https://localhost:8010
http://localhost:8012
```

The current API prefix is:

```text
/camplib/v1
```

## Reader Module

A Reader represents a domain-level library user of the CampusLibrary domain.

It is not the same as a technical user account. The technical identity reference is represented by `Subject`.

## Reader Routes

### Get all active readers

```http
GET /camplib/v1/readers
```

Returns all active readers. Inactive readers are not returned by this endpoint.

### Get all readers including inactive readers

```http
GET /camplib/v1/readers/with-inactive
```

Returns all readers, including inactive readers. This endpoint is intended for administrative or internal views.

### Get one active reader by id

```http
GET /camplib/v1/readers/{id}
```

Returns the reader only if the reader is active. A deactivated reader is treated as not found in this normal reader view.

### Get one reader by id including inactive readers

```http
GET /camplib/v1/readers/{id}/with-inactive
```

Returns the reader even if the reader is inactive.

### Get one active reader by email

```http
GET /camplib/v1/readers/email?email={email}
```

Returns the reader only if the reader is active.

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

Successful response: `201 Created`.

## ReaderDto

```csharp
public sealed record ReaderDto(
   Guid Id,
   string? Firstname,
   string? Lastname,
   string? Email,
   AddressDto AddressDto,
   bool IsActive,
   string? Subject
);
```

`IsActive` shows whether the reader is currently part of the active reader list.

Normal reader endpoints return only readers with `IsActive == true`. `WithInactive` endpoints can also return readers with `IsActive == false`.

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

`Id` is optional. It may be omitted in normal API usage, or provided for teaching, testing, or deterministic seed data.

## Update a reader

```http
PUT /camplib/v1/readers/{id}
```

`ReaderUpdateDto` supports partial updates:

```csharp
public sealed record ReaderUpdateDto(
   string? Lastname,
   string? Email,
   AddressDto? AddressDto
);
```

The meaning of `null` is:

```text
Lastname = null   -> no change
Email = null      -> no change
AddressDto = null -> no change
```

## Deactivate a reader

```http
DELETE /camplib/v1/readers/{id}
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

```text
400 Bad Request   -> invalid input
401 Unauthorized  -> authentication required
403 Forbidden     -> access denied
404 Not Found     -> resource not found or inactive in a normal active-reader view
409 Conflict      -> duplicate email, duplicate subject, or already deactivated reader
```

## Read Side and Write Side

```text
GET endpoints             -> IReaderReadModel
POST / PUT / DELETE       -> IReaderUseCases
```

This separation supports a clean teaching model:

```text
queries read DTOs
commands change aggregates
repositories work with domain objects
read models return DTOs directly
```
