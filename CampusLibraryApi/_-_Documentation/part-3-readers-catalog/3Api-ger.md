# API-Dokumentation

Dieses Dokument beschreibt die öffentliche HTTP-API der aktuellen `CampusLibraryApi`.

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

Die aktuelle API enthält zwei Endpoint-Gruppen:

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

Reihenfolge:

```text
1. Books.http
2. Readers.http
```

# Readers API

Ein Reader repräsentiert einen fachlichen Bibliotheksnutzer.

Die technische Identitätsreferenz wird durch `Subject` repräsentiert.

## Alle aktiven Reader abrufen

```http
GET /camplib/v1/readers
```

Erfolgreiche Antwort:

```http
200 OK
```

## Alle Reader inklusive inaktiver Reader abrufen

```http
GET /camplib/v1/readers/with-inactive
```

Erfolgreiche Antwort:

```http
200 OK
```

## Einen aktiven Reader nach Id abrufen

```http
GET /camplib/v1/readers/{id}
```

Beispiel:

```http
GET /camplib/v1/readers/10000000-0000-0000-0000-000000000000
```

Mögliche Antworten:

```http
200 OK
401 Unauthorized
403 Forbidden
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
401 Unauthorized
403 Forbidden
404 Not Found
```

## Einen Reader nach E-Mail abrufen

```http
GET /camplib/v1/readers/email?email={email}
```

Beispiel:

```http
GET /camplib/v1/readers/email?email=e.mustermann@t-online.de
```

Mögliche Antworten:

```http
200 OK
400 Bad Request
401 Unauthorized
403 Forbidden
404 Not Found
```

## Reader anlegen

```http
POST /camplib/v1/readers
```

Request Body:

```json
{
  "firstname": "Erika",
  "lastname": "Mustermann",
  "email": "e.mustermann@t-online.de",
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
401 Unauthorized
403 Forbidden
409 Conflict
```

## Reader aktualisieren

```http
PUT /camplib/v1/readers/{id}
```

Request Body:

```json
{
  "lastname": "Meier",
  "email": "e.meier@gmx.de",
  "addressDto": {
    "street": "Neue Straße 5",
    "postalCode": "30123",
    "city": "Hannover",
    "country": "DE"
  }
}
```

Mögliche Antworten:

```http
200 OK
400 Bad Request
401 Unauthorized
403 Forbidden
404 Not Found
409 Conflict
```

## Reader deaktivieren

```http
DELETE /camplib/v1/readers/{id}
```

Erfolgreiche Antwort:

```http
204 No Content
```

Ein deaktivierter Reader ist in normalen Reader-Abfragen verborgen, bleibt aber über `with-inactive`-Endpunkte sichtbar.

# Catalog API

Die Catalog API verwaltet Books und physische BookItems.

## Alle aktiven Books abrufen

```http
GET /camplib/v1/books
```

Beispielantwort:

```json
[
  {
    "id": "b0000001-0000-0000-0000-000000000000",
    "authorsText": "Robert C. Martin",
    "title": "Clean Code",
    "subtitle": "A Handbook of Agile Software Craftsmanship",
    "isbn": "9780132350884",
    "totalBookItems": 2,
    "availableBookItems": 2
  }
]
```

## Ein aktives Book nach Id abrufen

```http
GET /camplib/v1/books/{id}
```

Beispielantwort:

```json
{
  "id": "b0000001-0000-0000-0000-000000000000",
  "authorsText": "Robert C. Martin",
  "title": "Clean Code",
  "subtitle": "A Handbook of Agile Software Craftsmanship",
  "isbn": "9780132350884",
  "bookItems": [
    {
      "id": "be000001-0000-0000-0000-000000000000",
      "bookId": "b0000001-0000-0000-0000-000000000000",
      "inventoryNumber": "CL-BOOK-0001",
      "status": "Available"
    }
  ],
  "totalBookItems": 2,
  "availableBookItems": 2,
  "isActive": true,
  "createdAt": "2025-01-01T00:00:00Z",
  "updatedAt": "2025-01-01T00:00:00Z"
}
```

## Aktive Books suchen

```http
GET /camplib/v1/books/search?searchField={searchField}&searchText={searchText}
```

Beispiele:

```http
GET /camplib/v1/books/search?searchField=Title&searchText=Clean
GET /camplib/v1/books/search?searchField=AuthorLastName&searchText=Martin
GET /camplib/v1/books/search?searchField=Isbn&searchText=9780132350884
```

Unterstützte Suchfelder:

```text
Title
AuthorLastName
Isbn
```

`AuthorLastName` durchsucht den Autorentext nach der Nachnamenregel.

```text
Robert C. Martin -> Martin
Martin Fowler -> Fowler
Kent Beck -> Beck
```

## Book anlegen

```http
POST /camplib/v1/books
```

Request Body:

```json
{
  "authorsText": "Robert C. Martin",
  "title": "Clean Code",
  "subtitle": "A Handbook of Agile Software Craftsmanship",
  "isbn": "9780132350884",
  "id": "b0000001-0000-0000-0000-000000000000"
}
```

Erfolgreiche Antwort:

```http
201 Created
```

## Physisches BookItem hinzufügen

```http
POST /camplib/v1/books/{bookId}/items
```

Request Body:

```json
{
  "inventoryNumber": "CL-BOOK-0001",
  "id": "be000001-0000-0000-0000-000000000000"
}
```

Erfolgreiche Antwort:

```http
200 OK
```

Ein neues BookItem startet mit Status `Available`.

## Book deaktivieren

```http
PATCH /camplib/v1/books/{bookId}/deactivate
```

Erfolgreiche Antwort:

```http
200 OK
```

Ein deaktiviertes Book ist in normalen Book-Lese-Endpunkten und Suchen verborgen.

# DTO-Überblick

## BookCreateDto

```csharp
public sealed record BookCreateDto(
   string? AuthorsText,
   string? Title,
   string? Subtitle,
   string? Isbn,
   string? Id
);
```

## BookSearchField

```csharp
public enum BookSearchField {
   Title = 1,
   AuthorLastName = 2,
   Isbn = 3
}
```

## Fehlerbehandlung

Die API gibt Fehler als `ProblemDetails` zurück.

Typische Statuscodes:

```text
400 Bad Request
401 Unauthorized
403 Forbidden
404 Not Found
409 Conflict
```
