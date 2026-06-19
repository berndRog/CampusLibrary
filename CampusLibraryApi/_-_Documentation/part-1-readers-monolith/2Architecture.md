# Architecture: CampusLibrary Part 2 — Readers Modular Monolith

This document describes the architecture of Part 2 of the CampusLibraryApi.

Part 2 refactors the completed Readers monolith from Part 1 into a project-based modular monolith. The functional scope stays the same: the application still contains only the Readers module. The main goal of this part is not to add new business functionality, but to introduce stronger architectural boundaries through separate projects.

Part 1 already used a clean internal structure inside one project:

```text
_1_Web
_2_BuildingBlocks
_3_Core
_4_Infrastructure
```

Part 2 moves these architectural areas into separate projects.

This means:

* one deployable application
* multiple projects
* one database
* one first domain module: Readers
* stronger technical boundaries through project references
* unchanged business behavior
* existing tests remain green

The current test suite contains:

```text
66 tests
0 failed
```

## Architectural Goal

The architecture of Part 2 is intended to make the following concepts visible in teaching:

<<<<<<< HEAD:CampusLibraryApi/_-_Documentation/part-1-readers-monolith/ARCHITECTURE.md
* how to refactor a structured monolith into a modular monolith
* how to use projects as architectural boundaries
* how to separate Web/API, BuildingBlocks, Core module, Infrastructure and Tests
* how to keep the domain model independent from technical persistence details
* how to make dependency rules technically visible through project references
* how to keep existing behavior stable during architectural refactoring
* how to use tests as a safety net for structural changes
* how to prepare the solution for additional future modules such as Catalog and Loans
=======
- how to structure a Web API monolith internally
- how to separate Web, BuildingBlocks, Core, and Infrastructure code
- how to model a first domain module
- how to distinguish write-oriented use cases from read-oriented read models
- how to keep domain logic out of controllers
- how to use DDD fundamentals such as Entity, Aggregate Root, Value Object, and Domain Error
- how to use EF Core as a technical persistence mechanism
- how to use ports to decouple Core from Infrastructure
- how to model soft delete behavior with a domain operation
- how to prepare the codebase for a later modular-monolith project split
>>>>>>> 8711308 (Replace reader delete with deactivate soft delete):CampusLibraryApi/_-_Documentation/part-1-readers-monolith/2Architecture.md

Part 2 therefore answers this question:

```text
How can a clean one-project monolith be refactored into a project-based modular monolith without changing its business behavior?
```

## Current Project Structure

Current state with the first module Readers:

```text
CampusLibraryApi
├─ Program.cs
├─ appsettings.json
└─ Configure
   ├─ DiSwagger.cs
   └─ other application-level registrations

CampusLibraryApi_1_Web
└─ Controllers
   └─ ReadersController.cs

CampusLibraryApi_2_BuildingBlocks
├─ Result.cs
├─ _1_Ports
│  ├─ IClock.cs
│  └─ IUnitOfWork.cs
└─ _3_Domain
   ├─ Entities
   │  ├─ Entity.cs
   │  └─ AggregateRoot.cs
   └─ Errors
      └─ Error.cs

CampusLibraryApi_3_Core_Readers
├─ _1_Ports
│  ├─ IReaderRepository.cs
│  ├─ IReaderReadModel.cs
│  ├─ IReadersDbContext.cs
│  └─ IReaderUseCases.cs
│
├─ _2_Application
│  ├─ Dtos
│  │  ├─ AddressDto.cs
│  │  ├─ ReaderCreateDto.cs
│  │  ├─ ReaderUpdateDto.cs
│  │  └─ ReaderDto.cs
│  ├─ Mappings
│  └─ UseCases
│     ├─ ReaderUcCreate.cs
│     ├─ ReaderUcUpdate.cs
│     ├─ ReaderUcDelete.cs
│     └─ ReaderUseCases.cs
│
<<<<<<< HEAD:CampusLibraryApi/_-_Documentation/part-1-readers-monolith/ARCHITECTURE.md
└─ _3_Domain
   ├─ Entities
   │  └─ Reader.cs
   ├─ ValueObjects
   │  ├─ EmailVo.cs
   │  └─ AddressVo.cs
   └─ Errors
      └─ ReaderErrors.cs

CampusLibraryApi_4_Infrastructure
└─ Persistence
   ├─ Configurations
   │  └─ ConfigReader.cs
   ├─ Database
   │  ├─ AppDbContext.cs
   │  └─ UnitOfWorkEf.cs
   ├─ ReadModels
   │  └─ ReaderReadModelEf.cs
   ├─ Repositories
   │  └─ ReaderRepositoryEf.cs
   └─ Seed.cs

CampusLibraryApiTest
└─ Tests for domain, value objects, use cases, repositories, read models and controller/end-to-end scenarios
=======
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
│     │     ├─ ReaderUcDeactivate.cs
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
>>>>>>> 8711308 (Replace reader delete with deactivate soft delete):CampusLibraryApi/_-_Documentation/part-1-readers-monolith/2Architecture.md
```

