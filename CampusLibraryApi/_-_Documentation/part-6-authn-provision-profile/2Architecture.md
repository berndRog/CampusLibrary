# Architecture – CampusLibrary Part 6

German version: [2Architecture-ger.md](2Architecture-ger.md)

This document describes the architecture of the official branch:

```text
part-6/authn-provision-profile
```

## Architectural goal

Part 6 adds real authentication to the modular-monolith architecture without coupling Core modules to ASP.NET Core, `HttpContext`, claims or JWT libraries.

The central dependency rule remains:

```text
Core knows neither Web, Infrastructure nor IdentityAccessServer.
Web adapts HTTP and claims.
Infrastructure implements persistence ports.
The composition root wires all projects together.
```

## Projects and responsibilities

### `CampusLibraryApi`

Executable API project and composition root.

It configures:

- controllers
- API versioning
- Swagger/OpenAPI
- JWT Bearer authentication
- policies and technical authentication options
- business modules
- infrastructure
- database

It contains no business-domain logic.

### `CampusLibraryApi_1_Web`

HTTP adapter layer of the API.

It contains, among other things:

- `ReadersController`
- `BooksController`
- `LoansController`
- the claims/HttpContext-based `IIdentityGateway` adapter
- explicit translation of `Result` and `DomainError` into HTTP responses
- `ProblemDetails` creation

Web knows HTTP, routing, status codes, claims and Swagger. Use cases do not.

### `CampusLibraryApi_2_BuildingBlocks`

Shared abstractions independent from a specific business module:

- `Result` and `Result<T>`
- `DomainError`
- `IClock`
- `IUnitOfWork`
- `IIdentityGateway`
- `IdentitySubject.Check(...)`
- real BC-to-BC ports under `_1_Ports/Contracts`
- BC-to-BC data under `_2_Application/Dtos`

`IdentitySubject.Check(...)` validates an authenticated Reader identity:

```text
IsAuthenticated
IsReader
Subject present and no longer than 200 characters
Username present
CreatedAt valid
```

The subject is opaque and is not interpreted as a GUID.

### Core modules

```text
CampusLibraryApi_3_Core_Readers
CampusLibraryApi_3_Core_Catalog
CampusLibraryApi_3_Core_Loan
```

Each module owns:

```text
_1_Ports
_2_Application
_3_Domain
```

Public HTTP DTOs remain in the owning module. Use-case facades contain commands; read-model ports contain queries.

### `CampusLibraryApi_4_Infrastructure`

Implements technical details:

- EF Core DbContext
- SQLite
- entity configurations
- repositories
- read models
- unit of work
- BC-to-BC adapters

Infrastructure may reference Core modules because it implements their outbound ports. Core modules do not reference Infrastructure.

### `CampusLibraryClient`

Blazor SSR application.

The client contains:

- OIDC login and logout
- cookie-based client session
- token access
- `AccessTokenHandler`
- API clients
- its own transport DTOs
- Reader profile pages
- Catalog and Loan pages

The client does not reference API Core projects. Its DTOs mirror the public HTTP contract only.

### `IdentityAccessServer`

Technical identity provider for Part 6.

It is responsible for:

- user registration or development users
- login
- OIDC/OAuth flow
- identity- and access-token issuance
- technical claims

CampusLibrary does not manage passwords.

## Dependency direction

```text
IdentityAccessServer
        │  Tokens
        v
CampusLibraryClient
        │  Bearer Token
        v
CampusLibraryApi / Web
        │
        v
IIdentityGateway  ← BuildingBlocks
        │
        v
Readers / Loans use cases
        │
        v
Repositories and read models
        │
        v
Infrastructure / EF Core / SQLite
```

## Technical identity as a port

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

The port keeps application code independent from:

```text
HttpContext
ClaimsPrincipal
ClaimsIdentity
JwtSecurityToken
ASP.NET Core authentication libraries
```

`AdminRights` is part of the technical IdentityAccessServer contract, but CampusLibrary does not use it as a domain authorization value.

## Authentication and provisioning

Authentication and provisioning are separate workflows.

### Authentication

```text
IdentityAccessServer
→ access token
→ JWT Bearer validation
→ ClaimsPrincipal
→ IIdentityGateway
```

### Provisioning

```text
POST /readers/me/provision
→ IdentitySubject.Check(...)
→ find Reader by subject
→ create domain Reader
→ store subject permanently
```

Provisioning trusts the token, not a client form.

## Profile states

A provisioned Reader may still have an incomplete profile.

```text
Identity exists
→ Reader provisioned
→ IsProfileCompleted = false
→ profile completed
→ IsProfileCompleted = true
```

Initial profile completion and later selective updates are intentionally separated:

```text
PUT /readers/me/profile
PUT /readers/me/update
```

`ReaderProfileDto` contains the initially required domain values. `ReaderUpdateDto` contains optional values for later changes.

## Self-service through `/me`

Self-service endpoints do not accept a Reader ID to select the current user.

```text
HTTP request
→ IIdentityGateway.Subject
→ Reader read model/repository by subject
→ domain Reader.Id
→ operation
```

This prevents a client from switching to another Reader by manipulating a Reader ID.

## Module communication

Loan needs information from Readers and Catalog, but may not directly access their tables or EF entities.

It therefore uses BC-to-BC ports:

```text
Loan use case
→ IReaderLoanContract
→ ReaderLoanInfoDto

Loan use case
→ ILoanCatalogContract
→ BookItemLoanInfoDto
```

Interfaces live in BuildingBlocks under `_1_Ports/Contracts`; the related data-transfer objects live under `_2_Application/Dtos`.

Each module owns its data. Only the module-specific adapter accesses its table.

## DTO ownership

```text
Module HTTP DTO
→ owning module

Client transport DTO
→ client

BC-to-BC DTO
→ BuildingBlocks/_2_Application/Dtos
```

A shared `CampusLibrary.Contracts` project is intentionally avoided because it would blur ownership and couple the client and modules unnecessarily.

## Catalog model

Catalog uses unified DTOs:

```text
BookDto
BookCreateDto
BookItemDto
BookItemAddDto
BookDeactivationInfoDto
BookLoanInfoDto
```

Authors are transported as `AuthorsText` on the Book. `AuthorLastName` search splits the comma-separated text and compares last names explicitly.

`BookItem` represents a physical copy. Its identity is its `Id`; the current model does not require a separate `InventoryNumber`.

## Loan model

A Loan represents a currently active checkout.

- Borrow creates a Loan.
- Renew changes due date and renewal count.
- Return at desk removes the Loan.

The model therefore has no permanent returned status as history. Historical loan tracking would be a separate later concept.

## Authorization scope in Part 6

Part 6 focuses on AuthN, Reader provisioning, profile management and basic role checks around identity-related workflows.

Not every Catalog and administration endpoint is systematically protected by policies yet. Comprehensive AuthZ with scopes, policies and use-case guards is deferred to a later part.

## Error mapping

Use cases return `Result` or `Result<T>`. Controllers map errors explicitly:

```text
Validation / BadRequest  → 400
Unauthenticated          → 401
AccessNotAllowed         → 403
NotFound                 → 404
Conflict                 → 409
```

`DomainProblemDetailsFactory` only creates `ProblemDetails`. The actual HTTP status remains visible in each controller action.
