# API-Dokumentation

Dieses Dokument beschreibt die öffentliche HTTP-API der aktuellen `CampusLibraryApi`.

Swagger/OpenAPI ist die verbindliche technische API-Beschreibung. Dieses Dokument ergänzt Swagger um eine didaktische Übersicht für Studierende.

## Base URL

Im Development-Modus hört die API auf:

```text
https://localhost:8010
http://localhost:8012
```

Der aktuelle API-Präfix lautet:

```text
/camplib/v1
```

Die Version ist Bestandteil der URL.

Beispiel:

```http
GET /camplib/v1/readers
```

## Swagger

Swagger UI ist im Development-Modus erreichbar unter:

```text
https://localhost:8010/swagger
```

Das generierte OpenAPI-Dokument beschreibt:

```text
Routen
Request Bodies
Response Bodies
Statuscodes
ProblemDetails-Antworten
DTO-Schemas
```

Die aktuelle API enthält drei Endpoint-Gruppen:

```text
Readers
Authors
Books
```

## Manuelle HTTP-Dateien

Für manuelle API-Tests wird die Datenbank zuerst gelöscht oder zurückgesetzt.

Danach werden die HTTP-Dateien in dieser Reihenfolge ausgeführt:

```text
1. Authors.http
2. Books.http
3. Readers.http
```

`Seed.cs` definiert die stabilen IDs.

Die `.http`-Dateien erzeugen diese Datensätze über die öffentliche API.

```text
Authors.http erzeugt die Authors.
Books.http erzeugt die Books, verwendet die vorhandenen Authors, ordnet Authors zu Books zu und fügt BookItems hinzu.
Readers.http erzeugt oder prüft Reader-Daten.
```

Dadurch bleiben manuelle API-Tests reproduzierbar und hängen nicht von verstecktem Datenbankzustand ab.

## Module

Die aktuelle API enthält zwei fachliche Module:

```text
Readers-Modul
Catalog-Modul
```

Das Readers-Modul verwaltet Bibliotheksnutzer.

Das Catalog-Modul verwaltet Bücher, Authors und physische Buchexemplare.

# Readers-Modul

Ein Reader repräsentiert einen fachlichen Bibliotheksnutzer der CampusLibrary.

Ein Reader ist nicht dasselbe wie ein technisches Benutzerkonto.

Die technische Identitätsreferenz wird dargestellt durch:

```text
Subject
```

## Reader-Routen

### Alle Reader abrufen

```http
GET /camplib/v1/readers
```

Gibt alle Reader zurück.

Erfolgreiche Antwort:

```http
200 OK
```

Beispiel für den Response Body:

```json
[
  {
    "id": "00000001-0000-0000-0000-000000000000",
    "subject": "a00090ad-d9df-486a-8757-4a649e26a54e",
    "firstname": "Erika",
    "lastname": "Mustermann",
    "email": "erika.mustermann@t-online.de",
    "addressDto": {
      "street": "Hauptstr. 23",
      "postalCode": "29556",
      "city": "Suderburg",
      "country": "DE"
    }
  }
]
```

Mögliche Fehlerantworten:

```http
400 Bad Request
401 Unauthorized
403 Forbidden
```

### Einen Reader anhand der ID abrufen

```http
GET /camplib/v1/readers/{id}
```

Beispiel:

```http
GET /camplib/v1/readers/00000001-0000-0000-0000-000000000000
```

Erfolgreiche Antwort:

```http
200 OK
```

Mögliche Fehlerantworten:

```http
401 Unauthorized
403 Forbidden
404 Not Found
```

### Einen Reader anhand der Email abrufen

```http
GET /camplib/v1/readers/email?email={email}
```

Beispiel:

```http
GET /camplib/v1/readers/email?email=erika.mustermann@t-online.de
```

Erfolgreiche Antwort:

```http
200 OK
```

Mögliche Fehlerantworten:

```http
400 Bad Request
401 Unauthorized
403 Forbidden
404 Not Found
```

### Reader anlegen

```http
POST /camplib/v1/readers
```

Request Body:

```json
{
  "firstname": "Edgar",
  "lastname": "Engel",
  "email": "e.engel@freenet.de",
  "addressDto": {
    "street": "Am Markt 14",
    "postalCode": "04109",
    "city": "Leipzig",
    "country": "DE"
  },
  "subject": "70000000-0007-0000-0000-000000000000",
  "id": "00000007-0000-0000-0000-000000000000"
}
```

