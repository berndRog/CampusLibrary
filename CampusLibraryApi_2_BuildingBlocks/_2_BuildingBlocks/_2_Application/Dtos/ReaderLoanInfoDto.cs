namespace CampusLibraryApi._2_BuildingBlocks._2_Application.Contracts;

// Loan-relevant reader data shared between modules.
public sealed record ReaderLoanInfoDto(
   Guid Id,
   string Firstname,
   string Lastname,
   string Email,
   bool IsActive,
   bool IsProfileCompleted
);

/*
Lernziele und Didaktik
----------------------

Dieses DTO enthält nur Reader-Daten, die für Ausleihvorgänge relevant sind.

Es ist kein ReaderDto des Readers-Moduls und auch kein Reader-Aggregate.
Es ist ein bewusst kleines Contract-DTO für die Kommunikation zwischen
Modulen.

Part 6 ergänzt IsProfileCompleted. Dadurch kann an der Modulgrenze sichtbar
werden, dass Authentifizierung allein noch nicht genügt: Ein Reader muss auch
fachlich vollständig provisioniert sein, bevor er ausleihen darf.
*/