## Why This Is a Modular Monolith

Part 2 is still a monolith because the application is deployed as one application.

There is still:

```text
one deployable application
one database
one runtime process
```

However, it is now modular because the solution is split into separate projects with explicit dependency rules.

The important difference from Part 1 is this:

```text
Part 1: architectural boundaries are represented by folders.
Part 2: architectural boundaries are represented by projects.
```

This makes the architecture more explicit and harder to accidentally violate.

## Project Responsibilities

Part 2 uses the following main projects:

```text
CampusLibraryApi
CampusLibraryApi_1_Web
CampusLibraryApi_2_BuildingBlocks
CampusLibraryApi_3_Core_Readers
CampusLibraryApi_4_Infrastructure
CampusLibraryApiTest
```

Each project has a clear responsibility.

## CampusLibraryApi

`CampusLibraryApi` is the executable application project.

It contains the composition root of the application.

Typical responsibilities are:

<<<<<<< HEAD:CampusLibraryApi/_-_Documentation/part-1-readers-monolith/ARCHITECTURE.md
* configure the application host
* load configuration
* register controllers
* register Swagger/OpenAPI
* register API versioning
* register modules
* register infrastructure
* build and run the application
=======
This makes the transition to Part 2 easier. Students first learn the architectural boundaries inside one project. Later, the same boundaries can be moved into separate projects.
>>>>>>> 8711308 (Replace reader delete with deactivate soft delete):CampusLibraryApi/_-_Documentation/part-1-readers-monolith/2Architecture.md

`CampusLibraryApi` wires the application together.

It may reference all other production projects because it is responsible for composing the running application.

It must not contain domain logic.

## CampusLibraryApi_1_Web

<<<<<<< HEAD:CampusLibraryApi/_-_Documentation/part-1-readers-monolith/ARCHITECTURE.md
`CampusLibraryApi_1_Web` contains the HTTP API surface.

In Part 2, this mainly includes:
=======
- `Reader` as Aggregate Root
- `EmailVo` as Value Object
- `AddressVo` as Value Object
- `ReaderErrors` as domain errors
- `ReaderCreateDto`
- `ReaderUpdateDto`
- `ReaderDto`
- `ReaderUcCreate` as write use case
- `ReaderUcUpdate` as write use case for partial updates
- `ReaderUcDeactivate` as write use case for soft delete behavior
- `ReaderUseCases` as facade for write use cases
- `IReaderRepository` for the write side
- `IReaderReadModel` for the read side
- `IReadersDbContext` as restricted DbContext port
- `ReaderRepositoryEf` as EF Core repository
- `ReaderReadModelEf` as EF Core read model

The current HTTP API supports:

