# CampusLibrary — Teil 2: Readers Modular Monolith

Lehrprojekt für eine modular aufgebaute, an DDD orientierte ASP.NET Core Web API.

Teil 2 setzt das `Readers`-Modul aus Teil 1 fort und verschiebt die internen Architekturgrenzen in separate Projekte. Die Anwendung bleibt weiterhin ein deploybarer modularer Monolith, aber Web, BuildingBlocks, Core, Infrastructure und Tests sind nun deutlicher getrennt.

## Aktueller Stand

Die aktuelle Version enthält das erste funktionsfähige Modul:

- Modul `Readers`
- ASP.NET Core Web API
- API-Versionierung
- Swagger/OpenAPI-Dokumentation
- SQLite-Persistenz mit EF Core
- separate Projekte für Web, BuildingBlocks, Readers Core, Infrastructure und Tests
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

## Solution-Struktur

Teil 2 verwendet mehrere Projekte:

```text
CampusLibraryApi
CampusLibraryApi_1_Web
CampusLibraryApi_2_BuildingBlocks
CampusLibraryApi_3_Core_Readers
CampusLibraryApi_4_Infrastructure
CampusLibraryApiTest
```

Es handelt sich weiterhin um eine Anwendung und eine Deployment-Einheit. Die Projektaufteilung dient dazu, architektonische Abhängigkeiten sichtbar zu machen und spätere Module vorzubereiten.

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

| Begriff | Bedeutung |
|---|---|
| `Deactivate` | Fachliche Operation, die den Zustand des Readers ändert |
| `IsActive == false` | Technischer Zustand nach der Deaktivierung |
| `DELETE /readers/{id}` | HTTP-Endpunkt, der die Deaktivierung auslöst |
| Normale ReadModel-Abfragen | Liefern nur aktive Reader |
| `WithInactive`-Abfragen | Liefern aktive und inaktive Reader |

## Teststatus

```text
dotnet test

Test summary:
total:     72
failed:    0
succeeded: 72
skipped:   0
```
