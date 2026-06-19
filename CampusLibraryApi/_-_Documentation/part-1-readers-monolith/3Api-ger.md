# API-Dokumentation

Dieses Dokument beschreibt die öffentliche HTTP API der aktuellen `CampusLibraryApi`.

Swagger/OpenAPI ist die maßgebliche technische API-Beschreibung. Diese Datei liefert zusätzlich eine didaktische Übersicht für Studierende.

## Base URL

In der Entwicklungsumgebung lauscht die API auf:

```text
https://localhost:8010
http://localhost:8012
```

Das aktuelle API-Präfix lautet:

```text
/camplib/v1
```

Die Version ist Teil der URL.

Beispiel:

```http
GET /camplib/v1/readers
```

## Swagger

Die Swagger UI ist im Development-Modus verfügbar:

```text
https://localhost:8010/swagger
```

Das generierte OpenAPI-Dokument beschreibt:

```text
Routen
Request Bodies
Response Bodies
Status Codes
ProblemDetails-Antworten
DTO-Schemas
```

## Readers-Modul

Die aktuelle API enthält das Readers-Modul.

Ein Reader repräsentiert einen fachlichen Bibliotheksnutzer der CampusLibrary-Domäne.

Ein Reader ist nicht dasselbe wie ein technisches Benutzerkonto.

Die technische Identity-Referenz wird dargestellt durch:

```text
Subject
```

## Reader-Routen

### Alle aktiven Reader abfragen

```http
GET /camplib/v1/readers
```

Liefert alle aktiven Reader.

Inaktive Reader werden von diesem Endpunkt nicht zurückgegeben.

Erfolgreiche Antwort:

```http
200 OK
```

Beispiel für einen Response Body:

```json
[
  {
    "id": "10000000-0000-0000-0000-000000000000",
    "subject": "a00090ad-d9df-486a-8757-4a649e26a54e",
    "firstname": "Erika",
    "lastname": "Mustermann",
    "email": "erika.mustermann@t-online.de",
    "addressDto": {
      "street": "Hauptstr. 23",
      "postalCode": "29556",
      "city": "Suderburg",
      "country": "DE"
    },
    "isActive": true
  }
]
```

### Alle Reader inklusive inaktiver Reader abfragen

```http
GET /camplib/v1/readers/with-inactive
```

Liefert alle Reader inklusive inaktiver Reader.

Dieser Endpunkt ist für administrative oder interne Sichten gedacht.

Erfolgreiche Antwort:

```http
200 OK
```

### Einen aktiven Reader per Id abfragen

```http
GET /camplib/v1/readers/{id}
```

Beispiel:

```http
GET /camplib/v1/readers/10000000-0000-0000-0000-000000000000
```

Liefert den Reader nur, wenn er aktiv ist.

Ein deaktivierter Reader gilt in dieser normalen Reader-Sicht als nicht gefunden.

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

### Einen Reader per Id inklusive inaktiver Reader abfragen

```http
GET /camplib/v1/readers/{id}/with-inactive
```

Beispiel:

```http
GET /camplib/v1/readers/10000000-0000-0000-0000-000000000000/with-inactive
```

Liefert den Reader auch dann, wenn er inaktiv ist.

Dieser Endpunkt ist für administrative oder interne Sichten gedacht.

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

### Einen aktiven Reader per E-Mail abfragen

```http
GET /camplib/v1/readers/email?email={email}
```

Beispiel:

```http
GET /camplib/v1/readers/email?email=erika.mustermann@t-online.de
```

Liefert den Reader nur, wenn er aktiv ist.

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