Erfolgreiche Antwort:

```http
201 Created
```

Mögliche Fehlerantworten:

```http
400 Bad Request
401 Unauthorized
403 Forbidden
409 Conflict
```

### ReaderCreateDto

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

`Id` ist optional.

Bei normaler API-Nutzung kann die ID weggelassen werden. Der UseCase erzeugt dann eine neue ID.

Für Lehre, Tests oder deterministische Seed-Daten darf die ID mitgegeben werden.

### Reader aktualisieren

```http
PUT /camplib/v1/readers/{id}
```

Beispiel:

```http
PUT /camplib/v1/readers/00000001-0000-0000-0000-000000000000
```

Request Body, um nur den Nachnamen zu ändern:

```json
{
  "lastname": "Meier",
  "email": null,
  "addressDto": null
}
```

Erfolgreiche Antwort:

```http
200 OK
```

Mögliche Fehlerantworten:

```http
400 Bad Request
401 Unauthorized
403 Forbidden
404 Not Found
409 Conflict
```

### ReaderUpdateDto

```csharp
public sealed record ReaderUpdateDto(
   string? Lastname,
   string? Email,
   AddressDto? AddressDto
);
```

Alle Eigenschaften sind bewusst nullable.

Die Bedeutung von `null` ist:

```text
Lastname = null   -> keine Änderung
Email = null      -> keine Änderung
AddressDto = null -> keine Änderung
```

`Firstname` ist absichtlich nicht Bestandteil des Update-DTOs.

`Subject` ist ebenfalls absichtlich nicht Bestandteil des Update-DTOs.

Die technische Identitätsreferenz wird durch normale Profiländerungen nicht verändert.

### Reader löschen

```http
DELETE /camplib/v1/readers/{id}
```

Beispiel:

```http
DELETE /camplib/v1/readers/00000003-0000-0000-0000-000000000000
```

Erfolgreiche Antwort:

```http
204 No Content
```

Mögliche Fehlerantworten:

```http
400 Bad Request
401 Unauthorized
403 Forbidden
404 Not Found
```

### AddressDto

```csharp
public sealed record AddressDto(
   string Street,
   string PostalCode,
   string City,
   string? Country
);
```

Die Adresse wird in der Application Layer in ein `AddressVo` umgewandelt.

Das Value Object führt seine eigene Validierung durch.

# Catalog-Modul

Das Catalog-Modul verwaltet Bücher, Authors und physische Buchexemplare.

Es enthält:

```text
Book
Author
BookItem
IsbnVo
```

Ein `Book` repräsentiert das bibliografische Werk.

Ein `Author` repräsentiert eine Person, die Books zugeordnet werden kann.

Ein `BookItem` repräsentiert ein physisches Exemplar eines Books.

Ein `IsbnVo` schützt die ISBN-Validierungsregeln.

# Author-Routen

## Alle aktiven Authors abrufen

```http
GET /camplib/v1/authors
```

Gibt alle aktiven Authors zurück.

Erfolgreiche Antwort:

```http
200 OK
```

Beispiel für den Response Body:

```json
[
  {
    "id": "a0000001-0000-0000-0000-000000000000",
    "firstname": "Robert C.",
    "lastname": "Martin",
    "displayName": "Robert C. Martin",
    "isActive": true
  }
]
```

Mögliche Fehlerantworten:

```http
400 Bad Request
401 Unauthorized
403 Forbidden
```

## Einen aktiven Author anhand der ID abrufen

```http
GET /camplib/v1/authors/{id}
```

Beispiel:

```http
GET /camplib/v1/authors/a0000001-0000-0000-0000-000000000000
```

Erfolgreiche Antwort:

```http
200 OK
```

Mögliche Fehlerantworten:

```http
400 Bad Request
401 Unauthorized
403 Forbidden
404 Not Found
```

Inaktive Authors werden über diesen normalen Lese-Endpunkt nicht zurückgegeben.

## Aktive Authors suchen

```http
GET /camplib/v1/authors/search?searchText={searchText}
```

Beispiel:

```http
GET /camplib/v1/authors/search?searchText=Martin
```

Sucht aktive Authors anhand des Nachnamens.

Erfolgreiche Antwort:

```http
200 OK
```

Beispiel für den Response Body:

```json
[
  {
    "id": "a0000001-0000-0000-0000-000000000000",
    "firstname": "Robert C.",
    "lastname": "Martin",
    "displayName": "Robert C. Martin",
    "isActive": true
  }
]
```

