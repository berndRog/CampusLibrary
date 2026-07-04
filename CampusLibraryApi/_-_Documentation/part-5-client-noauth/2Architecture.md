# Architecture: CampusLibrary Part 5 — Client without active Auth

This document describes the architecture of Part 5 of the `CampusLibrary` project.

Part 5 adds a Blazor SSR client to the modular CampusLibrary API. The API still consists of the Readers, Catalog and Loans modules from Part 4. The client consumes the API through HTTP and does not reference the API core projects.

German version: [2Architecture-ger.md](2Architecture-ger.md)

## Architectural goal

Part 5 makes the following concepts visible:

```text
backend API is consumed by a real web client
frontend and backend remain separated
API clients encapsulate HTTP access
DTOs model the transport boundary
Result<T> and ErrorAlert encapsulate error handling
Bootstrap provides the UI layout
DevIdentity simulates UI perspectives without real AuthN/AuthZ
```

## Solution view

```text
CampusLibraryApi
├─ CampusLibraryApi_1_Web
├─ CampusLibraryApi_2_BuildingBlocks
├─ CampusLibraryApi_3_Core_Readers
├─ CampusLibraryApi_3_Core_Catalog
├─ CampusLibraryApi_3_Core_Loan
├─ CampusLibraryApi_4_Infrastructure
└─ CampusLibraryApiTest

CampusLibraryClient
```

The client is intentionally a separate project.

```text
CampusLibraryClient -> HTTP -> CampusLibraryApi
CampusLibraryClient -/-> Core_Readers
CampusLibraryClient -/-> Core_Catalog
CampusLibraryClient -/-> Core_Loan
```

## Client architecture

```text
CampusLibraryClient
├─ Api
│  ├─ Clients        concrete HTTP clients
│  ├─ Contracts      client interfaces
│  ├─ Dtos           transport models
│  ├─ Errors         ApiError
│  └─ Auth           prepared token infrastructure
├─ Core              Result<T>, FeatureFlags, Common
├─ Extensions        DI registration
├─ Security          CurrentUserProvider, roles, policies
├─ Shared            shared helper types
└─ Ui
   ├─ Components     layout, navigation, ErrorAlert
   ├─ Controllers    prepared Auth controllers
   ├─ Models         UI form models
   └─ Pages          Razor pages/components
```

## Dependency rule

The client knows the API only through HTTP.

```text
UI Page
  -> IBookClient / IReaderClient / ILoanClient
    -> BookClient / ReaderClient / LoanClient
      -> HttpClient
        -> CampusLibraryApi
```

Business rules remain in the API. The client only checks UI-near concerns, such as whether a button should be shown or whether local input is complete.

## Render model

Interactive pages use:

```razor
@rendermode InteractiveServer
```

This allows buttons, forms and loading states in Razor components without introducing a separate JavaScript frontend for Part 5.

## API client layer

The three functional client adapters are:

```text
ReaderClient
BookClient
LoanClient
```

All use the named HttpClient:

```text
Common.CampusLibraryApiClientName
```

The base URL comes from:

```json
{
  "CampusLibraryApi": {
    "BaseUrl": "https://localhost:8010/"
  }
}
```

## DTOs as transport boundary

The client defines its own DTOs matching the HTTP API. These DTOs are not domain objects.

Important current DTO decisions:

```text
BookItemDto contains Id, BookId and Status.
BookItemDto no longer contains InventoryNumber.
BookItemAddDto only contains an optional Id.
LoanListItemDto contains BookItemId but no InventoryNumber.
LoanDetailDto contains reader email and BookItemId but no InventoryNumber.
```

The UI may label `BookItemId` as `Inventory number`. The code remains based on `BookItemId`.

## CurrentUserProvider

Part 5 separates the current user perspective through an interface:

```text
ICurrentUserProvider
```

Implementations:

