# CampusLibraryClient

Blazor SSR client for the CampusLibraryApi.

This version belongs to **Part 5 – CampusLibraryClient ohne aktive Auth**.

## Goal of Part 5

The client uses the modular CampusLibraryApi from a real web UI:

- Reader pages
- Catalog pages
- Loan pages
- API clients per module
- centralized error display
- simple navigation
- no active authentication
- no active authorization
- no Bearer token for API calls

AuthN/AuthZ is intentionally not active in Part 5. The related building blocks remain in the code so that Part 6 and Part 8 can be introduced with minimal restructuring.

## Structure

The structure is intentionally close to the earlier `BankingBlazorSsr` client:

```text
Api
├── Auth          # prepared for later parts, inactive in Part 5
├── Clients      # typed API clients for Readers, Catalog and Loans
├── Contracts    # client interfaces
├── Dtos         # transport models
└── Errors       # API error model

Core             # Result<T>, common constants, feature flags
Shared           # logging and diagnostics
Ui               # Razor components, pages, controllers and form models
```

## API route base

The typed clients use this base path:

```csharp
private const string Base = "camplib/v1";
```

Therefore `CampusLibraryApi:BaseUrl` contains only scheme, host and port:

```json
"CampusLibraryApi": {
  "BaseUrl": "https://localhost:8010/"
}
```

## Feature flags

Part 5 defaults:

```json
"Features": {
  "AuthNEnabled": false,
  "ApiAccessTokenEnabled": false,
  "AuthZEnabled": false
}
```

Meaning:

- `AuthNEnabled=false`: no login/logout flow is active.
- `ApiAccessTokenEnabled=false`: API calls are sent without Bearer token.
- `AuthZEnabled=false`: no policy/role based UI filtering is active.

## Prepared AuthN/AuthZ building blocks

These files remain on purpose:

- `AuthenticationExtensions`
- `AuthorizationExtensions`
- `AccessTokenHandler`
- `AuthTokenRefreshExtensions`
- `IdentityController`
- `EntryController`
- `CampusLibraryPolicies`
- `CampusLibraryRoles`
- `IdentityAccessServer` configuration

They are prepared for later parts, but Part 5 does not activate them.

## Planned follow-up parts

```text
Part 6 – CampusLibraryClient with AuthN
- login/logout in the Blazor SSR client
- SSR cookie
- show user and claims
- CampusLibraryApi remains anonymous
- no AccessTokenHandler yet

Part 7 – AuthN/AuthZ in CampusLibraryApi
- API accepts Bearer tokens
- API policies/scopes/roles
- 401/403 behavior

Part 8 – protected API access from CampusLibraryClient
- AccessTokenHandler active
- client sends Bearer token to CampusLibraryApi
- AuthorizeView / AuthorizeRouteView
- role/policy based navigation and actions
```

## Didaktik

Part 5 keeps the focus on the client/API interaction.

The students see how a modular API is consumed by a real Blazor SSR client without mixing in OIDC, claims, roles, access tokens and policies too early.

At the same time, the prepared auth structure shows where the next teaching steps will connect.
