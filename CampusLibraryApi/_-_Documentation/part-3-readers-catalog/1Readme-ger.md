# CampusLibrary — Teil 3: Readers + Catalog

Lehrprojekt für eine modular aufgebaute, DDD-orientierte ASP.NET-Core-Web-API.

Englische Version: [1Readme.md](1Readme.md)

## Aktueller Stand

Diese Version enthält zwei fachliche Module:

* Readers
* Catalog

Sie beschreibt den Stand vor Einführung des Loans-Moduls. Das Catalog-Modul ist bereits vereinfacht: Es gibt kein eigenes Author-Aggregate und keine m:n-Beziehung zwischen Book und Author. Autorennamen werden direkt in `Book.AuthorsText` gespeichert.

Finales automatisiertes Testergebnis für diesen Teil:

```text
139 Tests
0 fehlgeschlagen
0 übersprungen
```

## Aktueller Branch

```text
part-3/readers-catalog
```

## Projektstruktur

```text
CampusLibraryApi
CampusLibraryApi_1_Web
CampusLibraryApi_2_BuildingBlocks
CampusLibraryApi_3_Core_Readers
CampusLibraryApi_3_Core_Catalog
CampusLibraryApi_4_Infrastructure
CampusLibraryApiTest
```

## Architekturidee

Die Lösung ist ein projektbasierter modularer Monolith.

Die Web/API-Schicht stellt HTTP-Endpunkte bereit. Die Core-Module enthalten Domänenmodell, Use Cases, DTOs und Ports. BuildingBlocks enthält wiederverwendbare Abstraktionen. Infrastructure implementiert EF-Core-Persistenz, Repositories, ReadModels und Datenbankkonfiguration. Tests prüfen das Verhalten über Domain-, Application-, Infrastructure- und API-Grenzen hinweg.

Zentrale Abhängigkeitsregel:

```text
Core-Module hängen nicht von Web/API oder Infrastructure ab.
Infrastructure implementiert Outbound Ports der Core-Module.
Das ausführbare API-Projekt verdrahtet alle Module.
```

## Readers-Modul

Das Readers-Modul verwaltet Bibliotheksleserinnen und -leser.

Es enthält:

* Reader-Aggregate
* Reader-Value-Objects
* Reader-Use-Cases
* Reader-Repository-Port
* Reader-ReadModel-Port
* Reader-Controller
* Reader-Tests

Typische Operationen sind:

* Reader anlegen
* Reader-Profildaten ändern
* Reader deaktivieren
* aktive Reader abfragen
* Reader inklusive inaktiver Reader abfragen
* Reader nach Id oder E-Mail suchen

Ein Reader verwendet ein `IsActive`-Flag. Deaktivierung ist eine Soft-Delete-artige fachliche Operation: Der Reader bleibt gespeichert, normale Lese-Endpunkte blenden inaktive Reader jedoch aus. Spezielle `with-inactive`-Endpunkte beziehen sie mit ein.

## Catalog-Modul

Das Catalog-Modul verwaltet den Bibliothekskatalog.

Es enthält:

* Book-Aggregate
* BookItem-Entity
* ISBN-Value-Object
* Book-Use-Cases
* Book-ReadModel
* Book-Repository
* Books-Controller
* Catalog-Tests

Ein Book repräsentiert das bibliografische Werk. Ein BookItem repräsentiert ein physisches Exemplar eines Buchs.

## Catalog-Domänenmodell

### Book

`Book` ist ein Aggregate Root.

Ein Book enthält:

* Autorentext (`AuthorsText`)
* Titel
* optionalen Untertitel
* ISBN
* physische Exemplare
* `IsActive`
* Audit-Zeitstempel

Books können deaktiviert werden. Normale Katalogabfragen blenden inaktive Books aus.

### BookItem

`BookItem` ist eine Entity innerhalb des Book-Aggregates.

Es enthält:

* Id
* BookId
* Inventarnummer
* Status

BookItems verwenden kein `IsActive`. Ihr Lebenszyklus wird über `BookItemStatus` ausgedrückt, zum Beispiel `Available`, `Unavailable`, `Lost` oder `Damaged`.

### AuthorsText statt Author-Aggregate

Teil 3 enthält bewusst kein `Author`-Aggregate.

Das vereinfachte Modell lautet:

```text
Book
- AuthorsText
- Title
- Subtitle
- IsbnVo
- BookItems
```

Dadurch wird eine zweite m:n-Beziehung vermieden, bevor in einem späteren Teil Authentifizierung und Autorisierung eingeführt werden.

Die Suche nach Autorennachnamen wird durch Parsen von `AuthorsText` umgesetzt:

```text
"Martin Fowler, Kent Beck"
-> Fowler
-> Beck
```

Der letzte durch Leerzeichen getrennte Token jedes komma-separierten Autoreneintrags wird als Nachname behandelt.

## API-Überblick

Endpoint-Gruppen:

```text
Readers
Books
```

Wichtige Endpunkte:

```text
GET    /camplib/v1/readers
GET    /camplib/v1/readers/with-inactive
GET    /camplib/v1/readers/{id}
GET    /camplib/v1/readers/{id}/with-inactive
GET    /camplib/v1/readers/email?email=...
POST   /camplib/v1/readers
PUT    /camplib/v1/readers/{id}
DELETE /camplib/v1/readers/{id}

GET    /camplib/v1/books
GET    /camplib/v1/books/{id}
GET    /camplib/v1/books/search?searchField=Title&searchText=...
GET    /camplib/v1/books/search?searchField=Isbn&searchText=...
GET    /camplib/v1/books/search?searchField=AuthorLastName&searchText=...
POST   /camplib/v1/books
POST   /camplib/v1/books/{bookId}/items
PATCH  /camplib/v1/books/{bookId}/deactivate
```

`DELETE /readers/{id}` führt eine Deaktivierung aus, keine physische Löschung.

## Testing

Das Testprojekt deckt ab:

* Domain-Tests
* Value-Object-Tests
* Use-Case-Mock-Tests
* Use-Case-Integrationstests
* Repository-Integrationstests
* ReadModel-Integrationstests
* Controller/API-End-to-End-Tests
* manuelle `.http`-Dateien

Alle Tests ausführen:

```bash
dotnet test
```

## Manuelle HTTP-Dateien

Für reproduzierbare manuelle Tests sollte die Datenbank vor dem Ausführen der HTTP-Dateien zurückgesetzt oder gelöscht werden.

Empfohlene Reihenfolge für Teil 3:

```text
1. Readers.http
2. Books.http
```

Für späteres Lehrmaterial ist es sinnvoll, Seed-Aufbau und eigentliche Tests zu trennen, zum Beispiel:

```text
01_Seed_Readers.http
02_Seed_Books.http
11_Readers_Api.http
12_Books_Api.http
```
