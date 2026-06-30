# IdentityAccessServer – CampusLibrary client registration update

This project version keeps the existing generic demo clients and adds the
CampusLibrary clients explicitly.

## Client naming

Kept as generic clients:

- `BlazorWasm`
- `WebMvc`
- `ServiceClient`

Renamed / added clients:

- `WebBlazorSsr` was renamed to `BankingClientSsr`
- `CampusLibraryClientSsr` was added
- the old generic `Android` client was replaced by `CampusLibraryAndroidClient`

## CampusLibrary URLs

The CampusLibrary Blazor SSR client is registered with:

- Base URL: `https://localhost:6040`
- Sign-in callback: `https://localhost:6040/signin-oidc`
- Sign-out callback: `https://localhost:6040/signout-callback-oidc`

The CampusLibrary Android client is registered as a public PKCE client with:

- Client ID: `campus-library-android-client`
- Custom scheme redirect URI: `com.rogallab.campuslibrary:/callback`
- Loopback redirect URI: `http://127.0.0.1:8766/callback`
- Post-logout redirect URI: `com.rogallab.campuslibrary:/logout-callback`

## Scope / resource

The new API scope/resource mapping is:

- Scope: `campus_library_api`
- Resource: `campus-library-api`

The CampusLibrary SSR and Android clients may request this scope.

## Secrets

The confidential CampusLibrary SSR client expects its secret via configuration:

```bash
dotnet user-secrets set "IdentityAccessServer:CampusLibraryClientSsr:ClientSecret" "secret"
```

For the renamed Banking SSR client, use:

```bash
dotnet user-secrets set "IdentityAccessServer:BankingClientSsr:ClientSecret" "secret"
```

Existing secrets for `WebMvc` and `ServiceClient` remain unchanged.
