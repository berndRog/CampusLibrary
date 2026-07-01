# Part 5 update: German UI texts and borrow availability

This project version contains two UI-focused changes for Part 5.

## German UI texts

User-visible UI labels, headings, navigation items, table headers and buttons are now German.

API and validation error messages intentionally remain English because many errors come from the CampusLibraryApi and the API is not changed in Part 5.

## Borrow button availability

The catalog no longer shows the `Ausleihen` button just because the catalog item status is available.

The UI computes effective availability by combining:

- Book details and book items from the catalog API
- Currently borrowed loans from the loan API

A book can only be borrowed when at least one book item is not currently borrowed.

The displayed count is now interpreted as:

```text
Ausgeliehen / Gesamt
```

Examples:

```text
2 / 2 -> no available item -> no Ausleihen button
1 / 2 -> one available item -> Ausleihen button
0 / 2 -> two available items -> Ausleihen button
```

## DevIdentity

The demo reader id is aligned with the agreed test-data convention:

```text
00000099-0000-0000-0000-000000000000
```

This id must exist as a Reader in the CampusLibraryApi data.