```text
GET    /camplib/v1/readers
GET    /camplib/v1/readers/with-inactive
GET    /camplib/v1/readers/{id}
GET    /camplib/v1/readers/{id}/with-inactive
GET    /camplib/v1/readers/email?email=...
POST   /camplib/v1/readers
PUT    /camplib/v1/readers/{id}
DELETE /camplib/v1/readers/{id}
```

`DELETE /camplib/v1/readers/{id}` triggers a deactivation. It does not physically remove the reader from the database.

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
>>>>>>> 8711308 (Replace reader delete with deactivate soft delete):CampusLibraryApi/_-_Documentation/part-1-readers-monolith/2Architecture.md

```text
ReadersController
```

The Web project is responsible for translating HTTP requests into application calls.

Typical responsibilities are:

<<<<<<< HEAD:CampusLibraryApi/_-_Documentation/part-1-readers-monolith/ARCHITECTURE.md
* define routes
* receive DTOs
* call read models for GET requests
* call use cases for write requests
* translate Result errors into HTTP responses
* return DTOs or ProblemDetails

The Web project does not contain business rules.
=======
- define routes
- receive DTOs
- call read models for GET requests
- call use cases for write requests
- translate `Result` errors into HTTP responses
- return DTOs or `ProblemDetails`

The controller does not decide whether an email address is valid. It does not decide whether a reader can be created, updated or deactivated. These decisions belong to value objects, aggregates, and use cases.
>>>>>>> 8711308 (Replace reader delete with deactivate soft delete):CampusLibraryApi/_-_Documentation/part-1-readers-monolith/2Architecture.md

For example, the controller does not decide whether an email address is valid. That belongs to the Readers domain model.

<<<<<<< HEAD:CampusLibraryApi/_-_Documentation/part-1-readers-monolith/ARCHITECTURE.md
## CampusLibraryApi_2_BuildingBlocks

`CampusLibraryApi_2_BuildingBlocks` contains reusable architectural building blocks.
=======
The BuildingBlocks area contains common building blocks used by the application.

It contains concepts such as:

- `Result`
- `Error`
- `Entity`
- `AggregateRoot`
- `IClock`
- `IUnitOfWork`
>>>>>>> 8711308 (Replace reader delete with deactivate soft delete):CampusLibraryApi/_-_Documentation/part-1-readers-monolith/2Architecture.md

Typical contents are:

* Result
* Error
* Entity
* AggregateRoot
* IClock
* IUnitOfWork

These types are not specific to Readers.

They are reusable concepts for all current and future modules.

The important rule is:

```text
BuildingBlocks must not depend on a concrete business module.
```

BuildingBlocks are general architectural elements. They are not the place for reader-specific, catalog-specific or loan-specific business logic.

## CampusLibraryApi_3_Core_Readers

`CampusLibraryApi_3_Core_Readers` is the first business module.

It contains the reader-specific domain model, application use cases, DTOs, mappings and ports.

The Readers module is structured internally into:

```text
_1_Ports
_2_Application
_3_Domain
```

This structure remains the same as in Part 1, but it now lives in its own project.

The important rule is:

```text
The Readers core module does not depend on Web or Infrastructure.
```

This keeps the business module independent from HTTP, EF Core, SQLite and other technical details.

<<<<<<< HEAD:CampusLibraryApi/_-_Documentation/part-1-readers-monolith/ARCHITECTURE.md
## Readers Domain

The domain part of the Readers module contains:

* Reader
* EmailVo
* AddressVo
* ReaderErrors
=======
- `Reader`
- `EmailVo`
- `AddressVo`
- `ReaderErrors`
>>>>>>> 8711308 (Replace reader delete with deactivate soft delete):CampusLibraryApi/_-_Documentation/part-1-readers-monolith/2Architecture.md

The Domain layer contains business rules and domain validation.

It does not know:

- controllers
- EF Core
- HTTP
- Swagger
- database details
- dependency injection

