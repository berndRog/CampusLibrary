# Teststrategie — Teil 5

Dieses Dokument beschreibt die Teststrategie für Teil 5 des Projekts `CampusLibrary`.

Teil 5 ergänzt `CampusLibraryClient`, einen Blazor-SSR-Client ohne aktive Authentifizierung. Die vorhandenen Backend-Tests aus Teil 4 bleiben wichtig. Der neue Fokus liegt auf manuellen und explorativen Client/API-Tests.

Englische Version: [4Testing.md](4Testing.md)

## Bekannter Build-Stand

Der aktuelle Startstand von Teil 5 wurde geprüft mit:

```bash
dotnet build
```

Ergebnis:

```text
Build succeeded
```

## Testprojekte und Anwendungen

Automatisiertes Testprojekt:

```text
CampusLibraryApiTest
```

Backend-Produktionsprojekte:

```text
CampusLibraryApi
CampusLibraryApi_1_Web
CampusLibraryApi_2_BuildingBlocks
CampusLibraryApi_3_Core_Readers
CampusLibraryApi_3_Core_Catalog
CampusLibraryApi_3_Core_Loan
CampusLibraryApi_4_Infrastructure
```

Client-Projekt:

```text
CampusLibraryClient
```

## Testebenen

Teil 5 behält die automatisierten Backend-Testebenen aus Teil 4 bei:

```text
Domain-Tests
Value-Object-Tests
Use-Case-Mock-Tests
Use-Case-Integrationstests
Repository-Integrationstests
ReadModel-Integrationstests
Modulübergreifende Contract-Integrationstests
Controller/API-End-to-End-Tests
Manuelle HTTP-Dateien
```

Teil 5 ergänzt eine neue sichtbare Ebene:

```text
Manuelle Client + API Tests
```

Die erste Client-Version benötigt bewusst noch kein vollständiges automatisiertes UI-Testsetup. Ziel ist es, die Client/API-Interaktion sichtbar und verständlich zu machen.

## 1. Backend-Regressionstests

Alle automatisierten Tests ausführen:

```bash
dotnet test
```

Diese Tests prüfen, dass die API nach Ergänzung des Client-Projekts weiterhin funktioniert.

Wichtige Backend-Testbereiche:

```text
Readers-Deactivate-Verhalten
Catalog-Workflows für Book und BookItem
Loans-Workflows Borrow, Renew und Return
ReadModel-Projektionen
modulübergreifende Contracts
API-Statuscodes und ProblemDetails-Antworten
```

## 2. Build der vollständigen Solution

Ausführen:

```bash
dotnet build
```

Das prüft:

```text
alle Backend-Projekte kompilieren
CampusLibraryClient kompiliert
Projektreferenzen sind konsistent
Razor Components kompilieren
Auth-Vorbereitung beschädigt den No-Auth-Modus nicht
```

Das ist in Teil 5 besonders relevant, weil der Client vorbereitete AuthN/AuthZ-Dateien enthält, die noch nicht aktiv sind.

## 3. Manuelle Client + API Tests

Manuelle Client + API Tests verwenden eine laufende CampusLibraryApi und einen laufenden CampusLibraryClient.

Zuerst die API starten. Danach den Client starten.

Beispiel:

```bash
dotnet run --project CampusLibraryApi
dotnet run --project CampusLibraryClient
```

Der Client muss auf die richtige API-URL zeigen:

```json
{
  "CampusLibraryApi": {
    "BaseUrl": "https://localhost:8010/"
  }
}
```

## 4. Client-Smoke-Tests

### TC-P5-CLIENT-001 — Client startet

Schritte:

```text
1. CampusLibraryApi starten.
2. CampusLibraryClient starten.
3. Client im Browser öffnen.
```

Erwartetes Ergebnis:

```text
Die Startseite wird angezeigt.
Kein Login ist erforderlich.
Die Navigation ist sichtbar.
```

### TC-P5-CLIENT-002 — Navigation funktioniert

Schritte:

```text
1. Client öffnen.
2. Zu Readers navigieren.
3. Zu Catalog / Books navigieren.
4. Zu Loans navigieren.
```

Erwartetes Ergebnis:

```text
Alle Seiten können ohne Authentifizierung geöffnet werden.
Das Layout bleibt stabil.
Kein AuthorizeView blockiert den Benutzer.
```

### TC-P5-CLIENT-003 — Auth ist inaktiv

Vorbedingung:

```json
{
  "Features": {
    "AuthNEnabled": false,
    "ApiAccessTokenEnabled": false,
    "AuthZEnabled": false
  }
}
```

Schritte:

```text
1. Client starten.
2. Readers, Books und Loans öffnen.
```

Erwartetes Ergebnis:

```text
Es erfolgt keine Login-Weiterleitung.
Für API-Aufrufe ist kein AccessTokenHandler erforderlich.
Keine Rollen- oder Policy-Entscheidung blendet Seiten aus.
```

## 5. Readers-Client-Tests

### TC-P5-READERS-001 — Readers-Liste laden

Schritte:

```text
1. CampusLibraryApi mit Seed-Daten starten.
2. CampusLibraryClient starten.
3. /readers öffnen.
```

Erwartetes Ergebnis:

```text
Die Readers-Seite zeigt Reader-Zeilen an.
Die Tabelle zeigt Firstname, Lastname, Email, Subject und Status.
```

API-Aufruf:

```http
GET /camplib/v1/readers?includeInactive=false
```

### TC-P5-READERS-002 — Nicht verfügbare API zeigt Fehler

Schritte:

