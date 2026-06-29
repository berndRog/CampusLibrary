# Architecture: CampusLibrary Part 5 — Client without active Auth

This document describes the architecture of Part 5 of the `CampusLibrary` project.

Part 5 adds a Blazor Server-Side Rendering client to the modular CampusLibrary API. The API still consists of the Readers, Catalog and Loans modules from Part 4. The client consumes the API through HTTP and does not reference the API core projects.

Known build result:

```text
dotnet build
Build succeeded
```

## Architecture goal

Part 5 makes the following concepts visible for teaching:

* a modular backend API consumed by a real web client
* separation between backend modules and frontend client
* API clients as typed client-side adapters
* DTOs as transport models at the HTTP boundary
* client-side result and error handling
* Blazor SSR pages and components
* simple navigation across backend modules
* prepared but inactive AuthN/AuthZ infrastructure

## Solution structure

```text
CampusLibrary
├─ CampusLibraryApi
├─ CampusLibraryApi_1_Web
├─ CampusLibraryApi_2_BuildingBlocks
├─ CampusLibraryApi_3_Core_Readers
├─ CampusLibraryApi_3_Core_Catalog
├─ CampusLibraryApi_3_Core_Loan
├─ CampusLibraryApi_4_Infrastructure
├─ CampusLibraryApiTest
└─ CampusLibraryClient
```

## Backend architecture

The backend remains a project-based modular monolith.

The API is deployed as one ASP.NET Core application. Internally, the code is split by responsibility:

```text
Web/API project       -> HTTP controllers
BuildingBlocks        -> shared abstractions and cross-module contracts
Core_Readers          -> Readers module
Core_Catalog          -> Catalog module
Core_Loan             -> Loans module
Infrastructure        -> EF Core, repositories, read models, contract implementations
CampusLibraryApi      -> executable application and composition root
```

Core modules do not depend on the client. The client is a separate application that communicates through HTTP.

## Client architecture

The `CampusLibraryClient` project is a Blazor SSR application.

Main structure:

```text
CampusLibraryClient
├─ Api
│  ├─ Auth
│  ├─ Clients
│  ├─ Contracts
│  ├─ Dtos
│  └─ Errors
├─ Core
│  ├─ FeatureFlags.cs
│  ├─ Result.cs
│  └─ Utils
├─ Extensions
├─ Security
├─ Shared
│  └─ Logging
└─ Ui
   ├─ Components
   ├─ Controllers
   ├─ Models
   └─ Pages
```

The client architecture follows the same idea as the backend modules: separate responsibilities and keep boundaries visible.

## Client-side API adapter layer

The client has a typed API adapter per backend area:

```text
IReaderClient -> ReaderClient
IBookClient   -> BookClient
ILoanClient   -> LoanClient
```

These clients are not domain services. They are HTTP adapters on the client side.

They are responsible for:

```text
constructing URLs
serializing request DTOs
deserializing response DTOs
mapping ProblemDetails to ApiError
returning Result<T>
logging outgoing calls and failures
```

The base behavior is implemented in:

```text
BaseApiClient<TClient>
```

This avoids duplicating HTTP success/error handling in every concrete client.

## DTO boundary

The client uses its own DTOs under:

```text
CampusLibraryClient/Api/Dtos
```

The DTOs mirror the HTTP contract of CampusLibraryApi. They are transport models, not domain entities.

Examples:

```text
ReaderDto
BookListItemDto
BookDetailDto
BookCreateDto
BookItemDto
LoanListItemDto
LoanDetailDto
LoanCreateDto
```

This makes the HTTP boundary explicit:

```text
Domain objects live in the backend core modules.
DTOs are exchanged over HTTP.
The client never directly manipulates backend aggregates.
```

## Pages and components

The visible Part 5 UI is intentionally simple.

Pages:

```text
Home.razor
ReadersList.razor
BooksList.razor
LoansList.razor
Error.razor
AccessDenied.razor
```

Shared UI components:

```text
MainLayout.razor
NavMenu.razor
TopMenu.razor
ErrorAlert.razor
```

Common page behavior is placed in:

```text
BasePage.cs
```

The first vertical slices are:

```text
Navigation -> ReadersList -> IReaderClient -> CampusLibraryApi
Navigation -> BooksList   -> IBookClient   -> CampusLibraryApi
Navigation -> LoansList   -> ILoanClient   -> CampusLibraryApi
```

## Error handling architecture

The client uses a consistent result model:

```text
Result<T>
```

Successful API calls return a value. Failed API calls return an `ApiError`.

The error pipeline is:

```text
CampusLibraryApi returns ProblemDetails
BaseApiClient reads ProblemDetails
BaseApiClient maps it to ApiError
Page stores Error
ErrorAlert displays the error
```

This makes API failures visible in the UI without throwing exceptions directly into Razor components.

## Configuration

The API base URL is configured through:

```json
{
  "CampusLibraryApi": {
    "BaseUrl": "https://localhost:8010/"
  }
}
```

The API clients are registered in:

```text
CampusLibraryClientExtensions.AddCampusLibraryClients(...)
```

## Auth preparation without activation

Part 5 intentionally does not activate authentication or authorization.

However, the client already contains preparation for later parts:

```text
Api/Auth/AccessTokenHandler.cs
Extensions/AuthenticationExtensions.cs
Extensions/AuthorizationExtensions.cs
Security/CampusLibraryRoles.cs
Security/CampusLibraryPolicies.cs
Ui/Controllers/IdentityController.cs
Ui/Controllers/EntryController.cs
```

Feature flags control the activation:

```json
{
  "Features": {
    "AuthNEnabled": false,
    "ApiAccessTokenEnabled": false,
    "AuthZEnabled": false
  }
}
```

In Part 5:

```text
AuthNEnabled=false        -> no login/logout flow
ApiAccessTokenEnabled=false -> API calls are anonymous
AuthZEnabled=false        -> no role/policy based UI restrictions
```

This allows a smooth transition to the following parts:

```text
Part 6: activate client AuthN
Part 7: add AuthN/AuthZ to CampusLibraryApi
Part 8: activate token forwarding and protected API access
```

## Dependency rules

Part 5 adds a new dependency direction:

```text
Browser/User -> CampusLibraryClient -> HTTP -> CampusLibraryApi
```

But it does not create project references from the client to backend core modules.

Rules:

```text
CampusLibraryClient may depend on ASP.NET Core Blazor packages.
CampusLibraryClient may use DTOs that match the API contract.
CampusLibraryClient must not reference Core_Readers, Core_Catalog or Core_Loan.
CampusLibraryApi must not depend on CampusLibraryClient.
Backend domain rules stay in the API modules.
```

## Teaching summary

Part 5 shifts the perspective from backend implementation to backend consumption.

The students can now see:

```text
The API is a reusable boundary.
The client is another adapter.
HTTP contracts matter.
Error behavior is part of the user experience.
Auth can be prepared without dominating the first client lesson.
```