<<<<<<< HEAD:CampusLibraryApi/_-_Documentation/part-1-readers-monolith/ARCHITECTURE.md
The domain model should be understandable without knowing how the data is stored or how HTTP requests are received.
=======
The Domain layer should be understandable without knowing how the data is stored or how HTTP requests are received.
>>>>>>> 8711308 (Replace reader delete with deactivate soft delete):CampusLibraryApi/_-_Documentation/part-1-readers-monolith/2Architecture.md

## Reader as Aggregate Root

`Reader` is the Aggregate Root of the Readers module.

It owns the consistency rules for reader profile data and reader lifecycle state.

The aggregate is created through a factory method:

```csharp
Reader.Create(...)
```

It is changed through domain methods, for example:

```csharp
Reader.UpdateProfile(...)
Reader.Deactivate(...)
```

This avoids uncontrolled changes through public setters.

The didactic rule is:

```text
Domain state should be changed through explicit domain methods, not by setting properties from the outside.
```

## Value Objects

The Readers module currently uses two Value Objects:

* EmailVo
* AddressVo

Value Objects encapsulate validation and normalization rules.

For example, `EmailVo` is responsible for checking and normalizing an email address.

The goal is to avoid spreading validation logic across controllers, use cases and repositories.

## Domain Errors

Domain errors are represented explicitly.

Examples:

```text
ReaderErrors.InvalidEmail
ReaderErrors.EmailAlreadyInUse
ReaderErrors.ReaderNotFound
ReaderErrors.IsAlreadyDeactivated
```

Expected domain failures are returned through `Result`, not thrown as exceptions.

This makes success and failure paths visible in the code and easy to test.

## Readers Application Layer

The application part of the Readers module coordinates use cases.

It contains:

- DTOs
- use cases
- mapping helpers
- use case facade

Examples:

<<<<<<< HEAD:CampusLibraryApi/_-_Documentation/part-1-readers-monolith/ARCHITECTURE.md
* ReaderUcCreate
* ReaderUcUpdate
* ReaderUcDelete
* ReaderUseCases
=======
```text
ReaderUcCreate
ReaderUcUpdate
ReaderUcDeactivate
ReaderUseCases
```
>>>>>>> 8711308 (Replace reader delete with deactivate soft delete):CampusLibraryApi/_-_Documentation/part-1-readers-monolith/2Architecture.md

Use cases are responsible for workflows.

Typical responsibilities of a use case are:

<<<<<<< HEAD:CampusLibraryApi/_-_Documentation/part-1-readers-monolith/ARCHITECTURE.md
* validate basic input
* load aggregates
* create value objects
* check uniqueness rules through repositories
* call domain methods
* save changes through IUnitOfWork
* return DTOs
=======
- validate basic input
- load aggregates
- create value objects
- check uniqueness rules through repositories
- call domain methods
- save changes through `IUnitOfWork`
- return DTOs or errors
>>>>>>> 8711308 (Replace reader delete with deactivate soft delete):CampusLibraryApi/_-_Documentation/part-1-readers-monolith/2Architecture.md

Use cases should not contain detailed domain rules if those rules belong in the domain model.

## Readers Ports

Ports are interfaces required by the Readers core module.

The Readers module currently defines:

* IReaderRepository
* IReaderReadModel
* IReadersDbContext
* IReaderUseCases

Ports allow the Core module to depend on abstractions instead of concrete infrastructure.

The Core module can say:

```text
I need a reader repository.
```

But it does not need to know:

```text
This repository is implemented with EF Core and SQLite.
```

That knowledge belongs to Infrastructure.

## CampusLibraryApi_4_Infrastructure

`CampusLibraryApi_4_Infrastructure` contains technical implementations.

This includes:

* EF Core configurations
* AppDbContext
* repositories
* read models
* UnitOfWorkEf
* seed data
* later security or external system implementations

The Infrastructure project may know EF Core.

The Core module must not know EF Core.

The dependency direction is essential:

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

Read models typically use:

```csharp
.AsNoTracking()
.Select(...)
```

