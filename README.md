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
- Use cases for create, partial update and delete
- Controller/end-to-end tests with a real SQLite test database

The test suite currently contains 63 tests covering domain, value objects, use cases, repositories, read models and controller/end-to-end scenarios.