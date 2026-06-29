# Shared Logging – CampusLibrary

## Purpose

The `Shared/Logging` folder contains a small shared logging library for the CampusLibrary learning solution.

It is used by three web hosts:

- `CampusLibraryClient` – Blazor SSR client
- `CampusLibraryApi` – modular Web API
- `IdentityAccessServer` – OpenID Connect identity server

The files are stored once at solution level and linked into each host project with `Compile Include` / `Link` entries in the `.csproj` files.
This avoids duplicated logging code without introducing an additional shared project.

## Files

```text
Shared/Logging/
├─ AppDiagnosticsLogger.cs
├─ OutgoingHttpLoggingHandler.cs
├─ SharedWebAppLogging.cs
├─ TokenRefreshEvent.cs
└─ STUDENT_LOGGING_GUIDE.md
```

## Namespace

All shared logging classes use:

```csharp
using CampusLibrary.Shared.Logging;
```

## What is logged?

### Outgoing API calls

```text
➜ API Call: GET /camplib/v1/readers
✓ API Response: 200 after 42ms
```

### Authentication and token flow

```text
🔐 Authentication: Login started
🔄 OIDC Flow: Authorization code received
✓ Token Refresh: Token refreshed successfully
```

### Authorization

```text
🔑 Authorization: User 'alice@example.org' with roles [reader]
🚫 Authorization failed: Missing required scope campuslibrary_api
```

### Business operations

```text
▶ Operation: Borrow book item [readerId=...]
✓ Operation: Borrow book item succeeded
```

### Student-friendly errors

```text
ERROR: CampusLibraryApi error 404
  👤 For Student: The endpoint was not found
  🔧 Technical: ErrorCode=n/a
```

## Part 5: client-noauth

In Part 5 the client calls `CampusLibraryApi` anonymously. Missing Bearer tokens are expected and therefore logged only at Debug level.

The important Part 5 learning goal is:

- Blazor SSR client calls a modular API
- API calls are centralized in module-specific API clients
- Razor components do not contain scattered `HttpClient` calls
- errors are translated centrally and shown in the UI
- AuthN/AuthZ is prepared but not active

## Later parts

- Part 6: client AuthN, Login/Logout, OIDC cookie
- Part 7: AuthN/AuthZ in CampusLibraryApi
- Part 8: protected API access from the client with Bearer access tokens
