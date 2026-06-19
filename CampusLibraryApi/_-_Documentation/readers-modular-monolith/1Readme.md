# CampusLibrary — Part 2: Readers Modular Monolith

Teaching project for a modular DDD-oriented ASP.NET Core Web API.

Part 2 continues the `Readers` module from Part 1 and moves the internal architectural boundaries into separate projects. The application is still one deployable modular monolith, but Web, BuildingBlocks, Core, Infrastructure and Tests are now separated more explicitly.

## Current status

The current version contains the first functional module:

- `Readers` module
- ASP.NET Core Web API
- API versioning
- Swagger/OpenAPI documentation
- SQLite persistence with EF Core
- separate projects for Web, BuildingBlocks, Readers Core, Infrastructure and Tests
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

## Solution structure

Part 2 uses multiple projects:

```text
CampusLibraryApi
CampusLibraryApi_1_Web
CampusLibraryApi_2_BuildingBlocks
CampusLibraryApi_3_Core_Readers
CampusLibraryApi_4_Infrastructure
CampusLibraryApiTest
```

This is still one application and one deployment unit. The project split is used to make architectural dependencies visible and to prepare later modules.

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

| Term | Meaning |
|---|---|
| `Deactivate` | Domain operation that changes the reader state |
| `IsActive == false` | Technical state after deactivation |
| `DELETE /readers/{id}` | HTTP endpoint used to trigger the deactivation |
| Normal read model queries | Return only active readers |
| `WithInactive` queries | Return active and inactive readers |

## Testing status

```text
dotnet test

Test summary:
total:     72
failed:    0
succeeded: 72
skipped:   0
```
