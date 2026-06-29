# CampusLibrary — Part 5: Client without active Auth

Teaching project for a modular, DDD-oriented ASP.NET Core Web API and a Blazor Server-Side Rendering client.

German version: [1Readme-ger.md](1Readme-ger.md)

## Current status

This version adds a real web client to the modular CampusLibrary API.

Part 5 builds on Part 4:

* Readers
* Catalog
* Loans
* CampusLibraryClient

The API modules remain unchanged in their core responsibility. The new client consumes the existing HTTP API through module-specific API clients.

Known build result for the current Part 5 start state:

```text
dotnet build
Build succeeded
```

Part 5 does not introduce active authentication or authorization. The client runs anonymously against the CampusLibraryApi. AuthN/AuthZ preparation may remain in the codebase, but it is disabled by feature flags.

## Current branch

```text
part-5/client-noauth
```

## Project structure

```text
CampusLibraryApi
CampusLibraryApi_1_Web
CampusLibraryApi_2_BuildingBlocks
CampusLibraryApi_3_Core_Readers
CampusLibraryApi_3_Core_Catalog
CampusLibraryApi_3_Core_Loan
CampusLibraryApi_4_Infrastructure
CampusLibraryApiTest
CampusLibraryClient
```

## Architectural idea

Part 4 showed the modular API as a project-based modular monolith.

Part 5 adds a separate Blazor SSR client. The client is not part of the domain core. It is an external user interface that accesses the API over HTTP.

Central dependency idea:

```text
CampusLibraryClient does not reference the API core modules.
CampusLibraryClient uses HTTP clients and DTOs.
CampusLibraryApi remains the owner of business rules.
The client displays data and triggers API workflows.
```

This keeps a clear boundary between the web UI and the backend modules.

## CampusLibraryClient

The client is a Blazor Server-Side Rendering application.

Important concepts:

```text
Blazor SSR
Razor components
module-specific API clients
DTOs for API transport
Result<T> for client-side success/failure handling
ProblemDetails-based error display
simple navigation
prepared but inactive AuthN/AuthZ
```

Main client folders:

```text
CampusLibraryClient
├─ Api
│  ├─ Clients
│  ├─ Contracts
│  ├─ Dtos
│  ├─ Errors
│  └─ Auth
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

The current visible pages are:

```text
/
/readers
/catalog/books
/loans
```

The client already contains infrastructure for later command pages, such as create/update models and API client methods. The first Part 5 focus is the vertical slice from navigation to list pages and API error display.

## API clients per module

The client uses one API client abstraction per functional area:

```text
IReaderClient -> ReaderClient
IBookClient   -> BookClient
ILoanClient   -> LoanClient
```

These clients call the existing CampusLibraryApi routes under:

```text
/camplib/v1/readers
/camplib/v1/books
/camplib/v1/loans
```

The API clients are registered through:

```text
AddCampusLibraryClients(...)
```

The base URL is configured in `appsettings.json`:

```json
{
  "CampusLibraryApi": {
    "BaseUrl": "https://localhost:8010/"
  }
}
```

## Error handling

API errors are handled centrally in the client infrastructure.

Important types and components:

```text
BaseApiClient<TClient>
ApiError
Result<T>
ErrorAlert.razor
```

The API returns errors as `ProblemDetails`. The client maps them to `ApiError` and displays them through `ErrorAlert`.

Network failures, invalid JSON responses and later authorization errors are also mapped to client-side errors.

## Auth status in Part 5

Part 5 is intentionally a no-auth client part.

Active in Part 5:

```text
anonymous API calls
simple navigation
list pages
error display
prepared configuration
```

Inactive in Part 5:

```text
login
logout
AuthorizeView
[Authorize]
access token forwarding
role-based UI decisions
policy-based authorization
protected API calls
```

Feature flags in the client keep this explicit:

```json
{
  "Features": {
    "AuthNEnabled": false,
    "ApiAccessTokenEnabled": false,
    "AuthZEnabled": false
  }
}
```

Prepared AuthN/AuthZ classes may remain in the project because they are useful for the next parts.

Planned continuation:

```text
Part 6: Client AuthN with login/logout
Part 7: AuthN/AuthZ in CampusLibraryApi
Part 8: protected API access from the client
```

## Modules consumed by the client

## Readers

The Readers page displays readers returned by the API.

Important client concepts:

```text
ReaderDto
IReaderClient
ReaderClient
ReadersList.razor
```

Typical client call:

```text
GET /camplib/v1/readers?includeInactive=false
```

## Catalog

The Catalog page displays books and supports search.

Important client concepts:

```text
BookListItemDto
BookSearchField
IBookClient
BookClient
BooksList.razor
```

Typical client calls:

```text
GET /camplib/v1/books?includeInactive=false
GET /camplib/v1/books/search?searchField=Title&searchText=Clean%20Code&includeInactive=false
```

## Loans

The Loans page displays borrowed loans and allows renew/return actions.

Important client concepts:

```text
LoanListItemDto
LoanDto
ILoanClient
LoanClient
LoansList.razor
```

Typical client calls:

```text
GET   /camplib/v1/loans
PATCH /camplib/v1/loans/{id}/renew
PATCH /camplib/v1/loans/{id}/return-at-desk
```

## Run locally

Start CampusLibraryApi first.

Then start the client:

```bash
dotnet run --project CampusLibraryClient
```

The client calls the API base URL configured in `CampusLibraryClient/appsettings.json`.

## Teaching goal

Part 5 shows that a modular API is not only tested through HTTP files or automated tests, but is also used by a real web client.

Students see:

```text
how a Blazor SSR client calls a backend API
how client-side API wrappers are structured per module
how DTOs define the transport boundary
how API errors are displayed in the UI
how navigation connects pages to API workflows
how AuthN/AuthZ can be prepared without activating it too early
```
