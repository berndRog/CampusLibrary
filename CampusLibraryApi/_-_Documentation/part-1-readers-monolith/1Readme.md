# CampusLibrary

Teaching project for a modular DDD-oriented ASP.NET Core Web API.

The project demonstrates how a small modular monolith can be structured into Web, Core, Infrastructure and Test layers while keeping the domain model independent from technical persistence details.

## Current status

The current version contains the first functional module:

- `Readers` module
- ASP.NET Core Web API
- API versioning
- Swagger/OpenAPI documentation
- SQLite persistence with EF Core
- Repository and ReadModel infrastructure
- Use cases for create, partial update and deactivate
- Soft delete behavior for readers using `IsActive`
- Read model queries for active readers
- Administrative/internal read model queries including inactive readers
- Controller/end-to-end tests with a real SQLite test database

The original physical delete operation for readers has been replaced by a deactivate operation.

A reader is no longer removed from the database. Instead, the `Reader` aggregate is marked as inactive by setting `IsActive` to `false`.

Normal read model queries return only active readers. Special read model methods such as `FindByIdWithInactiveAsync` and `SelectAllWithInactiveAsync` can still return inactive readers for administrative or internal use cases.

This prepares the project for later modules such as `Loans`, where historical relationships must remain traceable even if a reader is no longer part of the active reader list.

The test suite currently contains 72 tests covering domain entities, value objects, use cases, repositories, read models, mock-based application tests and controller/end-to-end scenarios.

## Reader module

The `Readers` module currently supports the following operations:

- Create a reader
- Update mutable reader profile data
- Deactivate a reader
- Query all active readers
- Query one active reader by id
- Query one active reader by email
- Query all readers including inactive readers
- Query one reader by id including inactive readers

## Soft delete / deactivation rule

Deleting a reader is modeled as a domain operation named `Deactivate`.

The public HTTP API still uses the HTTP verb `DELETE`, because from a normal client perspective the reader disappears from the active reader resource collection. Internally, however, this is not a physical database delete.

The distinction is important:

| Term | Meaning |
|---|---|
| `Deactivate` | Domain operation that changes the reader state |
| `IsActive == false` | Technical state after deactivation |
| `DELETE /readers/{id}` | HTTP endpoint used to trigger the deactivation |
| Normal read model queries | Return only active readers |
| `WithInactive` queries | Return active and inactive readers |

This design keeps historical data available while still allowing normal clients to work with a clean active-reader view.

## Testing status

The automated test suite is green:

```text
dotnet test

Test summary:
total:     72
failed:    0
succeeded: 72
skipped:   0
```

The tests cover the current deactivate behavior across several layers:

- Domain tests for `Reader.Deactivate(...)`
- Use case tests for `ReaderUcDeactivate`
- Mock-based application tests
- Integration tests for read models
- Tests for normal active-reader queries
- Tests for `WithInactive` queries
- Controller/end-to-end scenarios
