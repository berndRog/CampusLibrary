# Architecture: CampusLibrary Part 1 — Readers Monolith

This document describes the architecture of **Part 1** of the `CampusLibraryApi`.

Part 1 implements the first domain module, `Readers`, inside a single ASP.NET Core Web API project. The application is intentionally kept as a **one-project monolith**. Inside this monolith, however, the code is already structured according to clear architectural boundaries.

This means:

```text
one deployable application
one project
one database
one first domain module: Readers
```

The goal of Part 1 is not yet to split the solution into multiple projects. That will happen in Part 2. The goal of Part 1 is to introduce the internal structure that will later make the project split understandable.

## Architectural Goal

The architecture of Part 1 is intended to make the following concepts visible in teaching:

* how to structure a Web API monolith internally
* how to separate Web, BuildingBlocks, Core, and Infrastructure code
* how to model a first domain module
* how to distinguish write-oriented use cases from read-oriented read models
* how to keep domain logic out of controllers
* how to use DDD fundamentals such as Entity, Aggregate Root, Value Object, and Domain Error
* how to use EF Core as a technical persistence mechanism
* how to use ports to decouple Core from Infrastructure
* how to prepare the codebase for a later modular-monolith project split

Part 1 therefore answers this question:

> How can a small Web API monolith already be structured in a clean, module-oriented way?

## Current Project Structure

Current state with the first module `Readers`:

```text
CampusLibraryApi
├─ _0_Documentation
│  └─ part-1-readers-monolith
│     ├─ README.md
│     ├─ ARCHITECTURE.md
│     ├─ API.md
│     └─ TESTING.md
│
├─ _1_Web
│  └─ Controllers
│     └─ ReadersController.cs
│
├─ _2_BuildingBlocks
│  ├─ Result.cs
│  ├─ _1_Ports
│  │  ├─ IClock.cs
│  │  └─ IUnitOfWork.cs
│  └─ _3_Domain
│     ├─ Entities
│     │  ├─ Entity.cs
│     │  └─ AggregateRoot.cs
│     └─ Errors
│        └─ Error.cs
│
├─ _3_Core
│  └─ Readers
│     ├─ _1_Ports
│     │  ├─ IReaderRepository.cs
│     │  ├─ IReaderReadModel.cs
│     │  ├─ IReadersDbContext.cs
│     │  └─ IReaderUseCases.cs
│     │
│     ├─ _2_Application
│     │  ├─ Dtos
│     │  │  ├─ AddressDto.cs
│     │  │  ├─ ReaderCreateDto.cs
│     │  │  ├─ ReaderUpdateDto.cs
│     │  │  └─ ReaderDto.cs
│     │  ├─ Mappings
│     │  └─ UseCases
│     │     ├─ ReaderUcCreate.cs
│     │     ├─ ReaderUcUpdate.cs
│     │     ├─ ReaderUcDelete.cs
│     │     └─ ReaderUseCases.cs
│     │
│     └─ _3_Domain
│        ├─ Entities
│        │  └─ Reader.cs
│        ├─ ValueObjects
│        │  ├─ EmailVo.cs
│        │  └─ AddressVo.cs
│        └─ Errors
│           └─ ReaderErrors.cs
│
├─ _4_Infrastructure
│  └─ Persistence
│     ├─ Configurations
│     │  └─ ConfigReader.cs
│     ├─ Database
│     │  ├─ AppDbContext.cs
│     │  └─ UnitOfWorkEf.cs
│     ├─ ReadModels
│     │  └─ ReaderReadModelEf.cs
│     ├─ Repositories
│     │  └─ ReaderRepositoryEf.cs
│     └─ Seed.cs
│
├─ Configure
│  ├─ DiReaders.cs
│  ├─ DiInfrastructure.cs
│  └─ DiSwagger.cs
│
└─ Program.cs
```

## Why This Is Still a Monolith

Part 1 is a monolith because all application code lives inside one project:

```text
CampusLibraryApi
```