```text
1. CampusLibraryClient starten.
2. CampusLibraryApi stoppen.
3. /readers öffnen oder Reload klicken.
```

Erwartetes Ergebnis:

```text
Die Seite stürzt nicht ab.
ErrorAlert zeigt einen Netzwerk-/API-Fehler an.
```

## 6. Catalog-Client-Tests

### TC-P5-CATALOG-001 — Bücherliste laden

Schritte:

```text
1. CampusLibraryApi mit Seed-Daten starten.
2. CampusLibraryClient starten.
3. /catalog/books öffnen.
```

Erwartetes Ergebnis:

```text
Die Books-Seite zeigt Buch-Zeilen an.
Die Tabelle zeigt Titel, Untertitel, Autoren, ISBN, Exemplarzahlen und Status.
```

API-Aufruf:

```http
GET /camplib/v1/books?includeInactive=false
```

### TC-P5-CATALOG-002 — Bücher nach Titel suchen

Schritte:

```text
1. /catalog/books öffnen.
2. Title auswählen.
3. Einen bekannten Suchtext eingeben.
4. Search klicken.
```

Erwartetes Ergebnis:

```text
Die Tabelle zeigt passende Bücher.
Wenn kein Buch passt, wird eine leere Treffer-Meldung angezeigt.
```

API-Aufruf:

```http
GET /camplib/v1/books/search?searchField=Title&searchText={text}&includeInactive=false
```

### TC-P5-CATALOG-003 — Bücher nach Autorennachname suchen

Schritte:

```text
1. /catalog/books öffnen.
2. Author last name auswählen.
3. Einen bekannten Autorennachnamen eingeben.
4. Search klicken.
```

Erwartetes Ergebnis:

```text
Die Tabelle zeigt Bücher, deren AuthorsText einen passenden Autorennachnamen enthält.
```

## 7. Loans-Client-Tests

### TC-P5-LOANS-001 — Ausgeliehene Loans laden

Schritte:

```text
1. CampusLibraryApi mit Seed-Daten starten.
2. CampusLibraryClient starten.
3. /loans öffnen.
```

Erwartetes Ergebnis:

```text
Die Loans-Seite zeigt aktuell ausgeliehene Loans.
Zeilen zeigen Reader, Titel, Inventarnummer, Ausleihdatum, Fälligkeitsdatum, Status und Overdue-Flag.
```

API-Aufruf:

```http
GET /camplib/v1/loans
```

### TC-P5-LOANS-002 — Loan verlängern

Schritte:

```text
1. /loans öffnen.
2. Bei einer verlängerbaren ausgeliehenen Loan auf Renew klicken.
```

Erwartetes Ergebnis:

```text
Die API verlängert die Loan.
Die Liste wird neu geladen.
Fälligkeitsdatum und/oder RenewalCount sind gemäß API-Antwort und Projektion aktualisiert.
Wenn die Loan nicht verlängert werden kann, zeigt ErrorAlert den API-Fehler.
```

API-Aufruf:

```http
PATCH /camplib/v1/loans/{id}/renew
```

### TC-P5-LOANS-003 — Loan an der Theke zurückgeben

Schritte:

```text
1. /loans öffnen.
2. Bei einer ausgeliehenen Loan auf Return klicken.
```

Erwartetes Ergebnis:

```text
Die API markiert die Loan als zurückgegeben.
Die Liste wird neu geladen.
Die zurückgegebene Loan erscheint nicht mehr in der Liste der borrowed loans.
Wenn die Loan nicht zurückgegeben werden kann, zeigt ErrorAlert den API-Fehler.
```

API-Aufruf:

```http
PATCH /camplib/v1/loans/{id}/return-at-desk
```

## 8. Fehlerbehandlungstests

### TC-P5-ERROR-001 — ProblemDetails wird angezeigt

Schritte:

```text
1. Einen bekannten API-Validierungs- oder Konfliktfehler über eine Client-Aktion auslösen.
2. Die Seite beobachten.
```

Erwartetes Ergebnis:

```text
Der Fehler wird über ErrorAlert angezeigt.
Die Seite bleibt benutzbar.
```

### TC-P5-ERROR-002 — Ungültige API-BaseUrl

Schritte:

```text
1. CampusLibraryApi:BaseUrl auf eine ungültige URL setzen.
2. Client starten.
3. Eine Seite öffnen, die Daten lädt.
```

Erwartetes Ergebnis:

```text
Die Seite zeigt einen Netzwerkfehler.
Die Client-Anwendung stürzt nicht ab.
```

## 9. Regressionsregel für vorbereitete Auth

Da Teil 5 vorbereiteten, aber inaktiven AuthN/AuthZ-Code enthält, sollte jeder Build prüfen:

```text
AuthNEnabled=false hält den Client anonym.
ApiAccessTokenEnabled=false hält API-Aufrufe tokenfrei.
AuthZEnabled=false hält die Navigation uneingeschränkt.
```

Wenn eine Auth-Änderung dazu führt, dass der No-Auth-Client Login verlangt, gehört diese Aktivierung in einen späteren Teil und nicht in Teil 5.

## 10. Spätere automatisierte Client-Tests

Spätere Teile können automatisierte UI-/Komponententests ergänzen.

Mögliche Kandidaten:

```text
Komponententests für ErrorAlert
Client-Tests mit Fake HttpMessageHandler
Navigation-Smoke-Tests
Playwright-Tests für vollständige Browser-Workflows
```

Für Teil 5 reichen manuelle Client + API Tests aus und sind didaktisch sinnvoll, weil sie die HTTP-Grenze direkt sichtbar machen.
