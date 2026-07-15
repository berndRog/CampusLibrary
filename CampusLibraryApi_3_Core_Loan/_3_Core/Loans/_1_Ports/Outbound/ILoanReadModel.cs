using CampusLibraryApi._2_BuildingBlocks;
using CampusLibraryApi._3_Core.Loans._2_Application.Dtos;

namespace CampusLibraryApi._3_Core.Loans._1_Ports.Outbound;

// Outbound port for read operations of the Loans module.
// The concrete implementation is provided by Infrastructure.
public interface ILoanReadModel {

   // Finds one current loan by its id.
   // Returns a detailed DTO enriched with reader and book item information.
   Task<Result<LoanDto>> FindByIdAsync(
      Guid id,
      CancellationToken ct
   );

   // Finds one current loan only if it belongs to the given Reader.
   // This method is used by Reader self-service endpoints.
   Task<Result<LoanDto>> FindByIdForReaderAsync(
      Guid id,
      Guid readerId,
      CancellationToken ct
   );

   // Returns all currently borrowed loans.
   // Every stored Loan represents an active borrowing process.
   Task<Result<IReadOnlyList<LoanDto>>> FindAllBorrowedAsync(
      CancellationToken ct
   );

   // Returns all current loans of one Reader.
   // The filtering is performed in the API and not in the client.
   Task<Result<IReadOnlyList<LoanDto>>> FindBorrowedByReaderIdAsync(
      Guid readerId,
      CancellationToken ct
   );
}

/*
Lernziele und Didaktik
----------------------

Dieses Interface ist der ReadModel-Port des Loans-Moduls.

Nur aktuelle Ausleihen werden gespeichert. Ein vorhandener Loan bedeutet,
dass das referenzierte BookItem ausgeliehen ist. Zurückgegebene Loans werden
nicht mehr im ReadModel angezeigt, weil sie bei der Rückgabe gelöscht werden.

Die Methoden mit ReaderId unterstützen Self-Service-Endpunkte wie:

- GET /loans/me
- GET /loans/me/{id}

Wichtig ist dabei: Der Client entscheidet nicht selbst, welche Loan-Datensätze
zum angemeldeten Reader gehören. Die API bestimmt zuerst den fachlichen Reader
aus dem Token-Subject und filtert anschließend serverseitig.
*/