There are no separate projects yet for:

```text
CampusLibrary.Api
CampusLibrary.Readers
CampusLibrary.Infrastructure
CampusLibrary.BuildingBlocks
```

That project split is intentionally postponed to Part 2.

However, Part 1 already uses a structured internal layout:

```text
_1_Web
_2_BuildingBlocks
_3_Core
_4_Infrastructure
```

This makes the transition to Part 2 easier. The students first learn the architectural boundaries inside one project. Later, the same boundaries can be moved into separate projects.

## The First Domain Module: Readers

The first implemented domain module is `Readers`.

The `Readers` module manages the domain concept of a library reader. A reader is the domain representation of a person who can use the library.

The module currently contains:

* `Reader` as Aggregate Root
* `EmailVo` as Value Object
* `AddressVo` as Value Object
* `ReaderErrors` as domain errors
* `ReaderCreateDto`
* `ReaderUpdateDto`
* `ReaderDto`
* `ReaderUcCreate` as write use case
* `ReaderUcUpdate` as write use case for partial updates
* `ReaderUcDelete` as write use case
* `ReaderUseCases` as facade for write use cases
* `IReaderRepository` for the write side
* `IReaderReadModel` for the read side
* `IReadersDbContext` as restricted DbContext port
* `ReaderRepositoryEf` as EF Core repository
* `ReaderReadModelEf` as EF Core read model

The current HTTP API supports:

```text
GET    /camplib/v1/readers
GET    /camplib/v1/readers/{id}
GET    /camplib/v1/readers/email?email=...
POST   /camplib/v1/readers
PUT    /camplib/v1/readers/{id}
DELETE /camplib/v1/readers/{id}
```

## Layer Overview

Part 1 uses four main areas inside the single API project.

```text
_1_Web
_2_BuildingBlocks
_3_Core
_4_Infrastructure
```

Each area has a different responsibility.

## _1_Web

The Web layer contains the HTTP controllers.

In Part 1, this is mainly:

```text
ReadersController
```

The controller is responsible for translating HTTP requests into application calls.

It should not contain domain logic.

Typical controller responsibilities are:

* define routes
* receive DTOs
* call read models for GET requests
* call use cases for write requests
* translate `Result` errors into HTTP responses
* return DTOs or `ProblemDetails`

The controller does not decide whether an email address is valid. It does not decide whether a reader can be created. These decisions belong to value objects, aggregates, and use cases.

## _2_BuildingBlocks

The BuildingBlocks area contains common building blocks used by the application.
It contains concepts such as:

* `Result`
* `Error`
* `Entity`
* `AggregateRoot`
* `IClock`
* `IUnitOfWork`

These are not part of one specific domain module. They are reusable building blocks for the whole API.

## _3_Core

The Core area contains the application’s domain-oriented code.

In Part 1, the Core contains one domain module:

```text
Readers
```

Each Core module follows the same internal structure:

```text
_1_Ports
_2_Application
_3_Domain
```

This structure is already used in Part 1 even though there is only one module. The reason is didactic: students learn the shape once and can then apply it to additional modules.

## _3_Core/Readers/_3_Domain

The Domain layer contains the domain model.

For the Readers module, this includes:

* `Reader`
* `EmailVo`
* `AddressVo`
* `ReaderErrors`

The Domain layer contains business rules and domain validation.

It does not know:

* controllers
* EF Core
* HTTP
* Swagger
* database details
* dependency injection

The Domain layer should be understandable without knowing how the data is stored or how HTTP requests are received.

Example:

```text
_3_Core/Readers/_3_Domain/Entities/Reader.cs
_3_Core/Readers/_3_Domain/ValueObjects/EmailVo.cs
_3_Core/Readers/_3_Domain/ValueObjects/AddressVo.cs
_3_Core/Readers/_3_Domain/Errors/ReaderErrors.cs
```

## Reader as Aggregate Root

`Reader` is the Aggregate Root of the Readers module.