Mögliche Fehlerantworten:

```http
400 Bad Request
401 Unauthorized
403 Forbidden
```

Wenn kein Author passt, liefert der Endpunkt eine leere Liste.

## Author anlegen

```http
POST /camplib/v1/authors
```

Request Body:

```json
{
  "firstname": "Robert C.",
  "lastname": "Martin",
  "id": "a0000001-0000-0000-0000-000000000000"
}
```

Erfolgreiche Antwort:

```http
201 Created
```

Beispiel für den Response Body:

```json
{
  "id": "a0000001-0000-0000-0000-000000000000",
  "firstname": "Robert C.",
  "lastname": "Martin",
  "displayName": "Robert C. Martin",
  "isActive": true
}
```

Mögliche Fehlerantworten:

```http
400 Bad Request
401 Unauthorized
403 Forbidden
409 Conflict
```

## AuthorCreateDto

```csharp
public sealed record AuthorCreateDto(
   string Firstname,
   string Lastname,
   string? Id
);
```

`Id` ist optional.

Bei normaler API-Nutzung kann die ID weggelassen werden. Der UseCase erzeugt dann eine neue ID.

Für Lehre, Tests oder deterministische Seed-Daten darf die ID mitgegeben werden.

## Author deaktivieren

```http
PATCH /camplib/v1/authors/{id}/deactivate
```

Beispiel:

```http
PATCH /camplib/v1/authors/a0000005-0000-0000-0000-000000000000/deactivate
```

Erfolgreiche Antwort:

```http
200 OK
```

Beispiel für den Response Body:

```json
{
  "id": "a0000005-0000-0000-0000-000000000000",
  "firstname": "Kent",
  "lastname": "Beck",
  "displayName": "Kent Beck",
  "isActive": false
}
```

Mögliche Fehlerantworten:

```http
400 Bad Request
401 Unauthorized
403 Forbidden
404 Not Found
```

Deactivate ist nicht dasselbe wie Delete.

Der Author bleibt in der Datenbank gespeichert.

Normale ReadModels entscheiden, ob inaktive Authors sichtbar sind.

## AuthorDto

```csharp
public sealed record AuthorDto(
   Guid Id,
   string Firstname,
   string Lastname,
   string DisplayName,
   bool IsActive
);
```

`AuthorDto` wird sowohl für Leseantworten als auch für Antworten von schreibenden UseCases verwendet.

# Book-Routen

## Alle aktiven Books abrufen

```http
GET /camplib/v1/books
```

Gibt alle aktiven Books als kompakte Listeneinträge zurück.

Erfolgreiche Antwort:

```http
200 OK
```

Beispiel für den Response Body:

```json
[
  {
    "id": "b0000001-0000-0000-0000-000000000000",
    "title": "Clean Code",
    "subtitle": "A Handbook of Agile Software Craftsmanship",
    "isbn": "9780132350884",
    "authors": [
      "Robert C. Martin"
    ],
    "totalBookItems": 2,
    "availableBookItems": 2
  }
]
```

Mögliche Fehlerantworten:

```http
400 Bad Request
401 Unauthorized
403 Forbidden
```

## Ein aktives Book anhand der ID abrufen

```http
GET /camplib/v1/books/{id}
```

Beispiel:

```http
GET /camplib/v1/books/b0000001-0000-0000-0000-000000000000
```

Gibt eine Detaildarstellung eines Books zurück.

Erfolgreiche Antwort:

```http
200 OK
```

Beispiel für den Response Body:

