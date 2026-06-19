# CampusLibrary

Lehrprojekt für eine modular aufgebaute, an DDD orientierte ASP.NET Core Web API.

Das Projekt zeigt, wie ein kleiner modularer Monolith in Web-, Core-, Infrastructure- und Testbereiche strukturiert werden kann, ohne das Domänenmodell von technischen Persistenzdetails abhängig zu machen.

## Aktueller Stand

Die aktuelle Version enthält das erste funktionsfähige Modul:

- Modul `Readers`
- ASP.NET Core Web API
- API-Versionierung
- Swagger/OpenAPI-Dokumentation
- SQLite-Persistenz mit EF Core
- Repository- und ReadModel-Infrastruktur
- Use Cases für Anlegen, partielles Ändern und Deaktivieren
- Soft-Delete-Verhalten für Reader über `IsActive`
- ReadModel-Abfragen für aktive Reader
- administrative/interne ReadModel-Abfragen inklusive inaktiver Reader
- Controller-/End-to-End-Tests mit einer echten SQLite-Testdatenbank

Die ursprüngliche physische Delete-Operation für Reader wurde durch eine Deactivate-Operation ersetzt.

Ein Reader wird nicht mehr aus der Datenbank entfernt. Stattdessen wird das `Reader`-Aggregate fachlich deaktiviert, indem `IsActive` auf `false` gesetzt wird.

Normale ReadModel-Abfragen liefern nur aktive Reader. Spezielle ReadModel-Methoden wie `FindByIdWithInactiveAsync` und `SelectAllWithInactiveAsync` können inaktive Reader weiterhin für administrative oder interne Anwendungsfälle zurückgeben.

Das bereitet das Projekt auf spätere Module wie `Loans` vor. Dort müssen historische Zusammenhänge nachvollziehbar bleiben, auch wenn ein Reader nicht mehr zur aktiven Reader-Liste gehört.

Die Testsuite enthält aktuell 72 Tests für Domain Entities, Value Objects, Use Cases, Repositories, ReadModels, Mock-basierte Application Tests und Controller-/End-to-End-Szenarien.

## Readers-Modul

Das Modul `Readers` unterstützt aktuell folgende Operationen:

- Reader anlegen
- veränderbare Profildaten eines Readers ändern
- Reader deaktivieren
- alle aktiven Reader abfragen
- einen aktiven Reader per Id abfragen
- einen aktiven Reader per E-Mail abfragen
- alle Reader inklusive inaktiver Reader abfragen
- einen Reader per Id inklusive inaktiver Reader abfragen

## Soft Delete / Deaktivierungsregel

Das Entfernen eines Readers wird als fachliche Operation `Deactivate` modelliert.

Die öffentliche HTTP API verwendet weiterhin den HTTP-Verb `DELETE`, weil der Reader aus Sicht eines normalen Clients aus der aktiven Reader-Ressourcensammlung verschwindet. Intern ist dies aber kein physisches Löschen aus der Datenbank.

Die Unterscheidung ist wichtig:

| Begriff | Bedeutung |
|---|---|
| `Deactivate` | Fachliche Operation, die den Zustand des Readers ändert |
| `IsActive == false` | Technischer Zustand nach der Deaktivierung |
| `DELETE /readers/{id}` | HTTP-Endpunkt, der die Deaktivierung auslöst |
| Normale ReadModel-Abfragen | Liefern nur aktive Reader |
| `WithInactive`-Abfragen | Liefern aktive und inaktive Reader |

Dieses Design erhält historische Daten und erlaubt normalen Clients trotzdem eine saubere Sicht auf den aktiven Reader-Bestand.

## Teststatus

Die automatisierte Testsuite ist grün:

```text
dotnet test

Test summary:
total:     72
failed:    0
succeeded: 72
skipped:   0
```

Die Tests decken das aktuelle Deactivate-Verhalten über mehrere Ebenen ab:

- Domain-Tests für `Reader.Deactivate(...)`
- UseCase-Tests für `ReaderUcDeactivate`
- Mock-basierte Application Tests
- Integrationstests für ReadModels
- Tests für normale Active-Reader-Abfragen
- Tests für `WithInactive`-Abfragen
- Controller-/End-to-End-Szenarien
