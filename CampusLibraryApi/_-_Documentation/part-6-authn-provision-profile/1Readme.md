# CampusLibrary – Part 6

Teaching project for a modular, DDD-oriented ASP.NET Core Web API with a Blazor SSR client and a dedicated IdentityAccessServer.

German version: [1Readme-ger.md](1Readme-ger.md)

## Current status

Part 6 builds on Part 5. Catalog, Readers, Loans, the Blazor client, EF Core persistence, read models, use cases and the project-based modular-monolith structure remain in place. The new focus is real technical identities and a reader self-service workflow.

The current state includes:

- ASP.NET Core Web API on .NET 10
- Blazor SSR client
- IdentityAccessServer for OIDC/OAuth 2.0
- JWT Bearer authentication for the API
- access-token forwarding by the client
- `IIdentityGateway` as the port to the technical identity
- reader provisioning from the token subject
- initial reader-profile completion
- later self-service profile updates
- reader and loan endpoints under `/me`
- unified HTTP DTOs owned by each business module
- BC-to-BC contracts in BuildingBlocks
- explicit mapping from domain errors to HTTP status codes
- SQLite and EF Core
- automated domain, application, infrastructure and API tests
- manual `.http` scripts for identity, reader, catalog and loan workflows

The last fully verified Part 6 state was:

```text
238 tests
0 failed
0 skipped
```

After the final DTO-refactoring merge, this result should be confirmed once more with `dotnet test`.

## Version

Official branch:

```text
part-6/authn-provision-profile
```

Planned final tag:

```text
v6-authn-provision-profile
```

The version sequence is therefore:

```text
v1-readers-monolith
v2-readers-modular-monolith
v3-readers-catalog
v4-readers-catalog-loans
v5-client-noauth
v6-authn-provision-profile
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
IdentityAccessServer
```

## Goal of Part 6

Part 5 simulates a user perspective without a real login. Part 6 replaces that simulation with a technical identity issued by the IdentityAccessServer.

The central distinction is:

```text
Authentication:
Who is the technical user?

Provisioning:
Which domain Reader belongs to that technical user?
```

The technical user is identified by a stable subject. The domain Reader ID remains independent from it.

```text
IdentityAccessServer subject
        ↓
Reader.Subject
        ↓
Reader.Id
```

The email address is not a stable key because it may be changed later. The relationship between the technical identity and the Reader therefore remains based on `Subject`.

## Authentication flow

```text
Browser
  → Blazor SSR Client
  → OIDC login at IdentityAccessServer
  → authentication cookie in the client
  → access token
  → AccessTokenHandler
  → Authorization: Bearer <token>
  → CampusLibrary API
  → JWT Bearer validation
  → ClaimsPrincipal
  → IIdentityGateway
  → use case
```

The client does not send a `ReaderId` to select the current Reader. `/me` use cases use the token subject.

## Reader provisioning and profile

An identity user is not automatically a domain Reader. Provisioning creates a Reader and permanently links it to the token subject.

```text
POST /camplib/v1/readers/me/provision
```

Provisioning consumes trusted technical values from the token:

- `sub` as the stable subject
- `preferred_username` or username as the initial email
- technical identity creation time
- account type or role

`AdminRights` remains in the identity port for compatibility, but CampusLibrary does not use it as a business authorization value.

After provisioning, the domain profile is completed through:

```text
PUT /camplib/v1/readers/me/profile
```

Initial completion uses `ReaderProfileDto` with first name, last name and address. The initial email is not taken from a freely editable profile form; it originates from the technical identity.

Later selective changes use:

```text
PUT /camplib/v1/readers/me/update
```

`ReaderUpdateDto` contains optional values. `null` means that the current value remains unchanged.

## Catalog and Loans

In Part 6, the Catalog intentionally remains largely visible without login. Systematic protection of all API operations and deeper use-case authorization are deferred to a later part.

Loans use self-service endpoints:

```text
GET   /camplib/v1/loans/me
POST  /camplib/v1/loans/me
GET   /camplib/v1/loans/me/{loanId}
PATCH /camplib/v1/loans/me/{loanId}/renew
```

The loan is associated with the current Reader through the subject. The client does not send a Reader ID.

Desk return is an employee operation:

```text
PATCH /camplib/v1/loans/{loanId}/return-at-desk
```

In the current model, a successful return deletes the Loan. A later `GET` for the same Loan therefore returns `404 Not Found`.

## DTO rules

Public HTTP DTOs are owned by the corresponding business module:

```text
Readers/_2_Application/Dtos
Catalog/_2_Application/Dtos
Loans/_2_Application/Dtos
```

The client owns separate transport types under:

```text
CampusLibraryClient/Api/Dtos
```

The client does not reference Core projects.

Only real BC-to-BC interfaces and data contracts live in BuildingBlocks:

```text
_1_Ports/Contracts
_2_Application/Dtos
```

Examples are Reader and Catalog information required by the Loan module through explicit module boundaries.

## Documentation

- [Architecture](2Architecture.md)
- [API](3Api.md)
- [Testing](4Testing.md)
