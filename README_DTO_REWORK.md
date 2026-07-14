# CampusLibrary Part 6 – überarbeiteter Stand

Dieser Stand basiert auf Part 6 und enthält:

- fachmodul-eigene öffentliche API-DTOs,
- eigene HTTP-Transporttypen im Blazor-Client,
- ausschließlich echte BC-to-BC-Contracts in BuildingBlocks,
- konsolidierte List-/Detail-DTOs,
- beibehaltene optionale Test-IDs,
- die drei getrennten Reader-Self-Service-Abläufe
  `provision`, `profile` und `update`,
- die beiden getrennten Domainmethoden `UpdateMyProfile(...)` und
  `UpdateProfile(...)`,
- explizite Fehlerbehandlung direkt in allen Controller-Methoden,
- korrigierte Namespaces und DI-Registrierungen des Loans-Moduls.

Es gibt kein Projekt `CampusLibrary.Contracts`.
