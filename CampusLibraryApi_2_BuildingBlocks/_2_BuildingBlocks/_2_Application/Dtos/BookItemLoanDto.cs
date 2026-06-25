namespace CampusLibraryApi._2_BuildingBlocks._2_Application.Contracts;

// Loan-relevant book item data shared between modules.
public sealed record BookItemLoanInfoDto(
   Guid BookItemId,
   Guid BookId,
   string InventoryNumber,
   string Title,
   string? Subtitle,
   string AuthorsText,
   string Isbn,
   bool BookIsActive,
   bool IsAvailableForLoan
);

/*
Lernziele und Didaktik
----------------------

Dieses DTO enthält nur BookItem-Daten, die für Ausleihvorgänge relevant sind.

Es ist kein BookDto, kein BookItemDto und keine Catalog-Entity. Es ist ein
kleines Contract-DTO für die Kommunikation zwischen Modulen.

Besonders wichtig ist IsAvailableForLoan. Das Loans-Modul muss nicht wissen,
welchen internen BookItemStatus das Catalog-Modul verwendet.

Catalog entscheidet, ob ein Exemplar grundsätzlich ausleihbar ist.
Loans entscheidet anschließend, ob dieses Exemplar aktuell bereits aktiv
ausgeliehen ist.

Dadurch bleibt die Kopplung zwischen Catalog und Loans gering.
*/