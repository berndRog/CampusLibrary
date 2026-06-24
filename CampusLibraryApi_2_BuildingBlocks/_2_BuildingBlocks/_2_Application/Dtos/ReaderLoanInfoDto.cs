namespace CampusLibraryApi._2_BuildingBlocks._2_Application.Contracts;

// Loan-relevant reader data shared between modules.
public sealed record ReaderLoanInfoDto(
   Guid Id,
   string Firstname,
   string Lastname,
   bool IsActive
);

/*
Lernziele und Didaktik
----------------------

Dieses DTO enthält nur Reader-Daten, die für Ausleihvorgänge relevant sind.

Es ist kein ReaderDto des Readers-Moduls und auch kein Reader-Aggregate.
Es ist ein bewusst kleines Contract-DTO für die Kommunikation zwischen
Modulen.

Dadurch bleibt sichtbar:
Ein Modul gibt nicht automatisch seine vollständigen internen Daten frei.
Es veröffentlicht nur die Informationen, die ein anderes Modul wirklich
benötigt.
*/