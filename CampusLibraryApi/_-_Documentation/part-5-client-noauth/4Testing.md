# Testing Strategy — Part 5

This document describes the testing strategy for branch `part-5/client-noauth`.

German version: [4Testing-ger.md](4Testing-ger.md)

## Verified state

The current state was successfully verified locally on July 15, 2026:

```text
dotnet clean
Build succeeded

dotnet build
Build succeeded

dotnet test
212 total, 212 succeeded, 0 failed, 0 skipped
```

The complete `Loan_Me.http` workflow was also executed successfully.

## Test projects and applications

Automated test project:

```text
CampusLibraryApiTest
```

Applications under test:

```text
CampusLibraryApi
CampusLibraryClient
```

Other projects included in the build:

```text
CampusLibraryApi_1_Web
CampusLibraryApi_2_BuildingBlocks
CampusLibraryApi_3_Core_Readers
CampusLibraryApi_3_Core_Catalog
CampusLibraryApi_3_Core_Loan
CampusLibraryApi_4_Infrastructure
IdentityAccessServer
Shared
```

The IdentityAccessServer is built, but is not required for Part 5 runtime or `/me` HTTP tests.

## Test levels

The current state uses:

```text
domain tests
value-object tests
use-case mock tests
use-case integration tests
repository integration tests
read-model integration tests
BC-to-BC contract integration tests
controller/API end-to-end tests
DI tests
manual HTTP files
client build
manual browser smoke tests
```

## 1. Full regression

After changing API, client, DTOs, DevIdentity, use cases or tests:

```bash
dotnet clean
dotnet build
dotnet test
```

Expected state:

```text
Build succeeded
212 tests succeeded
0 failed
0 skipped
```

## 2. Git checks before commit

```bash
git status
git diff --check
git add -A
git diff --cached --check
```

After commit:

```bash
git status
```

Expected:

```text
working tree clean
```

## 3. Reader test focus

Important Reader rules:

```text
Reader creation validates Subject and email
Subject and email are unique
Reader deactivation is soft delete
inactive Readers are hidden by default
a Reader with current Loans cannot be deactivated
UpdateMe resolves the Reader by Subject
UpdateMe accepts only mutable profile data
null in ReaderUpdateDto means unchanged
```

### Use-case tests for UpdateMe

Verify:

```text
IdentitySubject.Check is honored
unauthenticated identity -> error
Employee profile -> AccessNotAllowed
empty or invalid Subject -> error
Reader by Subject not found -> NotFound
new email invalid -> BadRequest
new email already used -> Conflict
valid values are persisted
omitted values remain unchanged
```

### API endpoint

```http
PUT /camplib/v1/readers/me/update
```

Expected status codes:

```text
200 success
400 validation
401 unauthenticated
403 not a Reader
404 Subject has no Reader
409 duplicate email
```

## 4. DevIdentity tests

Client and API read separate configurations.

### TC-P5-IDENTITY-001 — Consistent Reader profile

Preconditions:

```text
client ActiveProfile = ReaderRita
API ActiveProfile    = ReaderRita
API Subject          = reader-099
Reader.Subject       = reader-099
```

Expected:

```text
Client shows Reader perspective.
API /me endpoints operate on Rita Reader.
No token and no identity header are sent.
```

### TC-P5-IDENTITY-002 — Subject mismatch

Temporarily configure the API:

```text
Subject = unknown-reader
```

Restart the API and call a `/me` endpoint.

Expected:

```text
404 Not Found
```

This confirms that association uses Subject, not email or ReaderId.

### TC-P5-IDENTITY-003 — Employee profile on Reader endpoint

API configuration:

```text
ActiveProfile = EmployeeAdmin
```

Call:

```http
GET /camplib/v1/loans/me
```

Expected:

```text
403 Forbidden
```

### TC-P5-IDENTITY-004 — Simulated unauthenticated identity

Profile:

```text
IsAuthenticated = false
```

Expected for Reader self-service:

```text
401 Unauthorized
```

### TC-P5-IDENTITY-005 — AdminRights compatibility

Precondition:

```text
AdminRights = 0
```

Expected:

```text
IdentityGateway returns 0.
CampusLibrary does not evaluate the value as a business permission.
Self-service works unchanged.
```

## 5. Start API and client

API:

```bash
dotnet run --project CampusLibraryApi
```

Client:

```bash
dotnet run --project CampusLibraryClient
```

Addresses:

```text
API:    https://localhost:8010
Client: https://localhost:6040
```

The API must run for direct `.http` tests. The client is not required for them.

## 6. Database and HTTP scripts

Manual files:

```text
CampusLibraryApi/_5_ApiTest/Reader.http
CampusLibraryApi/_5_ApiTest/Reader_Post.http
CampusLibraryApi/_5_ApiTest/Book.http
CampusLibraryApi/_5_ApiTest/Book_Post.http
CampusLibraryApi/_5_ApiTest/BookItem_Post.http
CampusLibraryApi/_5_ApiTest/Loan.http
CampusLibraryApi/_5_ApiTest/Loan_Post.http
CampusLibraryApi/_5_ApiTest/Loan_Me.http
```

Before a deterministic full run:

```text
reset the database
create Reader test data
create Book and BookItem test data
run Loan scripts last
```

Optional `Id` fields in create DTOs allow fixed IDs in HTTP and integration tests.

## 7. Manual Reader test

### TC-P5-READERS-001 — Create Reader 99

```http
POST /camplib/v1/readers
```

Important values:

```text
ReaderId = 00000099-0000-0000-0000-000000000000
Subject  = reader-099
Email    = r.reader@library.local
```

Expected:

```text
201 Created
```

### TC-P5-READERS-002 — Update current Reader

API profile:

```text
ActiveProfile = ReaderRita
Subject = reader-099
```

Request:

```http
PUT /camplib/v1/readers/me/update
Content-Type: application/json

{
  "lastname": "Meier",
  "email": "e.meier@gmx.de",
  "addressDto": {
    "street": "Neue Straße 1",
    "postalCode": "29556",
    "city": "Suderburg",
    "country": "DE"
  }
}
```

Expected:

```text
200 OK
Reader.Subject remains reader-099
Reader.Email changes
```

### TC-P5-READERS-003 — Old email is not identity association

After the update:

```text
DevIdentity.Username may still be r.reader@library.local.
Reader.Email is e.meier@gmx.de.
/me endpoints continue to work through Subject.
```

### TC-P5-READERS-004 — Deactivate Reader

```http
DELETE /camplib/v1/readers/{id}
```

Expected without current Loans:

```text
204 No Content
normal GET -> 404
GET with includeInactive=true -> 200 and IsActive=false
```

## 8. Catalog regression

Verify:

```text
BookDto is used for list and detail
search returns BookDto[]
BookItemDto contains Id, BookId and Status
no InventoryNumber in the transport contract
book creation returns 201
BookItem creation returns 201
inactive books are hidden by default
deactivation info shows current Loans
book deactivation cannot violate current Loan rules
```

### TC-P5-CATALOG-001 — Books list

```http
GET /camplib/v1/books?includeInactive=false
```

Expected:

```text
200 OK
BookDto[]
```

### TC-P5-CATALOG-002 — Search

```http
GET /camplib/v1/books/search?searchField=Title&searchText=Clean%20Code&includeInactive=false
```

Expected:

```text
200 OK
BookDto[]
```

### TC-P5-CATALOG-003 — Add BookItem

```http
POST /camplib/v1/books/{bookId}/items
```

Expected:

```text
201 Created
BookItemDto.Status = 1
```

## 9. Loan regression

Important rules:

```text
Loan has no Status and no ReturnedAt
an existing Loan means a current borrowing process
Borrow validates Reader and BookItem through module contracts
BookItem must be available
Renew checks due state and maximum renewals
ReturnAtDesk deletes the Loan
```

### Administrative tests

```text
GET   /loans
GET   /loans/{id}
POST  /loans
PATCH /loans/{id}/renew
PATCH /loans/{id}/return-at-desk
```

### Reader self-service

```text
GET   /loans/me
GET   /loans/me/{id}
POST  /loans/me
PATCH /loans/me/{id}/renew
```

For every `/me` route verify that another Reader's Loan is not visible or renewable.

## 10. Verified Loan_Me.http workflow

File:

```text
CampusLibraryApi/_5_ApiTest/Loan_Me.http
```

Preconditions:

```text
ActiveProfile = ReaderRita
Subject = reader-099
Reader with reader-099 exists
BookItem 00000002-0000-0000-0000-000000000000 is available
LoanId 00000099-0000-0001-0000-000000000000 does not exist yet
```

Expected responses:

```text
GET /loans/me
-> 200 OK, initially possibly []

POST /loans/me
-> 201 Created

GET /loans/me/{id}
-> 200 OK

PATCH /loans/me/{id}/renew
-> 200 OK

PATCH /loans/{id}/return-at-desk
-> 204 No Content

GET /loans/me/{id}
-> 404 Not Found
```

The final 404 is not a test failure. It confirms the delete-on-return model.

## 11. Client smoke tests

### TC-P5-CLIENT-001 — Client starts without login

Precondition:

```text
AuthNEnabled = false
DevIdentityEnabled = true
ApiAccessTokenEnabled = false
AuthZEnabled = false
```

Expected:

```text
no login redirect
no Bearer token
home page and navigation are displayed
```

### TC-P5-CLIENT-002 — Reader perspective

Client profile:

```text
ActiveProfile = ReaderRita
```

Expected:

```text
catalog is visible
/my/loans is visible
Reader can borrow an available BookItem
BorrowMyAsync sends no ReaderId
```

### TC-P5-CLIENT-003 — Employee perspective

Client profile:

```text
ActiveProfile = EmployeeAdmin
```

Expected:

```text
/readers is visible
/loans is visible
Catalog actions for create, add item and deactivate are visible
```

### TC-P5-CLIENT-004 — API unavailable

Stop the API and open a client page that loads data.

Expected:

```text
no unhandled UI exception
understandable central error message
```

## 12. DTO regression rules

When public API DTOs change, always verify both sides:

```text
API module DTO
client transport DTO
API client method
Razor page or model
controller E2E test
```

Do not reintroduce:

```text
BookListItemDto
BookDetailDto
BookSearchDto
LoanListItemDto
LoanDetailDto
Loan.Status
Loan.ReturnedAt
BookItem.InventoryNumber in the current transport contract
```

## 13. Regression rules

After DevIdentity changes:

```text
compare API and client appsettings
compare ActiveProfile
verify Subject against Reader test data
restart the API
run Loan_Me.http
```

After Reader self-service changes:

```text
mock test
integration test
controller E2E test
Reader.http
client build
```

After Loan self-service changes:

```text
ownership tests
identity error tests
Loan_Me.http
client Borrow/MyLoans smoke test
```

A green `dotnet test` does not replace manual `.http` and browser tests because feature-flag and configuration errors may only appear at runtime.
