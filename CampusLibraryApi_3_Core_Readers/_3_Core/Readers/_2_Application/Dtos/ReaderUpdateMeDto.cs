namespace CampusLibraryApi._3_Core.Readers._2_Application.Dtos;

// DTO for the self-service update of the currently authenticated reader.
//
// This DTO is used by:
// PUT /readers/me/update
//
// It contains only mutable fachliche Reader data:
//
// - Lastname may be changed.
// - Email may be changed as fachliche contact email.
// - AddressDto may be changed.
//
// Firstname is intentionally not part of this DTO.
// The technical username in the IdentityAccessServer is also not changed.
// The current Reader is resolved server-side through the access token subject,
// not through a Reader id supplied by the client.
public sealed record ReaderUpdateMeDto(
   string? Lastname,
   string? Email,
   AddressDto? AddressDto
);

/*
Didaktik
--------

ReaderUpdateMeDto beschreibt die spätere Self-Service-Änderung des aktuell
angemeldeten Readers.

Dieser Schritt ist vom initialen Profilabschluss getrennt:

- ReaderProfileMeDto wird für PUT /readers/me/profile verwendet.
- ReaderUpdateMeDto wird für PUT /readers/me/update verwendet.

Beim initialen Profilabschluss werden Vorname, Nachname und Adresse gesetzt.
Bei der späteren Änderung darf der Reader nur ausgewählte fachliche Daten
ändern:

- Nachname
- fachliche Reader-Email
- Adresse

Der Vorname ist bewusst nicht enthalten und damit nicht änderbar. Auch der
technische Username im IdentityAccessServer wird nicht geändert. Die Email in
diesem DTO ist die fachliche Kontakt-Email des Readers in der CampusLibraryApi,
nicht zwingend der technische Loginname.

Das DTO enthält keine ReaderId. Der aktuelle Reader wird serverseitig über das
Subject aus dem Access Token ermittelt. Dadurch kann der Client nicht versuchen,
einen fremden Reader über eine übergebene Id zu ändern.

Alle Eigenschaften sind optional. Dadurch kann der Client gezielt nur die Werte
senden, die geändert werden sollen. Die fachliche Validierung entscheidet im
UseCase und im Aggregate, welche Kombinationen erlaubt sind.

Lernziele
---------

- initialen Profilabschluss und spätere Self-Service-Änderung trennen
- fachliche Reader-Email vom technischen Loginname unterscheiden
- Vorname bewusst aus dem späteren Update ausschließen
- Self-Service-Operationen ohne ReaderId modellieren
- aktuellen Reader über das Token-Subject bestimmen
- DTOs use-case-spezifisch und nicht entity-nah gestalten
*/