It owns the consistency rules for reader profile data.

The aggregate is created through a factory method:

```text
Reader.Create(...)
```

It is changed through domain methods, for example:

```text
Reader.UpdateProfile(...)
```

This avoids uncontrolled changes through public setters.

The didactic rule is:

> Domain state should be changed through explicit domain methods, not by setting properties from the outside.

## Value Objects

The Readers module currently uses two Value Objects:

```text
EmailVo
AddressVo
```

Value Objects encapsulate validation and normalization rules.

For example, `EmailVo` is responsible for checking and normalizing an email address.

The goal is to avoid spreading validation logic across controllers, use cases, and repositories.

## Domain Errors

Domain errors are represented explicitly.

Example:

```text
ReaderErrors.InvalidEmail
ReaderErrors.EmailAlreadyInUse
ReaderErrors.ReaderNotFound
```

Expected domain failures are returned through `Result`, not thrown as exceptions.

This makes success and failure paths visible in the code and easy to test.

## _3_Core/Readers/_2_Application

The Application layer coordinates use cases.

It contains:

* DTOs
* use cases
* mapping helpers
* use case facade

Examples:

```text
ReaderUcCreate
ReaderUcUpdate
ReaderUcDelete
ReaderUseCases
```

Use cases are responsible for the workflow. They coordinate several steps, but they should not contain detailed domain rules if those rules belong in the Domain layer.

Typical responsibilities of a use case are:

* validate basic input
* load aggregates
* create value objects
* check uniqueness rules through repositories
* call domain methods
* save changes through `IUnitOfWork`
* return DTOs

## _3_Core/Readers/_1_Ports

Ports are interfaces required by the Core.

The Readers module currently defines:

```text
IReaderRepository
IReaderReadModel
IReadersDbContext
IReaderUseCases
```

Ports allow the Core to depend on abstractions instead of concrete infrastructure.

The Core can say:

```text
I need a reader repository.
```

But it does not need to know:

```text
This repository is implemented with EF Core and SQLite.
```

That knowledge belongs to Infrastructure.

## Use Cases and Read Models

Part 1 intentionally separates write access and read access.

```text
Use Case  = write-oriented application workflow
ReadModel = read-oriented DB-to-DTO projection
```

Therefore:

```text
GET                  → ReadModel
POST / PUT / DELETE  → Use Case
```

This distinction is important for teaching.

GET requests should not accidentally become domain workflows. They simply query data and return DTOs.

Write requests, on the other hand, must protect domain consistency.

## Write Side

Write workflows go through use cases.

```text
Controller
→ Use Case
→ Domain / Aggregate
→ Repository
→ EF Core
→ UnitOfWork
```

Example for create:

```text
POST /camplib/v1/readers
→ ReadersController
→ ReaderUseCases.CreateAsync
→ ReaderUcCreate
→ EmailVo.Create(...)
→ AddressVo.Create(...)
→ Reader.Create(...)
→ IReaderRepository
→ ReaderRepositoryEf
→ UnitOfWorkEf
```

Example for update:

```text
PUT /camplib/v1/readers/{id}
→ ReadersController
→ ReaderUseCases.UpdateAsync
→ ReaderUcUpdate
→ Reader.UpdateProfile(...)
→ IReaderRepository
→ ReaderRepositoryEf
→ UnitOfWorkEf
```

Example for delete:

```text
DELETE /camplib/v1/readers/{id}
→ ReadersController
→ ReaderUseCases.DeleteAsync
→ ReaderUcDelete
→ IReaderRepository
→ ReaderRepositoryEf
→ UnitOfWorkEf
```

## Read Side

Read workflows go through read models.

```text
Controller
→ ReadModel
→ DbContext
→ DTO
```

Read models typically use:

```csharp
.AsNoTracking()
.Select(...)
```

Example:

```text
GET /camplib/v1/readers
→ ReadersController
→ IReaderReadModel.SelectAllAsync
→ ReaderReadModelEf
→ AppDbContext
→ ReaderDto
```

