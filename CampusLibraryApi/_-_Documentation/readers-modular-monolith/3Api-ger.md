# API-Dokumentation

Dieses Dokument beschreibt die öffentliche HTTP API der aktuellen `CampusLibraryApi`.

Swagger/OpenAPI ist die maßgebliche technische API-Beschreibung. Diese Datei liefert zusätzlich eine didaktische Übersicht für Studierende.

## Base URL

```text
https://localhost:8010
http://localhost:8012
```

Das aktuelle API-Präfix lautet:

```text
/camplib/v1
```

## Readers-Modul

Ein Reader repräsentiert einen fachlichen Bibliotheksnutzer der CampusLibrary-Domäne.

Ein Reader ist nicht dasselbe wie ein technisches Benutzerkonto. Die technische Identity-Referenz wird durch `Subject` dargestellt.

## Reader-Routen

### Alle aktiven Reader abfragen

```http
GET /camplib/v1/readers
```

Liefert alle aktiven Reader. Inaktive Reader werden von diesem Endpunkt nicht zurückgegeben.

### Alle Reader inklusive inaktiver Reader abfragen

```http
GET /camplib/v1/readers/with-inactive
```

Liefert alle Reader inklusive inaktiver Reader. Dieser Endpunkt ist für administrative oder interne Sichten gedacht.

### Einen aktiven Reader per Id abfragen

```http
GET /camplib/v1/readers/{id}
```

Liefert den Reader nur, wenn er aktiv ist. Ein deaktivierter Reader gilt in dieser normalen Reader-Sicht als nicht gefunden.

### Einen Reader per Id inklusive inaktiver Reader abfragen

```http
GET /camplib/v1/readers/{id}/with-inactive
```

Liefert den Reader auch dann, wenn er inaktiv ist.

### Einen aktiven Reader per E-Mail abfragen

```http
GET /camplib/v1/readers/email?email={email}
```

Liefert den Reader nur, wenn er aktiv ist.

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

Erfolgreiche Antwort: `201 Created`.

## ReaderDto

```csharp
public sealed record ReaderDto(
   Guid Id,
   string? Firstname,
   string? Lastname,
   string? Email,
   AddressDto AddressDto,
   bool IsActive,
   string? Subject
);
```

`IsActive` zeigt, ob der Reader aktuell Teil der aktiven Reader-Liste ist.

Normale Reader-Endpunkte liefern nur Reader mit `IsActive == true`. `WithInactive`-Endpunkte können auch Reader mit `IsActive == false` liefern.

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

`Id` ist optional. Die Id kann bei normaler API-Nutzung weggelassen oder für Lehre, Tests und deterministische Seed-Daten angegeben werden.

## Reader ändern

```http
PUT /camplib/v1/readers/{id}
```

`ReaderUpdateDto` unterstützt partielle Updates:

```csharp
public sealed record ReaderUpdateDto(
   string? Lastname,
   string? Email,
   AddressDto? AddressDto
);
```

Die Bedeutung von `null` ist:

```text
Lastname = null   -> keine Änderung
Email = null      -> keine Änderung
AddressDto = null -> keine Änderung
```

## Reader deaktivieren

```http
DELETE /camplib/v1/readers/{id}
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

```text
400 Bad Request   -> ungültige Eingabe
401 Unauthorized  -> Authentifizierung erforderlich
403 Forbidden     -> Zugriff verweigert
404 Not Found     -> Ressource nicht gefunden oder in normaler Active-Reader-Sicht inaktiv
409 Conflict      -> doppelte E-Mail, doppeltes Subject oder bereits deaktivierter Reader
```

## Read-Seite und Write-Seite

```text
GET-Endpunkte           -> IReaderReadModel
POST / PUT / DELETE     -> IReaderUseCases
```

Diese Trennung unterstützt ein klares Lehrmodell:

```text
Queries lesen DTOs
Commands ändern Aggregates
Repositories arbeiten mit Domainobjekten
ReadModels liefern DTOs direkt zurück
```
