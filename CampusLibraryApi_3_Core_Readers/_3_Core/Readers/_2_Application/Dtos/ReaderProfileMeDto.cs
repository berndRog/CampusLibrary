namespace CampusLibraryApi._3_Core.Readers._2_Application.Dtos;

// Profile data entered by the currently authenticated reader
// after the domain reader has been provisioned.
//
// This DTO is used for the initial profile completion step.
// It intentionally contains only fachliche profile data that may be entered
// by the reader through the UI.
//
// Subject is not part of this DTO because it is read from the access token.
// Email is not part of this DTO because the initial fachliche reader email
// is taken from the technical username during provisioning.
public sealed record ReaderProfileMeDto(
   string Firstname,
   string Lastname,
   AddressDto AddressDto
);

/*
Didaktik
--------

ReaderProfileMeDto beschreibt den initialen Profilabschluss des aktuell
angemeldeten Readers nach der Provisionierung.

Das DTO enthält nur fachliche Profildaten, die der Reader im Formular selbst
erfassen darf:

- Vorname
- Nachname
- Adresse

Das Subject ist nicht Teil des DTOs. Es kommt aus dem Access Token und dient
als stabiler technischer Schlüssel zwischen IdentityAccessServer und
CampusLibraryApi.

Die Email ist ebenfalls nicht Teil dieses DTOs. Die initiale fachliche
Reader-Email wird bereits während der Provisionierung aus dem technischen
Username übernommen. Eine spätere Änderung der fachlichen Reader-Email erfolgt
über ein separates Self-Service-Update-DTO.

Dadurch bleibt der Unterschied zwischen technischer Identität und fachlichem
Reader-Profil sichtbar.

Lernziele
---------

- initialen Profilabschluss von späterer Profiländerung unterscheiden
- technische Token-Daten von UI-Eingaben trennen
- Subject nicht aus einem Formular übernehmen
- initiale fachliche Email aus dem Provisioning verstehen
- DTOs use-case-spezifisch statt entity-nah modellieren
*/