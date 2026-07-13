namespace CampusLibraryApi._3_Core.Readers._2_Application.Dtos;

// Result DTO for the self-service provisioning step of the currently
// authenticated reader.
//
// This DTO is returned by:
// POST /readers/me/provision
//
// It intentionally contains only the information needed by the client
// after provisioning:
//
// - the fachliche Reader id
// - whether a new Reader was created or an existing Reader was returned
//
// Subject is not exposed because it is a technical identifier from the
// IdentityAccessServer.
// Profile data is not returned here because it belongs to the separate
// self-service profile endpoints.
public sealed record ReaderProvisionMeDto(
   Guid Id,
   bool WasCreated
);

/*
Didaktik
--------

ReaderProvisionMeDto beschreibt das Ergebnis der Self-Service-Provisionierung
des aktuell angemeldeten Readers.

Die Provisionierung ist der Übergang von der technischen Identität im
IdentityAccessServer zum fachlichen Reader in der CampusLibraryApi.

Das DTO ist bewusst klein. Es enthält nur:

- die fachliche ReaderId
- die Information, ob ein neuer Reader erzeugt wurde

Das Subject wird nicht an die UI zurückgegeben. Es ist ein technischer Schlüssel
aus dem Access Token und wird serverseitig verwendet, um den aktuellen Reader
eindeutig zuzuordnen.

Fachliche Profildaten wie Vorname, Nachname, Email und Adresse werden nicht über
dieses DTO transportiert. Sie gehören zu eigenen Self-Service-Schritten:

- PUT /readers/me/profile für den initialen Profilabschluss
- PUT /readers/me/update für spätere Änderungen

Dadurch bleibt die Provisionierung fachlich schlank und klar von der
Profilpflege getrennt.

Lernziele
---------

- Provisionierung als eigenen Anwendungsschritt verstehen
- technische Identität und fachlichen Reader unterscheiden
- Subject serverseitig verwenden, aber nicht unnötig an die UI zurückgeben
- idempotente Operationen über WasCreated sichtbar machen
- DTOs passend zum jeweiligen Use Case zuschneiden
- Provisionierung und Profilpflege bewusst trennen
*/