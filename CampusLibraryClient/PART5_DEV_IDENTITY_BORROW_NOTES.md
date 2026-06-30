# Part 5 update: DevIdentity and reader borrow workflow

This update keeps the CampusLibraryClient without active AuthN/AuthZ.

## Why DevIdentity?

Part 5 has no real login yet, but the UI already needs to distinguish two
perspectives:

- reader: search books, borrow available book items, view own loans
- employee: view readers, view all loans, operate library desk actions

`DevIdentity` is a temporary teaching aid. It is configured in `appsettings.json`
and replaced in Part 6 by the real OIDC/Claims-based identity.

## Important

DevIdentity is not security. CampusLibraryApi is still called without a bearer
token. Real authorization belongs to later parts.

## New pages

- `/catalog/books`: catalog with Borrow button for readers
- `/catalog/books/{bookId}/borrow`: detail/confirmation page before borrowing
- `/my/loans`: reader view, filtered to the current ReaderId
- `/loans`: employee view, all active loans

## Switch demo perspective

In `appsettings.json`:

```json
"DevIdentity": {
  "IsAuthenticated": true,
  "AccountType": "reader",
  "ReaderId": "00000000-0001-0000-0000-000000000001",
  "DisplayName": "Rita Reader",
  "Email": "r.reader@library.local"
}
```

Change `AccountType` to `employee` to test the employee navigation.
