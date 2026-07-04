# Teststrategie — Teil 5

Dieses Dokument beschreibt die Teststrategie für Teil 5 des Projekts `CampusLibrary`.

Teil 5 ergänzt `CampusLibraryClient`, einen Blazor-SSR-Client ohne echte Authentifizierung. Die vorhandenen Backend-Tests aus Teil 4 bleiben wichtig. Der neue Fokus liegt auf manuellen und explorativen Client/API-Tests.

Englische Version: [4Testing.md](4Testing.md)

## Bekannter Stand

Nach der Umstellung der BookItem-Identität wurde gemeldet:

```text
dotnet build
Build succeeded

dotnet test
196 total, 0 failed, 0 skipped
```

Wichtig:

```text
dotnet test prüft aktuell im Wesentlichen die API.
Reine Client-Änderungen werden durch dotnet build und Browsertests geprüft.
```

## Testprojekte und Anwendungen

Automatisiertes Testprojekt:

```text
CampusLibraryApiTest
```

Client-Projekt:

```text
CampusLibraryClient
```

Backend-Projekte:

```text
CampusLibraryApi
CampusLibraryApi_1_Web
CampusLibraryApi_2_BuildingBlocks
CampusLibraryApi_3_Core_Readers
CampusLibraryApi_3_Core_Catalog
CampusLibraryApi_3_Core_Loan
CampusLibraryApi_4_Infrastructure
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
modulübergreifende Contract-Integrationstests
Controller/API-End-to-End-Tests
manuelle HTTP-Dateien
```

Teil 5 ergänzt:

```text
Build-Test des Blazor-Clients
manuelle Client/API-Smoke-Tests
manuelle UI-Perspektiventests über DevIdentity
```

## 1. Backend-Regressionstests

Ausführen, wenn API, DTOs, UseCases, ReadModels, Seed/TestSeed oder Tests geändert wurden:

```bash
dotnet test
```

Wichtige Backend-Testbereiche:

```text
Readers-Deactivate-Verhalten
Catalog-Workflows für Book und BookItem
Loans-Workflows Borrow, Renew und Return
ReadModel-Projektionen
modulübergreifende Contracts
API-Statuscodes und ProblemDetails-Antworten
```

## 2. Client-Build

Ausführen nach Client-Änderungen:

```bash
dotnet build
```

Das prüft:

```text
CampusLibraryClient kompiliert
Razor Components kompilieren
DTOs passen zum aktuellen API-Vertrag
DI-Registrierung ist konsistent
vorbereitete Auth-Dateien beschädigen den No-Auth-Modus nicht
```

## 3. Manuelle Client + API Tests starten

API starten:

```bash
dotnet run --project CampusLibraryApi
```

Client starten:

```bash
dotnet run --project CampusLibraryClient
```

Client-Adresse:

```text
https://localhost:6040
```

API-BaseUrl im Client:

```json
{
  "CampusLibraryApi": {
    "BaseUrl": "https://localhost:8010/"
  }
}
```

