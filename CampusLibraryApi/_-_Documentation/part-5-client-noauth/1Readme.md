# CampusLibrary — Part 5: Client without active Auth

Teaching project for a modular, DDD-oriented ASP.NET Core Web API and a Blazor SSR client.

German version: [1Readme-ger.md](1Readme-ger.md)

## Current status

Part 5 adds a real web client to the modular CampusLibrary API from Part 4.

Part 5 contains:

```text
Readers
Catalog
Loans
CampusLibraryClient
```

The API modules remain the owners of the business rules. The client does not reference the API core projects. It consumes the API through HTTP clients and DTOs.

Known verification state after the BookItem identity change:

```text
dotnet build
Build succeeded

dotnet test
196 total, 0 failed, 0 skipped
```

Important: The automated tests currently mainly verify the API. Pure client layout and navigation changes are verified through `dotnet build` and manual browser tests.

## Branch

```text
part-5/client-noauth
```

## Goal of Part 5

Part 5 shows how an existing modular API is consumed by a Blazor SSR client.

Real authentication and real API authorization are not part of Part 5.

Active in Part 5:

```text
Blazor SSR client
HTTP access to CampusLibraryApi
Bootstrap-based layout
readers list
catalog search
borrow book from the reader perspective
create books from the employee perspective
add book items to active books
deactivate books
show loans
show loan details
renew and return loans
central error display
DevIdentity as simulated UI perspective
```

Inactive in Part 5:

```text
real registration
real login
real logout session against IdentityAccessServer
access token forwarding to the API
protected API calls
policy-based API authorization
reader provisioning from a token
reader creation in the UI
```

## Why no reader creation in Part 5?

A reader should later not simply be created through an employee form in the client. The correct business flow starts with a technical user in the IdentityAccessServer.

Planned flow for later parts:

```text
1. A reader registers in the IdentityAccessServer.
2. Email is initially the username.
3. IdentityAccessServer creates a technical user with a subject.
4. CampusLibraryApi provisions a business Reader from that identity.
5. The reader completes business profile data such as first name and last name.
```

For this reason, `Create reader` is intentionally not exposed in the Part 5 UI. Readers come from seed and test data in Part 5.

## Project structure

```text
CampusLibraryApi
CampusLibraryApi_1_Web
CampusLibraryApi_2_BuildingBlocks
CampusLibraryApi_3_Core_Readers
CampusLibraryApi_3_Core_Catalog
CampusLibraryApi_3_Core_Loan
CampusLibraryApi_4_Infrastructure
CampusLibraryApiTest
CampusLibraryClient
```

## CampusLibraryClient

The client is a Blazor Server-Side Rendering application.

Important concepts:

```text
Razor components
Interactive Server Render Mode for interactive pages
module-specific API clients
DTOs for API transport
Result<T> for client-side success/failure handling
ProblemDetails-based error display
Bootstrap utilities instead of custom layout CSS
DevIdentity for reader/employee perspective
prepared but inactive AuthN/AuthZ infrastructure
```

Main client folders:

```text
CampusLibraryClient
├─ Api
│  ├─ Clients
│  ├─ Contracts
│  ├─ Dtos
│  ├─ Errors
│  └─ Auth
├─ Core
├─ Extensions
├─ Security
├─ Shared
└─ Ui
   ├─ Components
   ├─ Controllers
   ├─ Models
   └─ Pages
```

## Visible pages

```text
/                                      home page
/readers                                readers list
/catalog/books                          catalog
/catalog/books/create                   create book
/catalog/books/{bookId}/items/add       add book item
/catalog/books/{bookId}/deactivate      deactivate book
/catalog/books/{bookId}/borrow          borrow book
/loans                                  loans from employee perspective
/loans/{loanId}                         loan details
/my/loans                               loans of the current reader
/logout                                 demo logout page
/access-denied                          prepared error page
/error                                  technical error page
```

## DevIdentity in Part 5

Part 5 does not use real authentication. To let the UI distinguish between reader and employee perspectives, the client uses DevIdentity.

Example:

```json
{
  "Features": {
    "AuthNEnabled": false,
    "DevIdentityEnabled": true,
    "ApiAccessTokenEnabled": false,
    "AuthZEnabled": false
  },
  "DevIdentity": {
    "ActiveProfile": "EmployeeAdmin",
    "Profiles": {
      "ReaderRita": {
        "IsAuthenticated": true,
        "AccountType": "reader",
        "ReaderId": "00000099-0000-0000-0000-000000000000",
        "DisplayName": "Rita Reader",
        "Email": "r.reader@library.local"
      },
      "EmployeeAdmin": {
        "IsAuthenticated": true,
        "AccountType": "employee",
        "ReaderId": null,
        "DisplayName": "Admin",
        "Email": "admin@mail.local"
      }
    }
  }
}
```

DevIdentity is not security. It is only a teaching aid for the UI.

## Navigation and layout

The client uses a horizontal Bootstrap menu. The active menu item is highlighted through Bootstrap nav links.

The menu depends on the simulated perspective:

```text
Reader:
Home | Catalog | Loans | Logout

Employee:
Home | Catalog | Readers | Loans | Logout
```

The home page contains a normal heading and current messages. The layout uses Bootstrap classes such as `container-fluid`, `px-4`, `navbar`, `nav-pills`, `table`, `card`, `row`, `col-*`, `badge` and `btn`.

Custom CSS remains limited to Blazor-specific validation and error display.

## API clients per module

The client uses one API client abstraction per functional area:

```text
IReaderClient -> ReaderClient
IBookClient   -> BookClient
ILoanClient   -> LoanClient
```

These clients call the existing CampusLibraryApi routes:

```text
/camplib/v1/readers
/camplib/v1/books
/camplib/v1/loans
```

The API clients are registered through:

```text
AddCampusLibraryClients(...)
```

The base URL is configured in `appsettings.json`:

```json
{
  "CampusLibraryApi": {
    "BaseUrl": "https://localhost:8010/"
  }
}
```

## Readers

The Readers page displays readers returned by the API.

Currently displayed:

```text
Name
Email
Status
```

`Subject` is not displayed in the UI. It is a technical identity and belongs to the later AuthN/AuthZ topic.

Readers are not created in the Part 5 UI. This is an intentional teaching decision because the later correct flow uses IdentityAccessServer, subject and provisioning.

## Catalog

The catalog is visible for readers and employees.

The catalog table uses this business structure:

```text
Action | Title | Authors | ISBN | Items | Status
```

The `Title` column contains title and subtitle together. The subtitle qualifies the title and is therefore not shown as a distant separate column.

The `Items` column shows:

```text
borrowed / total
```

Employee catalog functions:

```text
create book
add item to active book
deactivate active book
show active and inactive books
```

Reader catalog functions:

```text
search books
borrow a book if at least one item is actually available
```

## BookItem and inventory number

The API no longer has a separate `InventoryNumber` property.

Technically:

```text
BookItem.Id uniquely identifies an item.
```

The UI still displays this id using the business label `Inventory number`.

Therefore:

```text
technical:  BookItemId
business/UI: Inventory number
```

## Loans

The loan pages show loans and details.

Important pages:

```text
/loans          employee perspective: borrowed loans
/my/loans       reader perspective: own loans
/loans/{id}     loan details
```

Renew and return are executed in the detail view, not directly in the overview. The detail view shows book data, reader data and loan data.

`BookIsActive` and `IsAvailableForLoan` may still exist in the DTO, but they are not central business information in the regular loan detail page. Existing loans should not be made confusing by technical availability flags.

## Error handling

API errors are handled centrally.

Important types and components:

```text
BaseApiClient<TClient>
ApiError
Result<T>
ErrorAlert.razor
```

The API returns errors as `ProblemDetails`. The client maps them to `ApiError` and displays them through `ErrorAlert`.

## Planned continuation

```text
Part 6: client AuthN with login/logout against IdentityAccessServer
Part 7: AuthN/AuthZ in CampusLibraryApi, reader provisioning through token
Part 8: protected API access from the client using an access token
```

Planned reader flow:

```text
POST /camplib/v1/readers/me/provision
- API reads subject and email from the access token.
- No subject in the body.

POST /camplib/v1/readers/me/profile
- Client only sends business profile data such as first name and last name.
```
