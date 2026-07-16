# Teststrategie — Teil 5

Dieses Dokument beschreibt die Teststrategie für den Branch `part-5/client-noauth`.

Englische Version: [4Testing.md](4Testing.md)

## Verifizierter Stand

Der aktuelle Stand wurde am 15. Juli 2026 lokal erfolgreich geprüft:

```text
dotnet clean
Build succeeded

dotnet build
Build succeeded

dotnet test
212 total, 212 succeeded, 0 failed, 0 skipped
```

Zusätzlich wurde der komplette `Loan_Me.http`-Ablauf erfolgreich ausgeführt.

## Testprojekte und Anwendungen

Automatisiertes Testprojekt:

```text
CampusLibraryApiTest
```

Zu prüfende Anwendungen:

```text
CampusLibraryApi
CampusLibraryClient
```

Weitere Projekte im Build:

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

Der IdentityAccessServer wird mitgebaut, ist für die Part-5-Laufzeit und die `/me`-HTTP-Tests aber nicht erforderlich.

## Testebenen

Der aktuelle Stand verwendet:

```text
Domain-Tests
Value-Object-Tests
Use-Case-Mock-Tests
Use-Case-Integrationstests
Repository-Integrationstests
ReadModel-Integrationstests
BC-to-BC-Contract-Integrationstests
Controller/API-End-to-End-Tests
DI-Tests
manuelle HTTP-Dateien
Client-Build
manuelle Browser-Smoke-Tests
```

## 1. Vollständige Regression

Nach Änderungen an API, Client, DTOs, DevIdentity, Use Cases oder Tests:

```bash
dotnet clean
dotnet build
dotnet test
```

Erwarteter Stand:

```text
Build succeeded
212 tests succeeded
0 failed
0 skipped
```

## 2. Git-Prüfung vor Commit

```bash
git status
git diff --check
git add -A
git diff --cached --check
```

Nach dem Commit:

```bash
git status
```

Erwartung:

```text
working tree clean
```

## 3. Testfokus Readers

Wichtige Reader-Regeln:

```text
Reader-Erzeugung validiert Subject und E-Mail
Subject und E-Mail sind eindeutig
Reader-Deaktivierung ist Soft Delete
inaktive Reader werden standardmäßig ausgeblendet
Reader mit aktuellen Loans darf nicht deaktiviert werden
UpdateMe bestimmt den Reader über Subject
UpdateMe akzeptiert nur veränderbare Profildaten
null im ReaderUpdateDto bedeutet unverändert
```

### Use-Case-Tests für UpdateMe

Zu prüfen:

```text
IdentitySubject.Check wird berücksichtigt
nicht authentifizierte Identität -> Fehler
Employee-Profil -> AccessNotAllowed
leeres oder ungültiges Subject -> Fehler
Reader per Subject nicht gefunden -> NotFound
neue E-Mail ungültig -> BadRequest
neue E-Mail bereits verwendet -> Conflict
gültige Werte werden gespeichert
nicht übergebene Werte bleiben unverändert
```

### API-Endpunkt

```http
PUT /camplib/v1/readers/me/update
```

Erwartete Statuscodes:

```text
200 Erfolg
400 Validierung
401 nicht authentifiziert
403 kein Reader
404 Subject ohne Reader
409 doppelte E-Mail
```

## 4. DevIdentity-Tests

Client und API lesen getrennte Konfigurationen.

### TC-P5-IDENTITY-001 — Readerprofil konsistent

Voraussetzungen:

```text
Client ActiveProfile = ReaderRita
API ActiveProfile    = ReaderRita
API Subject          = reader-099
Reader.Subject       = reader-099
```

Erwartung:

```text
Client zeigt Reader-Perspektive.
API-/me-Endpunkte beziehen sich auf Rita Reader.
Kein Token und kein Identitätsheader wird gesendet.
```

### TC-P5-IDENTITY-002 — Subject stimmt nicht überein

API-Konfiguration temporär:

```text
Subject = unknown-reader
```

API neu starten und `/me` aufrufen.

Erwartung:

```text
404 Not Found
```

