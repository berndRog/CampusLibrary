# CampusLibrary — Part 5: Client without real AuthN

Teaching project for a modular, DDD-oriented ASP.NET Core Web API and a Blazor SSR client.

German version: [1Readme-ger.md](1Readme-ger.md)

## Current status

Part 5 already contains large parts of the business structure used in Part 6. The main difference is the source of the technical identity:

```text
Part 5: API-owned DevIdentity from appsettings.json
Part 6: validated claims from an IdentityAccessServer access token
```

In Part 5 the client sends neither a Bearer token nor custom identity headers to the API. Self-service endpoints under `/me` can still be used because the API simulates the current technical user itself.

Verified state from July 15, 2026:

```text
dotnet clean
Build succeeded

dotnet build
Build succeeded

dotnet test
212 total, 212 succeeded, 0 failed, 0 skipped
```

The manual `Loan_Me.http` workflow was also completed successfully:

```text
GET   /loans/me                    -> 200
POST  /loans/me                    -> 201
GET   /loans/me/{id}               -> 200
PATCH /loans/me/{id}/renew         -> 200
PATCH /loans/{id}/return-at-desk   -> 204
GET   /loans/me/{id} after return  -> 404
```

## Branch

```text
part-5/client-noauth
```

The branch is published on GitHub and tracks:

```text
origin/part-5/client-noauth
```

## Goal of Part 5

Part 5 demonstrates how a modular CampusLibrary API is consumed by a Blazor SSR client and how self-service endpoints can already be prepared without a real IdentityAccessServer.

Active in Part 5:

```text
Blazor SSR client
HTTP access to CampusLibraryApi
module-specific API clients
unified transport DTOs
Readers, Catalog and Loans modules
reader/employee perspective in the client
API-side technical DevIdentity
subject-based Reader association
Reader self-service update through /readers/me/update
Loan self-service through /loans/me
administrative Reader, Catalog and Loan endpoints
ProblemDetails-based error handling
Bootstrap-based layout
prepared but disabled AuthN/AuthZ infrastructure
```

Inactive in Part 5:

```text
real registration
real login against IdentityAccessServer
real logout session against IdentityAccessServer
access-token forwarding to the API
JWT Bearer authentication in the API
policy-based API authorization
Reader provisioning from an access token
protected API endpoints
```

## Project structure

```text
CampusLibraryApi                 executable API project / Composition Root
CampusLibraryApi_1_Web           controllers, ProblemDetails, DevIdentity adapter
CampusLibraryApi_2_BuildingBlocks shared ports, Result, errors, BC contracts
CampusLibraryApi_3_Core_Readers  Reader domain and application use cases
CampusLibraryApi_3_Core_Catalog  Catalog domain and application use cases
CampusLibraryApi_3_Core_Loan     Loan domain and application use cases
CampusLibraryApi_4_Infrastructure EF Core, repositories, read models, contract adapters
CampusLibraryApiTest             automated API tests
CampusLibraryClient              Blazor SSR client
IdentityAccessServer             prepared, not actively used in Part 5
Shared                           shared technical helpers
```

## Central architectural rule

The business API modules remain the owners of their public HTTP DTOs:

```text
Readers  -> ReaderDtos.cs
Catalog  -> CatalogDtos.cs
Loans    -> LoanDtos.cs
```

The client references no API core project. It owns transport types with the same JSON shape:

```text
CampusLibraryClient/Api/Dtos/ReaderDtos.cs
CampusLibraryClient/Api/Dtos/CatalogDtos.cs
CampusLibraryClient/Api/Dtos/LoanDtos.cs
```

Only true cross-module contracts live in BuildingBlocks:

```text
_1_Ports/Contracts
_2_Application/Dtos
```

Examples:

```text
IBookItemLoanContract
ILoanCatalogContract
ILoanReaderContract
IReaderLoanContract
BookItemLoanInfoDto
CurrentBookItemLoanInfoDto
ReaderLoanInfoDto
```

