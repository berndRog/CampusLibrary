namespace CampusLibraryApi._2_BuildingBlocks._2_Application.Dtos;

// Current loan information exposed by Loans to Catalog.
public sealed record CurrentBookItemLoanInfoDto(
   Guid BookItemId,
   string ReaderEmail,
   DateTime DueDate
);

/*
Lernziele und Didaktik
----------------------

Dieses DTO transportiert nur die Informationen, die das Catalog-Modul für
die Deaktivierungsprüfung eines Buchs benötigt.

Catalog erhält weder Loan-Entities noch direkten Zugriff auf die Loans- oder
Readers-Tabellen. Loans liefert nur das betroffene Exemplar, die E-Mail des
Readers und das Fälligkeitsdatum.
*/