Das bestätigt, dass die Zuordnung über Subject und nicht über E-Mail oder ReaderId erfolgt.

### TC-P5-IDENTITY-003 — Employee-Profil auf Reader-Endpunkt

API-Konfiguration:

```text
ActiveProfile = EmployeeAdmin
```

Aufruf:

```http
GET /camplib/v1/loans/me
```

Erwartung:

```text
403 Forbidden
```

### TC-P5-IDENTITY-004 — Nicht authentifiziert simuliert

Profil:

```text
IsAuthenticated = false
```

Erwartung für Reader-Self-Service:

```text
401 Unauthorized
```

### TC-P5-IDENTITY-005 — AdminRights-Kompatibilität

Voraussetzung:

```text
AdminRights = 0
```

Erwartung:

```text
IdentityGateway liefert 0.
CampusLibrary wertet den Wert fachlich nicht aus.
Self-Service funktioniert unverändert.
```

## 5. API und Client starten

API:

```bash
dotnet run --project CampusLibraryApi
```

Client:

```bash
dotnet run --project CampusLibraryClient
```

Adressen:

```text
API:    https://localhost:8010
Client: https://localhost:6040
```

Die API muss für direkte `.http`-Tests laufen. Der Client ist dafür nicht erforderlich.

## 6. Datenbank und HTTP-Skripte

Manuelle Dateien:

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

Vor einem deterministischen Komplettlauf:

```text
Datenbank zurücksetzen
Reader-Testdaten anlegen
Book-/BookItem-Testdaten anlegen
Loan-Skripte zuletzt ausführen
```

Die optionalen `Id`-Felder in Create-DTOs ermöglichen feste IDs in HTTP- und Integrationstests.

## 7. Manueller Reader-Test

### TC-P5-READERS-001 — Reader 99 anlegen

```http
POST /camplib/v1/readers
```

Wichtige Werte:

```text
ReaderId = 00000099-0000-0000-0000-000000000000
Subject  = reader-099
Email    = r.reader@library.local
```

Erwartung:

```text
201 Created
```

### TC-P5-READERS-002 — Aktuellen Reader aktualisieren

API-Profil:

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

Erwartung:

```text
200 OK
Reader.Subject bleibt reader-099
Reader.Email wird geändert
```

### TC-P5-READERS-003 — Alte E-Mail ist keine Identitätszuordnung

Nach dem Update:

```text
DevIdentity.Username kann weiterhin r.reader@library.local sein.
Reader.Email ist e.meier@gmx.de.
/me-Endpunkte funktionieren weiterhin über Subject.
```

### TC-P5-READERS-004 — Reader deaktivieren

```http
DELETE /camplib/v1/readers/{id}
```

Erwartung ohne aktuelle Loans:

```text
204 No Content
normaler GET -> 404
GET mit includeInactive=true -> 200 und IsActive=false
```

## 8. Catalog-Regression

Zu prüfen:

```text
BookDto wird für Liste und Detail verwendet
Suche liefert BookDto[]
BookItemDto enthält Id, BookId und Status
keine InventoryNumber im Transportvertrag
Buch-Erzeugung liefert 201
BookItem-Erzeugung liefert 201
inaktive Bücher werden standardmäßig ausgeblendet
Deaktivierungsinfo zeigt aktuelle Loans
Buch mit ausgeliehenen Items kann nicht unzulässig deaktiviert werden
```

### TC-P5-CATALOG-001 — Bücherliste

```http
GET /camplib/v1/books?includeInactive=false
```

Erwartung:

```text
200 OK
BookDto[]
```

### TC-P5-CATALOG-002 — Suche

```http
GET /camplib/v1/books/search?searchField=Title&searchText=Clean%20Code&includeInactive=false
```

Erwartung:

```text
200 OK
BookDto[]
```

### TC-P5-CATALOG-003 — BookItem hinzufügen

```http
POST /camplib/v1/books/{bookId}/items
```

Erwartung:

```text
201 Created
BookItemDto.Status = 1
```

## 9. Loan-Regression

Wichtige Regeln:

```text
Loan besitzt keinen Status und kein ReturnedAt
bestehender Loan bedeutet aktuelle Ausleihe
Borrow prüft Reader und BookItem über Modul-Contracts
BookItem muss verfügbar sein
Renew prüft Fälligkeit und maximale Verlängerungen
ReturnAtDesk löscht den Loan
```

### Administrative Tests

```text
GET   /loans
GET   /loans/{id}
POST  /loans
PATCH /loans/{id}/renew
PATCH /loans/{id}/return-at-desk
```

### Reader-Self-Service

```text
GET   /loans/me
GET   /loans/me/{id}
POST  /loans/me
PATCH /loans/me/{id}/renew
```

Bei allen `/me`-Routen ist zu prüfen, dass ein Loan eines anderen Readers nicht sichtbar oder verlängerbar ist.

## 10. Verifizierter Loan_Me.http-Ablauf

Datei:

```text
CampusLibraryApi/_5_ApiTest/Loan_Me.http
```

Voraussetzungen:

```text
ActiveProfile = ReaderRita
Subject = reader-099
Reader mit reader-099 existiert
BookItem 00000002-0000-0000-0000-000000000000 ist verfügbar
LoanId 00000099-0000-0001-0000-000000000000 existiert noch nicht
```

Erwartete Antworten:

```text
GET /loans/me
-> 200 OK, anfangs gegebenenfalls []

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

Der letzte 404 ist kein Fehler im Test, sondern bestätigt die Löschsemantik der Rückgabe.

## 11. Client-Smoke-Tests

### TC-P5-CLIENT-001 — Client startet ohne Login

Voraussetzung:

```text
AuthNEnabled = false
DevIdentityEnabled = true
ApiAccessTokenEnabled = false
AuthZEnabled = false
```

Erwartung:

```text
keine Login-Weiterleitung
kein Bearer-Token
Startseite und Navigation werden angezeigt
```

### TC-P5-CLIENT-002 — Reader-Perspektive

Client-Profil:

```text
ActiveProfile = ReaderRita
```

Erwartung:

```text
Katalog ist sichtbar
/my/loans ist sichtbar
Reader kann verfügbares BookItem ausleihen
BorrowMyAsync sendet keine ReaderId
```

### TC-P5-CLIENT-003 — Mitarbeiter-Perspektive

Client-Profil:

```text
ActiveProfile = EmployeeAdmin
```

Erwartung:

```text
/readers sichtbar
/loans sichtbar
Catalog-Aktionen für Erzeugung, Item und Deaktivierung sichtbar
```

### TC-P5-CLIENT-004 — API nicht erreichbar

API stoppen und Clientseite mit Datenzugriff öffnen.

Erwartung:

```text
keine unbehandelte UI-Ausnahme
verständliche zentrale Fehlermeldung
```

## 12. DTO-Regressionsregeln

Bei Änderungen an öffentlichen API-DTOs müssen immer beide Seiten geprüft werden:

```text
API-Modul-DTo
Client-Transport-DTO
API-Client-Methode
Razor-Seite oder Modell
Controller-E2E-Test
```

Nicht wieder einführen:

```text
BookListItemDto
BookDetailDto
BookSearchDto
LoanListItemDto
LoanDetailDto
Loan.Status
Loan.ReturnedAt
BookItem.InventoryNumber im aktuellen Transportvertrag
```

## 13. Regressionsregeln

Nach Änderungen an DevIdentity:

```text
API und Client appsettings vergleichen
ActiveProfile vergleichen
Subject gegen Reader-Testdaten prüfen
API neu starten
Loan_Me.http ausführen
```

Nach Änderungen an Reader-Self-Service:

```text
Mock-Test
Integrationstest
Controller-E2E-Test
Reader.http
Client-Build
```

Nach Änderungen an Loan-Self-Service:

```text
Ownership-Tests
Identity-Fehlertests
Loan_Me.http
Client Borrow-/MyLoans-Smoke-Test
```

Ein grüner `dotnet test` ersetzt nicht die manuellen `.http`- und Browsertests, weil Feature-Flag- und Konfigurationsfehler erst zur Laufzeit sichtbar werden können.