```text
DevCurrentUserProvider       active Part 5 simulation
ClaimsCurrentUserProvider    prepared for real AuthN
AnonymousCurrentUserProvider fallback/no-user case
```

`DevCurrentUserProvider` reads the active profile from `appsettings.json`.

Examples:

```text
ReaderRita      AccountType=reader, ReaderId set
EmployeeAdmin   AccountType=employee, ReaderId=null
```

This information only controls the UI perspective. It does not replace real authorization.

## UI perspectives

### Reader

Readers can:

```text
view catalog
search books
borrow a book if an item is actually available
view own loans
open loan details
```

### Employee

Employees can:

```text
view readers list
view catalog including inactive books
create book
add item to active book
deactivate active book
view loans
open loan details
renew loan
return loan
```

## Why reader creation is not in the client

Creating a reader is intentionally not implemented as an employee function in Part 5.

The target business architecture for later parts is:

```text
technical user in IdentityAccessServer
  -> subject and email
  -> reader provisioning in CampusLibraryApi
  -> reader completes first name and last name
```

A Part 5 form `Create reader` would teach the wrong flow. Therefore readers remain seed/test data in Part 5.

## Catalog architecture

The catalog uses:

```text
BooksList.razor        list, search, perspective-dependent actions
BookCreate.razor       create book
BookItemAdd.razor      add book item
BookDeactivate.razor   deactivate book
BorrowBook.razor       borrow book from reader perspective
```

The catalog table is structured as:

```text
Action | Title | Authors | ISBN | Items | Status
```

The action comes first so it is not cut off on narrow windows. Title and subtitle are shown together in one column because the subtitle qualifies the title.

## Item identity

The separate inventory number has been removed.

```text
BookItem.Id is unique.
```

The UI still labels this id as inventory number:

```text
BookItemId -> Inventory number in the UI
```

This avoids a duplicate identity and keeps the model simpler.

## Borrow architecture

`BorrowBook.razor` loads:

```text
BookDetailDto with BookItems
currently borrowed loans
```

From this, the UI calculates which items are actually available:

```text
BookItem.Status == Available
and
BookItem.Id is not part of the currently borrowed BookItemIds
```

The borrow request sends:

```text
ReaderId from CurrentUserProvider
BookItemId of the selected inventory number
```

## Loan details

Overview pages lead to the detail page:

```text
/loans/{loanId}
```

Renew and return belong in the detail view. This keeps the overview simpler and makes the business decision visible before the action.

## Auth preparation without activation

Part 5 contains prepared classes, but does not activate them:

```text
AccessTokenHandler
AuthenticationExtensions
AuthorizationExtensions
IdentityController
EntryController
ClaimsCurrentUserProvider
```

Feature flags:

```json
{
  "Features": {
    "AuthNEnabled": false,
    "DevIdentityEnabled": true,
    "ApiAccessTokenEnabled": false,
    "AuthZEnabled": false
  }
}
```

Meaning:

```text
AuthNEnabled=false          -> no real login/logout flow
DevIdentityEnabled=true     -> simulated UI perspective
ApiAccessTokenEnabled=false -> no access token forwarding
AuthZEnabled=false          -> no real policy authorization
```

## Planned Auth architecture

Later target architecture:

```text
Part 6: client signs users in at IdentityAccessServer.
Part 7: CampusLibraryApi validates bearer tokens.
Part 8: client sends access token to protected API endpoints.
```

Reader provisioning later:

```text
POST /camplib/v1/readers/me/provision
Authorization: Bearer <access_token>

API reads subject and email from the token.
```

Profile update later:

```text
POST /camplib/v1/readers/me/profile
Authorization: Bearer <access_token>

Body contains only first name and last name.
```

## Didactic core

Part 5 should show:

```text
A modular API backend can be consumed by a real web client.
The client remains technically separated from the domain core.
UI perspectives can be simulated for teaching without introducing real security too early.
Reader provisioning belongs to the later AuthN/AuthZ part.
```
