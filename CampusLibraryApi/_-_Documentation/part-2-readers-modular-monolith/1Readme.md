# CampusLibrary

Teaching project for a modular DDD-oriented ASP.NET Core Web API.

The project demonstrates how a small modular monolith can be structured into separate projects for Web/API, Building Blocks, Core modules, Infrastructure and Tests while keeping the domain model independent from technical persistence details.

## Current status

The project currently contains the first functional module:

* Readers module
* ASP.NET Core Web API
* API versioning
* Swagger/OpenAPI documentation
* SQLite persistence with EF Core
* Repository and ReadModel infrastructure
* Use cases for create, partial update and delete
* Controller/end-to-end tests with a real SQLite test database

The initial monolith has been refactored into a project-based modular monolith. Shared abstractions and base types have been moved into `BuildingBlocks`. The `Readers` module is now an independent core module, while technical persistence details are located in the Infrastructure project.

The test suite currently contains 66 tests covering domain logic, value objects, use cases, repositories, read models and controller/end-to-end scenarios.

## Versions

* `v1-readers-monolith`
  First completed version with the Readers module inside a single monolithic project structure.

* `v2-readers-modular-monolith`
  Refactored version with a project-based modular monolith structure.

## Current branch

```text
part-2/readers-modular-monolith
```

## Project structure

```text
CampusLibraryApi
CampusLibraryApi_1_Web
CampusLibraryApi_2_BuildingBlocks
CampusLibraryApi_3_Core_Readers
CampusLibraryApi_4_Infrastructure
CampusLibraryApiTest
```

## Architectural idea

The Web/API layer exposes the HTTP endpoints.

The Core module contains the reader-specific domain model, application use cases and ports.

The BuildingBlocks project contains shared abstractions that are independent of a specific business module.

The Infrastructure project implements technical details such as EF Core persistence, repositories and read models.

The Test project verifies the behavior across domain, application, infrastructure and API boundaries.

The most important dependency rule is:

```text
Core modules do not depend on Web/API or Infrastructure.
Infrastructure depends on Core modules because it implements their outbound ports.
The API project acts as the composition root and wires all modules together.
```
