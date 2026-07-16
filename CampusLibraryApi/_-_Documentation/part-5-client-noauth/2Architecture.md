# Architecture: CampusLibrary Part 5 — Client without real AuthN

This document describes the current architecture of branch `part-5/client-noauth`.

German version: [2Architecture-ger.md](2Architecture-ger.md)

## Architectural goal

Part 5 should already be as close as practical to the business structure of Part 6, without requiring real authentication through the IdentityAccessServer.

The central separation is:

```text
business use of identity stays the same
the technical identity source is different
```

```text
Part 5:
appsettings -> DevIdentityGateway -> IIdentityGateway

Part 6:
access token -> claims/HttpContext adapter -> IIdentityGateway
```

This allows subject-based use cases and `/me` endpoints to be developed and tested in Part 5.

## Solution view

```text
CampusLibraryApi
  Composition Root, configuration, hosting

CampusLibraryApi_1_Web
  controllers
  ProblemDetails mapping
  API-side DevIdentity options and adapter

CampusLibraryApi_2_BuildingBlocks
  Result / Result<T>
  DomainError and shared errors
  IIdentityGateway
  IClock and IUnitOfWork
  true BC-to-BC contracts and their small DTOs

CampusLibraryApi_3_Core_Readers
  Reader aggregate
  Reader use cases
  Reader read-model ports
  Reader HTTP DTOs

CampusLibraryApi_3_Core_Catalog
  Book and BookItem
  Catalog use cases
  Catalog read-model ports
  Catalog HTTP DTOs

CampusLibraryApi_3_Core_Loan
  Loan aggregate
  Loan use cases
  Loan read-model ports
  Loan HTTP DTOs

CampusLibraryApi_4_Infrastructure
  EF Core
  repositories
  read models
  cross-module contract adapters

CampusLibraryClient
  Blazor SSR
  UI perspective through DevCurrentUserProvider
  own HTTP clients and transport DTOs

IdentityAccessServer
  prepared, not actively involved in Part 5
```

## Dependency rule

Dependencies point inward:

```text
Composition Root
   ↓
Web / Infrastructure
   ↓
Core modules / BuildingBlocks
```

Core code does not know:

```text
HttpContext
ClaimsPrincipal
JWT libraries
IConfiguration
Blazor
EF Core implementations
```

The client does not know:

```text
API core projects
domain aggregates
EF Core entities
repository implementations
```

## Composition Root and Web module

`CampusLibraryApi` is the executable project and may reference `CampusLibraryApi_1_Web`.

The Web module must not reference the Composition Root. Therefore the technical options classes live next to the adapter in the Web project:

```text
CampusLibraryApi_1_Web/_1_Web/Security
├─ DevIdentityOptions.cs
├─ DevIdentityGateway.cs
└─ DevIdentityExtension.cs
```

Configuration values remain in the executable project:

```text
CampusLibraryApi/appsettings.json
```

Registration is initiated by the Composition Root through a Web extension:

```text
builder.Services.AddDevIdentityGateway(builder.Configuration)
```

This avoids a cyclic project dependency.

## API-side DevIdentity

The API simulates a technical identity from its own configuration.

```text
appsettings.json
   ↓ bind
DevIdentityOptions
   ↓ read
DevIdentityGateway
   ↓ implements
IIdentityGateway
```

The adapter exposes:

```text
Subject
Username
CreatedAt
AdminRights
IsAuthenticated
IsReader
IsEmployee
```

The API does not accept a `ReaderId` from the client for Reader self-service:

```text
Subject
   ↓
ReaderRepository.FindBySubjectAsync(...)
   ↓
business Reader
```

## IIdentityGateway as a stable port

`IIdentityGateway` lives in BuildingBlocks because use cases need technical identity information but must not know how it was obtained.

```csharp
public interface IIdentityGateway {
   string Subject { get; }
   string Username { get; }
   DateTime CreatedAt { get; }
   int AdminRights { get; }
   bool IsAuthenticated { get; }
   bool IsReader { get; }
   bool IsEmployee { get; }
}
```

`AdminRights` remains for compatibility with the later IA-server token. CampusLibrary does not evaluate this bitmask. Part 5 sets it to `0`.