The read side does not load the aggregate to return a list of DTOs. It projects database data directly into DTOs.

This keeps read operations simple and efficient.

## DbContext Access

There is one shared technical database and one shared EF Core DbContext.

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

```text
Even with one physical DbContext, modules can define their own logical view of the database.
```

## Dependency Rules

The most important project dependency rules are:

```text
BuildingBlocks does not depend on any business module.

Readers depends on BuildingBlocks.

Infrastructure depends on BuildingBlocks and Readers.

Web depends on Readers and BuildingBlocks.

The executable API project wires all projects together.

Tests may reference all projects that are required for testing.
```

A simplified dependency direction is:

```text
CampusLibraryApi_2_BuildingBlocks
        ↑
        │
CampusLibraryApi_3_Core_Readers
        ↑
        │
CampusLibraryApi_4_Infrastructure
```

The Web/API side calls into the Readers module through ports and use cases.

The Infrastructure side implements outbound ports defined by the Readers module.

The Readers module itself remains independent from Web and Infrastructure.

## Use Cases and Read Models

Part 2 keeps the same write/read separation as Part 1.

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

Example for deactivate:

```text
DELETE /camplib/v1/readers/{id}
→ ReadersController
→ ReaderUseCases.DeactivateAsync
→ ReaderUcDeactivate
→ Reader.Deactivate(...)
→ IReaderRepository
→ ReaderRepositoryEf
→ UnitOfWorkEf
```

## Soft Delete and `IsActive`

The reader lifecycle uses soft delete behavior.

A reader is not physically deleted from the database. Instead, `Reader.Deactivate(...)` sets `IsActive` to `false`.

This means:

```text
Deactivate = domain operation
Inactive   = state after deactivation
DELETE     = HTTP verb used to trigger the operation
```

Normal read model queries return only active readers.

Special read model queries include inactive readers:

```text
FindByIdWithInactiveAsync
SelectAllWithInactiveAsync
```

This preserves historical information for later modules such as `Loans`, while still giving normal clients a clean active-reader view.

## Read Side

Read workflows go through read models.

```text
Controller
→ ReadModel
→ DbContext
→ DTO
```

<<<<<<< HEAD:CampusLibraryApi/_-_Documentation/part-1-readers-monolith/ARCHITECTURE.md
=======
Read models typically use:

```csharp
.AsNoTracking()
```

>>>>>>> 8711308 (Replace reader delete with deactivate soft delete):CampusLibraryApi/_-_Documentation/part-1-readers-monolith/2Architecture.md
Example:

```text
GET /camplib/v1/readers
→ ReadersController
→ IReaderReadModel.SelectAllAsync
→ ReaderReadModelEf
→ AppDbContext
→ ReaderDto
```

<<<<<<< HEAD:CampusLibraryApi/_-_Documentation/part-1-readers-monolith/ARCHITECTURE.md
The read side does not load the aggregate. It projects database data directly into DTOs.
=======
The read side does not load the aggregate to return a list of DTOs. It projects database data directly into DTOs.

Normal read methods return only active readers. Methods with `WithInactive` in the name return active and inactive readers.

This keeps read operations simple, efficient, and explicit.
>>>>>>> 8711308 (Replace reader delete with deactivate soft delete):CampusLibraryApi/_-_Documentation/part-1-readers-monolith/2Architecture.md

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

So null does not mean "invalid" in this DTO. It means "no change".

## Create DTO and Optional Id

The `ReaderCreateDto` contains an optional Id:

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

It may be provided for teaching purposes, seed data or tests.

Therefore, `Id` is both technically and conceptually optional.

This is different from the required domain data:

* Firstname
* Lastname
* Email
* Address
* Subject

Those fields are required for creating a valid reader.

## Visibility and internal

Concrete infrastructure classes should be internal where possible.

Typical internal classes are:

