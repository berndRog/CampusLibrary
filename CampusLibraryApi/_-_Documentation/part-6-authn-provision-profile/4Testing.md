# Testing Strategy – CampusLibrary Part 6

German version: [4Testing-ger.md](4Testing-ger.md)

Official branch:

```text
part-6/authn-provision-profile
```

## Goal

Part 6 extends the test strategy with technical identity, provisioning, profile states, Bearer-token throughput and subject-based `/me` workflows.

The tests must prove two things at the same time:

```text
The business changes work correctly.
The architecture remains independent from HTTP, claim and JWT details.
```

The last fully verified state was:

```text
238 tests
0 failed
0 skipped
```

After the final merge, run again:

```bash
dotnet clean
dotnet build
dotnet test
```

## Test levels

```text
Domain tests
Application use-case mock tests
Application integration tests
Infrastructure repository tests
Infrastructure read-model tests
Controller/API end-to-end tests
manual HTTP scripts
```

Controller mock tests are not used as a broad separate test level. Controllers should remain thin HTTP adapters. Application workflows are tested through use-case tests; the public HTTP contract is tested through `WebApplicationFactory` and `HttpClient`.

## Identity tests

### `IdentitySubject.Check(...)`

Validation should cover at least:

```text
not authenticated                    → IdentityUnauthenticated
not a Reader account                 → AccessNotAllowed
empty subject                        → SubjectRequired
subject longer than 200 characters   → InvalidIdentitySubject
empty username                       → IdentityEmailRequired
CreatedAt is default                 → TimestampInvalid
valid Reader identity                → Success(subject)
```

`AdminRights` is not validated as a business rule. It belongs to the technical compatibility contract.

### Fake Identity Gateway

Use-case and integration tests use a controllable `IIdentityGateway` or fake. Identity scenarios can therefore be tested without a running IdentityAccessServer and without JWT libraries.

Typical identities:

```text
Reader with valid subject
Employee
unauthenticated user
Reader with missing subject
Reader with incomplete profile
```

## Reader provisioning

Tests for `ReaderUcCreateMeProvision` verify, among other things:

```text
valid technical Reader identity creates a domain Reader
subject is stored permanently
username is used as the initial email
optional test ID is adopted reproducibly
duplicate provisioning is rejected
Employee cannot provision a Reader
invalid identity data is rejected before persistence
UnitOfWork saves only on success
```

## Initial profile completion

Tests for `ReaderUcUpdateMeProfile` verify:

```text
Reader is resolved by subject
first name, last name and address are set
IsProfileCompleted becomes true
non-provisioned Reader returns NotFound
already completed profile follows the defined rule
validation errors are propagated
```

## Later self-service updates

Tests for `ReaderUcUpdateMe` verify:

```text
no Reader ID is required from the client
Reader is resolved through identity subject
last name may change
email may change
address may change
null keeps the current value
another Reader cannot be selected through an ID
```

An important regression test confirms that Reader resolution still works through `Subject` after an email change.

## Loan tests

### Borrow Me

Tests verify:

```text
Reader is authenticated
Reader account is required
Reader is provisioned
profile is complete
Reader is active
BookItem exists
BookItem may be borrowed
BookItem has no active Loan
Loan receives ReaderId from subject mapping
optional Loan test ID is adopted
```

### Select Me

Read-model and API tests confirm that `/loans/me` returns only Loans belonging to the current Reader.

### Renew Me

Tests verify:

```text
Loan belongs to the current Reader
renewal count is incremented
due date is calculated reproducibly through a fake clock
business renewal limits are enforced
foreign Loans return AccessNotAllowed or NotFound according to the API contract
```

### Return at Desk

Returning deletes the Loan. Tests verify:

```text
Loan exists before return
return succeeds
Loan no longer exists in the repository
GET afterwards returns 404
BookItem may be borrowed again afterwards
```

Old assumptions such as `Loan.Status`, `ReturnedAt` or `LoanAlreadyReturned` are no longer part of the current model.

## Catalog tests

Catalog tests continue to verify:

```text
Book creation
ISBN validation
AuthorsText
BookItem creation
Book search by Title, AuthorLastName and Isbn
deactivation
filtering inactive Books from read models
```

A BC-to-BC test for `BookUcDeactivate` verifies that a Book with active Loans cannot be deactivated. The use case uses `ILoanCatalogContract`, not direct access to the Loan table.

## Repository and read-model tests

Repositories test aggregates and write-side state. Read models test public projections.

```text
Repository:
Can the aggregate be stored and loaded for a use case?

Read model:
Which data is exposed by the application?
```

Reader read-model tests must correctly project subject-based lookup and `IsProfileCompleted`.

Loan read-model tests use the current `LoanDto`, without old list/detail duplicates and without a returned status.

## Controller/API end-to-end tests

E2E tests use:

```text
WebApplicationFactory<Program>
HttpClient
test database
test identity adapter or test authentication
```

They verify:

```text
routing
JWT/identity throughput to the gateway
model binding
status codes
JSON serialization
ProblemDetails
dependency injection
EF Core integration
public DTO structures
```

Important scenarios:

```text
POST /readers/me/provision → 204
GET /readers/me → 200
PUT /readers/me/profile → 200
PUT /readers/me/update → 200
GET /loans/me → 200
POST /loans/me → 201
GET /loans/me/{id} → 200
PATCH /loans/me/{id}/renew → 200
PATCH /loans/{id}/return-at-desk → 204
GET after return → 404
```

Unauthenticated and wrong-role tests verify `401` and `403`.

## IdentityAccessServer and manual tests

Automated CampusLibrary tests should not depend on an externally running IdentityAccessServer. This keeps the suite fast and reproducible.

The real end-to-end flow is additionally tested through `.http` scripts:

```text
create Development user
obtain token
provision Reader
read Reader
complete profile
create Book and BookItem
borrow Loan
renew Loan
return Loan
```

Scripts assert expected status codes.

## Client tests and manual client verification

The client should demonstrate:

```text
login and logout
show authenticated user
attach access token to API requests
call Catalog without unnecessary ReaderId
show Reader profile state
navigate provisioning/profile pages correctly
call Loans through /me
handle 401/403/ProblemDetails meaningfully
```

The API remains the business authority. Client-side visibility never replaces server-side validation.

## Completion criteria

Before merge and tag:

```bash
git diff --cached --check
dotnet clean
dotnet build
dotnet test
```

Then verify:

```text
working tree clean
branch part-6/authn-provision-profile pushed
tag v6-authn-provision-profile points to the final commit
only the official Part 6 branch remains published
```

## Didactic core statement

```text
Authentication provides a technical identity.
Provisioning links it to a domain Reader.
Use cases know only IIdentityGateway.
Tests replace technical token sources with controllable adapters.
```
