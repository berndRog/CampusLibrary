using CampusLibraryApi._2_BuildingBlocks;
using CampusLibraryApi._3_Core.Loans._2_Application.Dtos;

namespace CampusLibraryApi._3_Core.Loans._1_Ports.Outbound;

// Outbound port for read operations of the Loans module.
// The concrete implementation is provided by Infrastructure.
public interface ILoanReadModel {

   // Finds one loan by its id.
   // Returns a detailed DTO enriched with reader and book item information.
   Task<Result<LoanDetailDto>> FindByIdAsync(
      Guid id,
      CancellationToken ct
   );

   // Returns all currently borrowed loans.
   // A borrowed loan has LoanStatus.Borrowed and no return timestamp.
   Task<Result<IReadOnlyList<LoanListItemDto>>> FindAllBorrowedAsync(
      CancellationToken ct
   );
}

/*
Lernziele und Didaktik
----------------------

Dieses Interface ist der ReadModel-Port des Loans-Moduls.

Reader und Book verwenden IsActive, weil sie aktiviert oder deaktiviert
werden können.

BookItem und Loan verwenden Status, weil sie fachliche Zustände besitzen.

Für Loan bedeutet der Status Borrowed, dass ein Exemplar aktuell ausgeliehen
ist. Deshalb heißt die Listenabfrage FindAllBorrowedAsync und nicht
FindAllActiveAsync.

Damit bleibt die Sprache im Modell eindeutig:

- Reader ist aktiv oder deaktiviert.
- Book ist aktiv oder deaktiviert.
- BookItem ist verfügbar, nicht verfügbar, verloren oder beschädigt.
- Loan ist ausgeliehen, zurückgegeben oder storniert.
*/