## Two separate DevIdentity uses

Part 5 has a DevIdentity in the client and a DevIdentity in the API. Both read their own `appsettings.json`; no identity data is transferred between the applications.

### Client

The client uses `DevCurrentUserProvider` for:

```text
visible navigation
reader/employee perspective
DisplayName
ReaderId for UI purposes
email display
```

### API

The API uses `DevIdentityGateway` as the `IIdentityGateway` implementation for:

```text
IsAuthenticated
Subject
AccountType -> IsReader / IsEmployee
Email -> Username
CreatedAt
AdminRights = 0
```

Both applications should use the same `ActiveProfile` when they are tested together.

## Example configuration

Using the same profile shape in both applications avoids unnecessary drift. Fields not required by one adapter are ignored there.

```json
{
  "Features": {
    "AuthNEnabled": false,
    "DevIdentityEnabled": true,
    "ApiAccessTokenEnabled": false,
    "AuthZEnabled": false
  },
  "DevIdentity": {
    "ActiveProfile": "ReaderRita",
    "Profiles": {
      "ReaderRita": {
        "IsAuthenticated": true,
        "Subject": "reader-099",
        "AccountType": "reader",
        "ReaderId": "00000099-0000-0000-0000-000000000000",
        "DisplayName": "Rita Reader",
        "Email": "r.reader@library.local",
        "CreatedAt": "2025-01-01T00:00:00Z",
        "AdminRights": 0
      },
      "EmployeeAdmin": {
        "IsAuthenticated": true,
        "Subject": "employee-admin",
        "AccountType": "employee",
        "ReaderId": null,
        "DisplayName": "Admin",
        "Email": "admin@mail.local",
        "CreatedAt": "2025-01-01T00:00:00Z",
        "AdminRights": 0
      }
    }
  }
}
```

Important:

```text
DevIdentity.Subject must exactly match Reader.Subject in the database.
```

For Rita in the manual test state:

```text
Subject  = reader-099
ReaderId = 00000099-0000-0000-0000-000000000000
```

Subject and ReaderId are different identifiers.

The email address may be changed later. Therefore a Reader is resolved by the stable subject, not by email.

## API-side technical identity

The Part 5 data flow is:

```text
CampusLibraryApi/appsettings.json
        ↓
DevIdentityOptions
        ↓
DevIdentityGateway
        ↓
IIdentityGateway
        ↓
IdentitySubject.Check(...)
        ↓
load Reader by Subject
        ↓
/readers/me/update and /loans/me
```

`IdentitySubject.Check(...)` verifies:

```text
the user is simulated as authenticated
the user is a Reader
Subject is present
Subject is no longer than 200 characters
Username is present
CreatedAt is valid
```

`AdminRights` remains in the port for Part 6 compatibility. CampusLibrary does not evaluate it as a business permission, and Part 5 sets it to `0`.

## CampusLibraryClient

The client is a Blazor SSR application with interactive server components.

Important concepts:

```text
Razor Components
Interactive Server Render Mode
module-specific API clients
Result<T> for success/failure handling
ProblemDetails-based error messages
Bootstrap utilities
ICurrentUserProvider as UI abstraction
prepared AccessTokenHandler, inactive in Part 5
```

Important folders:

```text
CampusLibraryClient
├─ Api
│  ├─ Auth
│  ├─ Clients
│  ├─ Contracts
│  ├─ Dtos
│  └─ Errors
├─ Core
├─ Extensions
├─ Security
├─ Shared
└─ Ui
   ├─ Components
   ├─ Controllers
   ├─ Models
   └─ Pages
```

## Visible pages

```text
/                                      home page
/readers                               Reader list for employees
/catalog/books                         catalog
/catalog/books/create                  create book
/catalog/books/{bookId}/items/add      add item
/catalog/books/{bookId}/deactivate     deactivate book
/catalog/books/{bookId}/borrow         borrow book
/loans                                 current loans for employees
/loans/{loanId}                        loan details
/my/loans                              current Reader's loans
/logout                                demo/prepared logout page
/access-denied                         prepared error page
/error                                 technical error page
```

