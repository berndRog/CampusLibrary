namespace CampusLibraryApi._3_Core.Readers._2_Application.Dtos;

// Profile data entered by the reader after provisioning.
// Subject and email are intentionally not part of this DTO.
public sealed record ReaderProfileUpdateDto(
   string Firstname,
   string Lastname,
   AddressDto AddressDto
);

/*
Didaktik
--------

ReaderProfileUpdateDto enthält nur die fachlichen Profildaten, die der Reader
selbst erfassen darf.

Subject und Email kommen aus dem IdentityAccessServer und dürfen nicht durch
ein Formular überschrieben werden. Dadurch bleibt die technische Identität
vertrauenswürdig.

Lernziele
---------

- vertrauenswürdige Token-Daten von UI-Eingaben unterscheiden
- Self-Service-Profil bewusst klein halten
- DTOs nicht automatisch aus Entity-Eigenschaften ableiten
*/
