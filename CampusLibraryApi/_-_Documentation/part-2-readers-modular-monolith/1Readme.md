# CampusLibrary

Teaching project for a modular DDD-oriented ASP.NET Core Web API.

This version is **Part 2 – Readers Modular Monolith**. It takes the reader functionality from Part 1 and moves the architecture from a folder-based monolith into a project-based modular monolith.

The functional scope is intentionally small: the application contains only the **Readers** module. The purpose of this part is to make module boundaries, dependency direction, ports, adapters, repositories, read models and tests visible before additional modules are introduced in later parts.

## Current Status

The project currently contains:

- Readers module only
- ASP.NET Core Web API
- API versioning
- Swagger/OpenAPI documentation
- SQLite persistence with EF Core
- Repository and ReadModel infrastructure
- Use cases for create, update and deactivate
- Controller/end-to-end tests with a real SQLite test database
- Modular project structure with Web, BuildingBlocks, Core_Readers, Infrastructure and Tests

The Reader behavior has been aligned with the current model used in the later project parts:

- `Reader` is an aggregate root.
- `Reader` has an `IsActive` flag.
- Readers are not physically deleted.
- The former delete operation is modeled as **Deactivate**.
- Normal read queries return only active readers.
- Special read model queries can include deactivated readers.
- Command use cases are separated from query read models.

The current test status is:

```text
Test summary: total: 70, failed: 0, succeeded: 70, skipped: 0
```

## Version

```text
v2-readers-modular-monolith
```

## Current Branch

```text
part-2/readers-modular-monolith
```

## Project Structure

```text
CampusLibraryApi
CampusLibraryApi_1_Web
CampusLibraryApi_2_BuildingBlocks
CampusLibraryApi_3_Core_Readers
CampusLibraryApi_4_Infrastructure
CampusLibraryApiTest
```

## Architectural Idea

The Web/API layer exposes HTTP endpoints.

The BuildingBlocks project contains shared abstractions and base types that are independent from a specific business module.

The Readers core project contains the reader-specific domain model, DTOs, mappings, application use cases and ports.

The Infrastructure project implements EF Core persistence, repositories, read models, database configuration and UnitOfWork.

The test project verifies the behavior across domain, application, infrastructure and API boundaries.

The most important dependency rule is:

```text
Core modules do not depend on Web/API or Infrastructure.
Infrastructure depends on Core modules because it implements their outbound ports.
The executable API project acts as the composition root and wires all modules together.
```

## What Is Not Included Yet

Part 2 intentionally does not contain:

- Catalog module
- Books
- BookItems
- Loans
- Authentication and authorization
- Cross-module contracts

These topics are introduced in later parts of the teaching sequence.