## Reader anlegen

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
  "id": null
}
```

Erfolgreiche Antwort:

```http
201 Created
```

Response Body:

```json
{
  "id": "generated-or-provided-id",
  "subject": "70000000-0007-0000-0000-000000000000",
  "firstname": "Edgar",
  "lastname": "Engel",
  "email": "e.engel@freenet.de",
  "addressDto": {
    "street": "Am Markt 14",
    "postalCode": "04109",
    "city": "Leipzig",
    "country": "DE"
  },
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

## ReaderCreateDto

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

Das ist Absicht.

Die Id kann bei normaler API-Nutzung weggelassen werden. Der Use Case erzeugt dann eine neue Id.

Die Id kann für Lehre, Tests oder deterministische Seed-Daten angegeben werden.

Deshalb ist `Id` sowohl technisch als auch fachlich nullable.

## Reader ändern

```http
PUT /camplib/v1/readers/{id}
```

Beispiel:

```http
PUT /camplib/v1/readers/10000000-0000-0000-0000-000000000000
```

Request Body zum Ändern nur des Nachnamens:

```json
{
  "lastname": "Meier",
  "email": null,
  "addressDto": null
}
```

Request Body zum Ändern nur der E-Mail:

```json
{
  "lastname": null,
  "email": "e.meier@gmx.de",
  "addressDto": null
}
```

Request Body zum Ändern nur der Adresse:

```json
{
  "lastname": null,
  "email": null,
  "addressDto": {
    "street": "Schillerstr. 1",
    "postalCode": "30123",
    "city": "Hannover",
    "country": "DE"
  }
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

## ReaderUpdateDto

```csharp
public sealed record ReaderUpdateDto(
   string? Lastname,
   string? Email,
   AddressDto? AddressDto
);
```

Alle Properties sind bewusst nullable.

Das ist für partielle Updates erforderlich.

Die Bedeutung von `null` ist:

```text
Lastname = null   -> keine Änderung
Email = null      -> keine Änderung
AddressDto = null -> keine Änderung
```

`Firstname` ist bewusst nicht Teil des Update-DTOs.

`Subject` ist ebenfalls bewusst nicht Teil des Update-DTOs.

Die technische Identity-Referenz wird durch normale Profiländerungen nicht geändert.

## Reader deaktivieren

```http
DELETE /camplib/v1/readers/{id}
```

Beispiel:

```http
DELETE /camplib/v1/readers/30000000-0000-0000-0000-000000000000
```

Erfolgreiche Antwort:

```http
204 No Content
```

Dieser Endpunkt löscht den Reader nicht physisch aus der Datenbank.

Er löst einen Soft Delete aus:

```text
Reader.Deactivate(...)
IsActive = false
```

Nach der Deaktivierung gilt:

```text
GET /camplib/v1/readers/{id}               -> 404 Not Found
GET /camplib/v1/readers/{id}/with-inactive -> 200 OK
GET /camplib/v1/readers                    -> Reader ist nicht enthalten
GET /camplib/v1/readers/with-inactive      -> Reader ist enthalten
```

Mögliche Fehlerantworten:

```http
400 Bad Request
401 Unauthorized
403 Forbidden
404 Not Found
409 Conflict
```

`409 Conflict` wird zurückgegeben, wenn ein Reader bereits deaktiviert ist.

## AddressDto

```csharp
public sealed record AddressDto(
   string Street,
   string PostalCode,
   string City,
   string? Country
);
```

Die Adresse wird in der Application-Schicht in ein `AddressVo` umgewandelt.

Das Value Object führt seine eigene Validierung durch.

## Fehlerantworten

Die API verwendet `ProblemDetails` für Fehlerantworten.

Beispielstruktur:

```json
{
  "type": "about:blank",
  "title": "Bad Request",
  "status": 400,
  "detail": "Invalid email.",
  "code": "Reader.InvalidEmail",
  "traceId": "..."
}
```

Der genaue `code` hängt vom Domain- oder Application-Fehler ab.

Typische Kategorien:

```text
400 Bad Request   -> ungültige Eingabe
401 Unauthorized  -> Authentifizierung erforderlich
403 Forbidden     -> Zugriff verweigert
404 Not Found     -> Ressource nicht gefunden oder in normaler Active-Reader-Sicht inaktiv
409 Conflict      -> doppelte E-Mail, doppeltes Subject oder bereits deaktivierter Reader
```

## Read-Seite und Write-Seite

Der Reader-Controller verwendet zwei unterschiedliche Application-Ports:

```text
IReaderReadModel
IReaderUseCases
```

Die Read-Seite wird für Queries verwendet:

```text
GET /readers
GET /readers/with-inactive
GET /readers/{id}
GET /readers/{id}/with-inactive
GET /readers/email
```

Die Write-Seite wird für Commands verwendet:

```text
POST /readers
PUT /readers/{id}
DELETE /readers/{id}
```

Diese Trennung unterstützt ein klares Lehrmodell:

```text
Queries lesen DTOs
Commands ändern Aggregates
Repositories arbeiten mit Domainobjekten
ReadModels liefern DTOs direkt zurück
```

## Didaktische Ziele

Diese API demonstriert:

```text
REST-artige Endpunkte
API-Versionierung
Swagger/OpenAPI-Dokumentation
DTOs als API-Verträge
Result-basiertes Fehlerhandling
ProblemDetails-Antworten
partielle Update-Semantik
Soft Delete durch Deaktivierung
Trennung von Read- und Write-Pfaden
Controller-Tests mit WebApplicationFactory
```

Swagger sollte für die technische Erkundung verwendet werden.

Dieses Dokument dient der konzeptionellen Erklärung.