```json
{
  "id": "b0000001-0000-0000-0000-000000000000",
  "title": "Clean Code",
  "subtitle": "A Handbook of Agile Software Craftsmanship",
  "isbn": "9780132350884",
  "authors": [
    {
      "id": "a0000001-0000-0000-0000-000000000000",
      "firstname": "Robert C.",
      "lastname": "Martin",
      "displayName": "Robert C. Martin",
      "isActive": true
    }
  ],
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

Mögliche Fehlerantworten:

```http
400 Bad Request
401 Unauthorized
403 Forbidden
404 Not Found
```

Inaktive Books werden über diesen normalen Lese-Endpunkt nicht zurückgegeben.

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

Sucht aktive Books nach genau einem Suchkriterium.

Unterstützte Suchfelder:

```text
Title
AuthorLastName
Isbn
```

`AuthorLastName` sucht ausschließlich im Nachnamen der zugeordneten Authors.

Der Vorname wird nicht durchsucht. Dadurch werden zufällige Treffer vermieden, zum Beispiel `Martin` als Vorname bei `Martin Fowler`, wenn eigentlich nach dem Nachnamen `Martin` gesucht wird.

Erfolgreiche Antwort:

```http
200 OK
```

Beispiel für den Response Body:

```json
[
  {
    "id": "b0000001-0000-0000-0000-000000000000",
    "title": "Clean Code",
    "subtitle": "A Handbook of Agile Software Craftsmanship",
    "isbn": "9780132350884",
    "authors": [
      "Robert C. Martin"
    ],
    "totalBookItems": 2,
    "availableBookItems": 2
  }
]
```

Mögliche Fehlerantworten:

```http
400 Bad Request
401 Unauthorized
403 Forbidden
```

Wenn kein Book zum Suchkriterium passt, liefert der Endpunkt eine leere Liste.

## Aktive Books anhand einer Author-ID abrufen

```http
GET /camplib/v1/books/by-author/{authorId}
```

Beispiel:

```http
GET /camplib/v1/books/by-author/a0000001-0000-0000-0000-000000000000
```

Gibt alle aktiven Books zurück, die einem bestimmten Author zugeordnet sind.

Erfolgreiche Antwort:

```http
200 OK
```

Beispiel für den Response Body:

```json
[
  {
    "id": "b0000001-0000-0000-0000-000000000000",
    "title": "Clean Code",
    "subtitle": "A Handbook of Agile Software Craftsmanship",
    "isbn": "9780132350884",
    "authors": [
      "Robert C. Martin"
    ],
    "totalBookItems": 2,
    "availableBookItems": 2
  }
]
```

Mögliche Fehlerantworten:

```http
400 Bad Request
401 Unauthorized
403 Forbidden
```

Wenn dem Author kein aktives Book zugeordnet ist, liefert der Endpunkt eine leere Liste.

## Book anlegen

```http
POST /camplib/v1/books
```

Request Body:

```json
{
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

Beispiel für den Response Body:

```json
{
  "id": "b0000001-0000-0000-0000-000000000000",
  "title": "Clean Code",
  "subtitle": "A Handbook of Agile Software Craftsmanship",
  "isbn": "9780132350884",
  "bookItemCount": 0,
  "isActive": true
}
```

Mögliche Fehlerantworten:

```http
400 Bad Request
401 Unauthorized
403 Forbidden
409 Conflict
```

## BookCreateDto

```csharp
public sealed record BookCreateDto(
   string Title,
   string? Subtitle,
   string Isbn,
   string? Id
);
```

`Id` ist optional.

Bei normaler API-Nutzung kann die ID weggelassen werden. Der UseCase erzeugt dann eine neue ID.

Für Lehre, Tests oder deterministische Seed-Daten darf die ID mitgegeben werden.

Die ISBN wird durch `IsbnVo` validiert.

## Physisches BookItem hinzufügen

```http
POST /camplib/v1/books/{bookId}/items
```

Beispiel:

```http
POST /camplib/v1/books/b0000001-0000-0000-0000-000000000000/items
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

Beispiel für den Response Body:

```json
{
  "id": "be000001-0000-0000-0000-000000000000",
  "bookId": "b0000001-0000-0000-0000-000000000000",
  "inventoryNumber": "CL-BOOK-0001",
  "status": "Available"
}
```

Mögliche Fehlerantworten:

```http
400 Bad Request
401 Unauthorized
403 Forbidden
404 Not Found
409 Conflict
```

Ein neu hinzugefügtes BookItem startet mit dem Status:

```text
Available
```

Das Enum kann in der Datenbank weiterhin als Integer gespeichert werden.

In der JSON-API wird es als String serialisiert, weil die API Enum-String-Serialisierung verwendet.

## BookItemAddDto

```csharp
public sealed record BookItemAddDto(
   string InventoryNumber,
   string? Id
);
```

`InventoryNumber` muss eindeutig sein.

`Id` ist optional.

Bei normaler API-Nutzung kann die ID weggelassen werden. Der UseCase erzeugt dann eine neue ID.

Für Lehre, Tests oder deterministische Seed-Daten darf die ID mitgegeben werden.

## Author einem Book zuordnen

```http
POST /camplib/v1/books/{bookId}/authors
```

Beispiel:

```http
POST /camplib/v1/books/b0000001-0000-0000-0000-000000000000/authors
```

Request Body:

```json
{
  "authorId": "a0000001-0000-0000-0000-000000000000"
}
```

Erfolgreiche Antwort:

```http
200 OK
```

Beispiel für den Response Body:

```json
{
  "id": "b0000001-0000-0000-0000-000000000000",
  "title": "Clean Code",
  "subtitle": "A Handbook of Agile Software Craftsmanship",
  "isbn": "9780132350884",
  "bookItemCount": 0,
  "isActive": true
}
```

Mögliche Fehlerantworten:

```http
400 Bad Request
401 Unauthorized
403 Forbidden
404 Not Found
409 Conflict
```

Die `bookId` kommt aus der Route.

Die `authorId` kommt aus dem Request Body.

Es gibt keine `BookAuthorId`.

Die Join-Tabelle ist ein Infrastructure-Detail.

## BookAssignAuthorDto

```csharp
public sealed record BookAssignAuthorDto(
   Guid AuthorId
);
```

Dieses DTO enthält nur die Author-ID, weil die Book-ID bereits Bestandteil der Route ist.

## Book deaktivieren

```http
PATCH /camplib/v1/books/{bookId}/deactivate
```

Beispiel:

```http
PATCH /camplib/v1/books/b0000004-0000-0000-0000-000000000000/deactivate
```

Erfolgreiche Antwort:

```http
200 OK
```

Beispiel für den Response Body:

```json
{
  "id": "b0000004-0000-0000-0000-000000000000",
  "title": "Design Patterns",
  "subtitle": "Elements of Reusable Object-Oriented Software",
  "isbn": "9780201633610",
  "bookItemCount": 2,
  "isActive": false
}
```

Mögliche Fehlerantworten:

```http
400 Bad Request
401 Unauthorized
403 Forbidden
404 Not Found
```

Deactivate ist nicht dasselbe wie Delete.

Das Book bleibt in der Datenbank gespeichert.

Normale ReadModels entscheiden, ob inaktive Books sichtbar sind.

## BookSearchField

```csharp
public enum BookSearchField {
   Title = 1,
   AuthorLastName = 2,
   Isbn = 3
}
```

`AuthorLastName` ist das katalogorientierte Suchfeld für Authors.

Es sucht Books anhand des Nachnamens der zugeordneten Authors.

## BookDto

```csharp
public sealed record BookDto(
   Guid Id,
   string Title,
   string? Subtitle,
   string Isbn,
   int BookItemCount,
   bool IsActive
);
```

`BookDto` wird hauptsächlich als Ergebnis von schreibenden Book-UseCases verwendet.

## BookListItemDto

```csharp
public sealed record BookListItemDto(
   Guid Id,
   string Title,
   string? Subtitle,
   string Isbn,
   IReadOnlyList<string> Authors,
   int TotalBookItems,
   int AvailableBookItems
);
```

`BookListItemDto` wird für Listen und Suchergebnisse verwendet.

Es ist für Katalogübersichten optimiert.

## BookDetailDto

```csharp
public sealed record BookDetailDto(
   Guid Id,
   string Title,
   string? Subtitle,
   string Isbn,
   IReadOnlyList<AuthorDto> Authors,
   IReadOnlyList<BookItemDto> BookItems,
   int TotalBookItems,
   int AvailableBookItems,
   bool IsActive,
   DateTime CreatedAt,
   DateTime UpdatedAt
);
```

`BookDetailDto` wird für die Detailansicht eines Books verwendet.

Es enthält mehr Informationen als `BookListItemDto`.

## BookItemDto

```csharp
public sealed record BookItemDto(
   Guid Id,
   Guid BookId,
   string InventoryNumber,
   BookItemStatus Status
);
```

`BookItemDto` repräsentiert ein physisches Buchexemplar.

## BookItemStatus

```csharp
public enum BookItemStatus {
   Available = 1,
   Unavailable = 2,
   Lost = 3,
   Damaged = 4
}
```

Die fachliche Bedeutung bleibt über die Enum-Namen sichtbar.

Die Datenbank kann das Enum weiterhin als Integer speichern.

Die JSON-API serialisiert Enum-Werte als Strings.

Beispiel:

```json
{
  "status": "Available"
}
```

# Fehlerantworten

Die API verwendet `ProblemDetails` für Fehlerantworten.

Beispielstruktur:

```json
{
  "type": "https://httpstatuses.com/404",
  "title": "Catalog: Author NotFound",
  "status": 404,
  "detail": "The author was not found.",
  "instance": "/camplib/v1/books/b0000001-0000-0000-0000-000000000000/authors",
  "traceId": "..."
}
```

Typische Kategorien:

```text
400 Bad Request   -> ungültige Eingabe
401 Unauthorized  -> Authentifizierung erforderlich
403 Forbidden     -> Zugriff verweigert
404 Not Found     -> Ressource nicht gefunden
409 Conflict      -> doppelte oder widersprüchliche Ressource
```

Beispiele:

```text
409 Conflict -> doppelte Reader-Email
409 Conflict -> doppeltes Reader-Subject
409 Conflict -> doppelte Book-ISBN
409 Conflict -> doppelter Author-Name
409 Conflict -> doppelte InventoryNumber
409 Conflict -> Author ist dem Book bereits zugeordnet
```

# Lese- und Schreibseite

Die Controller verwenden unterschiedliche Application Ports für Lese- und Schreibverhalten.

Readers:

```text
IReaderReadModel
IReaderUseCases
```

Authors:

```text
IAuthorReadModel
IAuthorUseCases
```

Books:

```text
IBookReadModel
IBookUseCases
```

Die Leseseite wird für Queries verwendet:

```text
GET /readers
GET /readers/{id}
GET /readers/email

GET /authors
GET /authors/{id}
GET /authors/search

GET /books
GET /books/{id}
GET /books/search
GET /books/by-author/{authorId}
```

Die Schreibseite wird für Commands verwendet:

```text
POST   /readers
PUT    /readers/{id}
DELETE /readers/{id}

POST  /authors
PATCH /authors/{id}/deactivate

POST  /books
POST  /books/{bookId}/items
POST  /books/{bookId}/authors
PATCH /books/{bookId}/deactivate
```

Diese Trennung unterstützt ein klares Lehrmodell:

```text
Queries lesen DTOs.
Commands verändern Aggregates.
Repositories arbeiten mit Domain-Objekten.
ReadModels liefern DTOs direkt zurück.
```

# Wichtige Designentscheidungen

## Book zu BookItem

Ein Book kann mehrere physische BookItems besitzen.

Das ist eine 1:n-Beziehung:

```text
Book 1 --- n BookItem
```

Ein BookItem wird über das `Book`-Aggregate hinzugefügt:

```text
POST /camplib/v1/books/{bookId}/items
```

## Book zu Author

Ein Book kann mehrere Authors haben.

Ein Author kann mehreren Books zugeordnet sein.

Das ist eine m:n-Beziehung:

```text
Book n --- m Author
```

Die API veröffentlicht die Zuordnung als:

```text
POST /camplib/v1/books/{bookId}/authors
```

Der Request Body enthält nur die `authorId`.

Die technische Join-Tabelle wird nicht als API-Ressource veröffentlicht.

## Katalogsuche nach Author-Nachname

Für die Katalogsuche ist der Nachname des Authors das fachlich relevante Suchkriterium.

Der Vorname wird nicht durchsucht.

Dadurch werden zufällige Treffer vermieden.

Beispiel:

```text
AuthorLastName = Martin -> Clean Code
AuthorLastName = Fowler -> Refactoring und Design Patterns
```

## Deactivate statt Delete im Catalog

Books und Authors werden nicht physisch gelöscht.

Sie werden deaktiviert.

```text
IsActive = false
```

Repositories dürfen sie weiterhin laden.

ReadModels entscheiden, ob inaktive Daten sichtbar sind.

Normale Kataloglisten und Suchen liefern nur aktive Books und Authors.

# Didaktische Ziele

Diese API soll folgende Themen demonstrieren:

```text
REST-artige Endpunkte
API-Versionierung
Swagger/OpenAPI-Dokumentation
DTOs als API-Verträge
Result-basiertes Fehlerhandling
ProblemDetails-Antworten
Semantik partieller Updates
Trennung von Lese- und Schreibpfaden
Controller Tests mit WebApplicationFactory
1:n-Beziehung innerhalb eines Aggregates
m:n-Beziehung über Infrastructure-Mapping
Katalogsuche nach Author-Nachname
Deactivate statt Delete
modulspezifische ReadModels und UseCases
```

Swagger soll für die technische Exploration verwendet werden.

Dieses Dokument soll für die konzeptionelle Erklärung verwendet werden.

```
```