## 4. Smoke-Tests

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
Kein echter Login ist erforderlich.
Die horizontale Bootstrap-Navigation ist sichtbar.
```

### TC-P5-CLIENT-002 — Navigation funktioniert

Schritte:

```text
1. Home öffnen.
2. Katalog öffnen.
3. Leser öffnen, falls Mitarbeiterprofil aktiv ist.
4. Ausleihen öffnen.
```

Erwartetes Ergebnis:

```text
Alle sichtbaren Seiten können geöffnet werden.
Der aktive Menüpunkt ist erkennbar.
Das Layout bleibt stabil.
```

### TC-P5-CLIENT-003 — Auth ist nicht aktiv

Vorbedingung:

```json
{
  "Features": {
    "AuthNEnabled": false,
    "DevIdentityEnabled": true,
    "ApiAccessTokenEnabled": false,
    "AuthZEnabled": false
  }
}
```

Erwartetes Ergebnis:

```text
Es erfolgt keine Login-Weiterleitung.
Für API-Aufrufe wird kein AccessTokenHandler benötigt.
DevIdentity steuert nur die UI-Perspektive.
```

## 5. DevIdentity-Tests

### TC-P5-IDENTITY-001 — Mitarbeiterperspektive

Vorbedingung:

```json
{
  "DevIdentity": {
    "ActiveProfile": "EmployeeAdmin"
  }
}
```

Erwartetes Ergebnis:

```text
Navigation zeigt Home, Katalog, Leser, Ausleihen, Logout.
Katalog zeigt Mitarbeiteraktionen.
Leser-Seite ist sichtbar.
```

### TC-P5-IDENTITY-002 — Readerperspektive

Vorbedingung:

```json
{
  "DevIdentity": {
    "ActiveProfile": "ReaderRita"
  }
}
```

Erwartetes Ergebnis:

```text
Navigation zeigt Home, Katalog, Ausleihen, Logout.
Katalog zeigt Ausleihen-Aktion, wenn ein Exemplar verfügbar ist.
ReaderId ist für Borrow vorhanden.
```

## 6. Readers-Client-Tests

### TC-P5-READERS-001 — Readers-Liste laden

Schritte:

```text
1. EmployeeAdmin aktivieren.
2. CampusLibraryApi mit Seed-Daten starten.
3. /readers öffnen.
```

Erwartetes Ergebnis:

```text
Die Readers-Seite zeigt Reader-Zeilen an.
Die Tabelle zeigt Name, Email und Status.
Subject wird nicht angezeigt.
```

### TC-P5-READERS-002 — Kein Reader-Anlegen in Teil 5

Schritte:

```text
1. /readers öffnen.
2. Nach einer Aktion Reader hinzufügen suchen.
```

Erwartetes Ergebnis:

```text
Es gibt keine UI-Funktion zum Anlegen eines Readers.
Reader-Provisionierung ist für spätere AuthN/AuthZ-Teile vorgesehen.
```

## 7. Catalog-Client-Tests

### TC-P5-CATALOG-001 — Bücherliste laden

Schritte:

```text
1. CampusLibraryApi mit Seed-Daten starten.
2. /catalog/books öffnen.
```

Erwartetes Ergebnis:

```text
Die Katalogtabelle wird angezeigt.
Spaltenstruktur: Aktion | Titel | Autorinnen/Autoren | ISBN | Exemplare | Status.
Titel und Untertitel stehen gemeinsam in der Titel-Spalte.
Aktion steht vorne.
Exemplare zeigt ausgeliehen / gesamt.
```

### TC-P5-CATALOG-002 — Suche nach Titel

Schritte:

```text
1. /catalog/books öffnen.
2. Suchfeld Titel auswählen.
3. Suchtext eingeben.
4. Suchen klicken.
```

Erwartetes Ergebnis:

```text
Passende Bücher werden angezeigt.
Wenn nichts passt, erscheint eine leere Trefferanzeige.
```

### TC-P5-CATALOG-003 — Suche nach Autorennachname

Schritte:

```text
1. /catalog/books öffnen.
2. Suchfeld Nachname Autor/in auswählen.
3. Bekannten Nachnamen eingeben.
4. Suchen klicken.
```

Erwartetes Ergebnis:

```text
Bücher mit passendem Autorennachnamen werden angezeigt.
```

### TC-P5-CATALOG-004 — Buch hinzufügen

Vorbedingung:

```text
EmployeeAdmin ist aktiv.
```

Schritte:

```text
1. /catalog/books öffnen.
2. Buch hinzufügen klicken.
3. Titel, optional Untertitel, Autorinnen/Autoren und ISBN eingeben.
4. Buch speichern.
```

Erwartetes Ergebnis:

```text
Das Buch wird über POST /camplib/v1/books angelegt.
Eine Erfolgsmeldung erscheint.
Das erste Exemplar kann anschließend hinzugefügt werden.
```

### TC-P5-CATALOG-005 — Exemplar zu aktivem Buch hinzufügen

Vorbedingung:

```text
EmployeeAdmin ist aktiv.
Ein aktives Buch existiert.
```

Schritte:

```text
1. /catalog/books öffnen.
2. Bei aktivem Buch Exemplar hinzufügen klicken.
3. Exemplar hinzufügen ausführen.
```

Erwartetes Ergebnis:

```text
POST /camplib/v1/books/{bookId}/items wird aufgerufen.
Die API erzeugt eine eindeutige BookItem.Id.
Die UI zeigt diese Id als Inventarnummer.
```

### TC-P5-CATALOG-006 — Buch deaktivieren

Vorbedingung:

```text
EmployeeAdmin ist aktiv.
Ein aktives Buch existiert.
```

Schritte:

```text
1. /catalog/books öffnen.
2. Bei aktivem Buch Deaktivieren klicken.
3. Bestätigung ausführen.
```

Erwartetes Ergebnis:

```text
PATCH /camplib/v1/books/{bookId}/deactivate wird aufgerufen.
Das Buch ist anschließend inaktiv.
Für inaktive Bücher wird keine Aktion zum Hinzufügen von Exemplaren angeboten.
```

## 8. Borrow-Tests

### TC-P5-BORROW-001 — Buch als Reader ausleihen

Vorbedingung:

```text
ReaderRita ist aktiv.
Das Buch hat mindestens ein tatsächlich verfügbares Exemplar.
```

Schritte:

```text
1. /catalog/books öffnen.
2. Bei verfügbarem Buch Ausleihen klicken.
3. Inventarnummer auswählen.
4. Ausleihe abschließen.
```

Erwartetes Ergebnis:

```text
POST /camplib/v1/loans wird aufgerufen.
Der Request enthält ReaderId und BookItemId.
BookItemId wird in der UI als Inventarnummer angezeigt.
Nach Erfolg navigiert der Client zu /my/loans.
```

### TC-P5-BORROW-002 — Nicht verfügbare Bücher können nicht ausgeliehen werden

Schritte:

```text
1. ReaderRita aktivieren.
2. /catalog/books öffnen.
3. Buch ohne verfügbares Exemplar prüfen.
```

Erwartetes Ergebnis:

```text
Es wird kein Ausleihen-Button angeboten.
Die UI berücksichtigt BookItem-Status und aktuell ausgeliehene BookItemIds.
```

## 9. Loans-Client-Tests

### TC-P5-LOANS-001 — Ausleihenliste laden

Schritte:

```text
1. CampusLibraryApi mit Seed-Daten starten.
2. /loans öffnen.
```

Erwartetes Ergebnis:

```text
Die Liste zeigt ausgeliehene Loans.
Die UI zeigt Reader, Titel, Inventarnummer, Ausleihdatum, Fälligkeitsdatum, Status und Overdue-Flag.
Inventarnummer ist die BookItemId.
```

### TC-P5-LOANS-002 — Ausleihe-Details öffnen

Schritte:

```text
1. /loans öffnen.
2. Details bei einer Loan öffnen.
```

Erwartetes Ergebnis:

```text
Die Detailseite zeigt Buchdaten, Inventarnummer, Readerdaten inklusive Email und Ausleihdaten.
Renew und Return befinden sich auf der Detailseite.
```

### TC-P5-LOANS-003 — Loan verlängern

Schritte:

```text
1. Loan-Details öffnen.
2. Wenn verlängerbar, Verlängern klicken.
```

Erwartetes Ergebnis:

```text
PATCH /camplib/v1/loans/{id}/renew wird aufgerufen.
Die Detaildaten werden aktualisiert oder eine API-Fehlermeldung wird angezeigt.
```

### TC-P5-LOANS-004 — Loan zurückgeben

Schritte:

```text
1. Loan-Details öffnen.
2. Zurückgeben klicken.
```

Erwartetes Ergebnis:

```text
PATCH /camplib/v1/loans/{id}/return-at-desk wird aufgerufen.
Die Loan wird als zurückgegeben markiert oder eine API-Fehlermeldung wird angezeigt.
```

## 10. Fehlerbehandlung

### TC-P5-ERROR-001 — API nicht erreichbar

Schritte:

```text
1. CampusLibraryClient starten.
2. CampusLibraryApi stoppen.
3. /catalog/books oder /readers öffnen.
```

Erwartetes Ergebnis:

```text
Die Seite stürzt nicht ab.
ErrorAlert zeigt einen Netzwerk-/API-Fehler.
```

## 11. Regressionsregeln

```text
Reader anlegen gehört nicht in Teil 5.
Subject wird in der Reader-Liste nicht angezeigt.
InventoryNumber darf nicht als DTO-Property zurückkehren.
BookItemId darf in der UI als Inventarnummer beschriftet werden.
Aktion steht in der Katalogtabelle vorne.
Titel und Untertitel stehen gemeinsam in der Titel-Spalte.
DevIdentity ist keine echte Authentifizierung.
```