<<<<<<< HEAD:CampusLibraryApi/_-_Documentation/part-1-readers-monolith/ARCHITECTURE.md
* ReaderRepositoryEf
* ReaderReadModelEf
* ConfigReader
* UnitOfWorkEf

Only the required ports, DTOs, use cases and DI extension methods remain publicly visible.
=======
- EF Core configurations
- `AppDbContext`
- repositories
- read models
- `UnitOfWorkEf`
- seed data
- later security implementations

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

- add reader
- find reader by id
- find reader by email
- check subject uniqueness

The repository no longer removes readers for the normal lifecycle. Deactivation is performed by changing the aggregate state and saving it through the UnitOfWork.

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

It contains separate query methods for normal active-reader views and views that include inactive readers.

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
>>>>>>> 8711308 (Replace reader delete with deactivate soft delete):CampusLibraryApi/_-_Documentation/part-1-readers-monolith/2Architecture.md

This keeps the public surface small and makes module boundaries clearer.

## Dependency Injection

Dependency Injection connects ports to implementations.

Examples:

```text
IReaderRepository → ReaderRepositoryEf
IReaderReadModel  → ReaderReadModelEf
IReaderUseCases   → ReaderUseCases
IUnitOfWork       → UnitOfWorkEf
```

The concrete implementation remains in Infrastructure.

The executable API project should only know high-level registrations, for example:

```csharp
builder.Services.AddReadersModule();
builder.Services.AddInfrastructureModule(builder.Configuration);
```

The goal is to keep the startup code readable and free of detailed implementation registrations.

## Program.cs

`Program.cs` is part of the executable API project.

Its purpose is to configure and start the application.

Typical responsibilities are:

- create the builder
- register controllers
- register the Readers module
- register infrastructure
- register Swagger and API versioning
- build the application
- enable Swagger in development
- map controllers
- run the application

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

## Current HTTP API

The current HTTP API supports:

```text
GET    /camplib/v1/readers
GET    /camplib/v1/readers/{id}
GET    /camplib/v1/readers/email?email=...
POST   /camplib/v1/readers
PUT    /camplib/v1/readers/{id}
DELETE /camplib/v1/readers/{id}
```

## Testing Architecture

Part 2 keeps the existing test strategy from Part 1.

Typical test groups are:

* Domain tests
* Value Object tests
* Use case tests
* Use case integration tests
* Repository integration tests
* Read model integration tests
* Controller / end-to-end tests

The current test suite verifies:

* Reader domain behavior
* Email and address validation
* Create use case
* Update use case
* Delete use case
* Repository behavior
* Read model projections
* HTTP controller behavior

The latest known test status for Part 2 is:

```text
<<<<<<< HEAD:CampusLibraryApi/_-_Documentation/part-1-readers-monolith/ARCHITECTURE.md
66 tests
0 failed
=======
Reader domain behavior
Email and address validation
Create use case
Update use case
Deactivate use case
Soft delete behavior through IsActive
Repository behavior
Read model projections
HTTP controller behavior
>>>>>>> 8711308 (Replace reader delete with deactivate soft delete):CampusLibraryApi/_-_Documentation/part-1-readers-monolith/2Architecture.md
```

The tests are especially important in Part 2 because the main change is structural.

The intended result is:

```text
<<<<<<< HEAD:CampusLibraryApi/_-_Documentation/part-1-readers-monolith/ARCHITECTURE.md
The architecture changes.
The business behavior stays the same.
```

## Version

Part 2 is represented by the following branch and tag:

```text
Branch: part-2/readers-modular-monolith
Tag:    v2-readers-modular-monolith
```

Part 1 remains available as:

```text
Tag: v1-readers-monolith
=======
72 tests
0 failed
>>>>>>> 8711308 (Replace reader delete with deactivate soft delete):CampusLibraryApi/_-_Documentation/part-1-readers-monolith/2Architecture.md
```

## Planned Evolution

Part 2 is the modular foundation for the next teaching steps.

The planned evolution is:

```text
Part 1: Readers, one-project monolith
Part 2: Readers, project-based modular monolith
Part 3: Readers + Catalog
Part 4: Readers + Catalog + Loans
Part 5: AuthN + AuthZ
```

Part 3 will add a second business module.

That step is important because the architecture will then show more clearly why modular boundaries matter. With only Readers, the modular structure is already visible. With Readers and Catalog, the separation between modules becomes more concrete.

## Rules for Extending Part 2

New business modules should follow the same structure as Readers.

A new core module should have its own project, for example:

```text
CampusLibraryApi_3_Core_Catalog
```

<<<<<<< HEAD:CampusLibraryApi/_-_Documentation/part-1-readers-monolith/ARCHITECTURE.md
Its internal structure should follow the same pattern:

```text
_1_Ports
_2_Application
_3_Domain
```

Infrastructure implements the ports of the core modules.

The important rule remains:

```text
Core modules define ports.
Infrastructure implements ports.
Core modules do not depend on Infrastructure.
```

Web controllers are placed in the Web project.
=======
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
>>>>>>> 8711308 (Replace reader delete with deactivate soft delete):CampusLibraryApi/_-_Documentation/part-1-readers-monolith/2Architecture.md

Controllers contain no domain logic. They translate HTTP requests into calls to use cases or read models.

## Architecture Rules

<<<<<<< HEAD:CampusLibraryApi/_-_Documentation/part-1-readers-monolith/ARCHITECTURE.md
The application is one deployable application.

The solution is split into multiple projects.

Project boundaries represent architectural boundaries.

Web translates HTTP and contains no domain logic.

BuildingBlocks contains reusable architectural base types.

Core modules contain domain and application logic.

Domain does not know Web, Infrastructure, EF Core or Swagger.

Use cases write domain state.

Read models read data directly as DTO projections.

Repositories are used on the write side.

Read models are used on the read side.

Infrastructure implements Core ports.

EF Core configuration belongs to Infrastructure.

Program.cs wires modules together but contains no domain logic.

Additional modules should follow the same structure as Readers.

AuthN/AuthZ will be added later without changing the basic structure.
=======
1. The application is one project in Part 1.
2. The internal structure already follows architectural boundaries.
3. Web translates HTTP and contains no domain logic.
4. Core contains the domain and application logic.
5. Domain does not know Web, Infrastructure, EF Core, or Swagger.
6. Use cases write domain state.
7. Deactivation is a write use case and changes domain state.
8. Read models read data directly as DTO projections.
9. Normal read model queries return only active readers.
10. `WithInactive` read model queries return active and inactive readers.
11. Repositories are used on the write side.
12. Read models are used on the read side.
13. Infrastructure implements Core ports.
14. EF Core configuration belongs to Infrastructure.
15. `Program.cs` wires modules together but contains no domain logic.
16. Additional modules should follow the same structure as `Readers`.
17. AuthN/AuthZ will be added later without changing the basic structure.
>>>>>>> 8711308 (Replace reader delete with deactivate soft delete):CampusLibraryApi/_-_Documentation/part-1-readers-monolith/2Architecture.md

## Didactic Rule of Thumb

Use cases protect domain rules on the write side.

Read models provide simple DTOs on the read side.

Or shorter:

```text
Use cases write.
Read models read.
```

For Part 2, another rule is important:

```text
First learn the boundaries inside one project.
Then move the boundaries into separate projects.
```

<<<<<<< HEAD:CampusLibraryApi/_-_Documentation/part-1-readers-monolith/ARCHITECTURE.md
Part 2 demonstrates the second step:

```text
Folders become projects.
Conventions become technical boundaries.
=======
For the current reader lifecycle:

```text
Deactivate changes state.
Read models decide visibility.
>>>>>>> 8711308 (Replace reader delete with deactivate soft delete):CampusLibraryApi/_-_Documentation/part-1-readers-monolith/2Architecture.md
```
