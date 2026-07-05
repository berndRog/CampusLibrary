namespace CampusLibraryApi._3_Core.Readers._2_Application.Dtos;

// DTO for the currently authenticated reader.
// Subject is intentionally not exposed to the client UI.
public sealed record ReaderProvisionDto(
   Guid Id,
   bool WasCreated
   // string Email,
   // string Firstname,
   // string Lastname,
   // AddressDto AddressDto,
   // bool IsActive,
   // bool IsProfileCompleted
);

/*
Didaktik
--------

ReaderMeDto beschreibt die Sicht des angemeldeten Readers auf sich selbst.

Es ist bewusst kleiner als ReaderDto. Technische Zuordnungsdaten wie Subject
werden nicht an die UI zurückgegeben. Die UI soll nur sehen, welche fachlichen
Profildaten fehlen oder bereits gepflegt sind.

Lernziele
---------

- Unterschied zwischen administrativer Reader-Sicht und Self-Service-Sicht erkennen
- technische Claims nicht unnötig an die UI zurückgeben
- DTOs passend zum jeweiligen Anwendungsfall zuschneiden
*/
