# Part 5 Notes

This ZIP is based on the uploaded CampusLibraryClient state and adjusts it for:

```text
Part 5 – CampusLibraryClient ohne aktive Auth
```

## Changed for Part 5

- Added `Core/FeatureFlags.cs`.
- Added feature flags to `appsettings.json`:
  - `Features:AuthNEnabled=false`
  - `Features:ApiAccessTokenEnabled=false`
  - `Features:AuthZEnabled=false`
- Updated `Program.cs` so AuthN/AuthZ/AccessTokenHandler are registered only when enabled.
- Updated `AddCampusLibraryClients(...)` to attach `AccessTokenHandler` only when requested.
- Replaced active `AuthorizeRouteView` with `RouteView` in `Routes.razor`.
- Replaced active `AuthorizeView` navigation with plain navigation in `NavMenu.razor`.
- Replaced active login/logout top menu with a Part 5 status text.
- Removed active `[Authorize]` attributes from Readers and Loans pages.
- Kept `IdentityController`, `EntryController`, Auth extensions, policies, roles and token handler as preparation for later parts.
- Removed generated `bin/`, `obj/` and macOS `.DS_Store` files from this ZIP.

## Intended follow-up

```text
Part 6 – activate AuthN in the client
Part 7 – add AuthN/AuthZ to CampusLibraryApi
Part 8 – activate protected API access from the client
```
