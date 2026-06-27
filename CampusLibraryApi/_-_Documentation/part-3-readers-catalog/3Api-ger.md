# API-Dokumentation — Teil 3

Dieses Dokument beschreibt die öffentliche HTTP-API von Teil 3 der `CampusLibraryApi`.

Swagger/OpenAPI ist die maßgebliche technische API-Beschreibung. Dieses Dokument bietet eine didaktische Übersicht für Studierende.

## Base URL

Development-URLs:

```text
https://localhost:8010
http://localhost:8012
```

API-Präfix:

```text
/camplib/v1
```

Endpoint-Gruppen:

```text
Readers
Books
```

Swagger UI ist im Development-Modus verfügbar:

```text
https://localhost:8010/swagger
```

## Manuelle HTTP-Dateien

Für manuelle API-Tests sollte die Datenbank zuerst zurückgesetzt oder gelöscht werden.

Empfohlene Reihenfolge:

```text
1. Readers.http
2. Books.http
```

# Readers API

Ein Reader repräsentiert einen fachlichen Bibliotheksnutzer.

Die technische Identitätsreferenz wird durch `Subject` repräsentiert.

## Alle aktiven Reader abrufen

```http
GET /camplib/v1/readers
```

Antwort:

```http
200 OK
```

## Alle Reader inklusive inaktiver Reader abrufen

```http
GET /camplib/v1/readers/with-inactive
```

Antwort:

```http
200 OK
```

## Einen aktiven Reader nach Id abrufen

```http
GET /camplib/v1/readers/{id}
```

Mögliche Antworten:

```http
200 OK
404 Not Found
```

## Einen Reader nach Id inklusive inaktiver Reader abrufen

```http
GET /camplib/v1/readers/{id}/with-inactive
```

Mögliche Antworten:

```http
200 OK
400 Bad Request
404 Not Found
```

## Einen Reader nach E-Mail abrufen

```http
GET /camplib/v1/readers/email?email={email}
```

Mögliche Antworten:

```http
200 OK
400 Bad Request
404 Not Found
```

## Reader anlegen

```http
POST /camplib/v1/readers
Content-Type: application/json
```

Beispiel:

```json
{
  "firstname": "Erika",
  "lastname": "Mustermann",
  "email": "e.mustermann@example.com",
  "addressDto": {
    "street": "Hauptstr. 12",
    "postalCode": "29556",
    "city": "Suderburg",
    "country": "DE"
  },
  "subject": "reader-001",
  "id": "10000000-0000-0000-0000-000000000000"
}
```

Mögliche Antworten:

```http
201 Created
400 Bad Request
409 Conflict
```

## Reader aktualisieren

```http
PUT /camplib/v1/readers/{id}
Content-Type: application/json
```

Mögliche Antworten:

```http
200 OK
400 Bad Request
404 Not Found
409 Conflict
```

## Reader deaktivieren

```http
DELETE /camplib/v1/readers/{id}
```

Der Endpunkt deaktiviert den Reader. Er löscht den Datensatz nicht physisch.

Mögliche Antworten:

```http
204 No Content
404 Not Found
409 Conflict
```

# Books API

Ein Book repräsentiert ein bibliografisches Werk. Ein BookItem repräsentiert ein physisches Exemplar.

In Teil 3 gibt es keine Author-API. Autoren werden in `authorsText` gespeichert.

## Alle aktiven Books abrufen

```http
GET /camplib/v1/books
```

Antwort:

```http
200 OK
```

## Ein aktives Book nach Id abrufen

```http
GET /camplib/v1/books/{id}
```

Mögliche Antworten:

```http
200 OK
404 Not Found
```

## Books suchen

```http
GET /camplib/v1/books/search?searchField=Title&searchText=Clean
GET /camplib/v1/books/search?searchField=Isbn&searchText=9780132350884
GET /camplib/v1/books/search?searchField=AuthorLastName&searchText=Martin
```

Unterstützte Suchfelder:

```text
Title
Isbn
AuthorLastName
```

Die API akzeptiert jeweils ein Suchfeld. Wenn kein Book passt, liefert die API `200 OK` mit einer leeren Liste.

## Book anlegen

```http
POST /camplib/v1/books
Content-Type: application/json
```

Beispiel:

```json
{
  "authorsText": "Robert C. Martin",
  "title": "Clean Code",
  "subtitle": "A Handbook of Agile Software Craftsmanship",
  "isbn": "9780132350884",
  "id": "b0000001-0000-0000-0000-000000000000"
}
```

Mögliche Antworten:

```http
201 Created
400 Bad Request
409 Conflict
```

## BookItem hinzufügen

```http
POST /camplib/v1/books/{bookId}/items
Content-Type: application/json
```

Beispiel:

```json
{
  "inventoryNumber": "CL-BOOK-0001",
  "id": "be000001-0000-0000-0000-000000000000"
}
```

Mögliche Antworten:

```http
200 OK
400 Bad Request
404 Not Found
409 Conflict
```

## Book deaktivieren

```http
PATCH /camplib/v1/books/{bookId}/deactivate
```

Mögliche Antworten:

```http
200 OK
404 Not Found
409 Conflict
```

Deaktivierte Books werden aus normalen Book-Abfragen und Suchergebnissen ausgeblendet.

## Status- und Deaktivierungskonzepte

```text
Reader und Book verwenden IsActive.
BookItem verwendet BookItemStatus.
```

Diese Unterscheidung bereitet das Modell auf Teil 4 vor, in dem auch Loan einen Status und kein `IsActive` verwendet.