The API already exposes `PUT /readers/me/update`; a complete Reader profile edit page is not yet visible in this client part.

## API clients per module

```text
IReaderClient -> ReaderClient
IBookClient   -> BookClient
ILoanClient   -> LoanClient
```

The client BaseUrl is configured as:

```json
{
  "CampusLibraryApi": {
    "BaseUrl": "https://localhost:8010/"
  }
}
```

Part 5 calls the API without a Bearer token.

## Readers

The Readers area provides:

```text
Reader list
Reader by id
Reader by email
administrative Reader creation
self-service update of the current Reader
soft delete / deactivation
```

The old update endpoint with an explicit ReaderId was replaced:

```text
old: PUT /readers/{id}
new: PUT /readers/me/update
```

Client method:

```text
UpdateMeAsync(ReaderUpdateDto dto)
```

`ReaderUpdateDto` contains optional values:

```text
Lastname
Email
AddressDto
```

`null` means: leave the current value unchanged.

## Catalog

Catalog uses one unified `BookDto` for list and detail views:

```text
Id
AuthorsText
Title
Subtitle
Isbn
BookItems
TotalItems
AvailableItems
IsActive
```

`BookItemDto` contains:

```text
Id
BookId
Status
```

An additional `InventoryNumber` is no longer part of the current transport contract. The BookItem identity is its `Guid Id`.

BookItem status values:

```text
1 = Available
2 = Unavailable
3 = Lost
4 = Damaged
```

## Loans

Loans uses one unified `LoanDto` for list and detail views.

A stored Loan always represents a currently existing borrowing process. Therefore the current Loan contract has no `Status` or `ReturnedAt` fields.

Return semantics:

```text
PATCH /loans/{id}/return-at-desk
        ↓
Loan is deleted
        ↓
a later GET returns 404
```

Reader self-service:

```text
GET   /loans/me
GET   /loans/me/{id}
POST  /loans/me
PATCH /loans/me/{id}/renew
```

Administrative endpoints:

```text
GET   /loans
GET   /loans/{id}
POST  /loans
PATCH /loans/{id}/renew
PATCH /loans/{id}/return-at-desk
```

For `/loans/me`, the client sends no ReaderId. The API resolves the Reader through `IIdentityGateway.Subject`.

## Error handling

Use cases and read models return `Result` or `Result<T>`.

Controllers explicitly map `DomainError.Status` to HTTP responses:

```text
BadRequest   -> 400
Unauthorized -> 401
Forbidden    -> 403
NotFound     -> 404
Conflict     -> 409
```

Errors are returned as `ProblemDetails`. The client handles them centrally in `BaseApiClient`.

## Manual HTTP tests without client or IA server

Only the running API is required for `/me` tests:

```bash
dotnet run --project CampusLibraryApi
```

No headers are required:

```http
GET https://localhost:8010/camplib/v1/loans/me
Accept: application/json
```

Preconditions:

```text
DevIdentity:ActiveProfile = ReaderRita
ReaderRita.Subject matches Reader.Subject in the database
required Reader, Book and BookItem test data exist
```

## Running the projects

API:

```bash
dotnet run --project CampusLibraryApi
```

Client:

```bash
dotnet run --project CampusLibraryClient
```

Verification:

```bash
dotnet clean
dotnet build
dotnet test
```

## Continuation in Part 6

Part 6 replaces the technical identity source:

```text
DevIdentityGateway
        ↓ replaced by
claim-/HttpContext-based IIdentityGateway
```

The following can remain unchanged:

```text
IIdentityGateway
IdentitySubject.Check(...)
subject-based Reader association
ReaderUcUpdateMe
/me endpoints
business use cases
DTO boundaries
```

Part 6 adds:

```text
OIDC login in the client
cookie session in the SSR client
access token
Bearer-token forwarding
JWT validation in the API
real claims instead of appsettings values
```