## IdentitySubject

Shared application logic validates identity through:

```text
IdentitySubject.Check(IIdentityGateway)
```

Checks:

```text
1. IsAuthenticated must be true.
2. IsReader must be true.
3. Subject must be present.
4. Subject must be no longer than 200 characters.
5. Username must be present.
6. CreatedAt must not be default.
7. Subject is returned as an opaque value.
```

The class does not interpret the subject. A subject may be a GUID, another identifier, or for example `reader-099`.

## Why subject instead of email?

Technical association must not depend on a mutable email address.

```text
Subject:
- stable
- opaque
- identity anchor

Email:
- initially the username
- may be changed as business data
- unsuitable as permanent association
```

After a Reader update, different values may therefore exist:

```text
IIdentityGateway.Username = r.reader@library.local
Reader.Email              = e.meier@gmx.de
```

Association remains stable through `Subject`.

## API and client configuration

Client and API each have their own `DevIdentity` section.

```text
client appsettings -> DevCurrentUserProvider
API appsettings    -> DevIdentityGateway
```

There is no automatic synchronization and no HTTP transfer.

For combined scenarios, these values must align:

```text
ActiveProfile
API profile Subject and Reader.Subject in the database
```

Using the same profile shape in both configurations reduces errors. The adapters read different subsets:

```text
Client reads:
IsAuthenticated, AccountType, ReaderId, DisplayName, Email

API reads:
IsAuthenticated, Subject, AccountType, Email, CreatedAt, AdminRights
```

## Client architecture

```text
Razor page / component
        ↓
IReaderClient / IBookClient / ILoanClient
        ↓
ReaderClient / BookClient / LoanClient
        ↓
BaseApiClient
        ↓
HttpClient
        ↓
CampusLibraryApi
```

The client API layer converts HTTP responses to `Result<T>` and handles `ProblemDetails` centrally.

## CurrentUserProvider

The UI depends only on `ICurrentUserProvider`.

Implementations:

```text
DevCurrentUserProvider       Part 5
ClaimsCurrentUserProvider    prepared for Part 6
AnonymousCurrentUserProvider fallback
```

The client selects the implementation through feature flags:

```text
AuthNEnabled
DevIdentityEnabled
ApiAccessTokenEnabled
AuthZEnabled
```

Part 5 configuration:

```text
AuthNEnabled          = false
DevIdentityEnabled    = true
ApiAccessTokenEnabled = false
AuthZEnabled          = false
```

## No identity transfer from the client

Part 5 intentionally uses none of the following:

```text
Authorization: Bearer ...
X-Dev-Subject
X-Dev-Username
X-Dev-Account-Type
```

The client controls only its UI perspective. The API determines its technical identity independently from its own configuration.

This also allows direct `.http` tests without the client and without the IdentityAccessServer.

## DTOs as transport boundary

Public HTTP DTOs belong to their modules:

```text
Readers/_2_Application/Dtos/ReaderDtos.cs
Catalog/_2_Application/Dtos/CatalogDtos.cs
Loans/_2_Application/Dtos/LoanDtos.cs
```

The client owns structurally matching copies:

```text
CampusLibraryClient/Api/Dtos
```

There is no shared `CampusLibrary.Contracts` project. This avoids coupling every module and the client to one central DTO package.

## BC-to-BC contracts

Only business communication between modules lives in BuildingBlocks.

```text
Catalog -> Loans:
IBookItemLoanContract
BookItemLoanInfoDto

Loans -> Catalog:
ILoanCatalogContract
CurrentBookItemLoanInfoDto

Readers -> Loans:
IReaderLoanContract
ReaderLoanInfoDto

Loans -> Readers:
ILoanReaderContract
```

A module receives only the information it actually needs. It never accesses another module's tables or aggregates directly.

## Reader architecture

Query side:

```text
IReaderReadModel
- SelectAllAsync
- FindByIdAsync
- FindByEmailAsync
- FindMeAsync for internal self-service resolution
```

Command side:

```text
IReaderUseCases
- CreateAsync
- UpdateMeAsync
- DeactivateAsync
```

The self-service update flow is:

```text
PUT /readers/me/update
        ↓
ReaderController
        ↓
IReaderUseCases.UpdateMeAsync
        ↓
ReaderUcUpdateMe
        ↓
IdentitySubject.Check
        ↓
load Reader by Subject
        ↓
validate optional values
        ↓
Reader.UpdateProfile
        ↓
IUnitOfWork.SaveAllChangesAsync
```

The client sends no ReaderId. Therefore it cannot choose which Reader is updated.

## Catalog architecture

`Book` is the aggregate for bibliographic data and its items.

```text
Book
├─ bibliographic data
├─ IsActive
└─ BookItems
```

`BookItem` has a Guid identity and a status:

```text
Available
Unavailable
Lost
Damaged
```

`InventoryNumber` is no longer part of the current DTO or UI contract.

List and detail projections were unified in `BookDto`. Old types such as these are gone:

```text
BookListItemDto
BookDetailDto
BookSearchDto
```

## Deactivating a book

Before deactivation, Catalog asks Loans through `ILoanCatalogContract` whether current Loans exist for BookItems.

```text
Catalog
   ↓ ILoanCatalogContract
Loans
```

The deactivation view receives a small projection containing:

```text
BookItemId
ReaderEmail
DueDate
```

Catalog receives no Loan entities and no direct access to the Loans table.

## Loan architecture

In the current model a Loan represents an active borrowing process.

```text
Loan exists   = currently borrowed
Loan deleted  = returned
```

Consequences:

```text
no Loan.Status
no Loan.ReturnedAt
no historical returned Loan in the current aggregate
```

`LoanDto` is unified for list and detail views. Old types such as `LoanListItemDto` and `LoanDetailDto` are gone.

## Administrative and self-service Loans

Administrative flows use explicit Reader or Loan IDs:

```text
GET   /loans
GET   /loans/{id}
POST  /loans
PATCH /loans/{id}/renew
PATCH /loans/{id}/return-at-desk
```

Reader self-service uses the technical identity:

```text
GET   /loans/me
GET   /loans/me/{id}
POST  /loans/me
PATCH /loans/me/{id}/renew
```

For `POST /loans/me`, `LoanBorrowMeDto` contains only:

```text
BookItemId
optional Id for deterministic tests
```

ReaderId is resolved server-side from the subject.

## Error architecture

Domain and application code return `Result` or `Result<T>` with a `DomainError`.

The Web layer creates `ProblemDetails` and explicitly chooses the status code:

```text
WebErrorStatus.BadRequest   -> 400
WebErrorStatus.Unauthorized -> 401
WebErrorStatus.Forbidden    -> 403
WebErrorStatus.NotFound     -> 404
WebErrorStatus.Conflict     -> 409
```

`DomainProblemDetailsFactory` creates only the ProblemDetails data. The controller remains responsible for the concrete HTTP response.

## Auth preparation without activation

The client already contains prepared components:

```text
ClaimsCurrentUserProvider
AccessTokenHandler
IdentityController
EntryController
ConfigureAuthN
ConfigureAuthZ
```

They remain inactive in Part 5 through feature flags.

The IdentityAccessServer may remain in the solution, but it is not required for a Part 5 run.

## Transition to Part 6

Part 6 primarily replaces the adapter:

```text
Part 5: DevIdentityGateway
Part 6: IdentityGateway from Claims/HttpContext
```

The following remain unchanged:

```text
IIdentityGateway
IdentitySubject
ReaderUcUpdateMe
Loan /me use cases
subject-based Reader resolution
module boundaries
DTO ownership
```

Part 6 activates:

```text
OIDC
cookie authentication in the SSR client
access token
Bearer-token handler
JWT validation
claim-based role and subject evaluation
```

## Didactic core

Part 5 demonstrates all of the following:

```text
- Client and API remain separate applications.
- UI perspective is not the same as API security.
- A technical identity can be simulated behind a port.
- Subject is more stable than a mutable email address.
- /me endpoints avoid business IDs chosen by the client.
- Cross-module communication uses small contracts.
- HTTP DTOs remain owned by their modules.
- Part 6 can replace the identity source without rewriting business logic.
```