The read side does not load the aggregate to return a list of DTOs. It projects database data directly into DTOs.

This keeps read operations simple and efficient.

## Partial Updates

The `ReaderUpdateDto` is intentionally nullable:

```csharp
public sealed record ReaderUpdateDto(
   string? Lastname,
   string? Email,
   AddressDto? AddressDto
);
```

This is technically and conceptually required for partial updates.

The meaning is:

```text
Lastname = null   → keep the current last name
Email = null      → keep the current email address
AddressDto = null → keep the current address
```

`Firstname` is intentionally not part of the update DTO. It is not changed by the current update use case.

The domain still validates values if they are provided:

```text
null          → no change
"" / "   "    → invalid value if the field is provided
"Meier"      → valid change
```

So `null` does not mean "invalid" in this DTO. It means "no change".

## Create DTO and Optional Id

The `ReaderCreateDto` contains an optional `Id`:

```csharp
public sealed record ReaderCreateDto(
   string Firstname,
   string Lastname,
   string Email,
   AddressDto AddressDto,
   string Subject,
   string? Id
);
```

`Id` is nullable by design.

It may be omitted in normal API usage. In that case, the application generates a new id.

It may be provided for teaching purposes, seed data, or tests.

Therefore, `Id` is both technically and conceptually optional.

This is different from the required domain data:

```text
Firstname
Lastname
Email
Address
Subject
```

Those fields are required for creating a valid reader.

## _4_Infrastructure

The Infrastructure area contains technical implementations.

This includes:

* EF Core configurations
* `AppDbContext`
* repositories
* read models
* `UnitOfWorkEf`
* seed data
* later security implementations

The Infrastructure layer may know EF Core.

The Core must not know EF Core.

This dependency direction is essential:

```text
Core defines ports.
Infrastructure implements ports.
```

## Repository Implementation

The repository implementation belongs to Infrastructure.

Example:

```text
ReaderRepositoryEf
```

It implements:

```text
IReaderRepository
```

The repository is used by write use cases.

It works with aggregates and supports operations such as:

* add reader
* find reader by id
* find reader by email
* check subject uniqueness
* remove reader

## Read Model Implementation

The read model implementation also belongs to Infrastructure.

Example:

```text
ReaderReadModelEf
```

It implements:

```text
IReaderReadModel
```

The read model is used by GET endpoints.

It returns DTOs directly and should not contain domain behavior.

## DbContext Access

There is one shared technical database and one shared EF Core DbContext.

In Part 1, this is still inside one project.

To restrict module access, the Readers module defines its own DbContext port:

```csharp
public interface IReadersDbContext {
   DbSet<Reader> Readers { get; }
   Task<int> SaveChangesAsync(CancellationToken ct);
}
```

`AppDbContext` implements this interface.

This allows the Readers module to depend only on the part of the DbContext it needs.

The didactic idea is:

> Even with one physical DbContext, modules can define their own logical view of the database.

## Visibility and `internal`

Concrete infrastructure classes should be `internal` where possible.

Typical internal classes:

```text
ReaderRepositoryEf
ReaderReadModelEf
ConfigReader
UnitOfWorkEf
```

Only the required ports, DTOs, use cases, and DI extension methods remain publicly visible.

This keeps the public surface small and makes module boundaries clearer.

## Dependency Injection

Dependency Injection connects ports to implementations.

Example:

```text
IReaderRepository → ReaderRepositoryEf
IReaderReadModel  → ReaderReadModelEf
IReaderUseCases   → ReaderUseCases
IUnitOfWork       → UnitOfWorkEf
```

The concrete implementation remains in Infrastructure.

`Program.cs` should only know high-level registrations, for example:

```csharp
builder.Services.AddReadersModule();
builder.Services.AddInfrastructureModule(builder.Configuration);
```

The goal is to keep `Program.cs` readable and free of detailed implementation registrations.

## Program.cs

