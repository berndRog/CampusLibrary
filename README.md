# CampusLibrary

Teaching project for a modular DDD-oriented ASP.NET Core Web API with a Blazor SSR client planned later.

## Current status

This initial version starts with the first API module:

- `Readers` domain and application module
- EF Core infrastructure for `Reader`
- ReadModel for DB-to-DTO projections
- Repository for the write side
- REST controller for `Reader`

## Architectural rules

- Write operations are implemented as **Use Cases**.
- Read operations are implemented as **ReadModels**.
- The domain model does not depend on EF Core.
- Infrastructure implements application ports.
- Controllers call either Use Cases or ReadModels.

## Initial API endpoints

```http
GET  /library/v1/readers
GET  /library/v1/readers/{id}
POST /library/v1/readers
```

This first module intentionally starts without AuthN/AuthZ. A later step will replace manual reader creation with token-based reader provisioning.
