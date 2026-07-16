# API – CampusLibrary Part 6

German version: [3Api-ger.md](3Api-ger.md)

Official branch:

```text
part-6/authn-provision-profile
```

CampusLibrary API base route:

```text
https://localhost:8010/camplib/v1
```

Swagger in Development:

```text
https://localhost:8010/swagger
```

## Authentication

Reader self-service endpoints require a valid access token:

```http
Authorization: Bearer <access-token>
```

The access token is issued by IdentityAccessServer and validated by the CampusLibrary API as a JWT Bearer token.

Important technical claims are:

```text
sub                 stable technical user identifier
preferred_username  username, initially identical to email
created_at          technical identity creation time
admin_rights        technical compatibility value
account type/role   reader or employee
```

The exact claim representation is translated into `IIdentityGateway` by the Web adapter. Use cases do not access claims directly.

## Reader endpoints

### Provision the current Reader

```http
POST /camplib/v1/readers/me/provision
```

Expected success:

```text
204 No Content
```

The request does not require a domain Reader form. Subject and initial username come from the access token. An optional test ID is only intended for reproducible Development and test workflows.

Typical errors:

```text
401 IdentityUnauthenticated
403 AccessNotAllowed
400 SubjectRequired
400 InvalidIdentitySubject
400 IdentityEmailRequired
400 TimestampInvalid
409 ReaderAlreadyProvisioned
```

### Read the current Reader

```http
GET /camplib/v1/readers/me
```

Success:

```text
200 OK
```

The Reader is resolved through `IIdentityGateway.Subject`.

### Complete the initial profile

```http
PUT /camplib/v1/readers/me/profile
Content-Type: application/json
```

Example:

```json
{
  "firstname": "Rita",
  "lastname": "Reader",
  "addressDto": {
    "street": "Bibliotheksweg 99",
    "postalCode": "29556",
    "city": "Suderburg",
    "country": "DE"
  }
}
```

Success:

```text
200 OK
```

`ReaderProfileDto` intentionally has no email property. The initial email originates from the technical identity.

### Selectively update the current profile

```http
PUT /camplib/v1/readers/me/update
Content-Type: application/json
```

Example:

```json
{
  "lastname": "Reader-New",
  "email": "rita.new@example.org",
  "addressDto": null
}
```

`null` means that the current value remains unchanged.

Success:

```text
200 OK
```

The later domain email may differ from `preferred_username`. Reader lookup remains based on the subject.

## Catalog endpoints

Catalog largely remains as in Part 5.

### Read and search Books

```http
GET /camplib/v1/books
GET /camplib/v1/books/{bookId}
GET /camplib/v1/books/search?searchField=Title&searchText=...
GET /camplib/v1/books/search?searchField=AuthorLastName&searchText=...
GET /camplib/v1/books/search?searchField=Isbn&searchText=...
```

### Create a Book

```http
POST /camplib/v1/books
Content-Type: application/json
```

Example:

```json
{
  "authorsText": "Robert C. Martin",
  "title": "Clean Code",
  "subtitle": null,
  "isbn": "9780132350884",
  "id": "00000001-0000-0000-0000-000000000000"
}
```

Success:

```text
201 Created
```

`id` is optional and supports reproducible Development and test workflows.

### Add a BookItem

```http
POST /camplib/v1/books/{bookId}/items
Content-Type: application/json
```

A `BookItem` is a physical copy. Its `Id` is its identity; the current model does not use an additional inventory number.

### Deactivate a Book

```http
PATCH /camplib/v1/books/{bookId}/deactivate
```

Deactivation checks for active Loans. The Catalog module queries the Loan module through a BC-to-BC port.

## Loan self-service

### Read current Reader Loans

```http
GET /camplib/v1/loans/me
```

Success when there are no Loans:

```text
200 OK
[]
```

### Borrow a BookItem

```http
POST /camplib/v1/loans/me
Content-Type: application/json
```

Example:

```json
{
  "bookItemId": "00000002-0000-0000-0000-000000000000",
  "id": "00000099-0000-0001-0000-000000000000"
}
```

Success:

```text
201 Created
Location: /camplib/v1/loans/me/{loanId}
```

The client sends no Reader ID. The Reader is resolved from the token subject.

Borrow validates, among other things:

```text
identity is authenticated
identity represents a Reader
subject is valid
Reader is provisioned
Reader profile is complete
Reader is active
BookItem exists and may be borrowed
BookItem is not already on loan
```

### Read a current Reader Loan

```http
GET /camplib/v1/loans/me/{loanId}
```

The Loan must belong to the current Reader.

### Renew a current Reader Loan

```http
PATCH /camplib/v1/loans/me/{loanId}/renew
```

Success:

```text
200 OK
```

The domain validates renewal rules, increments the renewal count and calculates a new due date through `IClock`.

### Return at desk

```http
PATCH /camplib/v1/loans/{loanId}/return-at-desk
```

Success:

```text
204 No Content
```

Returning removes the Loan. The following result is therefore correct:

```http
GET /camplib/v1/loans/me/{loanId}
```

```text
404 Not Found
```

## DTOs

Server-side HTTP DTOs are grouped by module:

```text
ReaderDtos.cs
CatalogDtos.cs
LoanDtos.cs
```

The client owns separate files with matching transport structures. There is no project reference from the client to Core modules.

## Error responses

Domain errors are returned as `ProblemDetails`. Controllers select the HTTP status explicitly.

Example:

```json
{
  "type": "...",
  "title": "Access not allowed",
  "status": 403,
  "detail": "..."
}
```

Typical mapping:

```text
400 Bad Request   validation or invalid identity data
401 Unauthorized  no authenticated technical identity
403 Forbidden     wrong account type or access to another user's resource
404 Not Found     Reader, Book, BookItem or Loan not found
409 Conflict      already provisioned, already borrowed or another conflict
```

## Recommended manual workflow

1. Start IdentityAccessServer.
2. Start the CampusLibrary API.
3. Optionally start the Blazor client.
4. Create a Development user in IdentityAccessServer.
5. Obtain an access token.
6. Execute `POST /readers/me/provision`.
7. Verify `GET /readers/me`.
8. Execute `PUT /readers/me/profile`.
9. Create Books and BookItems.
10. Borrow and renew through `/loans/me`.
11. Return the Loan at the desk.

The `.http` files use token variables and script assertions for expected status codes.