Part 1 uses a normal `Program` class, not top-level statements.

The purpose of `Program.cs` is to configure and start the application.

Typical responsibilities are:

* create the builder
* register controllers
* register the Readers module
* register infrastructure
* register Swagger and API versioning
* build the application
* enable Swagger in development
* map controllers
* run the application

`Program.cs` is not a place for domain logic.

## API Versioning and Swagger

The API uses versioned routes.

Current Reader routes use:

```text
/camplib/v1/readers
```

Swagger/OpenAPI is configured for documentation and manual testing.

Swagger is not the architecture itself. It documents the HTTP surface of the application.

The architecture rule remains:

```text
Swagger documents the API.
Controllers translate HTTP.
Use cases write.
Read models read.
```

## Testing Architecture

Part 1 includes tests for several levels.

Typical test groups are:

```text
Domain tests
Use case tests with mocks
Use case integration tests
Repository integration tests
Read model integration tests
Controller / end-to-end tests
```

The current test suite verifies:

```text
Reader domain behavior
Email and address validation
Create use case
Update use case
Delete use case
Repository behavior
Read model projections
HTTP controller behavior
```

The latest known test status for Part 1 is:

```text
63 tests
0 failed
```

## Planned Evolution

Part 1 is the foundation.

The planned teaching steps are:

```text
Part 1: Readers, one-project monolith
Part 2: Readers, project-based modular monolith
Part 3: Readers + Catalog
Part 4: Readers + Catalog + Loans
Part 5: AuthN + AuthZ
```

## Transition to Part 2

Part 2 will split the current one-project structure into several projects.

A possible project structure for Part 2 is:

```text
CampusLibrary.Api
CampusLibrary.BuildingBlocks
CampusLibrary.Readers
CampusLibrary.Infrastructure
CampusLibrary.ApiTest
```

In that step, `_2_Shared` should likely be renamed to `BuildingBlocks`.

Reason:

* `_2_Shared` currently contains common building blocks inside the API project.
* Later, there may be a real shared library for cross-application concerns such as logging.
* That future shared library should not be confused with domain/application building blocks.

A later cross-application logging project could be named:

```text
Campus.Observability
```

or:

```text
Campus.Logging
```

## Rules for Extending Part 1

Even though Part 1 is still a monolith, new code should follow the same rules.

### Core

A new domain module receives its own Core area:

```text
_3_Core/<ModuleName>
├─ _1_Ports
├─ _2_Application
└─ _3_Domain
```

### Infrastructure

Infrastructure implements the ports of the Core module.

Currently, Infrastructure is grouped technically:

```text
Configurations
Database
Repositories
ReadModels
```

For larger modules, it can later be grouped more explicitly by module.

The important rule is:

> Infrastructure implements the ports of the Core. Core does not depend on Infrastructure.

### Web

Controllers are placed in:

```text
_1_Web/Controllers
```

Controllers contain no domain logic. They translate HTTP requests into calls to use cases or read models.

## Architecture Rules

1. The application is one project in Part 1.
2. The internal structure already follows architectural boundaries.
3. Web translates HTTP and contains no domain logic.
4. Core contains the domain and application logic.
5. Domain does not know Web, Infrastructure, EF Core, or Swagger.
6. Use cases write domain state.
7. Read models read data directly as DTO projections.
8. Repositories are used on the write side.
9. Read models are used on the read side.
10. Infrastructure implements Core ports.
11. EF Core configuration belongs to Infrastructure.
12. `Program.cs` wires modules together but contains no domain logic.
13. Additional modules should follow the same structure as `Readers`.
14. AuthN/AuthZ will be added later without changing the basic structure.

## Didactic Rule of Thumb

> Use cases protect domain rules on the write side. Read models provide simple DTOs on the read side.

Or shorter:

```text
Use cases write.
Read models read.
```

For Part 1, another rule is important:

> First learn the boundaries inside one project. Then move the boundaries into separate projects.

```
```
